namespace SportAcademy.Domain.ValueObjects
{
    public sealed class TraineeCode
    {
        public string Value { get; }
        public int MyProperty { get; }

        private TraineeCode(string value)
        {
            Value = value;
        }

        public static TraineeCode FromString(string value)
            => new (value);

        public override string ToString() => Value;
    }
}
