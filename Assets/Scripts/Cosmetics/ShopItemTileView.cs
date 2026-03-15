using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Kwiztime.UI
{
    using Kwiztime.Cosmetics;

    public class ShopItemTileView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private GameObject ownedBadge;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionLabel;

        private Action _onBuy;
        private Action _onEquip;

        public void Bind(CosmeticItemDefinition item, bool owned, Action onBuy, Action onEquip)
        {
            _onBuy = onBuy;
            _onEquip = onEquip;

            if (icon != null) { icon.sprite = item.icon != null ? item.icon : item.sprite; icon.preserveAspect = true; }
            if (nameText != null) nameText.text = item.displayName;

            if (ownedBadge != null) ownedBadge.SetActive(owned);

            if (owned)
            {
                if (priceText != null) priceText.text = "Owned";
                if (actionLabel != null) actionLabel.text = "Equip";
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => _onEquip?.Invoke());
            }
            else
            {
                if (priceText != null) priceText.text = $"{item.priceCoins} coins";
                if (actionLabel != null) actionLabel.text = "Buy";
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => _onBuy?.Invoke());
            }
        }
    }
}