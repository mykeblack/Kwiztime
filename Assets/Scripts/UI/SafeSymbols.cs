namespace Kwiztime.UI
{
    public static class SafeSymbols
    {
        // Answer feedback
        public const string Correct = "✔";   // U+2714
        public const string Wrong   = "✖";   // U+2716

        // Winner / ranking
        public const string Winner  = "★";   // U+2605 (fallback to "#1" if needed)

        // UI helpers
        public const string Selected = ">";
        public const string Divider  = "-";
    }
}