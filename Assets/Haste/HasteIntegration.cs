using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using JWT;

public class HasteCliResult
{
    public string browserUrl;
    public string requestorId;
    public string cliUrl;
    public string token;
}

public class HasteLoginResult
{
    public string access_token;
    public DateTime expiration;
}

public class JwtToken
{
    public long exp { get; set; }
}

public class JWTService
  {
    public DateTime GetExpiryTimestamp(string accessToken)
    {
        var decoded = JsonWebToken.Decode(accessToken, "", false);
        Debug.Log("The decoded value is " + decoded);

        var results = JsonConvert.DeserializeObject<JwtToken>(decoded);
        return DateTimeOffset.FromUnixTimeSeconds(results.exp).LocalDateTime;
    }
  }

public class HasteIntegration
{
    public static async UniTask<HasteLoginResult> Login()
    {
        var jwtService = new JWTService();
        Debug.Log("Starting haste auth flow");
        var data = new Dictionary<string, string>();
        var authServerUrl = "https://authservice.hastearcade.com"; // These should be production
        var authClientUrl = "https://authclient.hastearcade.com";
        var completed = false;
        HasteLoginResult loginResults = new HasteLoginResult();

        // make the initial request to the cli service to initiate a login
        data.Add("description", $"{SystemInfo.operatingSystem} - {SystemInfo.deviceName}");
        UnityWebRequest www = UnityWebRequest.Post($"{authServerUrl}/cli", data);
        await www.SendWebRequest();

        Debug.Log("after send request");
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            return loginResults;
        }

        // parse the results and then open the browser for authentication
        var hasteCliResults = JsonConvert.DeserializeObject<HasteCliResult>(www.downloadHandler.text);
        var browserUrl = $"{authClientUrl}{hasteCliResults.browserUrl}";
        var cliUrl = $"{authServerUrl}{hasteCliResults.cliUrl}/{hasteCliResults.requestorId}";
        Application.OpenURL(browserUrl);

        // loop until the user logs in
        while(!completed)
        {
            Debug.Log("looping");
            await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);
            UnityWebRequest checkLoggedIn = UnityWebRequest.Get($"{cliUrl}");
            checkLoggedIn.SetRequestHeader("Authorization", $"Bearer {hasteCliResults.token}");
            await checkLoggedIn.SendWebRequest();

            if (checkLoggedIn.result == UnityWebRequest.Result.Success)
            {
                completed = true;
                loginResults = JsonConvert.DeserializeObject<HasteLoginResult>(checkLoggedIn.downloadHandler.text);
            }
        }

        Debug.Log("finished");

        Debug.Log(loginResults.access_token);

        var expiration = jwtService.GetExpiryTimestamp(loginResults.access_token);
        loginResults.expiration = expiration;
        Debug.Log("Expiration is ");
        Debug.Log(expiration);
        return loginResults;
    }
}


public static class ExtensionMethods
{
    public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += obj => { tcs.SetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }
}