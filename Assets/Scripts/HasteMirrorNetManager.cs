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

    [Header("MultiScene Setup")]
    public int instances = 3;

    [Scene]
    public string gameScene;

    [Scene]
    public string leaderboardScene;

    [Scene]
    public string titleScreen;

    // This is set true after server loads all subscene instances
    bool subscenesLoaded;

    // subscenes are added to this list as they're loaded
    readonly List<Scene> subScenes = new List<Scene>();

    // Sequential index used in round-robin deployment of players into instances and score positioning
    int clientIndex;

    /// <summary>
    /// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Adding player");
        // StartCoroutine(OnServerAddPlayerDelayed(conn));
    }

    // This delay is mostly for the host player that loads too fast for the
    // server to have subscenes async loaded from OnStartServer ahead of it.
    void OnServerAddPlayerDelayed(NetworkConnection conn)
    {
        // base.OnServerAddPlayer(conn);
        //yield return LoadLeaderboardScene(conn.identity.gameObject);
        // Send Scene message to client to additively load the game scene
        //conn.Send(new SceneMessage { sceneName = leaderboardScene, sceneOperation = SceneOperation.LoadAdditive });
        //conn.Send(new SceneMessage { sceneName = titleScreen, sceneOperation = SceneOperation.UnloadAdditive });
        // Wait for end of frame before adding the player to ensure Scene Message goes first
        //yield return new WaitForEndOfFrame();

        /*

        // wait for server to async load all subscenes for game instances
        while (!subscenesLoaded)
            yield return null;


                    PlayerScore playerScore = conn.identity.GetComponent<PlayerScore>();
                    playerScore.playerNumber = clientIndex;
                    playerScore.scoreIndex = clientIndex / subScenes.Count;
                    playerScore.matchIndex = clientIndex % subScenes.Count;

                    // Do this only on server, not on clients
                    // This is what allows the NetworkSceneChecker on player and scene objects
                    // to isolate matches per scene instance on server.
                    if (subScenes.Count > 0)
                        SceneManager.MoveGameObjectToScene(conn.identity.gameObject, subScenes[clientIndex % subScenes.Count]);

        */
    }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer()
    {
        Debug.Log("Server started");
        StartCoroutine(HasteIntegration.Instance.Server.GetServerToken(GetHasteTokenCompleted));
    }

    private IEnumerator LoadLeaderboardScene(GameObject playerGameObject)
    {
        yield return SceneManager.LoadSceneAsync(leaderboardScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
        clientIndex++;
        Scene newScene = SceneManager.GetSceneAt(clientIndex);
        subScenes.Add(newScene);
        SceneManager.MoveGameObjectToScene(playerGameObject, subScenes[clientIndex]);
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
