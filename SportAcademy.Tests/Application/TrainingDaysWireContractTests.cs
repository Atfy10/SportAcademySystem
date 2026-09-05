using System.Text.Json;
using System.Text.Json.Serialization;
using SportAcademy.Application.DTOs.TraineeGroupDtos;

namespace SportAcademy.Tests.Application;

/// <summary>
/// Locks the wire format for training days. These values cross the boundary in both directions
/// - read as day names on DTOs, written back as DayOfWeek on commands - and getting the shape
/// wrong is invisible at compile time, so it's pinned here instead. The options mirror
/// Program.cs exactly; if that registration changes, these tests are the ones that should fail.
/// </summary>
public class TrainingDaysWireContractTests
{
    private static readonly JsonSerializerOptions ApiOptions = CreateApiOptions();

    private static JsonSerializerOptions CreateApiOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        return options;
    }

    private sealed record TrainingDaysPayload(List<DayOfWeek> TrainingDays);

    [Fact]
    public void DayNames_SentByTheClient_BindToDayOfWeek()
    {
        // What the subscription form posts: the same day names the API handed it.
        var json = """{"trainingDays":["Sunday","Tuesday","Thursday"]}""";

        var payload = JsonSerializer.Deserialize<TrainingDaysPayload>(json, ApiOptions);

        Assert.NotNull(payload);
        Assert.Equal(
            new[] { DayOfWeek.Sunday, DayOfWeek.Tuesday, DayOfWeek.Thursday },
            payload!.TrainingDays);
    }

    [Fact]
    public void Integers_AreRejected()
    {
        // allowIntegerValues:false in Program.cs - this is why training days can't travel as the
        // numeric day indexes they're modelled as in JavaScript.
        var json = """{"trainingDays":[0,2,4]}""";

        Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<TrainingDaysPayload>(json, ApiOptions));
    }

    [Fact]
    public void DayPatternDto_SerializesDaysAsNames_NotCamelCasedEnums()
    {
        // Day names are the client's enums.dayOfWeek translation keys, so they must arrive
        // capitalised. Exposing raw DayOfWeek values here would emit "sunday" and the client
        // would render a missing-translation key.
        var dto = new GroupDayPatternDto([DayOfWeek.Sunday.ToString(), DayOfWeek.Tuesday.ToString()], 3);

        var json = JsonSerializer.Serialize(dto, ApiOptions);

        Assert.Contains("\"Sunday\"", json);
        Assert.Contains("\"Tuesday\"", json);
        Assert.DoesNotContain("\"sunday\"", json);
    }
}
