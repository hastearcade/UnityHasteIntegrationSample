using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasteServerIntegration : HasteRequestBase
{
    private string _apiUrl = "https://api.hastearcade.com";
    private DateTime _tokenExpiration = DateTime.MinValue;
    private HasteServerAuthResult _configuration;
    public HasteLeaderboardDetail[] Leaderboards { get; set; }

    public IEnumerator GetHasteLeaderboards(System.Action<HasteAllLeaderboards> callback)
    {
        var path = $"/arcades/{_configuration.arcadeId}/developergames/{_configuration.gameId}";
        yield return this.GetRequest<HasteAllLeaderboards>($"{_apiUrl}{path}", callback, _configuration.access_token);
    }

    public IEnumerator ConfigureHasteServer(HasteServerAuthResult serverAuthResult, System.Action<HasteAllLeaderboards> leaderboardCallback)
    {
        _configuration = serverAuthResult;
        TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        _tokenExpiration = DateTimeOffset.FromUnixTimeSeconds((long)(span.TotalSeconds + serverAuthResult.expires_in)).LocalDateTime;
        yield return GetHasteLeaderboards(leaderboardCallback);
    }

    public IEnumerator GetServerToken(string hasteServerClientId, string hasteServerSecret, string hasteServerEnvironment, System.Action<HasteServerAuthResult> callback)
    {
        // first you need to get a token 
        var path = "/oauth/writetoken";
        var data = new Dictionary<string, string>();
        data.Add("clientId", hasteServerClientId);
        data.Add("clientSecret", hasteServerSecret);
        data.Add("environment", hasteServerEnvironment);

        yield return this.PostRequest<HasteServerAuthResult>($"{_apiUrl}{path}", data, callback);
    }

    public IEnumerator Play(string jwt, string leaderboardId, System.Action<HasteServerPlayResult> callback)
    {
        var jwtService = new JWTService();
        var playerId = jwtService.GetPlayerId(jwt);

        // first you need to get a token 
        var path = $"/arcades/{_configuration.arcadeId}/games/{_configuration.gameId}/play";
        var data = new Dictionary<string, string>();
        data.Add("playerId", playerId);
        data.Add("leaderboardId", leaderboardId);
        yield return this.PostRequest<HasteServerPlayResult>($"{_apiUrl}{path}", data, callback, _configuration.access_token);
    }

    public IEnumerator Score(string score, string playId, string leaderboardId, System.Action<HasteServerScoreResult> callback)
    {
        // first you need to get a token 
        var path = $"/arcades/{_configuration.arcadeId}/games/{_configuration.gameId}/score";
        var data = new Dictionary<string, string>();
        data.Add("playId", playId);
        data.Add("leaderboardId", leaderboardId);
        data.Add("score", score);
        yield return this.PostRequest<HasteServerScoreResult>($"{_apiUrl}{path}", data, callback, _configuration.access_token);
    }
}