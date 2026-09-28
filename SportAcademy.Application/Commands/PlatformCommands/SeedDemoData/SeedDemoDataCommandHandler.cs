using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.SeedDemoData;

// Three outcomes, each with its own status code and stable code:
//   seeded          200  (no code)
//   already seeded  409  errors.demoData.alreadySeeded
//   not development 403  errors.demoData.notDevelopment
public class SeedDemoDataCommandHandler : IRequestHandler<SeedDemoDataCommand, Result<SeedDemoDataDto>>
{
    private readonly IHostEnvironmentInfo _environment;
    private readonly IDemoDataSeeder _seeder;
    private readonly string _operation = OperationType.Add.ToString();

    public SeedDemoDataCommandHandler(IHostEnvironmentInfo environment, IDemoDataSeeder seeder)
    {
        _environment = environment;
        _seeder = seeder;
    }

    public async Task<Result<SeedDemoDataDto>> Handle(SeedDemoDataCommand request, CancellationToken ct)
    {
        // Checked before anything is touched: outside Development this endpoint must be inert.
        if (!_environment.IsDevelopment)
        {
            return Failure(
                403,
                "Demo data can only be seeded in the Development environment.",
                DemoDataErrorCodes.NotDevelopment);
        }

        var seeded = await _seeder.SeedAsync(ct);
        request.ResolvedTenantId = seeded.TenantId;

        if (seeded.Outcome == DemoSeedOutcome.AlreadySeeded)
        {
            return Failure(
                409,
                "Demo data is already seeded.",
                DemoDataErrorCodes.AlreadySeeded);
        }

        return Result<SeedDemoDataDto>.Success(
            new SeedDemoDataDto(seeded.TenantSlug!, seeded.OwnerUserName!),
            _operation,
            "Demo data seeded successfully.");
    }

    private Result<SeedDemoDataDto> Failure(int statusCode, string message, string code)
    {
        var result = Result<SeedDemoDataDto>.Failure(_operation, message, statusCode);
        result.Code = code;
        return result;
    }
}
