using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mirror.Examples.MultipleAdditiveScenes
{
    public class PlayerScore : NetworkBehaviour
    {
        [SyncVar]
        public float timeRemaining = 1000; // this gets reset to 15 after game starts

        [SyncVar]
        public int playerNumber;

        [SyncVar]
        public uint score;

        [SyncVar]
        public bool hasEnded = false;

        [SyncVar]
        public bool hasStarted = false;

        void OnGUI()
        {
            if (isLocalPlayer && hasStarted)
            {
                GUI.Box(new Rect(10f, 10f, 100f, 50f), $"P{playerNumber}: {score}");
                GUI.Box(new Rect(10f, 80f, 100f, 50f), $"Time: {((int)timeRemaining)}");
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
        }

        [TargetRpc]
        void RpcEndGame(string message)
        {
            var ui = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == "UI");
            ui.SetActive(true);
            var finalScreen = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "FinalScoreScreen");
            var leaderboardPanel = GameObject.Find("LeaderboardSelection");
            finalScreen.SetActive(true);
            leaderboardPanel.SetActive(false);

            var textResults = finalScreen.gameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().FirstOrDefault(g => g.name == "Results");
            textResults.text = message;
            hasStarted = false; // turn off the score hud
        }
        void ScoreResult(HasteServerScoreResult scoreResult)
        {
            if (scoreResult != null)
            {
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