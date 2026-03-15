using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kwiztime.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text footerText;

        [Header("Main Buttons")]
        [SerializeField] private Button playOnlineButton;
        [SerializeField] private Button privateRoomButton;
        [SerializeField] private Button customiseButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button characterCreatorButton; // your new button
        [SerializeField] private Button quitButton;

        [Header("Private Room Modal")]
        [SerializeField] private GameObject privateRoomModal;
        [SerializeField] private TMP_InputField roomCodeInput;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button closePrivateModalButton;
        [SerializeField] private TMP_Text privateRoomHintText;

        [Header("Options Modal")]
        [SerializeField] private GameObject optionsModal;
        [SerializeField] private Button closeOptionsModalButton;

        [Header("Scene Names")]
        [SerializeField] private string gameSceneName      = "Game";
        [SerializeField] private string customiseSceneName = "Customisation"; // FIX: unified spelling

        private void Awake()
        {
            // FIX: avatar check moved to Awake first, before any wiring, with early return
            if (PlayerPrefs.GetInt("avatar_created", 0) == 0)
            {
                SceneManager.LoadScene("CharacterCreation");
                return;
            }

            SetupText();
            CloseAllModals();
            WireButtons();

#if UNITY_WEBGL
            if (quitButton != null) quitButton.gameObject.SetActive(false);
#endif
        }

        // FIX: Start() removed entirely — avatar check now lives in Awake

        private void SetupText()
        {
            if (titleText    != null) titleText.text    = "Kwiztime";
            if (subtitleText != null) subtitleText.text = "A soft party quiz for everyone";
            if (footerText != null) footerText.text = "Bots may join if lobbies aren't full. Legend bots are rare and award bonus coins. <sprite name=\"icon_coin\">";
        }

        private void CloseAllModals()
        {
            if (privateRoomModal != null) privateRoomModal.SetActive(false);
            if (optionsModal     != null) optionsModal.SetActive(false);
        }

        private void WireButtons()
        {
            if (playOnlineButton       != null) playOnlineButton.onClick.AddListener(OnPlayOnline);
            if (privateRoomButton      != null) privateRoomButton.onClick.AddListener(OpenPrivateRoomModal);
            if (customiseButton        != null) customiseButton.onClick.AddListener(OnCustomise);
            if (optionsButton          != null) optionsButton.onClick.AddListener(OpenOptions);
            if (characterCreatorButton != null) characterCreatorButton.onClick.AddListener(OpenCharacterCreator);
            if (quitButton             != null) quitButton.onClick.AddListener(OnQuit);

            if (closePrivateModalButton != null) closePrivateModalButton.onClick.AddListener(ClosePrivateRoomModal);
            if (joinRoomButton          != null) joinRoomButton.onClick.AddListener(OnJoinPrivateRoom);
            if (createRoomButton        != null) createRoomButton.onClick.AddListener(OnCreatePrivateRoom);

            if (closeOptionsModalButton != null) closeOptionsModalButton.onClick.AddListener(CloseOptions);
        }

        private void OnPlayOnline()
        {
            Debug.Log("[MainMenu] Play Online.");
            SceneManager.LoadScene(gameSceneName);
        }

        private void OpenPrivateRoomModal()
        {
            if (privateRoomModal != null) privateRoomModal.SetActive(true);

            if (privateRoomHintText != null)
                privateRoomHintText.text = "Enter a 6-character code to join, or create a new private room.";

            if (roomCodeInput != null)
            {
                roomCodeInput.characterLimit = 6;
                roomCodeInput.SetTextWithoutNotify("");
                roomCodeInput.onValueChanged.RemoveAllListeners();
                roomCodeInput.onValueChanged.AddListener(NormalizeRoomCode);
                roomCodeInput.ActivateInputField();
            }
        }

        private void ClosePrivateRoomModal()
        {
            if (privateRoomModal != null) privateRoomModal.SetActive(false);
        }

        private void NormalizeRoomCode(string raw)
        {
            if (roomCodeInput == null) return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder(6);
            foreach (char c in (raw ?? "").ToUpperInvariant())
            {
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                    sb.Append(c);
                if (sb.Length >= 6) break;
            }

            string cleaned = sb.ToString();
            if (cleaned != raw)
            {
                roomCodeInput.SetTextWithoutNotify(cleaned);
                roomCodeInput.caretPosition = cleaned.Length;
            }
        }

        private void OnJoinPrivateRoom()
        {
            string code = roomCodeInput != null ? roomCodeInput.text.Trim().ToUpperInvariant() : "";

            if (code.Length != 6)
            {
                if (privateRoomHintText != null)
                    privateRoomHintText.text = "Room code must be 6 letters/numbers.";
                return;
            }

            // FIX: show coming soon instead of silently loading game scene
            if (privateRoomHintText != null)
                privateRoomHintText.text = "Private rooms coming soon!";

            Debug.Log($"[MainMenu] Join private room stub: {code}");
            // Hook later: resolve code -> server address via master server
        }

        private void OnCreatePrivateRoom()
        {
            // FIX: show coming soon instead of silently loading game scene
            if (privateRoomHintText != null)
                privateRoomHintText.text = "Private rooms coming soon!";

            Debug.Log("[MainMenu] Create private room stub.");
            // Hook later: create room via master server -> get code + address
        }

        private void OnCustomise()
        {
            Debug.Log("[MainMenu] Customise.");
            TryLoadOrWarn(customiseSceneName);
        }

        private void OpenOptions()
        {
            Debug.Log("[MainMenu] Options.");
            if (optionsModal != null) optionsModal.SetActive(true);
        }

        private void CloseOptions()
        {
            if (optionsModal != null) optionsModal.SetActive(false);
        }

        public void OpenCharacterCreator()
        {
            SceneManager.LoadScene("CharacterCreation");
        }

        private void OnQuit()
        {
            Debug.Log("[MainMenu] Quit.");
            Application.Quit();
        }

        private void TryLoadOrWarn(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return;

            // FIX: log actual exception instead of swallowing silently
            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[MainMenu] Could not load '{sceneName}': {e.Message}");
            }
        }

        // FIX: removed duplicate OpenCustomisation() method with conflicting scene name spelling
    }
}