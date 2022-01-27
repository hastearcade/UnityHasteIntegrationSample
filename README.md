# Haste Integration for Unity Example 

## Overview

This readme is intended for developers looking to utilize the Haste ecosystem within a Unity based game. 

The UnityHasteIntegrationSample repository contains a simple game based on [this](https://github.com/vis2k/Mirror/tree/master/Assets/Mirror/Examples/MultipleAdditiveScenes) example game in the [Mirror](https://mirror-networking.gitbook.io/docs/) repository. The code is a modified version of the game, but contains code and assets from the mirror example.

You will need a valid, production account for the Haste Arcade and will need to create a game in the [Haste Developer Portal](https://developer.hastearcade.com/)

In order to run the example perform the following steps:

1. `git clone git@github.com:hastearcade/UnityHasteIntegrationSample.git`
2. Import project into Unity
3. Either create a `.env` file in the root directory (recommended) or set the environment variables manually in your operating system with the three environment variables found [here](.env.sample). The `HASTE_SERVER_ENVIRONMENT` variable should be set to `nonproduction` for testing and `production` when your game is ready to begin processing payments (i.e. final testing / production launch of the game).
3. Create clone via ParrelSync menu.
4. Open clone in a separate editor. One will be the game client and the other will be the game server.
5. Ensure that in both instances of Unity the TitleScreen scene is loaded into the main hierarchy.
5. For the game server instance of Unity, click the 'Play' button to start the game. You should see a 'Server Started' message.
6. For the game client instance of Unity there is one additional step. Click on the UI component under the main TitleScreen scene. In the inspector, change the Role from "Server" to "Client". Then click the Play button to start the game.

## Table of Contents

- [Guide](#guide)
- [License](#license)
- [Contributing](#contributing)
- [Authors](#authors)

## Guide

The example game utilizes Mirror to faciliatate a client/server interaction for the game. *Note: All games developed for the Haste Arcade must adhere to the security requirements defined [here](https://github.com/hastearcade/haste-sdk/blob/main/security.md). Specifically, your game must follow a server authoritative model even for single player games.*

The utilization of Mirror to develop server authoritative models is beyond the scope of this document. This document will focus specifically on the Haste Integration touchpoints. To integrate with Haste, the developer will need to integrate both on the game client and the game server.

The Haste integration code can all be found under `Assets\Haste`. The primary entry point is the `HasteIntegration` class. The integration class is a singleton with two static properties: `Client` and `Server`. The developer can utilize the `HasteIntegration.Instance.Client.FuncName` or `HasteIntegration.Instance.Server.FuncName` to call the appropriate function from the game client or game server respectfully. Generally speaking, any function that performs an asyncronous operation will utilize a callback to receive the result of the operation.

If you are utilizing a different server backend than Unity, then please contact the Haste team on our [Discord](https://discord.gg/mqPN8gDF3A) for assistance.

### Client

The game client integration is focused entirely upon authentication. In order to utilize Haste, the game client will need to first receive a player access token (JWT) from the Haste authentication system. This is a two step process. The first step is to initiate the login flow. The login process will open a new browser window with the appropriate options for the user to authenticate. The second step is to wait for the login to return. The time for the user to authenticate is an unknown amount of time; thus, this step will poll the Haste APIs until a token is returned.

#### Step 1
The following code can be found in `TitleScreen.cs`. It will first validate that the user is logged in. If the user is not logged in, it will utilize the `HasteIntegration` class to initiate the login flow.

```csharp
public void Login()
{
    // The access token should be stored somewhere.
    // then in this function you should check that storage location to see if the user is already logged in
    // it does have an expiration date embedded in teh access_token. This should be parsed and stored as well
    // the expiratin date must be checked to ensure their token is still valid
    var expirationDate = !PlayerPrefs.HasKey("HasteAccessToken") ?
        DateTime.MinValue :
        (!PlayerPrefs.HasKey("HasteExpiration") ?
            DateTime.MinValue :
            DateTime.ParseExact(PlayerPrefs.GetString("HasteExpiration"), "yyyyMMddHHmmss", new CultureInfo("en-US")));

    if (expirationDate < DateTime.Now)
    {
        // Starts the login flow, which will open the browser. The login function expects a callback
        // The callback will ultimately have an error message or the login details.
        StartCoroutine(HasteIntegration.Instance.Client.Login(this.CompletedLoginInit)); // see below for implementation of callback
    }
    else
    {
        // The user is already logged in
        StartClient();
    }
}
```

#### Step 2
Once the login flow has initiated, the developer will need to poll the Haste system to obtain the player access token. The following code demonstrates this step:

```csharp
private void CompletedLoginInit(HasteCliResult cliResult)
{
    // Once the /cli endpoint is hit, then we need to poll to wait for the user to truly login
    StartCoroutine(HasteIntegration.Instance.Client.WaitForLogin(cliResult, this.CompletedLogin));
}

private void CompletedLogin(HasteLoginResult loginResult)
{
    // Store the token and expiration date in player prefs to prevent extra logins
    PlayerPrefs.SetString("HasteAccessToken", loginResult.access_token);
    PlayerPrefs.SetString("HasteExpiration", loginResult.expiration.ToString("yyyyMMddHHmmss"));

    if (String.IsNullOrEmpty(loginResult.access_token))
    {
        Debug.Log("An error occurred during login");
        // TODO Need to display an error message to the user here
    }
    else
    {
        StartClient();
    }

}
```

Once the authentication token is stored in PlayerPrefs, the game client should direct the user to select their payment tier or Leaderboard.

### Server

The server side integration has three primary responsibilities: retrieve list of payment tiers or leaderboards, start game play, and submit score. As a player in the Haste arcade, the first step is to select which leaderboard you wish to participate in. The higher the payment, the higher the payouts if you make it to the top of the leaderboard. The next step is to actually submit payment. This process is called 'Play' in the Haste arcade. You can compare 'Play' to inserting a quarter into the arcade machine. Finally, once the game is over, the Haste ecosystem needs to be notified of the user's score to determine if the player has made it to the leaderboard.

In order to perform these three responsibilities, the developer will first need to configure the game server with the Haste game server id and private key. You can retrieve these details by creating a game in the Haste [developer portal](https://developer.hastearcade.com). Once the developer has these values, integration can begin.

*The haste server secret should be kept secret and not stored in git, etc. Please use environment variables or another appropriate secret keeping process to maintain the integrity of your key*

The configuration of the Haste server integration is a two step process: Get Server token (JWT) and then call a configuration function.

#### Step 1
Note, the retrieving of the server token is called in `OnStartServer` or equivalent for your game server.

```csharp
public override void OnStartServer()
{
    DotEnv.Load("./.env");
    var secret = System.Environment.GetEnvironmentVariable("HASTE_SERVER_SECRET");
    var clientId = System.Environment.GetEnvironmentVariable("HASTE_SERVER_CLIENT_ID");
    var environment = System.Environment.GetEnvironmentVariable("HASTE_SERVER_ENVIRONMENT");
    StartCoroutine(HasteIntegration.Instance.Server.GetServerToken(clientId, secret, environment, GetHasteTokenCompleted));
    }
```

#### Step 2

```csharp
private void GetHasteTokenCompleted(HasteServerAuthResult result)
{
    if (result != null)
    {
        StartCoroutine(HasteIntegration.Instance.Server.ConfigureHasteServer(result, ConfigureHasteServerCompleted));
    }
}
```

Once the Haste integration is configurated for the server, the developer may then perform the three primary actions (leaderboard retrieval, play, and score). 

#### Get Leaderboards
The leaderboards are populated during the configuration process and can be retrieved with `HasteIntegration.Instance.Server.Leaderboards`.

#### Play
Play is an asyncronous operation and must include a callback. The play method requires the Players JWT from the Haste Client integration along with a leaderboard id. The leaderboard id is the `id` field from `HasteIntegration.Instance.Server.Leaderboards`.

```csharp
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

void PlayResult(HasteServerPlayResult playResult)
{
    if (playResult != null)
    {
        // handle any errors that occur in the call to play.
        // these errrors include messages like 'User does not have sufficient funds', etc.
        if (!string.IsNullOrEmpty(playResult.message))
        {
            RpcSetError(playResult.message);
        }
        else
        {
            PlayerPrefs.SetString("HastePlayId", playResult.id); // Store the play id for later use in the call to .Score
            StartCoroutine(((HasteMirrorNetManager)NetworkManager.singleton).StartGameInstanceForPlayer(GetComponent<NetworkIdentity>().connectionToClient));
            RpcStartGame(); // start the game
        }
    }
}
```

#### Score
The score method requires an id from the call to `.Play`, a leaderboard id, and a score.

```csharp
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
            RpcEndGame($"Your score was {score} and you placed {scoreResult.leaderRank} on the leaderboard.");
        }
    }
}
```

## License

The UnityHasteIntegrationSample repository is currently licensed under [MIT](https://github.com/haste-arcade/UnityHasteIntegrationSample/blob/main/LICENSE)

## Contributing

If you are a developer looking to contribute to the Haste ecosystem please review our [Contributing Guidelines](https://github.com/haste-arcade/UnityHasteIntegrationSample/blob/main/CONTRIBUTING.md)

## Authors

- Keith LaForce ([rallieon](https://github.com/rallieon/))