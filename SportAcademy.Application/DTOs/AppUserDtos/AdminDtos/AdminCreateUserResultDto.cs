namespace SportAcademy.Application.DTOs.AppUserDtos.AdminDtos
{
    public class AdminCreateUserResultDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string GeneratedPassword { get; set; } = null!;
    }
}
