namespace leafy_transport.api.Interfaces;

public interface IModule
{
    void RegisterEndpoints(IEndpointRouteBuilder app);
}