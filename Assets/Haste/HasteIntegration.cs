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

public class HasteServerAuthResult
{
    public string arcadeId;
    public string gameId;
    public string access_token;
    public int expires_in;
}

public class HasteAllLeaderboards
{
    public HasteLeaderboardDetail[] leaderboards;
}
public class HasteLeaderboardDetail
{
    public string id;
    public string name;
    public int cost;
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
    public static string Jwt { get; set; }
    private static string _hasteGameServerClientId = "cKLXxdH7l0DCoZmQMOrbc1h40ao7fE5A";
    private static string _hasteGameServerSecret = "5PjsTcYvgAEmB5ju7-PBDgzQWe7vQzrF84vSoat9gr8QBEwaAzf-lnXBywIYizKC";
    private static string _environment = "nonproduction"; // valid values are nonproduction or production
    private static string _apiUrl = "https://api.hastearcade.com";
    private static DateTime tokenExpiration = DateTime.MinValue;
    private static HasteServerAuthResult configuration;
    public static HasteLeaderboardDetail[] Leaderboards { get; set; }

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

    public static async UniTask GetHasteLeaderboards()
    {
        var path = $"/arcades/{configuration.arcadeId}/developergames/{configuration.gameId}";
        Debug.Log("making a call to " + $"{_apiUrl}{path}");
        UnityWebRequest www = UnityWebRequest.Get($"{_apiUrl}{path}");
        Debug.Log(configuration.access_token);
        www.SetRequestHeader("Authorization", $"Bearer {configuration.access_token}");
        await www.SendWebRequest();

        Debug.Log("after send request");
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            return; 
        }

        // parse the results and then open the browser for authentication
        var hasteGetLeaderboardResults = JsonConvert.DeserializeObject<HasteAllLeaderboards>(www.downloadHandler.text);
        Debug.Log("Finished getting leaderboards");
        Leaderboards = hasteGetLeaderboardResults.leaderboards;
        Debug.Log(hasteGetLeaderboardResults.leaderboards.Length);
        Debug.Log(hasteGetLeaderboardResults.leaderboards[0].name);
    }

    public static async UniTask<bool> ConfigureHasteServer()
    {
        // first you need to get a token 
        var path = "/oauth/writetoken";
        var data = new Dictionary<string, string>();
        data.Add("clientId", _hasteGameServerClientId);
        data.Add("clientSecret", _hasteGameServerSecret);
        data.Add("environment", _environment);

        UnityWebRequest www = UnityWebRequest.Post($"{_apiUrl}{path}", data);
        await www.SendWebRequest();

        Debug.Log("after send request");
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            return false;
        }

        // parse the results and then open the browser for authentication
        var hasteServerAuthResults = JsonConvert.DeserializeObject<HasteServerAuthResult>(www.downloadHandler.text);
        Debug.Log("Finished server auth");
        Debug.Log(hasteServerAuthResults.access_token);
        Debug.Log(hasteServerAuthResults.arcadeId);
        Debug.Log(hasteServerAuthResults.gameId);
        Debug.Log(hasteServerAuthResults.expires_in);

        configuration = hasteServerAuthResults;

        TimeSpan span= DateTime.Now.Subtract(new DateTime(1970,1,1,0,0,0));
        tokenExpiration = DateTimeOffset.FromUnixTimeSeconds((long)(span.TotalSeconds + hasteServerAuthResults.expires_in)).LocalDateTime;

        await GetHasteLeaderboards();
        return true;
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