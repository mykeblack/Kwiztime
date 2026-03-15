using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kwiztime.UI
{
    using Kwiztime.Cosmetics;

    public class WardrobeShopUI : MonoBehaviour
    {
        [Header("Catalog")]
        [SerializeField] private CosmeticCatalog catalog;

        [Header("Preview")]
        [SerializeField] private AvatarPreviewView preview;

        [Header("Coins UI")]
        [SerializeField] private TMP_Text coinsText;

        [Header("Category Buttons")]
        [SerializeField] private Button hairBtn;
        [SerializeField] private Button hatsBtn;
        [SerializeField] private Button topsBtn;
        [SerializeField] private Button legwearBtn;
        [SerializeField] private Button wholeBtn;
        [SerializeField] private Button shoesBtn;
        [SerializeField] private Button accBtn;
        [SerializeField] private Button mascotsBtn;

        [Header("Item Grid")]
        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemTilePrefab;

        private CosmeticSlot currentSlot = CosmeticSlot.Top;
        private PlayerInventoryData inv;

        // FIX: lockedBase is now a separate stored value, never mutated
        // ApplyPreview uses a local copy each time
        private PlayerCosmetics lockedBase;

        private void Awake()
        {
            if (PlayerPrefs.GetInt("avatar_created", 0) != 1)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterCreation");
                return;
            }

            inv = InventoryService.Load();
            LoadLockedBaseCosmetics();

            WireCategoryButtons();
            Refresh();
        }

        private void WireCategoryButtons()
        {
            hairBtn?.onClick.AddListener(()     => { currentSlot = CosmeticSlot.Hair;        Refresh(); });
            hatsBtn?.onClick.AddListener(()     => { currentSlot = CosmeticSlot.Hat;         Refresh(); });
            topsBtn?.onClick.AddListener(()     => { currentSlot = CosmeticSlot.Top;         Refresh(); });
            legwearBtn?.onClick.AddListener(()  => { currentSlot = CosmeticSlot.Legwear;     Refresh(); });
            wholeBtn?.onClick.AddListener(()    => { currentSlot = CosmeticSlot.WholeOutfit; Refresh(); });
            shoesBtn?.onClick.AddListener(()    => { currentSlot = CosmeticSlot.Shoes;       Refresh(); });
            accBtn?.onClick.AddListener(()      => { currentSlot = CosmeticSlot.Accessory;   Refresh(); });
            mascotsBtn?.onClick.AddListener(()  => { currentSlot = CosmeticSlot.Mascot;      Refresh(); });
        }

        private void LoadLockedBaseCosmetics()
        {
            lockedBase = PlayerCosmetics.Default();
            lockedBase.bodyShapeId = PlayerPrefs.GetInt("bodyShapeId", 0);
            lockedBase.skinToneId  = PlayerPrefs.GetInt("skinToneId",  0);
            lockedBase.eyesId      = PlayerPrefs.GetInt("eyesId",      0);
            lockedBase.mouthId     = PlayerPrefs.GetInt("mouthId",     0);
            lockedBase.hairId      = -1;
            lockedBase.mascotId    = 0;
        }

        private void Refresh()
        {
            if (coinsText != null) coinsText.text = $"Coins: {inv.coins}";

            for (int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            var items = catalog.items.Where(i => i != null && i.slot == currentSlot).ToArray();

            foreach (var item in items)
            {
                var go   = Instantiate(itemTilePrefab, content);
                var tile = go.GetComponent<ShopItemTileView>();
                tile.Bind(
                    item,
                    inv.IsOwned(item.itemId),
                    onBuy:   () => TryBuy(item),
                    onEquip: () => Equip(item)
                );
            }

            ApplyPreview();
        }

        private void TryBuy(CosmeticItemDefinition item)
        {
            if (inv.IsOwned(item.itemId))
            {
                Equip(item);
                return;
            }

            if (inv.coins < item.priceCoins)
            {
                // TODO: show toast "Not enough coins"
                return;
            }

            inv.coins -= item.priceCoins;
            inv.AddOwned(item.itemId);

            Equip(item); // auto-equip after purchase

            InventoryService.Save(inv);
            Refresh();
        }

        private void Equip(CosmeticItemDefinition item)
        {
            if (!inv.IsOwned(item.itemId)) return;

            switch (item.slot)
            {
                case CosmeticSlot.Hair:        inv.equippedHair    = item.itemId; break;
                case CosmeticSlot.Hat:         inv.equippedHat     = item.itemId; break;
                case CosmeticSlot.Top:         inv.equippedTop     = item.itemId; inv.equippedWhole = ""; break;
                case CosmeticSlot.Legwear:     inv.equippedLegwear = item.itemId; inv.equippedWhole = ""; break;
                case CosmeticSlot.WholeOutfit: inv.equippedWhole   = item.itemId; break;
                case CosmeticSlot.Shoes:       inv.equippedShoes   = item.itemId; break;
                case CosmeticSlot.Mascot:      inv.equippedMascot  = item.itemId; break;
                case CosmeticSlot.Accessory:
                    if (string.IsNullOrEmpty(inv.equippedAccA))      inv.equippedAccA = item.itemId;
                    else if (string.IsNullOrEmpty(inv.equippedAccB)) inv.equippedAccB = item.itemId;
                    else                                             inv.equippedAccC = item.itemId;
                    break;
            }

            InventoryService.Save(inv);
            ApplyPreview();
        }

        private void ApplyPreview()
        {
            // FIX: copy lockedBase into a local variable so lockedBase is never mutated
            // (previously 'var c = lockedBase' was a reference copy for classes, causing drift)
            var c = new PlayerCosmetics
            {
                bodyShapeId  = lockedBase.bodyShapeId,
                skinToneId   = lockedBase.skinToneId,
                eyesId       = lockedBase.eyesId,
                mouthId      = lockedBase.mouthId,
                hairId       = lockedBase.hairId,
                mascotId     = lockedBase.mascotId,
                hatId        = -1,
                topId        = 0,
                legwearId    = 0,
                wholeOutfitId = -1,
                shoesId      = -1,
                accessoryAId = -1,
                accessoryBId = -1,
                accessoryCId = -1,
            };

            c.topId         = FindIndex(CosmeticSlot.Top,         inv.equippedTop,     fallback: 0);
            c.legwearId     = FindIndex(CosmeticSlot.Legwear,     inv.equippedLegwear, fallback: 0);
            c.wholeOutfitId = FindIndexOrNone(CosmeticSlot.WholeOutfit, inv.equippedWhole);
            c.hatId         = FindIndexOrNone(CosmeticSlot.Hat,         inv.equippedHat);
            c.shoesId       = FindIndexOrNone(CosmeticSlot.Shoes,       inv.equippedShoes);
            c.accessoryAId  = FindIndexOrNone(CosmeticSlot.Accessory,   inv.equippedAccA);
            c.accessoryBId  = FindIndexOrNone(CosmeticSlot.Accessory,   inv.equippedAccB);
            c.accessoryCId  = FindIndexOrNone(CosmeticSlot.Accessory,   inv.equippedAccC);
            c.mascotId      = FindIndex(CosmeticSlot.Mascot, inv.equippedMascot, fallback: 0);

            // Resolve hair sprite
            var hairItem = FindItem(CosmeticSlot.Hair, inv.equippedHair);
            var hatItem  = FindItem(CosmeticSlot.Hat,  inv.equippedHat);
            bool hatCoversHair = hatItem != null && hatItem.hatCoversHair;

            Sprite chosenHair = null;
            if (hairItem != null)
            {
                chosenHair = hatCoversHair ? hairItem.hairUnderHat : hairItem.hairFull;
                if (hatCoversHair && chosenHair == null)
                    chosenHair = hairItem.hairFull; // fallback
            }

            // FIX: Apply() called first, then SetHairSprite() overrides hair after
            // (previously SetHairSprite was called first and Apply() overwrote it)
            preview?.Apply(c);
            preview?.SetHairSprite(chosenHair);
        }

        private int FindIndex(CosmeticSlot slot, string itemId, int fallback)
        {
            if (string.IsNullOrEmpty(itemId)) return fallback;
            int idx = IndexInSlot(slot, itemId);
            return idx >= 0 ? idx : fallback;
        }

        private int FindIndexOrNone(CosmeticSlot slot, string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return -1;
            int idx = IndexInSlot(slot, itemId);
            return idx >= 0 ? idx : -1;
        }

        private int IndexInSlot(CosmeticSlot slot, string itemId)
        {
            int index = 0;
            foreach (var it in catalog.items)
            {
                if (it == null || it.slot != slot) continue;
                if (it.itemId == itemId) return index;
                index++;
            }
            return -1;
        }

        private CosmeticItemDefinition FindItem(CosmeticSlot slot, string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;
            foreach (var it in catalog.items)
                if (it != null && it.slot == slot && it.itemId == itemId)
                    return it;
            return null;
        }
    }
}