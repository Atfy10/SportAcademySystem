using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Services
{
    // The one definition of an attendance rate, used for a trainee, the dashboard and reports:
    //   attended = Present + Late            (they came - late is still attendance)
    //   counted  = every marked session except Excused (an approved excuse doesn't count against them)
    //   rate     = attended / counted, as a percentage with one decimal; 0 when nothing is counted.
    // Computed in floating point - the old code divided two ints first, so every rate below
    // 100% came out as 0.
    public static class AttendanceRate
    {
        public static bool IsAttended(AttendanceStatus status)
            => status is AttendanceStatus.Present or AttendanceStatus.Late;

        public static bool IsCounted(AttendanceStatus status)
            => status != AttendanceStatus.Excused;

        public static double Percent(int attended, int counted)
            => counted <= 0 ? 0 : Math.Round(attended * 100.0 / counted, 1);
    }
}
