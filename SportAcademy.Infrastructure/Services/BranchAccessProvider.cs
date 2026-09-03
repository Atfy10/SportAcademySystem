using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Services;

public class BranchAccessProvider : IBranchAccessProvider
{
    private static readonly AsyncLocal<bool> _isRestricted = new();
    private static readonly AsyncLocal<IReadOnlyList<int>> _allowedBranchIds = new();

    public bool IsRestricted => _isRestricted.Value;
    public IReadOnlyList<int> AllowedBranchIds => _allowedBranchIds.Value ?? [];

    public void SetBranchAccess(bool isRestricted, IReadOnlyList<int> allowedBranchIds)
    {
        _isRestricted.Value = isRestricted;
        _allowedBranchIds.Value = allowedBranchIds;
    }
}
