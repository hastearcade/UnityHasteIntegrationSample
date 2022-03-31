using Mirror;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Leaderboards : NetworkBehaviour
{
    private readonly SyncList<HasteLeaderboardDetail> hasteLeaderboards = new SyncList<HasteLeaderboardDetail>();
    // Start is called before the first frame update
    void Start()
    {
        this.name = "Leaderboards";
        if (isServer)
        {
            // populate the sync list with results from the Haste Server integration
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
        // kick off the payment flow via haste Play endpoint
        PlayerPrefs.SetString("HasteLeaderboardId", leaderboardId);
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
    }

    [TargetRpc]
    void RpcStartGame()
    {
        // hide all canvas elements to show the underlying game
        var uiComponent = GameObject.Find("UI");

        var uiCamera = GameObject.Find("MenuCamera");
        uiComponent.SetActive(false);
        uiCamera.SetActive(false);

        var player = GameObject.Find("Player(Clone)");
        player.gameObject.transform.position = new Vector3(-5, 5, 4);
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
                PlayerPrefs.SetString("HastePlayId", playResult.id);
                ((HasteMirrorNetManager)NetworkManager.singleton).StartGameInstanceForPlayer(GetComponent<NetworkIdentity>().connectionToClient);
                RpcStartGame();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
}
