namespace Elearning.Common.Domain;

public class TokenModel
{
  public string access_token { get; set; }
  public int expires_in_access_token { get; set; }
  public string refresh_token { get; set; }
  public int expires_in_refresh_token { get; set; }
}