using SportAcademy.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
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
        
        public Guid? UserId
        {
            get
            {
                var claim = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

                return claim is null
                    ? null
                    : Guid.Parse(claim);
            }
        }

        public Guid? TenantId
        {
            get
            {
                var claim = User?.FindFirstValue("tenant_id");

                return claim is null
                    ? null
                    : Guid.Parse(claim);
            }
        }

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
