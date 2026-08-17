using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantSettingsOptions;

public class GetTenantSettingsOptionsQueryHandler : IRequestHandler<GetTenantSettingsOptionsQuery, Result<TenantSettingsOptionsDto>>
{
    private readonly string _operation = OperationType.Get.ToString();

    public Task<Result<TenantSettingsOptionsDto>> Handle(GetTenantSettingsOptionsQuery request, CancellationToken ct)
    {
        var result = new TenantSettingsOptionsDto
        {
            Timezones =
            [
                "Asia/Kuwait", "UTC", "Asia/Riyadh", "Asia/Dubai", "Asia/Bahrain", "Asia/Qatar",
                "Asia/Muscat", "Asia/Baghdad", "Asia/Tehran", "Europe/London", "America/New_York",
                "America/Los_Angeles", "Europe/Paris", "Europe/Berlin", "Asia/Tokyo"
            ],
            Languages =
            [
                new LanguageOption("ar-KW", "Arabic (Kuwait)"),
                new LanguageOption("en-US", "English (US)"),
                new LanguageOption("ar-SA", "Arabic (Saudi Arabia)"),
                new LanguageOption("en-GB", "English (UK)")
            ],
            Currencies =
            [
                new CurrencyOption("KWD", "\u062f.\u0643"),
                new CurrencyOption("USD", "$"),
                new CurrencyOption("SAR", "\u0631.\u0633"),
                new CurrencyOption("EUR", "\u20ac"),
                new CurrencyOption("AED", "\u062f.\u0625"),
                new CurrencyOption("BHD", "\u0628.\u0639")
            ],
            DateFormats = ["dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd.MM.yyyy"],
            TimeFormats = ["HH:mm", "hh:mm tt", "HH:mm:ss"]
        };

        return Task.FromResult(Result<TenantSettingsOptionsDto>.Success(result, _operation));
    }
}
