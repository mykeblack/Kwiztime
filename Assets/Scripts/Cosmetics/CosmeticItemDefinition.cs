using UnityEngine;

namespace Kwiztime.Cosmetics
{
    public enum CosmeticSlot
    {
        Hair,
        Hat,
        Top,
        Legwear,
        WholeOutfit,
        Shoes,
        Accessory,
        Mascot
    }

    [CreateAssetMenu(menuName = "Kwiztime/Cosmetic Item")]
    public class CosmeticItemDefinition : ScriptableObject
    {
        public string itemId;              // unique string key e.g. "top_vest_white"
        public CosmeticSlot slot;
        public string displayName;
        public int priceCoins;             // fixed cost

        [Header("Icon (UI)")]
        public Sprite icon;                // for shop list thumbnails

        [Header("Wearable Sprites")]
        public BodyShapeVariant wearableVariant; // for Top/Legwear/WholeOutfit (optional)

        [Header("Simple Sprites")]
        public Sprite sprite;              // for Hat/Shoes/Accessory OR Mascot

        [Header("Hair Sprites (slot = Hair)")]
        public Sprite hairFull;      // normal hair
        public Sprite hairUnderHat;  // only lower portion visible (hat covers top)

        [Header("Hat Behaviour (slot = Hat)")]
        public bool hatCoversHair = true; // if true: use hairUnderHat
    }
}