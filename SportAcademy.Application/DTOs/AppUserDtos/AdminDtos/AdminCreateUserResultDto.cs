namespace SportAcademy.Application.DTOs.AppUserDtos.AdminDtos
{
    public class AdminCreateUserResultDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string GeneratedPassword { get; set; } = null!;
    }
}
