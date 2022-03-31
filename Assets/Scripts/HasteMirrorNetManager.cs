using System;
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

    public void StartGameInstanceForPlayer(NetworkConnection conn)
    {
        PlayerScore playerScore = conn.identity.GetComponent<PlayerScore>();
        playerScore.playerNumber = conn.identity.netId;
        playerScore.timeRemaining = 15;
        playerScore.hasStarted = true;

        var newScene = subScenes[conn.identity.netId.ToString()];
        Spawner.InitialSpawn(newScene);

        SceneManager.MoveGameObjectToScene(conn.identity.gameObject, subScenes[(conn.identity.netId.ToString())]);
    }

    public void EndGameInstanceForPlayer(NetworkConnection conn)
    {
        /*
        THIS IS WHERE YOU WOULD PERFORM ANY SERVER SIDE CLEANUP OF YOUR PLAYER, ETC

        Example:
        var player = conn.identity.gameObject.GetComponent<Player>();
        player.obstacles.Clear();

        */
    }

    public override void OnStartServer()
    {
        DotEnv.Load("./.env");

        var secret = System.Environment.GetEnvironmentVariable("HASTE_SERVER_SECRET");
        var clientId = System.Environment.GetEnvironmentVariable("HASTE_SERVER_CLIENT_ID");
        var environment = System.Environment.GetEnvironmentVariable("HASTE_SERVER_ENVIRONMENT");

        if (string.IsNullOrEmpty(secret))
        {
            secret = System.Environment.GetEnvironmentVariable("HASTE_SERVER_SECRET", EnvironmentVariableTarget.User);
            clientId = System.Environment.GetEnvironmentVariable("HASTE_SERVER_CLIENT_ID", EnvironmentVariableTarget.User);
            environment = System.Environment.GetEnvironmentVariable("HASTE_SERVER_ENVIRONMENT", EnvironmentVariableTarget.User);
        }

        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(environment))
        {
            Debug.LogError("Please ensure that you have created a .env file in your root directory or you have set user level environment variables.");
        }

        StartCoroutine(HasteIntegration.Instance.Server.GetServerToken(clientId, secret, environment, GetHasteTokenCompleted));
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        var newScene = SceneManager.LoadScene(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics2D });

        subScenes.Add(conn.identity.netId.ToString(), newScene);
        conn.Send(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.LoadAdditive });

        SceneManager.MoveGameObjectToScene(conn.identity.gameObject, subScenes[(conn.identity.netId.ToString())]);
        SpawnLeaderboard(conn);
    }

    void SpawnLeaderboard(NetworkConnection conn)
    {
        if (!NetworkServer.active) return;

        Vector3 spawnPosition = new Vector3(0, 0, 0);
        GameObject leaderboard = UnityEngine.Object.Instantiate(((HasteMirrorNetManager)NetworkManager.singleton).leaderboardPrefab, spawnPosition, Quaternion.identity);
        leaderboard.name = "Leaderboards";
        NetworkServer.Spawn(leaderboard, conn);
        SceneManager.MoveGameObjectToScene(leaderboard, subScenes[(conn.identity.netId.ToString())]);
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

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer()
    {
        NetworkServer.SendToAll(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.UnloadAdditive });
        StartCoroutine(ServerUnloadSubScenes());
    }

    // Unload the subScenes and unused assets and clear the subScenes list.
    IEnumerator ServerUnloadSubScenes()
    {
        foreach (var scene in subScenes)
            yield return SceneManager.UnloadSceneAsync(scene.Value);

        subScenes.Clear();
        yield return Resources.UnloadUnusedAssets();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        try
        {
            StartCoroutine(ClientUnloadSubScenes(conn.identity.netId.ToString()));
            base.OnServerDisconnect(conn);
        }
        catch (Exception ex)
        {
            Debug.LogError("An error occurred while disconnecting a client. The error is " + ex.Message + ". The inner message is " + ex.InnerException.Message);
        }
    }

    public Nullable<Scene> GetScene(string netId)
    {
        Scene sceneToRemove;
        if (subScenes.TryGetValue(netId, out sceneToRemove))
            return sceneToRemove;
        else
            return null;
    }

    // Unload all but the active scene, which is the "container" scene
    IEnumerator ClientUnloadSubScenes(string netId)
    {
        Scene? sceneToRemove = GetScene(netId);
        if (sceneToRemove != null)
        {
            subScenes.Remove(netId);
            yield return SceneManager.UnloadSceneAsync(sceneToRemove.Value);
        }
    }

}
