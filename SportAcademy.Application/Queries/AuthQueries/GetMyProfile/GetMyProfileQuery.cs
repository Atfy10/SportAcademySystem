using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AppUserDtos;

namespace SportAcademy.Application.Queries.AuthQueries.GetMyProfile;

public record GetMyProfileQuery : IRequest<Result<MyProfileDto>>;
