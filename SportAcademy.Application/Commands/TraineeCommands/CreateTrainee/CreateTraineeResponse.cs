namespace SportAcademy.Application.Commands.Trainees.CreateTrainee
{
    public class CreateTraineeResponse
    {
        public int TraineeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
