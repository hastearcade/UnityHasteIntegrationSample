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
        playerScore.scoreIndex = clientIndex / subScenes.Count;
        playerScore.matchIndex = clientIndex % subScenes.Count;

        SceneManager.MoveGameObjectToScene(conn.identity.gameObject, subScenes[(clientIndex.ToString())]);
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
        //leaderboards.Add(conn.connectionId.ToString(), leaderboard);
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

    /*
        // We're additively loading scenes, so GetSceneAt(0) will return the main "container" scene,
        // therefore we start the index at one and loop through instances value inclusively.
        // If instances is zero, the loop is bypassed entirely.
        IEnumerator ServerLoadSubScenes()
        {
            for (int index = 1; index <= instances; index++)
            {
                yield return SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });

                Scene newScene = SceneManager.GetSceneAt(index);
                subScenes.Add(newScene);
                Spawner.InitialSpawn(newScene);
            }

            subscenesLoaded = true;
        }

        /// <summary>
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        public override void OnStopServer()
        {
            NetworkServer.SendToAll(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.UnloadAdditive });
            StartCoroutine(ServerUnloadSubScenes());
            clientIndex = 0;
        }

        // Unload the subScenes and unused assets and clear the subScenes list.
        IEnumerator ServerUnloadSubScenes()
        {
            for (int index = 0; index < subScenes.Count; index++)
                yield return SceneManager.UnloadSceneAsync(subScenes[index]);

            subScenes.Clear();
            subscenesLoaded = false;

            yield return Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// This is called when a client is stopped.
        /// </summary>
        public override void OnStopClient()
        {
            // make sure we're not in host mode
            if (mode == NetworkManagerMode.ClientOnly)
                StartCoroutine(ClientUnloadSubScenes());
        }

        // Unload all but the active scene, which is the "container" scene
        IEnumerator ClientUnloadSubScenes()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                    yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
            }
        }
        */
}
