using Microsoft.AspNetCore.Routing;

namespace API.MinimalApi {
    public interface IMinimalApiStrategy {
        void MapEndpoints(RouteGroupBuilder group);
    }
}
