using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Kwiztime.Cosmetics;

namespace Kwiztime.UI
{
    public class AvatarSlotView : MonoBehaviour
    {
        // -------------------------
        // Refs
        // -------------------------
        [Header("Mascot")]
        [SerializeField] private Image mascotImage;

        // -------------------------
        // Physical Layers (fixed)
        // -------------------------
        [Header("Physical Layers")]
        [SerializeField] private Image bodyShapeImage;
        [SerializeField] private Image eyesImage;
        [SerializeField] private Image mouthImage;
        [SerializeField] private Image hairImage;

        // -------------------------
        // Outfit Layers
        // -------------------------
        [Header("Outfit Layers")]
        [SerializeField] private Image wholeOutfitImage; // overrides top + legwear
        [SerializeField] private Image topImage;
        [SerializeField] private Image legwearImage;
        [SerializeField] private Image shoesImage;

        // -------------------------
        // Accessories
        // -------------------------
        [Header("Accessories")]
        [SerializeField] private Image accessoryAImage;
        [SerializeField] private Image accessoryBImage;
        [SerializeField] private Image accessoryCImage;

        // -------------------------
        // Hat
        // -------------------------
        [Header("Hat")]
        [SerializeField] private Image hatImage;

        // -------------------------
        // Text
        // -------------------------
        [Header("Text")]
        [SerializeField] private TMP_Text nameText;

        // -------------------------
        // Speech Bubble
        // -------------------------
        [Header("Speech Bubble")]
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private TMP_Text bubbleText;
        [SerializeField] private Image bubbleResultIcon;

        [Header("Bubble Icons")]
        [SerializeField] private Sprite iconCorrect;
        [SerializeField] private Sprite iconWrong;

        // -------------------------
        // Databases
        // -------------------------
        [Header("Databases")]
        [SerializeField] private AvatarCosmeticsDatabase cosmeticsDb;
        [SerializeField] private MascotDatabase mascotDb;
        [SerializeField] private SkinToneDatabase skinToneDb;

        // -------------------------
        // State
        // -------------------------
        private Coroutine bubbleRoutine;
        public uint NetId { get; private set; }

        // =====================================================================
        // Bind Player Data
        // =====================================================================
        public void Bind(Kwiztime.KwizPlayer p)
        {
            if (p == null) return;

            NetId = p.netId;

            // ---- Name ----
            if (nameText != null)
                nameText.text = string.IsNullOrWhiteSpace(p.displayName)
                    ? $"Player {p.netId}"
                    : p.displayName;

            // ---- Mascot ----
            int mascotId = p.isBot ? p.botMascotId : p.mascotId;
            ApplySprite(mascotImage, mascotDb != null ? mascotDb.Get(mascotId) : null);

            if (cosmeticsDb == null) return;

            int bodyShapeId = p.bodyShapeId;

            // ---- Body Shape + Skin Tone ----
            ApplySprite(bodyShapeImage, cosmeticsDb.GetBodyShape(bodyShapeId));

            if (skinToneDb != null && bodyShapeImage != null)
                bodyShapeImage.color = skinToneDb.Get(p.skinToneId);

            // ---- Face ----
            ApplySprite(eyesImage, cosmeticsDb.GetEyes(p.eyesId));
            ApplySprite(mouthImage, cosmeticsDb.GetMouth(p.mouthId));
            
            // Hair: choose full vs under-hat depending on hat
            Sprite hairSprite = null;
            if (p.hairId >= 0)
            {
                bool covers = cosmeticsDb.HatCoversHair(p.hatId);
                hairSprite = covers
                    ? cosmeticsDb.GetHairUnderHat(p.hairId)
                    : cosmeticsDb.GetHairFull(p.hairId);

                // fallback if underHat missing
                if (covers && hairSprite == null)
                    hairSprite = cosmeticsDb.GetHairFull(p.hairId);
            }

            ApplySprite(hairImage, hairSprite);

            // ---- Outfit ----
            bool hasWholeOutfit = p.wholeOutfitId >= 0;

            if (hasWholeOutfit)
            {
                ApplySprite(
                    wholeOutfitImage,
                    cosmeticsDb.GetWholeOutfit(p.wholeOutfitId, bodyShapeId)
                );

                SetActive(topImage, false);
                SetActive(legwearImage, false);
            }
            else
            {
                SetActive(wholeOutfitImage, false);

                ApplySprite(
                    topImage,
                    cosmeticsDb.GetTop(p.topId, bodyShapeId)
                );

                ApplySprite(
                    legwearImage,
                    cosmeticsDb.GetLegwear(p.legwearId, bodyShapeId)
                );
            }

            // ---- Shoes ----
            ApplySprite(shoesImage, cosmeticsDb.GetShoes(p.shoesId));

            // ---- Accessories ----
            ApplySprite(accessoryAImage, cosmeticsDb.GetAccessory(p.accessoryAId));
            ApplySprite(accessoryBImage, cosmeticsDb.GetAccessory(p.accessoryBId));
            ApplySprite(accessoryCImage, cosmeticsDb.GetAccessory(p.accessoryCId));

            // ---- Hat ----
            ApplySprite(hatImage, cosmeticsDb.GetHat(p.hatId));
        }

        // =====================================================================
        // Speech Bubbles
        // =====================================================================
        public void HideBubble()
        {
            if (bubbleRoutine != null)
                StopCoroutine(bubbleRoutine);

            bubbleRoutine = null;

            if (bubbleRoot != null)
                bubbleRoot.SetActive(false);
        }

        public void ShowAnswerBubble(string answerLabel, bool correct)
        {
            if (bubbleRoot == null || bubbleText == null || bubbleResultIcon == null)
                return;

            bubbleText.text = answerLabel;

            bubbleResultIcon.sprite = correct ? iconCorrect : iconWrong;
            bubbleResultIcon.preserveAspect = true;
            bubbleResultIcon.gameObject.SetActive(bubbleResultIcon.sprite != null);

            bubbleRoot.SetActive(true);

            RestartBubbleTimer();
        }

        public void ShowChatBubble(string message)
        {
            if (bubbleRoot == null || bubbleText == null)
                return;

            bubbleText.text = message;

            if (bubbleResultIcon != null)
                bubbleResultIcon.gameObject.SetActive(false);

            bubbleRoot.SetActive(true);

            RestartBubbleTimer();
        }

        private void RestartBubbleTimer()
        {
            if (bubbleRoutine != null)
                StopCoroutine(bubbleRoutine);

            bubbleRoutine = StartCoroutine(AutoHideBubble());
        }

        private IEnumerator AutoHideBubble()
        {
            yield return new WaitForSeconds(2.5f);

            if (bubbleRoot != null)
                bubbleRoot.SetActive(false);

            bubbleRoutine = null;
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        private void ApplySprite(Image img, Sprite sprite)
        {
            if (img == null) return;

            img.sprite = sprite;
            img.preserveAspect = true;
            img.gameObject.SetActive(sprite != null);
        }

        private void SetActive(Image img, bool active)
        {
            if (img == null) return;
            img.gameObject.SetActive(active);
        }
    }
}