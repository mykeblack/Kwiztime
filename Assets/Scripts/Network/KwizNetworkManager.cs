using Mirror;
using UnityEngine;

namespace Kwiztime
{
    public class KwizNetworkManager : NetworkManager
    {
        [Header("Kwiztime – Room")]
        [Tooltip("Prefab containing KwizRoomManager + NetworkIdentity")]
        [SerializeField] private KwizRoomManager roomPrefab;

        private KwizRoomManager activeRoom;

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (roomPrefab == null)
            {
                Debug.LogError("[KwizNetworkManager] Room Prefab is NOT assigned.");
                return;
            }

            // Spawn a single room for now (dev / prototype)
            GameObject roomObj = Instantiate(roomPrefab.gameObject);
            NetworkServer.Spawn(roomObj);

            activeRoom = roomObj.GetComponent<KwizRoomManager>();

            Debug.Log("[Server] Kwiz Room spawned.");
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // Spawn player
            GameObject playerObj = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, playerObj);

            KwizPlayer player = playerObj.GetComponent<KwizPlayer>();

            if (player == null)
            {
                Debug.LogError("[Server] Spawned player has no KwizPlayer component.");
                return;
            }

            // Assign to room
            if (activeRoom != null)
            {
                activeRoom.ServerRegisterPlayer(player);
            }
            else
            {
                Debug.LogError("[Server] No active room found.");
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            activeRoom = null;
            Debug.Log("[Server] Server stopped, room cleared.");
        }
    }
}