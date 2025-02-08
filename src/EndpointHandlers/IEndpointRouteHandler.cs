namespace GenAIService.EndpointHandlers;

public interface IEndpointRouteHandler
{
  public void MapEndpoints(IEndpointRouteBuilder app);
}