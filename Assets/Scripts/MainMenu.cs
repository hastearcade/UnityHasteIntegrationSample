using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Globalization;
using Mirror;
using Mirror.Examples.MultipleAdditiveScenes;

public enum Role
{
    Server,
    Client
}

public class MainMenu : MonoBehaviour
{
    public Role Role;

    void Start()
    {
        var clientUI = GameObject.Find("ClientUI");
        var serverUI = GameObject.Find("ServerUI");
        if (Role == Role.Server)
        {
            Debug.Log(clientUI);
            if (clientUI != null)
            {
                clientUI.SetActive(false);
                serverUI.SetActive(true);
            }
            NetworkManager.singleton.StartServer();
        }
        else
        {
            if (serverUI != null)
            {
                serverUI.SetActive(false);
                clientUI.SetActive(true);
            }
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
            StartCoroutine(HasteIntegration.Instance.Client.Login(this.CompletedLoginInit));
        }
        else
        {
            StartClient();
        }

    }

    private void StartClient()
    {
        SceneManager.LoadScene("LeaderboardSelection");
        NetworkManager.singleton.StartClient();
    }

    private void CompletedLoginInit(HasteCliResult cliResult)
    {
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
