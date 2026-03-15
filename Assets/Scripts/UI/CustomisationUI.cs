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
        [SerializeField] private Button[] skinSwatchButtons;
        [SerializeField] private Image[] skinSwatchImages;

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
            bodyPrevButton?.onClick.AddListener(() => { current.bodyShapeId = Wrap(current.bodyShapeId - 1, 6); RefreshAll(); });
            bodyNextButton?.onClick.AddListener(() => { current.bodyShapeId = Wrap(current.bodyShapeId + 1, 6); RefreshAll(); });

            hairPrevButton?.onClick.AddListener(() => { current.hairId = Wrap(current.hairId - 1, Len(cosmeticsDb?.hairs)); RefreshAll(); });
            hairNextButton?.onClick.AddListener(() => { current.hairId = Wrap(current.hairId + 1, Len(cosmeticsDb?.hairs)); RefreshAll(); });

            eyesPrevButton?.onClick.AddListener(() => { current.eyesId = Wrap(current.eyesId - 1, Len(cosmeticsDb?.eyes)); RefreshAll(); });
            eyesNextButton?.onClick.AddListener(() => { current.eyesId = Wrap(current.eyesId + 1, Len(cosmeticsDb?.eyes)); RefreshAll(); });

            mouthPrevButton?.onClick.AddListener(() => { current.mouthId = Wrap(current.mouthId - 1, Len(cosmeticsDb?.mouths)); RefreshAll(); });
            mouthNextButton?.onClick.AddListener(() => { current.mouthId = Wrap(current.mouthId + 1, Len(cosmeticsDb?.mouths)); RefreshAll(); });

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

            accAPrevButton?.onClick.AddListener(() => { current.accessoryAId = WrapMinusOne(current.accessoryAId - 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });
            accANextButton?.onClick.AddListener(() => { current.accessoryAId = WrapMinusOne(current.accessoryAId + 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });

            accBPrevButton?.onClick.AddListener(() => { current.accessoryBId = WrapMinusOne(current.accessoryBId - 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });
            accBNextButton?.onClick.AddListener(() => { current.accessoryBId = WrapMinusOne(current.accessoryBId + 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });

            accCPrevButton?.onClick.AddListener(() => { current.accessoryCId = WrapMinusOne(current.accessoryCId - 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });
            accCNextButton?.onClick.AddListener(() => { current.accessoryCId = WrapMinusOne(current.accessoryCId + 1, Len(cosmeticsDb?.accessories)); RefreshAll(); });

            saveButton?.onClick.AddListener(SaveToPlayer);
            backButton?.onClick.AddListener(BackToMenu);
        }

        private void SetupSkinSwatches()
        {
            if (skinToneDb == null || skinToneDb.skinTones == null || skinToneDb.skinTones.Length == 0)
            {
                Debug.LogWarning("[CustomisationUI] SkinToneDatabase missing or empty.");
                return;
            }

            if (skinSwatchButtons == null || skinSwatchButtons.Length == 0)
            {
                Debug.LogWarning("[CustomisationUI] skinSwatchButtons not assigned.");
                return;
            }

            if (skinSwatchImages == null || skinSwatchImages.Length != skinSwatchButtons.Length)
            {
                Debug.LogWarning($"[CustomisationUI] skinSwatchImages length ({skinSwatchImages?.Length ?? 0}) " +
                                 $"does not match skinSwatchButtons ({skinSwatchButtons.Length}).");
            }

            int count = Mathf.Min(skinSwatchButtons.Length, skinToneDb.skinTones.Length);

            for (int i = 0; i < skinSwatchButtons.Length; i++)
            {
                if (skinSwatchButtons[i] == null) continue;

                bool active = i < count;
                skinSwatchButtons[i].gameObject.SetActive(active);
                if (!active) continue;

                int index = i;

                if (skinSwatchImages != null && index < skinSwatchImages.Length && skinSwatchImages[index] != null)
                {
                    var col = skinToneDb.skinTones[index];
                    col.a = 1f;
                    skinSwatchImages[index].color = col;
                }

                skinSwatchButtons[index].onClick.RemoveAllListeners();
                skinSwatchButtons[index].onClick.AddListener(() =>
                {
                    current.skinToneId = index;
                    RefreshAll();
                });
            }

            Debug.Log($"[CustomisationUI] Skin swatches wired: {count}/{skinSwatchButtons.Length}");
        }

        private void RefreshAll()
        {
            if (bodyShapeLabel != null)
                bodyShapeLabel.text = bodyNames[Mathf.Clamp(current.bodyShapeId, 0, bodyNames.Length - 1)];

            SetLabel(hairLabel,        "Hair",    current.hairId,        Len(cosmeticsDb?.hairs));
            SetLabel(eyesLabel,        "Eyes",    current.eyesId,        Len(cosmeticsDb?.eyes));
            SetLabel(mouthLabel,       "Mouth",   current.mouthId,       Len(cosmeticsDb?.mouths));
            SetLabel(hatLabel,         "Hat",     current.hatId,         Len(cosmeticsDb?.hats),        allowNone: true);
            SetLabel(topLabel,         "Top",     current.topId,         Len(cosmeticsDb?.tops));
            SetLabel(legwearLabel,     "Legwear", current.legwearId,     Len(cosmeticsDb?.legwear));
            SetLabel(wholeOutfitLabel, "Whole",   current.wholeOutfitId, Len(cosmeticsDb?.wholeOutfits), allowNone: true);
            SetLabel(shoesLabel,       "Shoes",   current.shoesId,       Len(cosmeticsDb?.shoes),        allowNone: true);
            SetLabel(accALabel,        "Acc A",   current.accessoryAId,  Len(cosmeticsDb?.accessories),  allowNone: true);
            SetLabel(accBLabel,        "Acc B",   current.accessoryBId,  Len(cosmeticsDb?.accessories),  allowNone: true);
            SetLabel(accCLabel,        "Acc C",   current.accessoryCId,  Len(cosmeticsDb?.accessories),  allowNone: true);

            if (preview != null)
                preview.Apply(current);
        }

        private void SaveToPlayer()
        {
            PlayerPrefs.SetInt("bodyShapeId",    current.bodyShapeId);
            PlayerPrefs.SetInt("skinToneId",     current.skinToneId);
            PlayerPrefs.SetInt("hairId",         current.hairId);
            PlayerPrefs.SetInt("eyesId",         current.eyesId);
            PlayerPrefs.SetInt("mouthId",        current.mouthId);
            PlayerPrefs.SetInt("mascotId",       current.mascotId); // FIX: was missing from SaveToPlayer

            PlayerPrefs.SetInt("hatId",          current.hatId);
            PlayerPrefs.SetInt("topId",          current.topId);
            PlayerPrefs.SetInt("legwearId",      current.legwearId);
            PlayerPrefs.SetInt("wholeOutfitId",  current.wholeOutfitId);
            PlayerPrefs.SetInt("shoesId",        current.shoesId);

            PlayerPrefs.SetInt("accAId",         current.accessoryAId);
            PlayerPrefs.SetInt("accBId",         current.accessoryBId);
            PlayerPrefs.SetInt("accCId",         current.accessoryCId);

            PlayerPrefs.SetInt("profile_saved",  1);

            PlayerPrefs.Save();

            Debug.Log("[CustomisationUI] Cosmetics saved.");
        }

        private void BackToMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

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

            current.bodyShapeId   = PlayerPrefs.GetInt("bodyShapeId",   current.bodyShapeId);
            current.skinToneId    = PlayerPrefs.GetInt("skinToneId",    current.skinToneId);
            current.hairId        = PlayerPrefs.GetInt("hairId",        current.hairId);
            current.eyesId        = PlayerPrefs.GetInt("eyesId",        current.eyesId);
            current.mouthId       = PlayerPrefs.GetInt("mouthId",       current.mouthId);
            current.mascotId      = PlayerPrefs.GetInt("mascotId",      current.mascotId); // FIX: removed duplicate load line

            current.hatId         = PlayerPrefs.GetInt("hatId",         current.hatId);
            current.topId         = PlayerPrefs.GetInt("topId",         current.topId);
            current.legwearId     = PlayerPrefs.GetInt("legwearId",     current.legwearId);
            current.wholeOutfitId = PlayerPrefs.GetInt("wholeOutfitId", current.wholeOutfitId);
            current.shoesId       = PlayerPrefs.GetInt("shoesId",       current.shoesId);

            current.accessoryAId  = PlayerPrefs.GetInt("accAId",        current.accessoryAId);
            current.accessoryBId  = PlayerPrefs.GetInt("accBId",        current.accessoryBId);
            current.accessoryCId  = PlayerPrefs.GetInt("accCId",        current.accessoryCId);
        }
    }
}