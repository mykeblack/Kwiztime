using UnityEngine;

namespace Kwiztime.Cosmetics
{
    [CreateAssetMenu(menuName = "Kwiztime/Mascot Database")]
    public class MascotDatabase : ScriptableObject
    {
        public Sprite[] mascots; // index = mascotId

        // FIX: added null guard on mascots array to prevent NullReferenceException
        // if the array is never assigned in the Inspector
        public Sprite Get(int id)
        {
            if (mascots == null || mascots.Length == 0) return null;
            if (id < 0 || id >= mascots.Length) return null;
            return mascots[id];
        }
    }
}