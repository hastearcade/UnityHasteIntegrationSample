using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Leaderboards : NetworkBehaviour
{
    private readonly SyncList<HasteLeaderboardDetail> hasteLeaderboards = new SyncList<HasteLeaderboardDetail>();
    private string PlayId { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        this.name = "Leaderboards";
        if (isServer)
        {
            foreach (var leaderboard in HasteIntegration.Instance.Server.Leaderboards)
            {
                hasteLeaderboards.Add(leaderboard);
            }
        }
        else
        {
            // now that we are on the client with the leaderboard details we can dynamically add buttons to the canvas
            var leaderboardSelection = GameObject.Find("LeaderboardSelection");
            var startY = 900;
            foreach (var leaderboard in hasteLeaderboards)
            {
                var prefab = Instantiate(Resources.Load("Button"), new Vector3(900, startY, 0), Quaternion.identity, leaderboardSelection.transform) as GameObject;
                var label = prefab.GetComponentsInChildren<TMPro.TextMeshProUGUI>().FirstOrDefault();
                var button = prefab.GetComponentsInChildren<Button>().FirstOrDefault();

                label.text = leaderboard.name;
                prefab.name = leaderboard.id;
                button.name = leaderboard.id;

                button.onClick.AddListener(delegate { CmdSelectPayment(PlayerPrefs.GetString("HasteAccessToken"), button.name); });
                startY -= 150;
            }

        }
    }

    [Command]
    void CmdSelectPayment(string JWT, string leaderboardId)
    {
        StartCoroutine(HasteIntegration.Instance.Server.Play(JWT, leaderboardId, PlayResult));
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
    void RpcStartGame()
    {
        var uiComponent = GameObject.Find("UI");
        var uiCamera = GameObject.Find("MenuCamera");
        uiComponent.SetActive(false);
        uiCamera.SetActive(false);
    }

    void PlayResult(HasteServerPlayResult playResult)
    {
        if (playResult != null)
        {
            if (!string.IsNullOrEmpty(playResult.message))
            {
                RpcSetError(playResult.message);
            }
            else
            {
                // change scenes
                PlayId = playResult.id;
                StartCoroutine(((HasteMirrorNetManager)NetworkManager.singleton).StartGameInstanceForPlayer(GetComponent<NetworkIdentity>().connectionToClient));
                RpcStartGame();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
}
