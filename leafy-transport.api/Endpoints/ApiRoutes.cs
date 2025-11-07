namespace leafy_transport.api.Endpoints;

public class ApiRoutes
{
    private const string Base = "/api";
    private const string Version = "v1";
    private const string ApiBase = $"{Base}/{Version}";

    public static class Users
    {
        public const string GroupName = $"{ApiBase}/users";
    }
   
}