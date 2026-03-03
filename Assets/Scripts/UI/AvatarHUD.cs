using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Kwiztime.UI
{
    public class AvatarHUD : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform stripParent;         // UI_PlayerStrip
        [SerializeField] private GameObject avatarSlotPrefab;   // PF_AvatarSlot

        // Mapping netId -> slot view
        private readonly Dictionary<uint, AvatarSlotView> _slots = new();

        private void OnEnable()
        {
            ClientUIEvents.OnQuestion += OnQuestion;
            ClientUIEvents.OnReveal += OnReveal;
            ClientUIEvents.OnChat += OnChat; // public bot chatter
            // If you kept OnChatPrivate, you can also subscribe (optional)
            // ClientUIEvents.OnChatPrivate += OnChatPrivate;
        }

        private void OnDisable()
        {
            ClientUIEvents.OnQuestion -= OnQuestion;
            ClientUIEvents.OnReveal -= OnReveal;
            ClientUIEvents.OnChat -= OnChat;
            // ClientUIEvents.OnChatPrivate -= OnChatPrivate;
        }

        private void Start()
        {
            RebuildStrip();
        }

        private void Update()
        {
            // Keep it simple: rebuild if counts mismatch (prototype-friendly)
            // Later you can hook Mirror spawn events.
            if (NetworkClient.active && NeedsRebuild())
                RebuildStrip();
        }

        private bool NeedsRebuild()
        {
            int players = 0;
            foreach (var kvp in NetworkClient.spawned)
            {
                if (kvp.Value == null) continue;
                if (kvp.Value.GetComponent<Kwiztime.KwizPlayer>() != null) players++;
            }
            return players != _slots.Count;
        }

        private void RebuildStrip()
        {
            if (stripParent == null || avatarSlotPrefab == null) return;

            // Clear old
            foreach (Transform child in stripParent)
                Destroy(child.gameObject);

            _slots.Clear();

            // Build from spawned KwizPlayer objects
            foreach (var kvp in NetworkClient.spawned)
            {
                var obj = kvp.Value;
                if (obj == null) continue;

                var kp = obj.GetComponent<Kwiztime.KwizPlayer>();
                if (kp == null) continue;

                var go = Instantiate(avatarSlotPrefab, stripParent);
                var view = go.GetComponent<AvatarSlotView>();
                if (view == null) continue;

                view.Bind(kp);
                view.HideBubble();

                _slots[kp.netId] = view;
            }
        }

        private void OnQuestion(string prompt, string[] answers, float timeLimit)
        {
            // Hide bubbles at start of each question
            foreach (var s in _slots.Values)
                s.HideBubble();
        }

        private void OnReveal(int correctIndex)
        {
            // At reveal: show each player's selected answer + correct/wrong icon
            // Answer text labels are best taken from the current question UI,
            // but simplest is to map index -> A/B/C/D or use the label strings from QuestionUI.
            // We'll do A/B/C/D for now (you can upgrade to actual label text later).
            foreach (var kvp in NetworkClient.spawned)
            {
                var obj = kvp.Value;
                if (obj == null) continue;

                var kp = obj.GetComponent<Kwiztime.KwizPlayer>();
                if (kp == null) continue;

                if (!_slots.TryGetValue(kp.netId, out var slot) || slot == null)
                    continue;

                int ans = kp.selectedAnswer;
                if (ans < 0 || ans > 3)
                {
                    slot.ShowChatBubble("…"); // no answer
                    continue;
                }

                string label = ans switch
                {
                    0 => "A",
                    1 => "B",
                    2 => "C",
                    3 => "D",
                    _ => "?"
                };

                bool correct = (ans == correctIndex);
                slot.ShowAnswerBubble(label, correct);
            }
        }

        private void OnChat(string speaker, string message)
        {
            // Find the bot by displayName and show bubble on that bot slot.
            // (displayName is SyncVar for bots too)
            foreach (var kvp in NetworkClient.spawned)
            {
                var kp = kvp.Value != null ? kvp.Value.GetComponent<Kwiztime.KwizPlayer>() : null;
                if (kp == null) continue;

                if (kp.displayName == speaker && _slots.TryGetValue(kp.netId, out var slot) && slot != null)
                {
                    slot.ShowChatBubble(message);
                    return;
                }
            }
        }
    }
}