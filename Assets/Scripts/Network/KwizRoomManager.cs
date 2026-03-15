using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Kwiztime
{
    public enum RoomState
    {
        Lobby,
        InMatch,
        Results
    }

    /// <summary>
    /// Server-authoritative room/match controller.
    /// FIXES:
    /// - Bot accuracy/delay now read from BotRegistry (no more hardcoded duplicate values)
    /// - Legend Bot accuracy capped at 90% as per GDD
    /// - Wrong answer selection fixed (was off-by-one, could miss answer index 3)
    /// - _matchRoutine now resets in a finally block so early exits don't permanently block rematches
    /// - Legend Bot spawn rate (~1.5% of matches) now implemented
    /// - ServerStartMatchFromUI now respects minPlayersToStart
    /// </summary>
    public class KwizRoomManager : NetworkBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private QuestionService questionService;

        [Header("Match Settings")]
        [SerializeField] private int totalRounds = 5;
        [SerializeField] private int coinsPerCorrect = 10;
        [SerializeField] private float betweenRoundsDelay = 2.5f;

        [Header("Lobby / Start")]
        [SerializeField] private int minPlayersToStart = 1;

        [Header("Bots")]
        [SerializeField] private bool fillWithBots = true;
        [SerializeField] private int targetPlayers = 8;

        [Header("Legend Bot")]
        [SerializeField, Range(0f, 1f)] private float legendBotSpawnChance = 0.015f; // ~1.5% per GDD

        [Header("Results")]
        [SerializeField] private float resultsDuration = 6f;

        [Header("Bot Chatter (Public)")]
        [SerializeField] private int maxBotChatsPerRound = 2;

        [Header("Bot Private Reactions")]
        [SerializeField] private bool botPrivateReactionsEnabled = true;
        [SerializeField, Range(0f, 1f)] private float rareBossSpeakChance = 0.12f;
        [SerializeField] private int losingByCoinsThreshold = 20;

        private RoomState _state = RoomState.Lobby;
        private bool _answersLocked = false;
        private Coroutine _matchRoutine;

        private readonly List<KwizPlayer> _players = new();
        private readonly List<KwizPlayer> _botPlayers = new();

        private int _botChatsThisRound = 0;

        public static KwizRoomManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // ------------------------
        // Unity / Mirror lifecycle
        // ------------------------

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (questionService == null)
                questionService = FindFirstObjectByType<QuestionService>();

            _players.Clear();
            _botPlayers.Clear();
            _state = RoomState.Lobby;

            Debug.Log("[KwizRoomManager] Server started.");
        }

        // ------------------------
        // Public API (server)
        // ------------------------

        [Server]
        public void ServerRegisterPlayer(KwizPlayer player)
        {
            if (player == null) return;
            if (_players.Contains(player)) return;

            _players.Add(player);
            RpcStatus($"Player joined. {_players.Count}/{targetPlayers}");

            TryStartIfReady();
        }

        [Server]
        public void ServerUnregisterPlayer(KwizPlayer player)
        {
            if (player == null) return;

            _players.Remove(player);
            _botPlayers.Remove(player);

            RpcStatus($"Player left. {_players.Count}/{targetPlayers}");
        }

        // ------------------------
        // Match start conditions
        // ------------------------

        [Server]
        private void TryStartIfReady()
        {
            if (_state != RoomState.Lobby) return;
            if (_matchRoutine != null) return;

            int realPlayers = CountRealPlayers();
            if (realPlayers < minPlayersToStart) return;

            ServerFillBotsIfNeeded();

            _matchRoutine = StartCoroutine(ServerMatchFlow());
        }

        [Server]
        private int CountRealPlayers()
        {
            int count = 0;
            for (int i = 0; i < _players.Count; i++)
            {
                var p = _players[i];
                if (p == null || p.isBot) continue;
                count++;
            }
            return count;
        }

        // ------------------------
        // Match loop
        // ------------------------

        [Server]
        private IEnumerator ServerMatchFlow()
        {
            _state = RoomState.InMatch;
            RpcStatus("Get ready!");
            yield return new WaitForSeconds(2f);

            // Reset match state
            for (int i = 0; i < _players.Count; i++)
            {
                var p = _players[i];
                if (p == null) continue;
                p.coins = 0;
                p.selectedAnswer = -1;
            }

            // FIX: wrap in try/finally so _matchRoutine always resets even on early exit
            try
            {
                for (int round = 1; round <= totalRounds; round++)
                {
                    if (questionService == null)
                    {
                        Debug.LogError("[KwizRoomManager] QuestionService missing.");
                        break;
                    }

                    var q = questionService.GetNextQuestion();
                    if (string.IsNullOrWhiteSpace(q.prompt))
                    {
                        Debug.LogError("[KwizRoomManager] Invalid question.");
                        break;
                    }

                    _answersLocked = false;
                    _botChatsThisRound = 0;

                    for (int i = 0; i < _players.Count; i++)
                    {
                        var p = _players[i];
                        if (p == null) continue;
                        p.selectedAnswer = -1;
                    }

                    RpcRound(round, totalRounds);
                    RpcQuestionMeta($"{q.category} • {q.difficulty}");
                    RpcShowQuestion(q.prompt, q.answers, q.timeLimit);

                    ServerBotRoundStartChatter();

                    for (int i = 0; i < _players.Count; i++)
                    {
                        var p = _players[i];
                        if (p == null || !p.isBot) continue;
                        StartCoroutine(ServerBotAnswerRoutine(p, q.correctIndex));
                    }

                    float remaining = q.timeLimit;
                    while (remaining > 0f)
                    {
                        RpcTimer(remaining);
                        yield return new WaitForSeconds(1f);
                        remaining -= 1f;
                    }

                    _answersLocked = true;

                    for (int i = 0; i < _players.Count; i++)
                    {
                        var p = _players[i];
                        if (p == null) continue;

                        bool answered = p.selectedAnswer >= 0 && p.selectedAnswer <= 3;
                        bool correct = answered && (p.selectedAnswer == q.correctIndex);

                        if (correct)
                            p.coins += coinsPerCorrect;
                    }

                    RpcReveal(q.correctIndex);
                    ServerBotRevealChatter(q.correctIndex);
                    ServerSendTargetedBotReactions(q.correctIndex);

                    yield return new WaitForSeconds(betweenRoundsDelay);
                }
            }
            finally
            {
                // FIX: always clean up match state, even if coroutine exits early
                _state = RoomState.Results;
                BuildAndSendResults();

                // Wait for results display before cleanup
                // (can't yield in finally, so we chain a cleanup coroutine)
                StartCoroutine(PostResultsCleanup());
            }
        }

        [Server]
        private IEnumerator PostResultsCleanup()
        {
            yield return new WaitForSeconds(resultsDuration);

            ServerDespawnBots();

            _matchRoutine = null;
            _state = RoomState.Lobby;
            RpcStatus("Back to lobby. Waiting for players...");
        }

        // ------------------------
        // Results
        // ------------------------

        [Server]
        private void BuildAndSendResults()
        {
            var sorted = new List<KwizPlayer>(_players);
            sorted.RemoveAll(p => p == null);
            sorted.Sort((a, b) => b.coins.CompareTo(a.coins));

            int count = sorted.Count;
            var netIds   = new uint[count];
            var names    = new string[count];
            var coins    = new int[count];
            var isBots   = new bool[count];
            var mascotIds = new int[count];

            for (int i = 0; i < count; i++)
            {
                var p = sorted[i];
                netIds[i]    = p.netId;
                names[i]     = string.IsNullOrWhiteSpace(p.displayName) ? $"Player {p.netId}" : p.displayName;
                coins[i]     = p.coins;
                isBots[i]    = p.isBot;
                mascotIds[i] = p.isBot ? p.botMascotId : -1;
            }

            Debug.Log($"[Server] Sending results. Entries={count}");
            RpcShowResults(netIds, names, coins, isBots, mascotIds);
        }

        // ------------------------
        // Bots: fill/spawn/despawn
        // ------------------------

        [Server]
        private void ServerFillBotsIfNeeded()
        {
            if (!fillWithBots) return;

            _botPlayers.RemoveAll(b => b == null);

            int currentTotal = _players.Count;
            int needed = Mathf.Clamp(targetPlayers - currentTotal, 0, targetPlayers);
            if (needed <= 0) return;

            var nm = NetworkManager.singleton;
            if (nm == null || nm.playerPrefab == null)
            {
                Debug.LogError("[Server] Cannot spawn bots: NetworkManager/playerPrefab missing.");
                return;
            }

            // FIX: ~1.5% chance to include one Legend Bot per match (per GDD)
            bool spawnLegendBot = Random.value < legendBotSpawnChance;
            bool legendSpawned = false;

            for (int i = 0; i < needed; i++)
            {
                BotDefinition def;

                // Try to spawn a legend bot once per match if roll succeeded
                if (spawnLegendBot && !legendSpawned)
                {
                    def = BotRegistry.GetRandomRare();
                    if (def != null)
                    {
                        legendSpawned = true;
                    }
                    else
                    {
                        def = BotRegistry.GetRandomNonRare();
                    }
                }
                else
                {
                    def = BotRegistry.GetRandomNonRare();
                }

                if (def == null) break;

                var go = Instantiate(nm.playerPrefab);
                var kp = go.GetComponent<KwizPlayer>();
                if (kp == null)
                {
                    Debug.LogError("[Server] Spawned bot has no KwizPlayer.");
                    Destroy(go);
                    continue;
                }

                kp.isBot       = true;
                kp.displayName = def.displayName;
                kp.botMascotId = def.mascotId;
                kp.botId       = def.botId;

                NetworkServer.Spawn(go);

                _botPlayers.Add(kp);
                _players.Add(kp);

                Debug.Log($"[Server] Spawned bot: {def.botId} ({def.displayName}){(def.isRare ? " [LEGEND]" : "")}");
            }

            RpcStatus($"Lobby filled: {_players.Count}/{targetPlayers} (including bots)");
        }

        [Server]
        private void ServerDespawnBots()
        {
            for (int i = 0; i < _botPlayers.Count; i++)
            {
                var b = _botPlayers[i];
                if (b == null) continue;
                NetworkServer.Destroy(b.gameObject);
            }

            _botPlayers.Clear();
            _players.RemoveAll(p => p == null || p.isBot);

            Debug.Log("[Server] Bots despawned.");
        }

        // ------------------------
        // Bots: answering
        // ------------------------

        [Server]
        private IEnumerator ServerBotAnswerRoutine(KwizPlayer bot, int correctIndex)
        {
            if (bot == null) yield break;

            // FIX: read stats from BotRegistry instead of hardcoded switch
            float accuracy = 0.65f;
            float minDelay = 1.5f;
            float maxDelay = 3.5f;

            var def = BotRegistry.GetById(bot.botId);
            if (def != null)
            {
                // FIX: enforce 90% accuracy cap on Legend Bots per GDD
                accuracy = def.isRare ? Mathf.Min(def.accuracy, 0.90f) : def.accuracy;
                minDelay = def.minDelay;
                maxDelay = def.maxDelay;
            }

            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            if (!ServerCanAcceptAnswers()) yield break;

            bool pickCorrect = Random.value < accuracy;
            int chosen;

            if (pickCorrect)
            {
                chosen = correctIndex;
            }
            else
            {
                // FIX: build explicit wrong answer list to avoid off-by-one bug
                var wrongAnswers = new List<int> { 0, 1, 2, 3 };
                wrongAnswers.Remove(correctIndex);
                chosen = wrongAnswers[Random.Range(0, wrongAnswers.Count)];
            }

            bot.selectedAnswer = chosen;
        }

        [Server]
        public bool ServerCanAcceptAnswers()
        {
            return _state == RoomState.InMatch && !_answersLocked;
        }

        // ------------------------
        // Bots: chatter
        // ------------------------

        [Server]
        private void ServerBotRoundStartChatter()
        {
            if (_botChatsThisRound >= maxBotChatsPerRound) return;

            var bot = PickRandomBot();
            if (bot == null) return;

            var def = BotRegistry.GetById(bot.botId);
            if (def == null) return;

            RpcChat(def.displayName, BotRegistry.PickRoundStartLine(def.personality));
            _botChatsThisRound++;
        }

        [Server]
        private void ServerBotRevealChatter(int correctIndex)
        {
            for (int attempt = 0; attempt < 8 && _botChatsThisRound < maxBotChatsPerRound; attempt++)
            {
                var bot = PickRandomBot();
                if (bot == null) return;

                var def = BotRegistry.GetById(bot.botId);
                if (def == null) continue;

                bool botAnswered = bot.selectedAnswer >= 0 && bot.selectedAnswer <= 3;
                bool botCorrect  = botAnswered && (bot.selectedAnswer == correctIndex);

                RpcChat(def.displayName,
                    botCorrect
                        ? BotRegistry.PickCorrectLine(def.personality)
                        : BotRegistry.PickWrongLine(def.personality));

                _botChatsThisRound++;
            }
        }

        [Server]
        private void ServerSendTargetedBotReactions(int correctIndex)
        {
            if (!botPrivateReactionsEnabled) return;

            int topCoins = int.MinValue;
            for (int i = 0; i < _players.Count; i++)
            {
                var p = _players[i];
                if (p == null || p.isBot) continue;
                topCoins = Mathf.Max(topCoins, p.coins);
            }
            if (topCoins == int.MinValue) topCoins = 0;

            for (int i = 0; i < _players.Count; i++)
            {
                var p = _players[i];
                if (p == null || p.isBot) continue;

                var conn = p.connectionToClient;
                if (conn == null) continue;

                bool answered = p.selectedAnswer >= 0 && p.selectedAnswer <= 3;
                bool correct  = answered && (p.selectedAnswer == correctIndex);

                int deltaFromLead  = topCoins - p.coins;
                bool leadingOrTied = deltaFromLead <= 0;
                bool losingBadly   = deltaFromLead >= losingByCoinsThreshold;

                var speaker = ChooseReactionBotForSituation(correct, answered, leadingOrTied, losingBadly);
                if (speaker == null) continue;

                var def = BotRegistry.GetById(speaker.botId);
                if (def == null) continue;

                string line;
                if (!answered)
                    line = BotRegistry.PickComfortLine(def.personality);
                else if (correct)
                    line = BotRegistry.PickPraiseLine(def.personality);
                else
                    line = losingBadly
                        ? BotRegistry.PickComfortLine(def.personality)
                        : BotRegistry.PickTauntLine(def.personality);

                TargetPrivateChat(conn, def.displayName, line);
            }
        }

        [Server]
        private KwizPlayer ChooseReactionBotForSituation(bool playerCorrect, bool playerAnswered, bool playerLeadingOrTied, bool playerLosingBadly)
        {
            if (Random.value < rareBossSpeakChance)
            {
                var boss = PickBotByPersonality(BotPersonality.RareBoss);
                if (boss != null) return boss;
            }

            if (!playerAnswered)
                return PickFirstAvailableBot(BotPersonality.Friendly, BotPersonality.Chill, BotPersonality.Competitive, BotPersonality.Snarky);

            if (playerCorrect)
            {
                if (playerLosingBadly)   return PickFirstAvailableBot(BotPersonality.Friendly, BotPersonality.Chill, BotPersonality.Competitive);
                if (playerLeadingOrTied) return PickFirstAvailableBot(BotPersonality.Competitive, BotPersonality.Snarky, BotPersonality.Friendly);
                return PickFirstAvailableBot(BotPersonality.Chill, BotPersonality.Friendly, BotPersonality.Competitive);
            }

            if (playerLosingBadly)   return PickFirstAvailableBot(BotPersonality.Friendly, BotPersonality.Chill, BotPersonality.Competitive);
            if (playerLeadingOrTied) return PickFirstAvailableBot(BotPersonality.Snarky, BotPersonality.Competitive, BotPersonality.Chill);
            return PickFirstAvailableBot(BotPersonality.Competitive, BotPersonality.Snarky, BotPersonality.Chill);
        }

        [Server]
        private KwizPlayer PickFirstAvailableBot(params BotPersonality[] preferredOrder)
        {
            for (int i = 0; i < preferredOrder.Length; i++)
            {
                var b = PickBotByPersonality(preferredOrder[i]);
                if (b != null) return b;
            }

            for (int i = 0; i < _players.Count; i++)
            {
                var p = _players[i];
                if (p != null && p.isBot) return p;
            }

            return null;
        }

        [Server]
        private KwizPlayer PickBotByPersonality(BotPersonality personality)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                var p = _players[i];
                if (p == null || !p.isBot) continue;

                var def = BotRegistry.GetById(p.botId);
                if (def != null && def.personality == personality)
                    return p;
            }
            return null;
        }

        [Server]
        private KwizPlayer PickRandomBot()
        {
            List<KwizPlayer> bots = null;

            for (int i = 0; i < _players.Count; i++)
            {
                var p = _players[i];
                if (p == null || !p.isBot) continue;
                bots ??= new List<KwizPlayer>();
                bots.Add(p);
            }

            if (bots == null || bots.Count == 0) return null;
            return bots[Random.Range(0, bots.Count)];
        }

        [Server]
        public void ServerStartMatchFromUI()
        {
            if (_state != RoomState.Lobby && _state != RoomState.Results) return;
            if (_matchRoutine != null) return;

            // FIX: respect minPlayersToStart (was bypassed before)
            if (CountRealPlayers() < minPlayersToStart)
            {
                Debug.LogWarning("[Server] Not enough real players to start.");
                return;
            }

            ServerDespawnBots();
            ServerFillBotsIfNeeded();

            _matchRoutine = StartCoroutine(ServerMatchFlow());
        }

        // ------------------------
        // RPCs
        // ------------------------

        [ClientRpc] private void RpcShowQuestion(string prompt, string[] answers, float timeLimit)
            => Kwiztime.UI.ClientUIEvents.OnQuestion?.Invoke(prompt, answers, timeLimit);

        [ClientRpc] private void RpcTimer(float remaining)
            => Kwiztime.UI.ClientUIEvents.OnTimer?.Invoke(remaining);

        [ClientRpc] private void RpcReveal(int correctIndex)
            => Kwiztime.UI.ClientUIEvents.OnReveal?.Invoke(correctIndex);

        [ClientRpc] private void RpcRound(int round, int total)
            => Kwiztime.UI.ClientUIEvents.OnRound?.Invoke(round, total);

        [ClientRpc] private void RpcStatus(string status)
            => Kwiztime.UI.ClientUIEvents.OnStatus?.Invoke(status);

        [ClientRpc] private void RpcQuestionMeta(string meta)
            => Kwiztime.UI.ClientUIEvents.OnQuestionMeta?.Invoke(meta);

        [ClientRpc] private void RpcChat(string speaker, string message)
            => Kwiztime.UI.ClientUIEvents.OnChat?.Invoke(speaker, message);

        [TargetRpc] private void TargetPrivateChat(NetworkConnectionToClient target, string speaker, string message)
            => Kwiztime.UI.ClientUIEvents.OnChatPrivate?.Invoke(speaker, message);

        [ClientRpc] private void RpcShowResults(uint[] playerNetIds, string[] names, int[] playerCoins, bool[] isBots, int[] mascotIds)
        {
            Kwiztime.UI.ClientUIEvents.OnResultsDetailed?.Invoke(playerNetIds, names, playerCoins, isBots, mascotIds);
            Kwiztime.UI.ClientUIEvents.OnResults?.Invoke();
        }
    }
}