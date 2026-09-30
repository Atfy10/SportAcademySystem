using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Domain.Exceptions.TraineeGroupExceptions
{
    public class GroupAlreadyPrivateException : LocalizableException
    {
        public GroupAlreadyPrivateException()
            : base(
                "errors.traineeGroup.alreadyPrivate",
                "This group is already private.")
        {
        }
    }
}
