using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Application.DTOs.SubscriptionTypeDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.UpdateSubscriptionType
{
    public class UpdateSubscriptionTypeCommandHandler : IRequestHandler<UpdateSubscriptionTypeCommand, Result<SubscriptionTypeDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionTypeRepository _repository;

        public UpdateSubscriptionTypeCommandHandler(ISubscriptionTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<SubscriptionTypeDto>> Handle(UpdateSubscriptionTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdWithSportsAsync(request.Id, cancellationToken);

            if (entity is null)
                return Result<SubscriptionTypeDto>.Failure($"Subscription type with ID {request.Id} not found.", _operation, 404);

            if (request.Name is not null)
                entity.Name = (SubType)System.Enum.Parse(typeof(SubType), request.Name);

            if (request.DaysPerMonth.HasValue)
                entity.DaysPerMonth = request.DaysPerMonth.Value;

            if (request.NumberOfMonths.HasValue)
                entity.NumberOfMonths = request.NumberOfMonths.Value;

            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;

            if (request.IsOffer.HasValue)
                entity.IsOffer = request.IsOffer.Value;

            if (request.SportIds is not null)
            {
                entity.Sports.Clear();
                foreach (var sportId in request.SportIds)
                {
                    entity.Sports.Add(new SportSubscriptionType
                    {
                        SportId = sportId,
                        SubscriptionTypeId = entity.Id
                    });
                }
            }

            await _repository.UpdateAsync(entity, cancellationToken);

            var dto = entity.ToDto();
            return Result<SubscriptionTypeDto>.Success(dto, _operation);
        }
    }
}