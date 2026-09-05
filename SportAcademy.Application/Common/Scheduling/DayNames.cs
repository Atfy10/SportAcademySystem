namespace SportAcademy.Application.Common.Scheduling
{
    /// <summary>
    /// Turns day-of-week values into the names this API exposes them as ("Sunday"), in week
    /// order.
    /// <para>
    /// Always call this <b>after</b> materializing, never inside a query. Training days are
    /// persisted as a single value-converted string column, so a database provider has no
    /// collection to order or project over - `.OrderBy(d => d).Select(d => d.ToString())` inside
    /// an IQueryable fails at runtime with "The LINQ expression 'd => d' could not be
    /// translated". The lists are at most seven items, so doing it in memory costs nothing.
    /// </para>
    /// </summary>
    public static class DayNames
    {
        public static List<string> From(IEnumerable<DayOfWeek> days)
            => days.Distinct().OrderBy(d => d).Select(d => d.ToString()).ToList();
    }
}
