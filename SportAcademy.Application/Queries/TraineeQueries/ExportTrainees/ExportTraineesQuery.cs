using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.Queries.TraineeQueries.ExportTrainees;

public record ExportTraineesQuery(List<int> Ids) : IRequest<Result<List<TraineeExportDto>>>;
