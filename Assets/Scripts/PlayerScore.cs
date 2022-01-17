using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror.Examples.MultipleAdditiveScenes
{
    public class PlayerScore : NetworkBehaviour
    {
        [SyncVar]
        public float timeRemaining = 10;

        [SyncVar]
        public int playerNumber;

        [SyncVar]
        public int scoreIndex;

        [SyncVar]
        public int matchIndex;

        [SyncVar]
        public uint score;

        public int clientMatchIndex = -1;

        public bool hasEnded = false;

        void OnGUI()
        {
            if (!isServerOnly && !isLocalPlayer && clientMatchIndex < 0)
                clientMatchIndex = NetworkClient.connection.identity.GetComponent<PlayerScore>().matchIndex;

            if (isLocalPlayer || matchIndex == clientMatchIndex)
            {
                GUI.Box(new Rect(10f + (scoreIndex * 110), 10f, 100f, 25f), $"P{playerNumber}: {score}");
                GUI.Box(new Rect(10f + (scoreIndex * 110), 30f, 100f, 25f), $"Time: {((int)timeRemaining)}");
            }
        }

        [Command]
        void CmdEndGame()
        {
            var playId = PlayerPrefs.GetString("HastePlayId");
            var leaderboardId = PlayerPrefs.GetString("HasteLeaderboardId");
            StartCoroutine(HasteIntegration.Instance.Server.Score(score.ToString(), playId, leaderboardId, ScoreResult));
        }

        void Update()
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                if (!hasEnded)
                {
                    Debug.Log("ending game");
                    hasEnded = true;
                    CmdEndGame();
                }
            }
        }

        [TargetRpc]
        void RpcSetError(string errorMessage)
        {
            var leaderboardSelection = GameObject.Find("LeaderboardSelection");
            var prefab = Instantiate(Resources.Load("ErrorLabel"), new Vector3(900, 50, 0), Quaternion.identity, leaderboardSelection.transform) as GameObject;
            var label = prefab.GetComponentsInChildren<TMPro.TextMeshProUGUI>().FirstOrDefault();
            label.name = "ErrorLabel";
            label.text = errorMessage;
            Debug.Log(errorMessage);
        }

        [TargetRpc]
        void RpcEndGame(string message)
        {
            Debug.Log(message);
        }

        void ScoreResult(HasteServerScoreResult scoreResult)
        {
            if (scoreResult != null)
            {
                // RpcEndGame();
                if (!string.IsNullOrEmpty(scoreResult.message))
                {
                    RpcEndGame(scoreResult.message);
                }
                else
                {
                    // change scenes
                    RpcEndGame($"Your score was {score} and you placed {scoreResult.leaderRank} on the leaderboard.");
                }
            }
        }
    }

}
