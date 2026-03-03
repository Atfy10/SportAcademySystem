using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup;
using SportAcademy.Application.Interfaces;
using System.Text;

namespace SportAcademy.Application.Services
{
    public class TraineeGroupService
    {
        private readonly ICoachRepository _coachRepository;

        public TraineeGroupService(ICoachRepository coachRepository)
        {
            _coachRepository = coachRepository;
        }

        public async Task<string> GenerateTraineeGroupNameAsync(CreateTraineeGroupCommand traineeGroup)
        {
            if (!string.IsNullOrWhiteSpace(traineeGroup.Name))
                return traineeGroup.Name;

            var coach = await _coachRepository.GetByIdAsync(traineeGroup.CoachId);

            string fullName = $"{coach?.Employee?.FirstName} {coach?.Employee?.LastName}".Trim();
            if (coach == null || coach.Sport == null)
                return "Unknown Group";

            var sportName = coach.Sport.Name;
            var levelName = traineeGroup.SkillLevel;
            var coachName = GetInitials(fullName);

            return $"{sportName} - {levelName} - {coachName}";
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = new StringBuilder();
            foreach (var part in parts)
            {
                initials.Append(char.ToUpper(part[0]));
            }
            return initials.ToString();
        }
    }
}
