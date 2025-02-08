using System.Reflection;


namespace GenAIService.EndpointHandlers;

public static class IEndpointRouteBuilderExtensions
{
  public static void MapEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
  {
    var endpointRouteHandlerInterfaceType = typeof(IEndpointRouteHandler);
    var endpointRouteHandlerTypes = assembly.GetTypes().Where(t => 
      t.IsClass 
      && !t.IsAbstract
      && !t.IsGenericType
      && t.GetConstructor(Type.EmptyTypes) != null
      && endpointRouteHandlerInterfaceType.IsAssignableFrom(t));
    foreach (var endpointRouteHandlerType in endpointRouteHandlerTypes)
    {
      var instantiatedType = (IEndpointRouteHandler) Activator.CreateInstance(endpointRouteHandlerType)!;
      instantiatedType.MapEndpoints(app);
    }
  }

  private static HashSet<string> PermissionsSet = new HashSet<string>();
}