using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Globalization;

public class MainMenu : MonoBehaviour
{
    public string startScene;

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
            SceneManager.LoadScene(startScene);
        }

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
            SceneManager.LoadScene(startScene);
        }

    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
