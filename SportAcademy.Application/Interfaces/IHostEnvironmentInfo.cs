namespace SportAcademy.Application.Interfaces
{
    /// <summary>
    /// What the Application layer is allowed to know about the hosting environment. The
    /// Application project doesn't reference ASP.NET Core's hosting abstractions, so the Web
    /// project implements this over <c>IHostEnvironment</c>.
    /// </summary>
    public interface IHostEnvironmentInfo
    {
        /// <summary>True only when the app is running in the "Development" environment.</summary>
        bool IsDevelopment { get; }
    }
}
