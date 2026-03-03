using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Kwiztime.Cosmetics;

namespace Kwiztime.UI
{
    public class CustomisationUI : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private AvatarPreviewView preview;

        [Header("Databases")]
        [SerializeField] private AvatarCosmeticsDatabase cosmeticsDb;
        [SerializeField] private SkinToneDatabase skinToneDb;

        [Header("Body Shape")]
        [SerializeField] private TMP_Text bodyShapeLabel;
        [SerializeField] private Button bodyPrevButton;
        [SerializeField] private Button bodyNextButton;

        [Header("Skin Tone Swatches (6)")]
        [SerializeField] private Button[] skinSwatchButtons; // size 6
        [SerializeField] private Image[] skinSwatchImages;   // size 6

        [Header("Face")]
        [SerializeField] private TMP_Text hairLabel;
        [SerializeField] private Button hairPrevButton;
        [SerializeField] private Button hairNextButton;

        [SerializeField] private TMP_Text eyesLabel;
        [SerializeField] private Button eyesPrevButton;
        [SerializeField] private Button eyesNextButton;

        [SerializeField] private TMP_Text mouthLabel;
        [SerializeField] private Button mouthPrevButton;
        [SerializeField] private Button mouthNextButton;

        [Header("Outfits")]
        [SerializeField] private TMP_Text hatLabel;
        [SerializeField] private Button hatPrevButton;
        [SerializeField] private Button hatNextButton;

        [SerializeField] private TMP_Text topLabel;
        [SerializeField] private Button topPrevButton;
        [SerializeField] private Button topNextButton;

        [SerializeField] private TMP_Text legwearLabel;
        [SerializeField] private Button legwearPrevButton;
        [SerializeField] private Button legwearNextButton;

        [SerializeField] private TMP_Text wholeOutfitLabel;
        [SerializeField] private Button wholePrevButton;
        [SerializeField] private Button wholeNextButton;

        [SerializeField] private TMP_Text shoesLabel;
        [SerializeField] private Button shoesPrevButton;
        [SerializeField] private Button shoesNextButton;

        [Header("Accessories")]
        [SerializeField] private TMP_Text accALabel;
        [SerializeField] private Button accAPrevButton;
        [SerializeField] private Button accANextButton;

        [SerializeField] private TMP_Text accBLabel;
        [SerializeField] private Button accBPrevButton;
        [SerializeField] private Button accBNextButton;

        [SerializeField] private TMP_Text accCLabel;
        [SerializeField] private Button accCPrevButton;
        [SerializeField] private Button accCNextButton;

        [Header("Bottom Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button backButton;

        private PlayerCosmetics current;

        private readonly string[] bodyNames = { "Regular", "Athletic", "Muscly", "Curvy", "Chunky", "Slinky" };

        private void Awake()
        {
            LoadFromPrefs();
            WireButtons();
            SetupSkinSwatches();
            RefreshAll();
        }

        private void WireButtons()
        {
            // Body
            bodyPrevButton?.onClick.AddListener(() => { current.bodyShapeId = Wrap(current.bodyShapeId - 1, 6); RefreshAll(); });
            bodyNextButton?.onClick.AddListener(() => { current.bodyShapeId = Wrap(current.bodyShapeId + 1, 6); RefreshAll(); });

            // Face
            hairPrevButton?.onClick.AddListener(() => { current.hairId = Wrap(current.hairId - 1, Len(cosmeticsDb?.hairs)); RefreshAll(); });
            hairNextButton?.onClick.AddListener(() => { current.hairId = Wrap(current.hairId + 1, Len(cosmeticsDb?.hairs)); RefreshAll(); });

            eyesPrevButton?.onClick.AddListener(() => { current.eyesId = Wrap(current.eyesId - 1, Len(cosmeticsDb?.eyes)); RefreshAll(); });
            eyesNextButton?.onClick.AddListener(() => { current.eyesId = Wrap(current.eyesId + 1, Len(cosmeticsDb?.eyes)); RefreshAll(); });

            mouthPrevButton?.onClick.AddListener(() => { current.mouthId = Wrap(current.mouthId - 1, Len(cosmeticsDb?.mouths)); RefreshAll(); });
            mouthNextButton?.onClick.AddListener(() => { current.mouthId = Wrap(current.mouthId + 1, Len(cosmeticsDb?.mouths)); RefreshAll(); });

            // Outfits (these can be -1 meaning none)
            hatPrevButton?.onClick.AddListener(() => { current.hatId = WrapMinusOne(current.hatId - 1, Len(cosmeticsDb?.hats)); RefreshAll(); });
            hatNextButton?.onClick.AddListener(() => { current.hatId = WrapMinusOne(current.hatId + 1, Len(cosmeticsDb?.hats)); RefreshAll(); });

            topPrevButton?.onClick.AddListener(() => { current.topId = Wrap(current.topId - 1, Len(cosmeticsDb?.tops)); current.wholeOutfitId = -1; RefreshAll(); });
            topNextButton?.onClick.AddListener(() => { current.topId = Wrap(current.topId + 1, Len(cosmeticsDb?.tops)); current.wholeOutfitId = -1; RefreshAll(); });

            legwearPrevButton?.onClick.AddListener(() => { current.legwearId = Wrap(current.legwearId - 1, Len(cosmeticsDb?.legwear)); current.wholeOutfitId = -1; RefreshAll(); });
            legwearNextButton?.onClick.AddListener(() => { current.legwearId = Wrap(current.legwearId + 1, Len(cosmeticsDb?.legwear)); current.wholeOutfitId = -1; RefreshAll(); });

            wholePrevButton?.onClick.AddListener(() => { current.wholeOutfitId = WrapMinusOne(current.wholeOutfitId - 1, Len(cosmeticsDb?.wholeOutfits)); RefreshAll(); });
            wholeNextButton?.onClick.AddListener(() => { current.wholeOutfitId = WrapMinusOne(current.wholeOutfitId + 1, Len(cosmeticsDb?.wholeOutfits)); RefreshAll(); });

            shoesPrevButton?.onClick.AddListener(() => { current.shoesId = WrapMinusOne(current.shoesId - 1, Len(cosmeticsDb?.shoes)); RefreshAll(); });
            shoesNextButton?.onClick.AddListener(() => { current.shoesId = WrapMinusOne(current.shoesId + 1, Len(cosmeticsDb?.shoes)); RefreshAll(); });

            // Accessories
            accAPrevButton?.onClick.AddListener(() => { current.accessoryAId = WrapMinusOne(current.accessoryAId - 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });
            accANextButton?.onClick.AddListener(() => { current.accessoryAId = WrapMinusOne(current.accessoryAId + 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });

            accBPrevButton?.onClick.AddListener(() => { current.accessoryBId = WrapMinusOne(current.accessoryBId - 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });
            accBNextButton?.onClick.AddListener(() => { current.accessoryBId = WrapMinusOne(current.accessoryBId + 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });

            accCPrevButton?.onClick.AddListener(() => { current.accessoryCId = WrapMinusOne(current.accessoryCId - 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });
            accCNextButton?.onClick.AddListener(() => { current.accessoryCId = WrapMinusOne(current.accessoryCId + 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });

            // Save / Back
            saveButton?.onClick.AddListener(SaveToPlayer);
            backButton?.onClick.AddListener(BackToMenu);
        }

        private void SetupSkinSwatches()
        {
            if (skinToneDb == null || skinToneDb.skinTones == null) return;

            int n = Mathf.Min(6, skinToneDb.skinTones.Length);

            for (int i = 0; i < skinSwatchButtons.Length; i++)
            {
                int idx = i;

                if (skinSwatchImages != null && i < skinSwatchImages.Length && skinSwatchImages[i] != null)
                    skinSwatchImages[i].color = (i < n) ? skinToneDb.skinTones[i] : Color.black;

                if (skinSwatchButtons[i] != null)
                {
                    skinSwatchButtons[i].onClick.RemoveAllListeners();

                    if (i < n)
                        skinSwatchButtons[i].onClick.AddListener(() => { current.skinToneId = idx; RefreshAll(); });
                    else
                        skinSwatchButtons[i].interactable = false;
                }
            }
        }

        private void RefreshAll()
        {
            // Labels
            if (bodyShapeLabel != null)
                bodyShapeLabel.text = bodyNames[Mathf.Clamp(current.bodyShapeId, 0, bodyNames.Length - 1)];

            SetLabel(hairLabel, "Hair", current.hairId, Len(cosmeticsDb?.hairs));
            SetLabel(eyesLabel, "Eyes", current.eyesId, Len(cosmeticsDb?.eyes));
            SetLabel(mouthLabel, "Mouth", current.mouthId, Len(cosmeticsDb?.mouths));

            SetLabel(hatLabel, "Hat", current.hatId, Len(cosmeticsDb?.hats), allowNone: true);
            SetLabel(topLabel, "Top", current.topId, Len(cosmeticsDb?.tops));
            SetLabel(legwearLabel, "Legwear", current.legwearId, Len(cosmeticsDb?.legwear));
            SetLabel(wholeOutfitLabel, "Whole", current.wholeOutfitId, Len(cosmeticsDb?.wholeOutfits), allowNone: true);
            SetLabel(shoesLabel, "Shoes", current.shoesId, Len(cosmeticsDb?.shoes), allowNone: true);

            SetLabel(accALabel, "Acc A", current.accessoryAId, Len(cosmeticsDb?.accessories), allowNone: true);
            SetLabel(accBLabel, "Acc B", current.accessoryBId, Len(cosmeticsDb?.accessories), allowNone: true);
            SetLabel(accCLabel, "Acc C", current.accessoryCId, Len(cosmeticsDb?.accessories), allowNone: true);

            // Preview
            if (preview != null)
                preview.Apply(current);
        }

        private void SaveToPlayer()
        {
            // Save locally (offline)
            PlayerPrefs.SetInt("bodyShapeId", current.bodyShapeId);
            PlayerPrefs.SetInt("skinToneId", current.skinToneId);
            PlayerPrefs.SetInt("hairId", current.hairId);
            PlayerPrefs.SetInt("eyesId", current.eyesId);
            PlayerPrefs.SetInt("mouthId", current.mouthId);

            PlayerPrefs.SetInt("mascotId", current.mascotId);

            PlayerPrefs.SetInt("hatId", current.hatId);
            PlayerPrefs.SetInt("topId", current.topId);
            PlayerPrefs.SetInt("legwearId", current.legwearId);
            PlayerPrefs.SetInt("wholeOutfitId", current.wholeOutfitId);
            PlayerPrefs.SetInt("shoesId", current.shoesId);

            PlayerPrefs.SetInt("accAId", current.accessoryAId);
            PlayerPrefs.SetInt("accBId", current.accessoryBId);
            PlayerPrefs.SetInt("accCId", current.accessoryCId);

            PlayerPrefs.SetInt("profile_saved", 1);

            PlayerPrefs.Save();

        }
 

        private void BackToMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        // ---------- Helpers ----------
        private int Len(System.Array a) => a == null ? 0 : a.Length;

        private int Wrap(int value, int length)
        {
            if (length <= 0) return 0;
            value %= length;
            if (value < 0) value += length;
            return value;
        }

        private int WrapMinusOne(int value, int length)
        {
            // Allows -1 for "none", otherwise 0..length-1
            if (length <= 0) return -1;

            if (value < -1) value = length - 1;
            if (value >= length) value = -1;

            return value;
        }

        private void SetLabel(TMP_Text t, string prefix, int id, int length, bool allowNone = false)
        {
            if (t == null) return;

            if (allowNone && id < 0)
                t.text = $"{prefix}: None";
            else
                t.text = $"{prefix}: {(length <= 0 ? 0 : id)}";
        }

        private void LoadFromPrefs()
        {
            current = PlayerCosmetics.Default();

            current.bodyShapeId = PlayerPrefs.GetInt("bodyShapeId", current.bodyShapeId);
            current.skinToneId = PlayerPrefs.GetInt("skinToneId", current.skinToneId);
            current.hairId = PlayerPrefs.GetInt("hairId", current.hairId);
            current.eyesId = PlayerPrefs.GetInt("eyesId", current.eyesId);
            current.mouthId = PlayerPrefs.GetInt("mouthId", current.mouthId);

            current.mascotId = PlayerPrefs.GetInt("mascotId", current.mascotId);

            current.hatId = PlayerPrefs.GetInt("hatId", current.hatId);
            current.topId = PlayerPrefs.GetInt("topId", current.topId);
            current.legwearId = PlayerPrefs.GetInt("legwearId", current.legwearId);
            current.wholeOutfitId = PlayerPrefs.GetInt("wholeOutfitId", current.wholeOutfitId);
            current.shoesId = PlayerPrefs.GetInt("shoesId", current.shoesId);

            current.accessoryAId = PlayerPrefs.GetInt("accAId", current.accessoryAId);
            current.accessoryBId = PlayerPrefs.GetInt("accBId", current.accessoryBId);
            current.accessoryCId = PlayerPrefs.GetInt("accCId", current.accessoryCId);

            current.mascotId = PlayerPrefs.GetInt("mascotId", current.mascotId);
        }
    }
}