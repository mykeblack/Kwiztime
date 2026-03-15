using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Kwiztime.UI
{
    public class AvatarHUD : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform stripParent;
        [SerializeField] private GameObject avatarSlotPrefab;

        // FIX: poll interval replaces per-frame NeedsRebuild() iteration
        [Header("Rebuild Settings")]
        [SerializeField] private float rebuildCheckInterval = 1f;

        private readonly Dictionary<uint, AvatarSlotView> _slots = new();
        private float _rebuildTimer = 0f;

        private void OnEnable()
        {
            ClientUIEvents.OnQuestion += OnQuestion;
            ClientUIEvents.OnReveal   += OnReveal;
            ClientUIEvents.OnChat     += OnChat;
        }

        private void OnDisable()
        {
            ClientUIEvents.OnQuestion -= OnQuestion;
            ClientUIEvents.OnReveal   -= OnReveal;
            ClientUIEvents.OnChat     -= OnChat;
        }

        private void Start()
        {
            RebuildStrip();
        }

        private void Update()
        {
            if (!NetworkClient.active) return;

            // FIX: throttle rebuild check to once per second instead of every frame
            _rebuildTimer += Time.deltaTime;
            if (_rebuildTimer < rebuildCheckInterval) return;
            _rebuildTimer = 0f;

            if (NeedsRebuild())
                RebuildStrip();
        }

        private bool NeedsRebuild()
        {
            int players = 0;
            foreach (var kvp in NetworkClient.spawned)
            {
                if (kvp.Value == null) continue;
                if (kvp.Value.GetComponent<Kwiztime.KwizPlayer>() != null)
                    players++;
            }
            return players != _slots.Count;
        }

        private void RebuildStrip()
        {
            if (stripParent == null || avatarSlotPrefab == null) return;

            foreach (Transform child in stripParent)
                Destroy(child.gameObject);

            _slots.Clear();

            foreach (var kvp in NetworkClient.spawned)
            {
                var obj = kvp.Value;
                if (obj == null) continue;

                var kp = obj.GetComponent<Kwiztime.KwizPlayer>();
                if (kp == null) continue;

                var go   = Instantiate(avatarSlotPrefab, stripParent);
                var view = go.GetComponent<AvatarSlotView>();
                if (view == null) continue;

                view.Bind(kp);
                view.HideBubble();

                _slots[kp.netId] = view;
            }
        }

        private void OnQuestion(string prompt, string[] answers, float timeLimit)
        {
            foreach (var s in _slots.Values)
                s.HideBubble();
        }

        private void OnReveal(int correctIndex)
        {
            foreach (var kvp in NetworkClient.spawned)
            {
                var obj = kvp.Value;
                if (obj == null) continue;

                var kp = obj.GetComponent<Kwiztime.KwizPlayer>();
                if (kp == null) continue;

                if (!_slots.TryGetValue(kp.netId, out var slot) || slot == null) continue;

                int ans = kp.selectedAnswer;
                if (ans < 0 || ans > 3)
                {
                    slot.ShowChatBubble("…");
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

                slot.ShowAnswerBubble(label, ans == correctIndex);
            }
        }

        private void OnChat(string speaker, string message)
        {
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