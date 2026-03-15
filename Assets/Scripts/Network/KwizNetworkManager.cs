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

            GameObject roomObj = Instantiate(roomPrefab.gameObject);
            NetworkServer.Spawn(roomObj);
            activeRoom = roomObj.GetComponent<KwizRoomManager>();

            Debug.Log("[Server] Kwiz Room spawned.");
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            GameObject playerObj = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, playerObj);

            KwizPlayer player = playerObj.GetComponent<KwizPlayer>();
            if (player == null)
            {
                Debug.LogError("[Server] Spawned player has no KwizPlayer component.");
                return;
            }

            if (activeRoom != null)
                activeRoom.ServerRegisterPlayer(player);
            else
                Debug.LogError("[Server] No active room found.");
        }

        // FIX: unregister player from room on disconnect so room state stays accurate
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                KwizPlayer player = conn.identity.GetComponent<KwizPlayer>();
                if (player != null && activeRoom != null)
                {
                    activeRoom.ServerUnregisterPlayer(player);
                    Debug.Log($"[Server] Player {conn.identity.netId} disconnected and unregistered from room.");
                }
            }

            base.OnServerDisconnect(conn);
        }

        // FIX: destroy room GameObject on stop, not just clear the reference
        public override void OnStopServer()
        {
            base.OnStopServer();

            if (activeRoom != null)
            {
                NetworkServer.UnSpawn(activeRoom.gameObject);
                Destroy(activeRoom.gameObject);
                activeRoom = null;
            }

            Debug.Log("[Server] Server stopped, room destroyed.");
        }
    }
}