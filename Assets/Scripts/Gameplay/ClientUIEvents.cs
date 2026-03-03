using System;

namespace Kwiztime.UI
{
    public static class ClientUIEvents
    {
        public static Action<string, string[], float> OnQuestion;
        public static Action<float> OnTimer;
        public static Action<int> OnReveal;

        public static Action<string> OnStatus;
        public static Action<int, int> OnRound;

        // If you're using the expanded results payload:
        public static Action<uint[], string[], int[], bool[], int[]> OnResultsDetailed;

        public static Action<string> OnQuestionMeta;

        // NEW
        public static Action<string, string> OnChat;

        public static Action OnResults;
        public static Action<string, string> OnChatPrivate;
    }
}