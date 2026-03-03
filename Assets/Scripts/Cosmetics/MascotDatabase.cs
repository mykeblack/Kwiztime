using UnityEngine;

namespace Kwiztime.Cosmetics
{
    [CreateAssetMenu(menuName = "Kwiztime/Mascot Database")]
    public class MascotDatabase : ScriptableObject
    {
        public Sprite[] mascots; // index = mascotId
        public Sprite Get(int id) => (id >= 0 && id < mascots.Length) ? mascots[id] : null;
    }
}