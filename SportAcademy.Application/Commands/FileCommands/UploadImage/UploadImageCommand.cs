using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FileUploadDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FileCommands.UploadImage;

// Content is a Stream (not a raw byte[]) - the controller passes the ASP.NET Core IFormFile's
// own stream straight through rather than buffering the whole upload into memory first.
public record UploadImageCommand(
    Stream Content, string FileName, string ContentType, long Length, ImageUploadCategory Category)
    : IRequest<Result<UploadedImageDto>>;
