using UnityEngine;

namespace Kwiztime.Cosmetics
{
    [System.Serializable]
    public class BodyShapeVariant
    {
        public Sprite regular;
        public Sprite athletic;
        public Sprite muscly;
        public Sprite curvy;
        public Sprite chunky;

        public Sprite Get(int bodyShapeId)
        {
            return bodyShapeId switch
            {
                1 => athletic,
                2 => muscly,
                3 => curvy,
                4 => chunky,
                _ => regular,
            };
        }
    }

    [CreateAssetMenu(menuName = "Kwiztime/Avatar Cosmetics Database")]
    public class AvatarCosmeticsDatabase : ScriptableObject
    {
        [Header("Body")]
        public Sprite[] bodyShapes; // index = bodyShapeId (0–4)

        [Header("Face")]
        public Sprite[] eyes;
        public Sprite[] mouths;
        public Sprite[] hairs;

        [Header("Outfits (per body shape)")]
        public BodyShapeVariant[] tops;
        public BodyShapeVariant[] legwear;
        public BodyShapeVariant[] wholeOutfits;

        [Header("Other")]
        public Sprite[] hats;
        public Sprite[] shoes;
        public Sprite[] accessories;

        // ---------- Body ----------
        public Sprite GetBodyShape(int id) => Get(bodyShapes, id);

        // ---------- Face ----------
        public Sprite GetEyes(int id) => Get(eyes, id);
        public Sprite GetMouth(int id) => Get(mouths, id);
        public Sprite GetHair(int id) => Get(hairs, id);

        // ---------- Outfits ----------
        public Sprite GetTop(int id, int bodyShapeId)
            => GetVariant(tops, id, bodyShapeId);

        public Sprite GetLegwear(int id, int bodyShapeId)
            => GetVariant(legwear, id, bodyShapeId);

        public Sprite GetWholeOutfit(int id, int bodyShapeId)
            => GetVariant(wholeOutfits, id, bodyShapeId);

        // ---------- Other ----------
        public Sprite GetHat(int id) => Get(hats, id);
        public Sprite GetShoes(int id) => Get(shoes, id);
        public Sprite GetAccessory(int id) => Get(accessories, id);

        // ---------- Helpers ----------
        private Sprite Get(Sprite[] arr, int id)
        {
            if (arr == null || arr.Length == 0) return null;
            if (id < 0 || id >= arr.Length) return null;
            return arr[id];
        }

        private Sprite GetVariant(BodyShapeVariant[] arr, int id, int bodyShapeId)
        {
            if (arr == null || id < 0 || id >= arr.Length) return null;
            return arr[id]?.Get(bodyShapeId);
        }
    }
}