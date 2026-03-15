using UnityEngine;

namespace Kwiztime.Cosmetics
{
    [CreateAssetMenu(menuName = "Kwiztime/Cosmetic Catalog")]
    public class CosmeticCatalog : ScriptableObject
    {
        public CosmeticItemDefinition[] items;
    }
}