using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.TenantCommands.UpdateTenantProfile;

public class UpdateTenantProfileCommandHandler : IRequestHandler<UpdateTenantProfileCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContext;
    private readonly IFileStorageService _fileStorage;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateTenantProfileCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserContextService userContext,
        IFileStorageService fileStorage)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _fileStorage = fileStorage;
    }

    public async Task<Result> Handle(UpdateTenantProfileCommand request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result.Failure(_operation, "Tenant ID is not available.", 400);

        var profile = await _tenantRepository.GetProfileAsync(tenantId.Value, ct);
        if (profile is null)
            return Result.Failure(_operation, "Profile not found.", 404);

        if (request.OrganizationName is not null) profile.OrganizationName = request.OrganizationName;

        // Old file cleaned up the same way UpdateMyProfileCommandHandler already does for an
        // avatar - without this, every re-upload (a tenant replacing their logo) leaves the
        // previous file behind on disk forever, since nothing else ever references it again.
        if (request.LogoUrl is not null && request.LogoUrl != profile.LogoUrl)
        {
            _fileStorage.DeleteImage(profile.LogoUrl);
            profile.LogoUrl = request.LogoUrl;
        }

        if (request.Email is not null) profile.Email = request.Email;
        if (request.Phone is not null) profile.Phone = request.Phone;
        if (request.Website is not null) profile.Website = request.Website;
        if (request.Address is not null) profile.Address = request.Address;
        if (request.TaxNumber is not null) profile.TaxNumber = request.TaxNumber;
        if (request.CommercialRegistration is not null) profile.CommercialRegistration = request.CommercialRegistration;
        if (request.Description is not null) profile.Description = request.Description;

        // One-way: only the onboarding wizard's own submit sets this, and nothing ever clears
        // it back to false again once a tenant has completed setup.
        if (request.MarkSetupComplete)
            profile.IsSetupComplete = true;

        _tenantRepository.UpdateProfile(profile);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Profile updated successfully.");
    }
}
