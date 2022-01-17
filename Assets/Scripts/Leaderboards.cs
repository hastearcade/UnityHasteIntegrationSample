using System.Collections;
using System.Collections.Generic;
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

                button.onClick.AddListener(delegate { SelectPayment(button.name); });
                startY -= 150;
            }
        }
    }

    void SelectPayment(string paymentTierId)
    {
        Debug.Log("You clicked " + paymentTierId);
    }
    // Update is called once per frame
    void Update()
    {
    }
}
