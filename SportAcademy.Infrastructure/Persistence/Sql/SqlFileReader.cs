namespace SportAcademy.Infrastructure.Persistence.Sql
{
    public static class SqlFileReader
    {
        public static string Read(string relativePath)
        {
            return File.ReadAllText(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Persistence",
                    "Sql",
                    relativePath));
        }
    }
}
