# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first so restore is cached independently of source changes. Tests is
# intentionally excluded - it's never part of the publish graph and pulls in test-only
# packages (Moq, EntityFrameworkCore.InMemory) that don't belong in a production image.
COPY SportAcademy.Web/*.csproj SportAcademy.Web/
COPY SportAcademy.Application/*.csproj SportAcademy.Application/
COPY SportAcademy.Domain/*.csproj SportAcademy.Domain/
COPY SportAcademy.Infrastructure/*.csproj SportAcademy.Infrastructure/
RUN dotnet restore SportAcademy.Web/SportAcademy.Web.csproj

COPY SportAcademy.Web/ SportAcademy.Web/
COPY SportAcademy.Application/ SportAcademy.Application/
COPY SportAcademy.Domain/ SportAcademy.Domain/
COPY SportAcademy.Infrastructure/ SportAcademy.Infrastructure/

# SportAcademy.Infrastructure.csproj marks the Persistence/Sql/**/*.sql scripts and the
# Localization/Resources/**/*.json files as CopyToOutputDirectory - MSBuild flows those content
# items through the ProjectReference into SportAcademy.Web's own publish output automatically,
# so no extra COPY step is needed for them here.
RUN dotnet publish SportAcademy.Web/SportAcademy.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# curl is needed for the container/compose HEALTHCHECK against /health; tzdata provides the
# IANA timezone database TimeZoneInfo.FindSystemTimeZoneById needs to resolve a tenant's
# configured zone (e.g. "Asia/Kuwait") - neither ships in the aspnet base image.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl tzdata \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Uploads and logs are volume-mounted at these paths in production (see
# deploy/docker-compose.yml) so they survive redeploys - create them up front and hand
# ownership to the base image's built-in non-root user before dropping root.
RUN mkdir -p /app/uploads /app/logs && chown -R $APP_UID:$APP_UID /app/uploads /app/logs
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SportAcademy.Web.dll"]
