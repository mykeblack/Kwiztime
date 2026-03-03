using TMPro;

namespace Kwiztime.UI
{
    /// <summary>
    /// Runtime glyph detection for TMP fonts (including fallbacks).
    /// Call EnsureInitialized(anyTMPText) once before using symbols.
    /// </summary>
    public static class SymbolResolver
    {
        private static bool _initialized;

        // Resolved strings (may be emoji/symbols or safe ASCII fallbacks)
        public static string Correct { get; private set; } = "OK";
        public static string Wrong   { get; private set; } = "NO";
        public static string Winner  { get; private set; } = "#1";
        public static string Selected { get; private set; } = ">";

        /// <summary>
        /// Call once (e.g., in QuestionUI.Awake) passing any TMP_Text that uses your normal UI font.
        /// </summary>
        public static void EnsureInitialized(TMP_Text anyText)
        {
            if (_initialized) return;
            if (anyText == null || anyText.font == null)
            {
                // Stay on ASCII fallbacks
                _initialized = true;
                return;
            }

            var font = anyText.font;

            // Prefer pretty symbols if the font (or its fallbacks) can render them
            // NOTE: WebGL emoji support varies; these are symbols, not color emoji.
            Correct  = Pick(font, "✔", "OK");   // U+2714
            Wrong    = Pick(font, "✖", "NO");   // U+2716
            Winner   = Pick(font, "★", "#1");   // U+2605 (if missing, use #1)
            Selected = Pick(font, "▶", ">");    // U+25B6 (if missing, use >)

            _initialized = true;
        }

        private static string Pick(TMP_FontAsset font, string preferred, string fallback)
        {
            if (string.IsNullOrEmpty(preferred)) return fallback;

            // If preferred is multiple chars (e.g., "★★"), ensure all chars exist
            foreach (char c in preferred)
            {
                if (!Has(font, c))
                    return fallback;
            }

            return preferred;
        }

        private static bool Has(TMP_FontAsset font, char c)
        {
            // searchFallbacks + recursive fallbacks = true
            return font.HasCharacter(c, true, true);
        }
    }
}