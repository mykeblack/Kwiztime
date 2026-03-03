using UnityEngine;
using UnityEngine.UI;
using Kwiztime.Cosmetics;

namespace Kwiztime.UI
{
    public class AvatarPreviewView : MonoBehaviour
    {
        [Header("Physical Layers")]
        [SerializeField] private Image bodyShapeImage;
        [SerializeField] private Image eyesImage;
        [SerializeField] private Image mouthImage;
        [SerializeField] private Image hairImage;

        [Header("Outfit Layers")]
        [SerializeField] private Image wholeOutfitImage;
        [SerializeField] private Image topImage;
        [SerializeField] private Image legwearImage;
        [SerializeField] private Image shoesImage;

        [Header("Accessories")]
        [SerializeField] private Image accessoryAImage;
        [SerializeField] private Image accessoryBImage;
        [SerializeField] private Image accessoryCImage;

        [Header("Hat")]
        [SerializeField] private Image hatImage;

        [Header("Mascot")]
        [SerializeField] private Image mascotImage;


        [Header("Databases")]
        [SerializeField] private AvatarCosmeticsDatabase cosmeticsDb;
        [SerializeField] private SkinToneDatabase skinToneDb;
        [SerializeField] private MascotDatabase mascotDb;

        public void Apply(PlayerCosmetics c)
        {
            if (cosmeticsDb == null) return;

            // Body + tint
            ApplySprite(bodyShapeImage, cosmeticsDb.GetBodyShape(c.bodyShapeId));
            if (skinToneDb != null && bodyShapeImage != null)
                bodyShapeImage.color = skinToneDb.Get(c.skinToneId);

            // Face
            ApplySprite(eyesImage, cosmeticsDb.GetEyes(c.eyesId));
            ApplySprite(mouthImage, cosmeticsDb.GetMouth(c.mouthId));
            ApplySprite(hairImage, cosmeticsDb.GetHair(c.hairId));

            // Outfits (whole overrides top+legwear)
            bool hasWhole = c.wholeOutfitId >= 0;
            if (hasWhole)
            {
                ApplySprite(wholeOutfitImage, cosmeticsDb.GetWholeOutfit(c.wholeOutfitId, c.bodyShapeId));
                SetActive(topImage, false);
                SetActive(legwearImage, false);
            }
            else
            {
                SetActive(wholeOutfitImage, false);
                ApplySprite(topImage, cosmeticsDb.GetTop(c.topId, c.bodyShapeId));
                ApplySprite(legwearImage, cosmeticsDb.GetLegwear(c.legwearId, c.bodyShapeId));
            }

            ApplySprite(shoesImage, cosmeticsDb.GetShoes(c.shoesId));

            // Accessories
            ApplySprite(accessoryAImage, cosmeticsDb.GetAccessory(c.accessoryAId));
            ApplySprite(accessoryBImage, cosmeticsDb.GetAccessory(c.accessoryBId));
            ApplySprite(accessoryCImage, cosmeticsDb.GetAccessory(c.accessoryCId));

            // Hat
            ApplySprite(hatImage, cosmeticsDb.GetHat(c.hatId));

            // Mascot
            if (mascotDb != null && mascotImage != null)
            {
                var sprite = mascotDb.Get(c.mascotId);
                mascotImage.sprite = sprite;
                mascotImage.preserveAspect = true;
                mascotImage.gameObject.SetActive(sprite != null);
            }
        }

        private void ApplySprite(Image img, Sprite sprite)
        {
            if (img == null) return;
            img.sprite = sprite;
            img.preserveAspect = true;
            img.gameObject.SetActive(sprite != null);
        }

        private void SetActive(Image img, bool active)
        {
            if (img == null) return;
            img.gameObject.SetActive(active);
        }
    }
}