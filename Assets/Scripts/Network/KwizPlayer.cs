using Mirror;
using UnityEngine;

namespace Kwiztime
{
    public class KwizPlayer : NetworkBehaviour
    {
        [SyncVar] public string displayName = "Player";
        [SyncVar] public int selectedAnswer = -1;
        [SyncVar] public int coins = 0;

        [SyncVar] public bool isBot = false;
        [SyncVar] public int botMascotId = 0;
        [SyncVar] public string botId = "";

        // Physical (fixed)
        [SyncVar] public int bodyShapeId = 0;
        [SyncVar] public int eyesId = 0;
        [SyncVar] public int mouthId = 0;
        [SyncVar] public int skinToneId = 0;

        // Outfit
        [SyncVar] public int hairId = -1;
        [SyncVar] public int hatId = -1;
        [SyncVar] public int topId = 0;
        [SyncVar] public int legwearId = 0;
        [SyncVar] public int wholeOutfitId = -1;
        [SyncVar] public int shoesId = -1;

        // Accessories
        [SyncVar] public int accessoryAId = -1;
        [SyncVar] public int accessoryBId = -1;
        [SyncVar] public int accessoryCId = -1;

        // Mascot
        [SyncVar] public int mascotId = 0;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // FIX: removed local variable shadowing — read directly into Cmd call
            CmdApplyCosmeticsFromPrefs(
                PlayerPrefs.GetInt("bodyShapeId", 0),
                PlayerPrefs.GetInt("skinToneId", 0),
                PlayerPrefs.GetInt("hairId", -1),
                PlayerPrefs.GetInt("eyesId", 0),
                PlayerPrefs.GetInt("mouthId", 0),
                PlayerPrefs.GetInt("mascotId", 0),
                PlayerPrefs.GetInt("hatId", -1),
                PlayerPrefs.GetInt("topId", 0),
                PlayerPrefs.GetInt("legwearId", 0),
                PlayerPrefs.GetInt("wholeOutfitId", -1),
                PlayerPrefs.GetInt("shoesId", -1),
                PlayerPrefs.GetInt("accAId", -1),
                PlayerPrefs.GetInt("accBId", -1),
                PlayerPrefs.GetInt("accCId", -1)
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

            if (answerIndex < 0 || answerIndex > 3)
            {
                Debug.LogWarning($"[Server] CmdSubmitAnswer invalid index {answerIndex} from Player {netId}");
                TargetNotify(connectionToClient, "Invalid answer.");
                return;
            }

            selectedAnswer = answerIndex;
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
            room.ServerStartMatchFromUI();
        }

        // FIX: added mascotId parameter (was missing, causing mascot changes to never sync)
        [Command]
        public void CmdApplyCosmetics(
            int bodyShapeId, int skinToneId, int hairId, int eyesId, int mouthId, int mascotId,
            int hatId, int topId, int legwearId, int wholeOutfitId, int shoesId,
            int accessoryAId, int accessoryBId, int accessoryCId
        )
        {
            this.bodyShapeId  = bodyShapeId;
            this.skinToneId   = skinToneId;
            this.hairId       = hairId;
            this.eyesId       = eyesId;
            this.mouthId      = mouthId;
            this.mascotId     = mascotId; // FIX: was missing

            this.hatId        = hatId;
            this.topId        = topId;
            this.legwearId    = legwearId;
            this.wholeOutfitId = wholeOutfitId;
            this.shoesId      = shoesId;

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
            this.bodyShapeId  = bodyShapeId;
            this.skinToneId   = skinToneId;
            this.hairId       = hairId;
            this.eyesId       = eyesId;
            this.mouthId      = mouthId;
            this.mascotId     = mascotId;

            this.hatId        = hatId;
            this.topId        = topId;
            this.legwearId    = legwearId;
            this.wholeOutfitId = wholeOutfitId;
            this.shoesId      = shoesId;

            this.accessoryAId = accessoryAId;
            this.accessoryBId = accessoryBId;
            this.accessoryCId = accessoryCId;
        }
    }
}