using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Kwiztime.UI
{
    public class DevNetworkUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button stopButton;

        private NetworkManager Manager
        {
            get
            {
                // NetworkManager.singleton is set in NetworkManager.Awake
                if (NetworkManager.singleton != null)
                    return NetworkManager.singleton;

                // Fallback: find one in the scene (useful if singleton not yet set)
                return FindFirstObjectByType<NetworkManager>();
            }
        }

        private void Awake()
        {
            if (hostButton != null) hostButton.onClick.AddListener(StartHost);
            if (clientButton != null) clientButton.onClick.AddListener(StartClient);
            if (stopButton != null) stopButton.onClick.AddListener(StopAll);
        }

        private void StartHost()
        {
            var manager = Manager;
            if (manager == null)
            {
                Debug.LogError("[DevNetworkUI] No NetworkManager found in the scene. Make sure PF_NetworkRoot is in the Game scene and enabled.");
                return;
            }

            if (!NetworkServer.active && !NetworkClient.isConnected)
            {
                Debug.Log("[DevNetworkUI] Starting Host...");
                manager.StartHost();
            }
        }

        private void StartClient()
        {
            var manager = Manager;
            if (manager == null)
            {
                Debug.LogError("[DevNetworkUI] No NetworkManager found in the scene. Make sure PF_NetworkRoot is in the Game scene and enabled.");
                return;
            }

            if (!NetworkClient.isConnected)
            {
                Debug.Log("[DevNetworkUI] Starting Client...");
                manager.StartClient();
            }
        }

        private void StopAll()
        {
            var manager = Manager;
            if (manager == null)
                return;

            if (NetworkServer.active || NetworkClient.isConnected)
            {
                Debug.Log("[DevNetworkUI] Stopping Host/Client...");
                manager.StopHost();
            }
        }
    }
}