using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateTenant;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<TenantDetailResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TenantDetailResponse>> Handle(UpdateTenantCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetDetailByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result<TenantDetailResponse>.Failure(_operation, "Tenant not found.", 404);

        if (request.Name is not null) tenant.Name = request.Name;
        if (request.DisplayName is not null) tenant.DisplayName = request.DisplayName;
        if (request.Email is not null) tenant.Email = request.Email;

        if (tenant.Profile is not null)
        {
            if (request.Phone is not null) tenant.Profile.Phone = request.Phone;
            if (request.Address is not null) tenant.Profile.Address = request.Address;
            if (request.Website is not null) tenant.Profile.Website = request.Website;
            if (request.Description is not null) tenant.Profile.Description = request.Description;
        }

        if (tenant.Settings is not null)
        {
            if (request.TimeZone is not null) tenant.Settings.TimeZone = request.TimeZone;
            if (request.Language is not null) tenant.Settings.Language = request.Language;
            if (request.Currency is not null) tenant.Settings.Currency = request.Currency;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<TenantDetailResponse>.Success(tenant.ToDetailResponse(), _operation);
    }
}
