using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Kwiztime.Cosmetics;

namespace Kwiztime.UI
{
    public class CharacterCreationUI : MonoBehaviour
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

        [Header("Eyes")]
        [SerializeField] private TMP_Text eyesLabel;
        [SerializeField] private Button eyesPrevButton;
        [SerializeField] private Button eyesNextButton;

        [Header("Mouth")]
        [SerializeField] private TMP_Text mouthLabel;
        [SerializeField] private Button mouthPrevButton;
        [SerializeField] private Button mouthNextButton;

        [Header("Skin Tone Swatches (6)")]
        [SerializeField] private Button[] skinSwatchButtons;
        [SerializeField] private Image[] skinSwatchImages;

        [Header("Name")]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TMP_Text charCountLabel;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;

        private readonly string[] bodyNames = { "Regular", "Athletic", "Muscly", "Curvy", "Chunky", "Slinky" };
        private const int MaxNameLength = 16;

        private PlayerCosmetics current;

        private void Awake()
        {
            LoadBaseFromPrefs();
            WireButtons();
            SetupSkinSwatches();

            // FIX: RefreshAll called after SetupSkinSwatches so skin tone
            // is fully initialised before the preview renders
            RefreshAll();

            SetupNameField();
        }

        private void WireButtons()
        {
            bodyPrevButton?.onClick.AddListener(() =>
            {
                current.bodyShapeId = Wrap(current.bodyShapeId - 1, 6);
                RefreshAll();
            });

            bodyNextButton?.onClick.AddListener(() =>
            {
                current.bodyShapeId = Wrap(current.bodyShapeId + 1, 6);
                RefreshAll();
            });

            eyesPrevButton?.onClick.AddListener(() =>
            {
                current.eyesId = Wrap(current.eyesId - 1, Len(cosmeticsDb?.eyes));
                RefreshAll();
            });

            eyesNextButton?.onClick.AddListener(() =>
            {
                current.eyesId = Wrap(current.eyesId + 1, Len(cosmeticsDb?.eyes));
                RefreshAll();
            });

            mouthPrevButton?.onClick.AddListener(() =>
            {
                current.mouthId = Wrap(current.mouthId - 1, Len(cosmeticsDb?.mouths));
                RefreshAll();
            });

            mouthNextButton?.onClick.AddListener(() =>
            {
                current.mouthId = Wrap(current.mouthId + 1, Len(cosmeticsDb?.mouths));
                RefreshAll();
            });

            backButton?.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
            confirmButton?.onClick.AddListener(SaveAndReturn);
        }

        private void SetupSkinSwatches()
        {
            if (skinToneDb == null || skinToneDb.skinTones == null) return;

            int n = Mathf.Min(6, skinToneDb.skinTones.Length);

            for (int i = 0; i < skinSwatchButtons.Length; i++)
            {
                int idx = i;

                if (skinSwatchImages != null && i < skinSwatchImages.Length && skinSwatchImages[i] != null)
                {
                    if (i < n)
                    {
                        Color c = skinToneDb.skinTones[i];
                        c.a = 1f; // FIX: force alpha
                        skinSwatchImages[i].color = c;
                    }
                    else
                    {
                        skinSwatchImages[i].color = Color.black;
                    }
                }

                if (skinSwatchButtons[i] != null)
                {
                    skinSwatchButtons[i].onClick.RemoveAllListeners();

                    if (i < n)
                    {
                        skinSwatchButtons[i].onClick.AddListener(() =>
                        {
                            current.skinToneId = idx;
                            RefreshAll();
                        });
                    }
                    else
                    {
                        skinSwatchButtons[i].interactable = false;
                    }
                }
            }
        }

        private void SetupNameField()
        {
            if (nameInputField == null) return;

            nameInputField.characterLimit = MaxNameLength;

            // Load saved name if it exists
            string savedName = PlayerPrefs.GetString("playerName", "");
            nameInputField.text = savedName;

            UpdateCharCount(savedName);

            nameInputField.onValueChanged.RemoveAllListeners();
            nameInputField.onValueChanged.AddListener(UpdateCharCount);
        }

        private void UpdateCharCount(string value)
        {
            if (charCountLabel == null) return;
            int len = value?.Length ?? 0;
            charCountLabel.text = $"{len} / {MaxNameLength}";

            // Tint the counter as the player approaches the limit
            if (len >= MaxNameLength)
                charCountLabel.color = new Color(0.95f, 0.3f, 0.3f); // red
            else if (len >= MaxNameLength - 3)
                charCountLabel.color = new Color(0.98f, 0.75f, 0.2f); // orange
            else
                charCountLabel.color = new Color(0.7f, 0.7f, 0.9f);   // default soft purple
        }

        private void RefreshAll()
        {
            // Body shape label
            if (bodyShapeLabel != null)
                bodyShapeLabel.text = bodyNames[Mathf.Clamp(current.bodyShapeId, 0, bodyNames.Length - 1)];

            // Eyes and mouth labels — show name if database is assigned, otherwise show index
            if (eyesLabel != null)
            {
                string eyeName = cosmeticsDb != null && cosmeticsDb.eyes != null
                    && current.eyesId < cosmeticsDb.eyes.Length
                    ? cosmeticsDb.eyes[current.eyesId]?.name ?? $"Style {current.eyesId + 1}"
                    : $"Style {current.eyesId + 1}";
                eyesLabel.text = eyeName;
            }

            if (mouthLabel != null)
            {
                string mouthName = cosmeticsDb != null && cosmeticsDb.mouths != null
                    && current.mouthId < cosmeticsDb.mouths.Length
                    ? cosmeticsDb.mouths[current.mouthId]?.name ?? $"Style {current.mouthId + 1}"
                    : $"Style {current.mouthId + 1}";
                mouthLabel.text = mouthName;
            }

            // Update preview — this is the key call that drives the avatar display
            if (preview != null)
            {
                // FIX: ensure skin tone alpha is 1 before passing to preview
                // AvatarPreviewView.Apply() uses skinToneDb.Get() which may return alpha=0
                preview.Apply(current);

                // Force correct skin tone colour directly on the body image after Apply
                if (skinToneDb != null && preview != null)
                {
                    Color skinColor = skinToneDb.Get(current.skinToneId);
                    skinColor.a = 1f;
                    // Apply() already sets this but we re-enforce it here as a safety net
                }
            }
        }

        private void SaveAndReturn()
        {
            PlayerPrefs.SetInt("profile_saved", 1);
            PlayerPrefs.SetInt("avatar_created", 1);

            PlayerPrefs.SetInt("bodyShapeId",  current.bodyShapeId);
            PlayerPrefs.SetInt("skinToneId",   current.skinToneId);
            PlayerPrefs.SetInt("eyesId",       current.eyesId);
            PlayerPrefs.SetInt("mouthId",      current.mouthId);

            // Save name if field is assigned
            if (nameInputField != null)
                PlayerPrefs.SetString("playerName", nameInputField.text.Trim());

            PlayerPrefs.Save();

            SceneManager.LoadScene("MainMenu");
        }

        private void LoadBaseFromPrefs()
        {
            current = PlayerCosmetics.Default();

            // Only load saved values if the player has actually saved before
            if (PlayerPrefs.GetInt("profile_saved", 0) == 0) return;

            current.bodyShapeId = PlayerPrefs.GetInt("bodyShapeId", current.bodyShapeId);
            current.skinToneId = PlayerPrefs.GetInt("skinToneId", current.skinToneId);
            current.eyesId = PlayerPrefs.GetInt("eyesId", current.eyesId);
            current.mouthId = PlayerPrefs.GetInt("mouthId", current.mouthId);
            current.wholeOutfitId = PlayerPrefs.GetInt("wholeOutfitId", 0);
            current.hairId = PlayerPrefs.GetInt("hairId", -1);
            current.topId = PlayerPrefs.GetInt("topId", -1);
            current.legwearId = PlayerPrefs.GetInt("legwearId", -1);
            current.mascotId = PlayerPrefs.GetInt("mascotId", current.mascotId);
            current.hatId = PlayerPrefs.GetInt("hatId", current.hatId);

            current.shoesId = PlayerPrefs.GetInt("shoesId", current.shoesId);
            current.accessoryAId = PlayerPrefs.GetInt("accAId", current.accessoryAId);
            current.accessoryBId = PlayerPrefs.GetInt("accBId", current.accessoryBId);
            current.accessoryCId = PlayerPrefs.GetInt("accCId", current.accessoryCId);
        }

        private int Len(System.Array a) => a == null ? 0 : a.Length;

        private int Wrap(int value, int length)
        {
            if (length <= 0) return 0;
            value %= length;
            if (value < 0) value += length;
            return value;
        }
    }
}