using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FileUploadDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FileCommands.UploadImage;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, Result<UploadedImageDto>>
{
    private readonly IFileStorageService _fileStorage;
    private readonly string _operation = OperationType.Add.ToString();

    public UploadImageCommandHandler(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<Result<UploadedImageDto>> Handle(UploadImageCommand request, CancellationToken ct)
    {
        var url = await _fileStorage.SaveImageAsync(
            request.Content, request.FileName, request.Category, ct);

        return Result<UploadedImageDto>.Success(
            new UploadedImageDto(url), _operation, "Image uploaded successfully.");
    }
}
