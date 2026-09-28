using SportAcademy.Application.Interfaces;

namespace SportAcademy.Web.Services
{
    /// <summary>Exposes just the environment facts the Application layer may act on.</summary>
    public class HostEnvironmentInfo : IHostEnvironmentInfo
    {
        private readonly IHostEnvironment _environment;

        public HostEnvironmentInfo(IHostEnvironment environment) => _environment = environment;

        public bool IsDevelopment => _environment.IsDevelopment();
    }
}
