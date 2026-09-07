namespace SportAcademy.Web.Services;

// Single source of truth for where uploaded files physically live on disk, shared between
// LocalFileStorageService (write/delete side) and Program.cs's static-file registration (read
// side) - they must always resolve to the same directory or an upload becomes either
// unreachable over HTTP or lost on container recreation.
//
// Storage:UploadsPath (appsettings.Production.json) points at the docker-compose volume mount
// (/app/uploads) so uploads survive a redeploy. The wwwroot/uploads fallback is for local
// development, where no such volume/config exists and the app's own wwwroot may not even exist
// on disk (a fresh checkout never creates it - nothing put anything there before this feature).
public static class UploadsPathResolver
{
    public static string Resolve(IConfiguration configuration, IWebHostEnvironment env) =>
        configuration["Storage:UploadsPath"]
        ?? Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
}
