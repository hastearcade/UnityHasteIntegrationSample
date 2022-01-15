using System;
using System.Collections;
using System.Collections.Generic;

public class HasteServerIntegration : HasteRequestBase
{
    private string _hasteGameServerClientId = "cKLXxdH7l0DCoZmQMOrbc1h40ao7fE5A";
    private string _hasteGameServerSecret = "5PjsTcYvgAEmB5ju7-PBDgzQWe7vQzrF84vSoat9gr8QBEwaAzf-lnXBywIYizKC";
    private string _environment = "nonproduction"; // valid values are nonproduction or production
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

    public IEnumerator GetServerToken(System.Action<HasteServerAuthResult> callback)
    {
        // first you need to get a token 
        var path = "/oauth/writetoken";
        var data = new Dictionary<string, string>();
        data.Add("clientId", _hasteGameServerClientId);
        data.Add("clientSecret", _hasteGameServerSecret);
        data.Add("environment", _environment);

        yield return this.PostRequest<HasteServerAuthResult>($"{_apiUrl}{path}", data, callback);
    }
}