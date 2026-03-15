using System;

namespace Kwiztime.Cosmetics
{
    [Serializable]
    public struct PlayerCosmetics
    {
        // -------------------------
        // Physical (fixed)
        // -------------------------
        public int bodyShapeId; // 0=Regular,1=Athletic,2=Muscly,3=SoftCurvy,4=Chunky,5=slinky
        public int skinToneId;  // 0..5 (for now)
        public int hairId;
        public int eyesId;
        public int mouthId;

        // -------------------------
        // Mascot
        // -------------------------
        public int mascotId;    // index into MascotDatabase (use -1 for none if desired)

        // -------------------------
        // Outfit slots
        // -------------------------
        public int hatId;           // -1 = none
        public int topId;           // default 0
        public int legwearId;       // default 0
        public int wholeOutfitId;   // -1 = none; overrides top+legwear when >= 0
        public int shoesId;         // -1 = none

        // -------------------------
        // Accessories (3 slots)
        // -------------------------
        public int accessoryAId;    // -1 = none
        public int accessoryBId;    // -1 = none
        public int accessoryCId;    // -1 = none

        public static PlayerCosmetics Default()
        {
            return new PlayerCosmetics
            {
                bodyShapeId = 0,
                skinToneId = 0,
                hairId = -1,  // no hair until chosen in wardrobe
                eyesId = 0,
                mouthId = 0,
                mascotId = 0,
                hatId = -1,
                topId = -1,  // disabled — whole outfit used instead
                legwearId = -1,  // disabled — whole outfit used instead
                wholeOutfitId = 0,   // base outfit at index 0
                shoesId = -1,
                accessoryAId = -1,
                accessoryBId = -1,
                accessoryCId = -1
            };
        }
    }
}