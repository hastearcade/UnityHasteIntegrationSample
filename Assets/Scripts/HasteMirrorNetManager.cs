using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.MultipleAdditiveScenes;
using UnityEngine;
using UnityEngine.SceneManagement;

// This example was started from the MultiSceneAdditive example in Mirror
[AddComponentMenu("")]
public class HasteMirrorNetManager : NetworkManager
{
    [Header("Spawner Setup")]
    [Tooltip("Reward Prefab for the Spawner")]
    public GameObject rewardPrefab;

    [Tooltip("Leaderboard Prefab")]
    public GameObject leaderboardPrefab;
    [Header("Game Setup")]
    [Scene]
    public string gameScene;

    readonly Dictionary<string, Scene> subScenes = new Dictionary<string, Scene>();

    readonly Dictionary<string, GameObject> leaderboards = new Dictionary<string, GameObject>();

    int clientIndex;

    public IEnumerator StartGameInstanceForPlayer(NetworkConnection conn)
    {
        yield return SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });

        clientIndex++;
        Scene newScene = SceneManager.GetSceneAt(clientIndex);
        subScenes.Add(clientIndex.ToString(), newScene);
        Spawner.InitialSpawn(newScene);

        conn.Send(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.LoadAdditive });

        // Wait for end of frame before adding the player to ensure Scene Message goes first
        yield return new WaitForEndOfFrame();

        PlayerScore playerScore = conn.identity.GetComponent<PlayerScore>();
        playerScore.playerNumber = clientIndex;
        playerScore.timeRemaining = 15;
        playerScore.hasStarted = true;

        SceneManager.MoveGameObjectToScene(conn.identity.gameObject, subScenes[(clientIndex.ToString())]);

        conn.identity.gameObject.transform.position = new Vector3(-5, 10, 4); // reset the position on start of game
    }

    public override void OnStartServer()
    {
        StartCoroutine(HasteIntegration.Instance.Server.GetServerToken(GetHasteTokenCompleted));
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        SpawnLeaderboard(conn);
    }

    void SpawnLeaderboard(NetworkConnection conn)
    {
        if (!NetworkServer.active) return;

        Vector3 spawnPosition = new Vector3(0, 0, 0);
        GameObject leaderboard = Object.Instantiate(((HasteMirrorNetManager)NetworkManager.singleton).leaderboardPrefab, spawnPosition, Quaternion.identity);
        leaderboard.name = "Leaderboards";
        NetworkServer.Spawn(leaderboard, conn);
    }
    private void GetHasteTokenCompleted(HasteServerAuthResult result)
    {
        if (result != null)
        {
            StartCoroutine(HasteIntegration.Instance.Server.ConfigureHasteServer(result, ConfigureHasteServerCompleted));
        }
    }

    private void ConfigureHasteServerCompleted(HasteAllLeaderboards leaderboards)
    {
        if (leaderboards != null)
        {
            HasteIntegration.Instance.Server.Leaderboards = leaderboards.leaderboards;
        }
    }

}
