using System;

public class HasteLoginResult
{
    public string access_token;
    public DateTime expiration;
}

public class HasteCliResult
{
    public string browserUrl;
    public string requestorId;
    public string cliUrl;
    public string token;
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
