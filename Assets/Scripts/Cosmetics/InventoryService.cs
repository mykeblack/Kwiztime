using UnityEngine;

namespace Kwiztime.Cosmetics
{
    public static class InventoryService
    {
        private const string KEY = "inventory_json";

        public static PlayerInventoryData Load()
        {
            if (!PlayerPrefs.HasKey(KEY))
            {
                // First time inventory (after character creation)
                var data = new PlayerInventoryData
                {
                    coins = 0
                };

                // Starter owned items
                data.AddOwned("top_vest_white");
                data.AddOwned("leg_shorts_white");
                data.AddOwned("mascot_cat");

                Save(data);
                return data;
            }

            var json = PlayerPrefs.GetString(KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                var data = new PlayerInventoryData();
                Save(data);
                return data;
            }

            return JsonUtility.FromJson<PlayerInventoryData>(json);
        }

        public static void Save(PlayerInventoryData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(KEY, json);
            PlayerPrefs.Save();
        }
    }
}