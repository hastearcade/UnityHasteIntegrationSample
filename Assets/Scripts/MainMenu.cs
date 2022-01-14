using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Globalization;

public class MainMenu : MonoBehaviour
{
    public string startScene;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public async void Login()
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

        Debug.Log($"The expriation date is " + expirationDate.ToLongDateString());


        if (expirationDate < DateTime.Now)
        {
            var result = await HasteIntegration.Login();
            PlayerPrefs.SetString("HasteAccessToken", result.access_token);
            PlayerPrefs.SetString("HasteExpiration", result.expiration.ToString("yyyyMMddHHmmss"));

        }

        var accessToken = PlayerPrefs.GetString("HasteAccessToken");
        HasteIntegration.Jwt = accessToken;
        SceneManager.LoadScene(startScene);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
