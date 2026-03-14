using SportAcademy.Application.DTOs.AppUserDtos.AdminDtos;

namespace SportAcademy.Application.Interfaces
{
    public interface IAdminQueries
    {
        Task<IReadOnlyList<AdminBasicDto>> GetAdminsAsync();

        Task<AdminBasicDto?> GetAdminByUsernameAsync(string username);
    }
}
