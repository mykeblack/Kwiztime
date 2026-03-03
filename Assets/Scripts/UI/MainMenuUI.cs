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
        [SerializeField] private Button shopButton;
        [SerializeField] private Button optionsButton;
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
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string customiseSceneName = "Customise";
        [SerializeField] private string shopSceneName = "Shop";

        private void Awake()
        {
            // Set default copy (safe if texts are not assigned)
            if (titleText != null) titleText.text = "Kwiztime";
            if (subtitleText != null) subtitleText.text = "A soft party quiz for everyone";
            if (footerText != null) footerText.text = "Bots may join if lobbies aren’t full. Legend bots are rare and award bonus coins. ⭐";

            // Ensure modals are closed on load
            if (privateRoomModal != null) privateRoomModal.SetActive(false);
            if (optionsModal != null) optionsModal.SetActive(false);

            // Main buttons
            if (playOnlineButton != null) playOnlineButton.onClick.AddListener(OnPlayOnline);
            if (privateRoomButton != null) privateRoomButton.onClick.AddListener(OpenPrivateRoomModal);
            if (customiseButton != null) customiseButton.onClick.AddListener(OnCustomise);
            if (shopButton != null) shopButton.onClick.AddListener(OnShop);
            if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

            // Private modal buttons
            if (closePrivateModalButton != null) closePrivateModalButton.onClick.AddListener(ClosePrivateRoomModal);
            if (joinRoomButton != null) joinRoomButton.onClick.AddListener(OnJoinPrivateRoom);
            if (createRoomButton != null) createRoomButton.onClick.AddListener(OnCreatePrivateRoom);

            // Options modal
            if (closeOptionsModalButton != null) closeOptionsModalButton.onClick.AddListener(CloseOptions);

#if UNITY_WEBGL
            // Quit does nothing in WebGL builds; hide the button.
            if (quitButton != null) quitButton.gameObject.SetActive(false);
#endif
        }

        private void OnPlayOnline()
        {
            Debug.Log("[MainMenu] Play Online (public matchmaking).");
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

            string upper = (raw ?? "").ToUpperInvariant();
            System.Text.StringBuilder sb = new System.Text.StringBuilder(6);

            foreach (char c in upper)
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
                if (privateRoomHintText != null) privateRoomHintText.text = "Room code must be 6 letters/numbers.";
                return;
            }

            Debug.Log($"[MainMenu] Join private room: {code}");
            // Hook later: call Master Server to resolve code -> game server address
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnCreatePrivateRoom()
        {
            Debug.Log("[MainMenu] Create private room.");
            // Hook later: call Master Server to create room -> get code + server address
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnCustomise()
        {
            Debug.Log("[MainMenu] Customise.");
            TryLoadOrWarn(customiseSceneName);
        }

        private void OnShop()
        {
            Debug.Log("[MainMenu] Shop.");
            TryLoadOrWarn(shopSceneName);
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

        private void OnQuit()
        {
            Debug.Log("[MainMenu] Quit.");
            Application.Quit();
        }

        private void TryLoadOrWarn(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            // Avoid hard errors early if scenes aren't added yet
            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch
            {
                Debug.LogWarning($"[MainMenu] Scene '{sceneName}' is not in Build Settings yet.");
            }
        }

        public void OpenCustomisation()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Customisation");
        }
    }
}