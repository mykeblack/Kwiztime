using System.Collections.Generic;

namespace Kwiztime
{
    public static class BotRegistry
    {
        private static readonly List<BotDefinition> _bots = new()
        {
            new BotDefinition { botId="bot_nori", displayName="Nori", personality=BotPersonality.Friendly,     accuracy=0.55f, minDelay=1.6f, maxDelay=4.2f, mascotId=0 },
            new BotDefinition { botId="bot_mika", displayName="Mika", personality=BotPersonality.Chill,       accuracy=0.60f, minDelay=1.8f, maxDelay=4.5f, mascotId=1 },
            new BotDefinition { botId="bot_sora", displayName="Sora", personality=BotPersonality.Competitive, accuracy=0.72f, minDelay=1.2f, maxDelay=3.2f, mascotId=2 },
            new BotDefinition { botId="bot_yuzu", displayName="Yuzu", personality=BotPersonality.Snarky,      accuracy=0.66f, minDelay=1.4f, maxDelay=3.6f, mascotId=3 },

            new BotDefinition { botId="bot_kumo", displayName="Kumo", personality=BotPersonality.RareBoss,    accuracy=0.92f, minDelay=0.9f, maxDelay=2.0f, mascotId=4, isRare=true, bonusCoins=50 },
            new BotDefinition { botId="bot_reina",displayName="Reina",personality=BotPersonality.RareBoss,    accuracy=0.95f, minDelay=0.8f, maxDelay=1.8f, mascotId=5, isRare=true, bonusCoins=75 },
        };

        private static int _cursor = 0;

        public static BotDefinition NextBot()
        {
            if (_bots.Count == 0) return null;
            if (_cursor >= _bots.Count) _cursor = 0;
            return _bots[_cursor++];
        }

        public static BotDefinition GetById(string botId)
        {
            if (string.IsNullOrWhiteSpace(botId)) return null;
            for (int i = 0; i < _bots.Count; i++)
                if (_bots[i].botId == botId) return _bots[i];
            return null;
        }

        // Personality phrase pools (keep them short and all-ages)
        public static string PickRoundStartLine(BotPersonality p) => p switch
        {
            BotPersonality.Friendly => Pick("You’ve got this!", "Good luck!", "Let’s do our best!"),
            BotPersonality.Chill => Pick("Hehe… ok!", "No rush~", "Let’s see…"),
            BotPersonality.Competitive => Pick("I’m warmed up.", "Let’s go.", "Try to keep up."),
            BotPersonality.Snarky => Pick("This should be fun.", "I *totally* know this.", "Easy… probably."),
            BotPersonality.RareBoss => Pick("You may begin.", "Don’t blink.", "Interesting…"),
            _ => Pick("Good luck!")
        };

        public static string PickCorrectLine(BotPersonality p) => p switch
        {
            BotPersonality.Friendly => Pick("Yay! Nice!", "We did it!", "That felt right!"),
            BotPersonality.Chill => Pick("Mm-hm.", "Nice.", "Cool."),
            BotPersonality.Competitive => Pick("Too easy.", "As expected.", "Point for me."),
            BotPersonality.Snarky => Pick("Obviously.", "I am *so* smart.", "Called it."),
            BotPersonality.RareBoss => Pick("Predictable.", "You’ll need more than that.", "Adequate."),
            _ => Pick("Nice!")
        };

        public static string PickWrongLine(BotPersonality p) => p switch
        {
            BotPersonality.Friendly => Pick("Oops—my bad!", "Aww, almost!", "Next one!"),
            BotPersonality.Chill => Pick("Whoops.", "Eh, it happens.", "Oopsie."),
            BotPersonality.Competitive => Pick("Ugh.", "That was unlucky.", "No way…"),
            BotPersonality.Snarky => Pick("I meant to do that.", "…Pretend you didn’t see that.", "Huh. Weird."),
            BotPersonality.RareBoss => Pick("A rare slip.", "…Tch.", "Impossible."),
            _ => Pick("Oops!")
        };

        public static string PickEncourageLine(BotPersonality p) => p switch
        {
            BotPersonality.Friendly => Pick("Nice one!", "Let’s gooo!", "That was awesome!"),
            BotPersonality.Chill => Pick("Good job.", "Nice.", "Solid."),
            BotPersonality.Competitive => Pick("Not bad.", "Okay, okay.", "You’re improving."),
            BotPersonality.Snarky => Pick("Wow, you got it!", "I’m impressed… maybe.", "Okay genius."),
            BotPersonality.RareBoss => Pick("Acceptable.", "Interesting.", "You may continue."),
            _ => Pick("Nice!")
        };

        public static string PickComfortLine(BotPersonality p) => p switch
        {
            BotPersonality.Friendly => Pick("Aww close! Next one!", "You’ve got this!", "No worries—keep going!"),
            BotPersonality.Chill => Pick("Happens.", "All good.", "Next."),
            BotPersonality.Competitive => Pick("Focus up.", "You’ll get the next.", "Don’t slip."),
            BotPersonality.Snarky => Pick("Yikes…", "That one was spicy.", "Oof. Unlucky."),
            BotPersonality.RareBoss => Pick("Weak.", "Predictable.", "Try harder."),
            _ => Pick("Next one!")
        };

        public static string PickTauntLine(BotPersonality p) => p switch
        {
            BotPersonality.Competitive => Pick("Nope. Focus.", "That’s a miss.", "Keep up."),
            BotPersonality.Snarky => Pick("Oof.", "That… was a choice.", "Yikes."),
            BotPersonality.RareBoss => Pick("Insufficient.", "Predictable.", "Try again."),
            BotPersonality.Chill => Pick("Aw, close.", "Unlucky.", "Next one."),
            BotPersonality.Friendly => Pick("Aww, nearly!", "You’ll get it!", "Next one for sure!"),
            _ => Pick("Oops!")
        };

        public static string PickPraiseLine(BotPersonality p) => p switch
        {
            BotPersonality.Friendly => Pick("Nice one!!", "Let’s go!", "That was awesome!"),
            BotPersonality.Chill => Pick("Nice.", "Good job.", "Clean."),
            BotPersonality.Competitive => Pick("Not bad.", "Okay.", "Respect."),
            BotPersonality.Snarky => Pick("Wow, you got it.", "Okay genius.", "Fine… nice."),
            BotPersonality.RareBoss => Pick("Acceptable.", "Interesting.", "You may continue."),
            _ => Pick("Nice!")
        };

        private static string Pick(params string[] options)
        {
            if (options == null || options.Length == 0) return "";
            return options[UnityEngine.Random.Range(0, options.Length)];
        }
    }
}