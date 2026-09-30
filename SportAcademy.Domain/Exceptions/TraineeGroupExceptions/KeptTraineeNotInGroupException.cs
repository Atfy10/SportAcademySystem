using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.TraineeGroupExceptions
{
    public class KeptTraineeNotInGroupException : LocalizableException
    {
        public KeptTraineeNotInGroupException()
            : base(
                "errors.traineeGroup.keptTraineeNotInGroup",
                "One or more of the selected trainees are not currently enrolled in this group.")
        {
        }
    }
}
