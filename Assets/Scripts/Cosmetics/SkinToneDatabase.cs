using UnityEngine;

namespace Kwiztime.Cosmetics
{
    [CreateAssetMenu(menuName = "Kwiztime/Skin Tone Database")]
    public class SkinToneDatabase : ScriptableObject
    {
        public Color[] skinTones;

        public Color Get(int id)
        {
            if (skinTones == null || skinTones.Length == 0) return Color.white;
            id = Mathf.Clamp(id, 0, skinTones.Length - 1);
            return skinTones[id];
        }
    }
}