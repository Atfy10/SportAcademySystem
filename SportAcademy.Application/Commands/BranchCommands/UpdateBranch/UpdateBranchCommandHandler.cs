using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.BranchCommands.UpdateBranch
{
	public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result<BranchDto>>
	{
		private readonly IMapper _mapper;
		private readonly IBranchRepository _branchRepository;
		private readonly IPhoneNumberNormalizer _phoneNormalizer;
		private readonly string _operationType = OperationType.Update.ToString();

		public UpdateBranchCommandHandler(
			IMapper mapper,
			IBranchRepository branchRepository,
			IPhoneNumberNormalizer phoneNormalizer)
		{
			_mapper = mapper;
			_branchRepository = branchRepository;
			_phoneNormalizer = phoneNormalizer;
		}
		public async Task<Result<BranchDto>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
		{
			var branch = await _branchRepository.GetByIdWithSportsAsync(request.Id, cancellationToken)
				?? throw new BranchNotFoundException($"{request.Id}");

			if (!string.IsNullOrEmpty(request.Email) && request.Email != branch.Email)
			{
				var emailExists = await _branchRepository.IsEmailExistAsync(request.Email, cancellationToken);
				if (emailExists)
					throw new EmailExistException();
			}

			var newCoX = string.IsNullOrWhiteSpace(request.CoX) ? null : request.CoX;
			var newCoY = string.IsNullOrWhiteSpace(request.CoY) ? null : request.CoY;
			var coordinatesChanged = (newCoX != branch.CoX) || (newCoY != branch.CoY);
			if (coordinatesChanged && newCoX is not null && newCoY is not null)
			{
				var coordinatesExist = await _branchRepository.IsCoordinatesExistAsync(newCoX, newCoY, cancellationToken);
				if (coordinatesExist)
					throw new CoordinateExistException();
			}

			// Normalized to E.164 before the uniqueness check below - see the matching comment
			// in CreateEmployeeCommandHandler.
			var normalizedPhone = !string.IsNullOrEmpty(request.PhoneNumber)
				? await _phoneNormalizer.NormalizeAsync(request.PhoneNumber, cancellationToken)
				: request.PhoneNumber;

			var isPhoneChanged = !string.IsNullOrEmpty(normalizedPhone)
				&& normalizedPhone != branch.PhoneNumber;
            if (isPhoneChanged)
			{
				var phoneExists = await _branchRepository.IsPhoneNumberExistAsync(normalizedPhone, cancellationToken);
				if (phoneExists)
					throw new PhoneExistException();
			}

			_mapper.Map(request with { PhoneNumber = normalizedPhone }, branch);
			branch.CoX = newCoX;
			branch.CoY = newCoY;

			// NameAr == null: leave any existing translation untouched.
			// NameAr == "" (after trim): explicit clear -> delete the translation row.
			// NameAr non-empty: upsert with the given name/city/country.
			if (request.NameAr is not null)
			{
				var trimmedName = request.NameAr.Trim();
				var existingTranslation = branch.Translations.FirstOrDefault(t => t.LangCode == "ar");

				if (trimmedName.Length == 0)
				{
					if (existingTranslation is not null) branch.Translations.Remove(existingTranslation);
				}
				else
				{
					var cityAr = string.IsNullOrWhiteSpace(request.CityAr) ? null : request.CityAr.Trim();
					var countryAr = string.IsNullOrWhiteSpace(request.CountryAr) ? null : request.CountryAr.Trim();

					if (existingTranslation is not null)
					{
						existingTranslation.Name = trimmedName;
						existingTranslation.City = cityAr;
						existingTranslation.Country = countryAr;
					}
					else
					{
						branch.Translations.Add(new BranchTranslation
						{
							LangCode = "ar",
							Name = trimmedName,
							City = cityAr,
							Country = countryAr,
						});
					}
				}
			}

			cancellationToken.ThrowIfCancellationRequested();

			await _branchRepository.UpdateAsync(branch, cancellationToken);

			var branchDto = _mapper.Map<BranchDto>(branch)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			return Result<BranchDto>.Success(branchDto, _operationType);
		}
	}
}
