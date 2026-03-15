using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Kwiztime.UI
{
    public class QuestionUI : MonoBehaviour
    {
        [Header("Core HUD")]
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text roundText;
        [SerializeField] private TMP_Text metaText;

        [Header("Answer Buttons")]
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TMP_Text[] answerLabels;

        [Header("Answer Visuals")]
        [SerializeField] private Outline[] outlines;
        [SerializeField] private OutlinePulse[] pulses;
        [SerializeField] private ScaleBounce[] scaleBounces;

        [Header("Stamp Icons")]
        [SerializeField] private Image[] stampIcons;
        [SerializeField] private Sprite correctStampSprite;
        [SerializeField] private Sprite wrongStampSprite;

        [Header("Results UI")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private Transform resultsContent;
        [SerializeField] private GameObject resultsRowPrefab;

        [Header("Outline Colors")]
        [SerializeField] private Color selectedOutlineColor = new Color(0.3f, 0.6f, 1f, 1f);
        [SerializeField] private Color correctOutlineColor  = new Color(0.2f, 0.85f, 0.35f, 1f);
        [SerializeField] private Color wrongOutlineColor    = new Color(0.95f, 0.25f, 0.25f, 1f);

        [Header("Results Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button backToMenuButton;

        private int _selectedIndex = -1;
        private int _correctIndex = -1;
        private readonly string[] _baseAnswers = new string[4];

        private void Awake()
        {
            AutoWireArraysSafe();
            ClearAllAnswerVisuals();

            if (resultsPanel != null)
                resultsPanel.SetActive(false);
        }

        private void Start()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);

            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        private void OnEnable()
        {
            ClientUIEvents.OnQuestion       += HandleQuestion;
            ClientUIEvents.OnTimer          += HandleTimer;
            ClientUIEvents.OnReveal         += HandleReveal;
            ClientUIEvents.OnStatus         += HandleStatus;
            ClientUIEvents.OnRound          += HandleRound;
            ClientUIEvents.OnQuestionMeta   += HandleMeta;
            ClientUIEvents.OnResults        += HandleResults;
            ClientUIEvents.OnResultsDetailed += HandleResultsDetailed;
        }

        private void OnDisable()
        {
            ClientUIEvents.OnQuestion       -= HandleQuestion;
            ClientUIEvents.OnTimer          -= HandleTimer;
            ClientUIEvents.OnReveal         -= HandleReveal;
            ClientUIEvents.OnStatus         -= HandleStatus;
            ClientUIEvents.OnRound          -= HandleRound;
            ClientUIEvents.OnQuestionMeta   -= HandleMeta;
            ClientUIEvents.OnResults        -= HandleResults;
            ClientUIEvents.OnResultsDetailed -= HandleResultsDetailed;
        }

        private void OnPlayAgainClicked()
        {
            if (NetworkServer.active)
            {
                KwizRoomManager.Instance.ServerStartMatchFromUI();
            }
            else
            {
                var local = NetworkClient.localPlayer;
                if (local == null) return;

                var kp = local.GetComponent<Kwiztime.KwizPlayer>();
                if (kp != null)
                    kp.CmdRequestPlayAgain();
            }
        }

        private void OnBackToMenuClicked()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
                NetworkManager.singleton.StopHost();
            else if (NetworkClient.isConnected)
                NetworkManager.singleton.StopClient();

            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        // ----------------------------------------------------
        // Question flow
        // ----------------------------------------------------

        private void HandleQuestion(string prompt, string[] answers, float timeLimit)
        {
            _selectedIndex = -1;
            _correctIndex  = -1;

            if (resultsPanel != null)
                resultsPanel.SetActive(false);

            ClearResultsRows();
            ClearAllAnswerVisuals();

            if (promptText != null)
                promptText.text = prompt;

            for (int i = 0; i < 4; i++)
            {
                _baseAnswers[i] = (answers != null && i < answers.Length) ? answers[i] : "";

                if (answerLabels != null && answerLabels.Length > i && answerLabels[i] != null)
                    answerLabels[i].text = _baseAnswers[i];

                if (answerButtons != null && answerButtons.Length > i && answerButtons[i] != null)
                {
                    int idx = i;
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => Submit(idx));
                    answerButtons[i].interactable = true;
                }
            }

            SetStatus("Answer now!");
        }

        private void HandleTimer(float remaining)
        {
            if (timerText != null)
                timerText.text = $"Time: {Mathf.CeilToInt(remaining)}";
        }

        private void HandleReveal(int correctIndex)
        {
            _correctIndex = correctIndex;

            // FIX: read server-authoritative selected answer from SyncVar
            // instead of trusting the local _selectedIndex which may be stale
            // if the server rejected a late submission
            var local = NetworkClient.localPlayer;
            if (local != null)
            {
                var kp = local.GetComponent<Kwiztime.KwizPlayer>();
                if (kp != null)
                    _selectedIndex = kp.selectedAnswer;
            }

            if (answerButtons != null)
            {
                for (int i = 0; i < answerButtons.Length; i++)
                    if (answerButtons[i] != null)
                        answerButtons[i].interactable = false;
            }

            ApplyRevealVisuals();

            if (_selectedIndex == -1)              SetStatus("Time's up!");
            else if (_selectedIndex == _correctIndex) SetStatus("Correct!");
            else                                   SetStatus("Wrong!");
        }

        private void Submit(int index)
        {
            _selectedIndex = index;
            ApplySelectedVisuals();

            var local = NetworkClient.localPlayer;
            if (local == null) return;

            var kp = local.GetComponent<Kwiztime.KwizPlayer>();
            if (kp != null)
                kp.CmdSubmitAnswer(index);
        }

        // ----------------------------------------------------
        // Results
        // ----------------------------------------------------

        private void HandleResults()
        {
            // Kept for backwards compatibility but panel is shown in HandleResultsDetailed
            if (resultsPanel != null)
                resultsPanel.SetActive(true);
        }

        private void HandleResultsDetailed(uint[] netIds, string[] names, int[] coins, bool[] isBots, int[] mascotIds)
        {
            Debug.Log($"[QuestionUI] HandleResultsDetailed called. Count={(netIds != null ? netIds.Length : 0)}");

            if (resultsPanel != null)
                resultsPanel.SetActive(true);

            if (resultsContent == null || resultsRowPrefab == null)
            {
                Debug.LogWarning("[QuestionUI] resultsContent or resultsRowPrefab not assigned.");
                return;
            }

            ClearResultsRows();

            if (netIds == null || names == null || coins == null || isBots == null || mascotIds == null)
            {
                Debug.LogWarning("[QuestionUI] Results arrays are null.");
                return;
            }

            int count = netIds.Length;
            if (names.Length != count || coins.Length != count || isBots.Length != count || mascotIds.Length != count)
            {
                Debug.LogWarning("[QuestionUI] Results arrays length mismatch.");
                return;
            }

            uint myNetId = (NetworkClient.localPlayer != null) ? NetworkClient.localPlayer.netId : 0;

            int best = int.MinValue;
            for (int i = 0; i < count; i++)
                best = Mathf.Max(best, coins[i]);

            for (int i = 0; i < count; i++)
            {
                var row  = Instantiate(resultsRowPrefab, resultsContent);
                var view = row.GetComponent<ResultsRowView>();
                if (view == null)
                {
                    Debug.LogWarning("[QuestionUI] ResultsRowView missing on resultsRowPrefab.");
                    continue;
                }

                bool isWinner = coins[i] == best;
                bool isYou    = (myNetId != 0 && netIds[i] == myNetId);
                string display = string.IsNullOrWhiteSpace(names[i]) ? $"Player {netIds[i]}" : names[i];

                view.Set(display, coins[i], isWinner, isYou, isBots[i], mascotIds[i]);
            }
        }

        private void ClearResultsRows()
        {
            if (resultsContent == null) return;
            for (int i = resultsContent.childCount - 1; i >= 0; i--)
                Destroy(resultsContent.GetChild(i).gameObject);
        }

        // ----------------------------------------------------
        // Status / meta
        // ----------------------------------------------------

        private void HandleStatus(string status) => SetStatus(status);

        private void HandleRound(int round, int total)
        {
            if (roundText != null)
                roundText.text = $"Round {round}/{total}";
        }

        private void HandleMeta(string meta)
        {
            if (metaText != null)
                metaText.text = meta ?? "";
        }

        private void SetStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg ?? "";
        }

        // ----------------------------------------------------
        // Answer visuals
        // ----------------------------------------------------

        private void ClearAllAnswerVisuals()
        {
            for (int i = 0; i < 4; i++)
            {
                if (outlines != null && outlines.Length > i && outlines[i] != null)
                    outlines[i].enabled = false;

                if (stampIcons != null && stampIcons.Length > i && stampIcons[i] != null)
                    stampIcons[i].gameObject.SetActive(false);
            }
        }

        private void ApplySelectedVisuals()
        {
            ClearAllAnswerVisuals();
            if (_selectedIndex < 0 || _selectedIndex > 3) return;

            var outline = outlines != null && outlines.Length > _selectedIndex ? outlines[_selectedIndex] : null;
            if (outline != null)
            {
                outline.effectColor = selectedOutlineColor;
                outline.enabled = true;
            }
        }

        private void ApplyRevealVisuals()
        {
            ClearAllAnswerVisuals();

            if (_correctIndex >= 0 && _correctIndex <= 3)
            {
                var o = outlines != null && outlines.Length > _correctIndex ? outlines[_correctIndex] : null;
                if (o != null) { o.effectColor = correctOutlineColor; o.enabled = true; }

                pulses?[_correctIndex]?.Play();
                scaleBounces?[_correctIndex]?.Play();
            }

            if (_selectedIndex >= 0 && _selectedIndex <= 3)
            {
                bool isCorrect = _selectedIndex == _correctIndex;

                var o = outlines != null && outlines.Length > _selectedIndex ? outlines[_selectedIndex] : null;
                if (o != null) { o.effectColor = isCorrect ? correctOutlineColor : wrongOutlineColor; o.enabled = true; }

                var icon = stampIcons != null && stampIcons.Length > _selectedIndex ? stampIcons[_selectedIndex] : null;
                if (icon != null)
                {
                    icon.sprite = isCorrect ? correctStampSprite : wrongStampSprite;
                    icon.preserveAspect = true;
                    icon.gameObject.SetActive(icon.sprite != null);
                }
            }
        }

        // ----------------------------------------------------
        // Auto wiring
        // ----------------------------------------------------

        private void AutoWireArraysSafe()
        {
            if (answerButtons == null || answerButtons.Length != 4)
            {
                Debug.LogError("[QuestionUI] answerButtons must be size 4 in Inspector.");
                return;
            }

            outlines     = EnsureArray(outlines);
            pulses       = EnsureArray(pulses);
            scaleBounces = EnsureArray(scaleBounces);
            stampIcons   = EnsureArray(stampIcons);

            for (int i = 0; i < 4; i++)
            {
                var btn = answerButtons[i];
                if (btn == null) continue;

                if (outlines[i]     == null) outlines[i]     = btn.GetComponent<Outline>();
                if (pulses[i]       == null) pulses[i]       = btn.GetComponent<OutlinePulse>();
                if (scaleBounces[i] == null) scaleBounces[i] = btn.GetComponent<ScaleBounce>();

                if (stampIcons[i] == null)
                {
                    var t = FindChildRecursive(btn.transform, "StampIcon");
                    if (t != null) stampIcons[i] = t.GetComponent<Image>();
                }
            }
        }

        private T[] EnsureArray<T>(T[] arr) where T : class
        {
            if (arr == null || arr.Length != 4) return new T[4];
            return arr;
        }

        private Transform FindChildRecursive(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;

            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindChildRecursive(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}