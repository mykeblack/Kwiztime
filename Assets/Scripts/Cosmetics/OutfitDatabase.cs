using UnityEngine;

namespace Kwiztime.Cosmetics
{
    [CreateAssetMenu(menuName = "Kwiztime/Outfit Database")]
    public class OutfitDatabase : ScriptableObject
    {
        public Sprite defaultBody;
        public Sprite[] vests;       // index = outfitVestId
        public Sprite[] shorts;      // index = outfitShortsId
        public Sprite[] hats;        // index = outfitHatId
        public Sprite[] accessories; // index = outfitAccessoryId

        public Sprite GetVest(int id) => (id >= 0 && id < vests.Length) ? vests[id] : null;
        public Sprite GetShorts(int id) => (id >= 0 && id < shorts.Length) ? shorts[id] : null;
        public Sprite GetHat(int id) => (id >= 0 && id < hats.Length) ? hats[id] : null;
        public Sprite GetAccessory(int id) => (id >= 0 && id < accessories.Length) ? accessories[id] : null;
    }
}