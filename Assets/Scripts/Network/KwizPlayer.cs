using Mirror;
using UnityEngine;

namespace Kwiztime
{
    public class KwizPlayer : NetworkBehaviour
    {
        [SyncVar] public string displayName = "Player";
        [SyncVar] public int selectedAnswer = -1; // -1 = none
        [SyncVar] public int coins = 0;

        [SyncVar] public bool isBot = false;
        [SyncVar] public int botMascotId = 0;
        [SyncVar] public string botId = "";

        // Physical (fixed)
        [SyncVar] public int bodyShapeId = 0;
        [SyncVar] public int eyesId = 0;
        [SyncVar] public int mouthId = 0;
        [SyncVar] public int hairId = 0;
        [SyncVar] public int skinToneId = 0;

        // Outfit
        [SyncVar] public int hatId = -1;
        [SyncVar] public int topId = 0;          // default basic vest/shirt
        [SyncVar] public int legwearId = 0;      // default shorts
        [SyncVar] public int wholeOutfitId = -1; // overrides top+legwear
        [SyncVar] public int shoesId = -1;

        // Accessories (multiple)
        [SyncVar] public int accessoryAId = -1;
        [SyncVar] public int accessoryBId = -1;
        [SyncVar] public int accessoryCId = -1;
        [SyncVar] public int mascotId = 0;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // Load saved settings locally
            int bodyShapeId = PlayerPrefs.GetInt("bodyShapeId", 0);
            int skinToneId = PlayerPrefs.GetInt("skinToneId", 0);
            int hairId = PlayerPrefs.GetInt("hairId", 0);
            int eyesId = PlayerPrefs.GetInt("eyesId", 0);
            int mouthId = PlayerPrefs.GetInt("mouthId", 0);
            int mascotId = PlayerPrefs.GetInt("mascotId", 0);

            int hatId = PlayerPrefs.GetInt("hatId", -1);
            int topId = PlayerPrefs.GetInt("topId", 0);
            int legwearId = PlayerPrefs.GetInt("legwearId", 0);
            int wholeOutfitId = PlayerPrefs.GetInt("wholeOutfitId", -1);
            int shoesId = PlayerPrefs.GetInt("shoesId", -1);

            int accAId = PlayerPrefs.GetInt("accAId", -1);
            int accBId = PlayerPrefs.GetInt("accBId", -1);
            int accCId = PlayerPrefs.GetInt("accCId", -1);

            CmdApplyCosmeticsFromPrefs(
                bodyShapeId, skinToneId, hairId, eyesId, mouthId, mascotId,
                hatId, topId, legwearId, wholeOutfitId, shoesId,
                accAId, accBId, accCId
            );
        }


        [Command]
        public void CmdSubmitAnswer(int answerIndex)
        {
            var room = KwizRoomManager.Instance;

            if (room == null)
            {
                Debug.LogWarning("[Server] CmdSubmitAnswer rejected: no room instance.");
                TargetNotify(connectionToClient, "Not ready yet.");
                return;
            }

            if (!room.ServerCanAcceptAnswers())
            {
                Debug.Log($"[Server] CmdSubmitAnswer rejected (locked). Player {netId} tried {answerIndex}");
                TargetNotify(connectionToClient, "Too late! Answers are locked.");
                return;
            }

            // Validate input: only 0..3 accepted (or -1 to clear, if you ever need it)
            if (answerIndex < 0 || answerIndex > 3)
            {
                Debug.LogWarning($"[Server] CmdSubmitAnswer invalid index {answerIndex} from Player {netId}");
                TargetNotify(connectionToClient, "Invalid answer.");
                return;
            }

            // Allow changing answer until lock
            selectedAnswer = answerIndex;

            // Optional: notify selection (commented to avoid spam)
            // TargetNotify(connectionToClient, $"Selected answer {answerIndex + 1}");
        }

        [TargetRpc]
        private void TargetNotify(NetworkConnectionToClient target, string message)
        {
            Kwiztime.UI.ClientUIEvents.OnStatus?.Invoke(message);
        }

        [Command]
        public void CmdRequestPlayAgain()
        {
            var room = KwizRoomManager.Instance;
            if (room == null) return;

            // Only allow if match is finished / in lobby
            room.ServerStartMatchFromUI();
        }

        [Command]
        public void CmdApplyCosmetics(
            int bodyShapeId, int skinToneId, int hairId, int eyesId, int mouthId,
            int hatId, int topId, int legwearId, int wholeOutfitId, int shoesId,
            int accessoryAId, int accessoryBId, int accessoryCId
        )
        {
            this.bodyShapeId = bodyShapeId;
            this.skinToneId = skinToneId;
            this.hairId = hairId;
            this.eyesId = eyesId;
            this.mouthId = mouthId;

            this.hatId = hatId;
            this.topId = topId;
            this.legwearId = legwearId;
            this.wholeOutfitId = wholeOutfitId;
            this.shoesId = shoesId;

            this.accessoryAId = accessoryAId;
            this.accessoryBId = accessoryBId;
            this.accessoryCId = accessoryCId;
        }

        [Command]
        private void CmdApplyCosmeticsFromPrefs(
            int bodyShapeId, int skinToneId, int hairId, int eyesId, int mouthId, int mascotId,
            int hatId, int topId, int legwearId, int wholeOutfitId, int shoesId,
            int accessoryAId, int accessoryBId, int accessoryCId
        )
        {
            this.bodyShapeId = bodyShapeId;
            this.skinToneId = skinToneId;
            this.hairId = hairId;
            this.eyesId = eyesId;
            this.mouthId = mouthId;
            this.mascotId = mascotId;

            this.hatId = hatId;
            this.topId = topId;
            this.legwearId = legwearId;
            this.wholeOutfitId = wholeOutfitId;
            this.shoesId = shoesId;

            this.accessoryAId = accessoryAId;
            this.accessoryBId = accessoryBId;
            this.accessoryCId = accessoryCId;
        }
    }
}