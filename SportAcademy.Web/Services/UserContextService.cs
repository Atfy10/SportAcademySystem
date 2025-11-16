using SportAcademy.Application.Interfaces;
using System.Security.Claims;

namespace SportAcademy.Web.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _accessor;
        private ClaimsPrincipal? User => _accessor.HttpContext?.User;

        public UserContextService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string UserId =>
            User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User?.FindFirstValue("sub")!;

        public List<string> Role
        {
            get
            {
                var roles = User?.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
                if (roles == null || roles.Count == 0)
                {
                    roles = User?.FindAll("role").Select(r => r.Value).ToList();
                }
                return roles ?? new List<string>();
            }
        }

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }
}
