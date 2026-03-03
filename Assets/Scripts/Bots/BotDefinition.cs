using UnityEngine;

namespace Kwiztime
{
    public enum BotPersonality
    {
        Friendly,
        Competitive,
        Chill,
        Snarky,
        RareBoss
    }

    [System.Serializable]
    public class BotDefinition
    {
        public string botId;              // persistent key like "bot_nori"
        public string displayName;        // shown in UI
        public BotPersonality personality;

        [Range(0f, 1f)] public float accuracy = 0.65f; // chance to pick correct answer
        public float minDelay = 1.0f;     // seconds before answering
        public float maxDelay = 4.0f;

        public int mascotId = 0;          // index into your bot mascot sprite array

        public bool isRare = false;       // rare boss bot
        public int bonusCoins = 0;        // optional later
    }
}