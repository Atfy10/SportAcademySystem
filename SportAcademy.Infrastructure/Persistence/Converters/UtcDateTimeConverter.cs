using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SportAcademy.Infrastructure.Persistence.Converters
{
    // SQL Server's datetime2 has no offset - EF Core always materializes a DateTime read back
    // from it with Kind=Unspecified, regardless of whether UtcNow or (a bug's) local Now wrote
    // it. System.Text.Json then serializes an Unspecified DateTime with no trailing Z/offset,
    // which the frontend's `new Date(...)` silently reinterprets as browser-local time - so an
    // already-correct UTC value round-trips into looking wrong. Applied tenant-wide via
    // ApplicationDbContext.ConfigureConventions to every DateTime/DateTime? property.
    //
    // Write-side is a deliberate pass-through, not a "helpful" ToUniversalTime() correction: by
    // the time this exists, every write site in the codebase has been audited to already use
    // UtcNow. Silently reinterpreting a value on write would mask a future DateTime.Now
    // regression instead of surfacing it as a wrong stored value someone would notice and fix.
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter()
            : base(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }
}
