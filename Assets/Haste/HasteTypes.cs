using System;

public class HasteError
{
  public string message;
}
public class HasteLoginResult : HasteError
{
  public string access_token;
  public DateTime expiration;
}

public class HasteCliResult : HasteError
{
  public string browserUrl;
  public string requestorId;
  public string cliUrl;
  public string token;
}
public class HasteServerAuthResult : HasteError
{
  public string arcadeId;
  public string gameId;
  public string access_token;
  public int expires_in;
}

public class HasteServerPlayResult : HasteError
{
  public string id;
}

public class HasteGameLeader
{
  public string playerId;
  public string avatar;
  public string name;
  public string score;
}

public class HasteServerLeaderResults : HasteError
{
  public string id;
  public string name;
  public HasteGameLeader[] leaders;
}

public class HasteServerTopScore : HasteError
{
  public string userId;
  public float value;
  public string paymentHandle;
}

public class HasteServerScoreResult : HasteError
{
  public string id;
  public bool isWinner;

  public int score;

  public int leaderRank;
}
public class HasteAllLeaderboards : HasteError
{
  public HasteLeaderboardDetail[] leaderboards;
}
public class HasteLeaderboardDetail : IEquatable<HasteLeaderboardDetail>
{
  public string id;
  public string name;
  public int cost;

  public bool Equals(HasteLeaderboardDetail other)
  {
    return this.id == other.id;
  }
}
