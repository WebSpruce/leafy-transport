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
    public static class Vehicles
    {
        public const string GroupName = $"{ApiBase}/vehicles";
    }
    public static class Invoices
    {
        public const string GroupName = $"{ApiBase}/invoices";
    }
   
}