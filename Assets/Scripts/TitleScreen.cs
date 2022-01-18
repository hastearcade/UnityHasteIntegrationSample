using System;
using UnityEngine;
using System.Globalization;
using Mirror;

public enum Role
{
    Server,
    Client
}

public class TitleScreen : MonoBehaviour
{
    public Role Role;

    void Start()
    {
        // Based on whether or not the application is the client or server
        // show the correct UI elements. Server has no user interaction capabilities
        var clientUI = GameObject.Find("ClientUI");
        var serverUI = GameObject.Find("ServerUI");
        if (Role == Role.Server)
        {
            clientUI.SetActive(false);
            serverUI.SetActive(true);
            NetworkManager.singleton.StartServer();
        }
        else
        {
            serverUI.SetActive(false);
            clientUI.SetActive(true);
        }
    }

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
            StartCoroutine(HasteIntegration.Instance.Client.Login(this.CompletedLoginInit));
        }
        else
        {
            // The user is already logged in
            StartClient();
        }

    }

    private void StartClient()
    {
        // Active the network client, which will create a player on the server.
        // The player is required to perform any kind of commands or RPCS
        // which are done in the Leaderboard selection
        NetworkManager.singleton.StartClient();

        // Activate the appropriate UI (leaderboard selection)
        var titlePanel = GameObject.Find("TitleScreen");
        var leaderboardPanel = GameObject.Find("LeaderboardSelection");
        var finalScreen = GameObject.Find("FinalScoreScreen");

        titlePanel.SetActive(false);
        finalScreen.SetActive(false);
        leaderboardPanel.SetActive(true);
    }

    private void CompletedLoginInit(HasteCliResult cliResult)
    {
        // Once the /cli endpoint is hit, then we need to poll to wait for the user to truly login
        StartCoroutine(HasteIntegration.Instance.Client.WaitForLogin(cliResult, this.CompletedLogin));
    }
    private void CompletedLogin(HasteLoginResult loginResult)
    {
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

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
