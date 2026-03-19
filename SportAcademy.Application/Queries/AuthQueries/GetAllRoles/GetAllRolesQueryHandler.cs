using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.AuthQueries.GetAllRoles;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<List<string>>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public GetAllRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<List<string>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllRolesAsync(cancellationToken);
        return Result<List<string>>.Success(roles, _operation);
    }
}
