using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kwiztime.UI
{
    public class ResultsRowView : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private Image background;

        [SerializeField] private Image trophyIcon;      // show only if winner
        [SerializeField] private Sprite trophySprite;

        [SerializeField] private TMP_Text youBadgeText; // shows "YOU" if local player
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text coinsText;

        [SerializeField] private Image botMascotIcon;   // show only if bot
        [SerializeField] private Sprite[] botMascotSprites; // indexed by botMascotId

        [Header("Row Styling")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private Color winnerColor = new Color(1f, 0.95f, 0.75f, 1f);
        [SerializeField] private Color youColor = new Color(0.80f, 0.92f, 1.00f, 1f);      // soft blue
        [SerializeField] private Color youWinnerColor = new Color(0.78f, 0.95f, 0.86f, 1f); // soft green-blue

        /// <summary>
        /// Sets up this row.
        /// </summary>
        public void Set(
            string playerName,
            int coins,
            bool isWinner,
            bool isYou,
            bool isBot,
            int botMascotId)
        {
            // Name + coins
            if (playerNameText != null)
                playerNameText.text = playerName ?? "Player";

            if (coinsText != null)
                coinsText.text = $"Coins: {coins}";

            // YOU badge
            if (youBadgeText != null)
                youBadgeText.gameObject.SetActive(isYou);

            // Trophy icon (winner)
            if (trophyIcon != null)
            {
                trophyIcon.sprite = trophySprite;
                trophyIcon.preserveAspect = true;
                trophyIcon.gameObject.SetActive(isWinner && trophySprite != null);
            }

            // Bot mascot icon
            if (botMascotIcon != null)
            {
                if (isBot && botMascotSprites != null && botMascotSprites.Length > 0)
                {
                    int idx = Mathf.Clamp(botMascotId, 0, botMascotSprites.Length - 1);
                    botMascotIcon.sprite = botMascotSprites[idx];
                    botMascotIcon.preserveAspect = true;
                    botMascotIcon.gameObject.SetActive(true);
                }
                else
                {
                    botMascotIcon.gameObject.SetActive(false);
                }
            }

            // Background color priority:
            // you+winner > winner > you > normal
            if (background != null)
            {
                if (isYou && isWinner) background.color = youWinnerColor;
                else if (isWinner) background.color = winnerColor;
                else if (isYou) background.color = youColor;
                else background.color = normalColor;
            }
        }

#if UNITY_EDITOR
        // Helpful warnings in editor if refs are missing
        private void OnValidate()
        {
            if (background == null)
                background = GetComponent<Image>();

            if (youBadgeText != null && youBadgeText.text != "YOU")
                youBadgeText.text = "YOU";
        }
#endif
    }
}