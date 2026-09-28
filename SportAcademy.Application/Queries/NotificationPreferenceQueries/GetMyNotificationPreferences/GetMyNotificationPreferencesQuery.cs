using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;

namespace SportAcademy.Application.Queries.NotificationPreferenceQueries.GetMyNotificationPreferences;

public record GetMyNotificationPreferencesQuery : IRequest<Result<List<NotificationPreferenceDto>>>;
