using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IUserContextService
    {
        public Guid? UserId { get; }
        public Guid? TenantId { get; }
        public List<string> Role { get; }
        public bool IsAuthenticated { get; }

        /// <summary>Caller's remote IP, if resolvable. Null outside an HTTP request (a
        /// background service, a test host with no HttpContext).</summary>
        public string? IpAddress { get; }

        /// <summary>Caller's User-Agent header, if present.</summary>
        public string? UserAgent { get; }

        /// <summary>Set only when the current request is running under a SuperAdmin
        /// impersonation session - see JwtTokenService.GenerateImpersonationToken and
        /// ImpersonationGuardMiddleware.</summary>
        public Guid? ImpersonationGrantId { get; }
    }
}
