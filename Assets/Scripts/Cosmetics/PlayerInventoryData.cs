using System;
using System.Collections.Generic;

namespace Kwiztime.Cosmetics
{
    [Serializable]
    public class PlayerInventoryData
    {
        public int coins = 0;
        public List<string> owned = new(); // itemId list

        // Equipped (IDs reference catalog itemIds, not numeric sprite indices)
        public string equippedHair = ""; // empty = none
        public string equippedHat = "";
        public string equippedTop = "top_vest_white";
        public string equippedLegwear = "leg_shorts_white";
        public string equippedWhole = "";
        public string equippedShoes = "";
        public string equippedAccA = "";
        public string equippedAccB = "";
        public string equippedAccC = "";
        public string equippedMascot = "mascot_cat";

        public bool IsOwned(string itemId) => owned.Contains(itemId);
        public void AddOwned(string itemId)
        {
            if (!owned.Contains(itemId)) owned.Add(itemId);
        }
    }
}