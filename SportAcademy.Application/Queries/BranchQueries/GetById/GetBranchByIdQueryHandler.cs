using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;

namespace SportAcademy.Application.Queries.BranchQueries.GetById
{
	public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, Result<BranchDto>>
	{
		private readonly IBranchRepository _branchRepository;
		private readonly IMapper _mapper;
		private readonly ICurrentLanguageProvider _languageProvider;
		private readonly string _operationType = OperationType.Get.ToString();

		public GetBranchByIdQueryHandler(IBranchRepository branchRepository, IMapper mapper, ICurrentLanguageProvider languageProvider)
		{
			_branchRepository = branchRepository;
			_mapper = mapper;
			_languageProvider = languageProvider;
		}

		public async Task<Result<BranchDto>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
		{
			var branch = await _branchRepository.GetByIdWithSportsAsync(request.Id, cancellationToken)
				?? throw new BranchNotFoundException($"{request.Id}");

			var branchDto = _mapper.Map<BranchDto>(branch)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			// GetByIdWithSportsAsync already loads Translations - resolve the request-language
			// override here (same fallback-to-English behavior as BranchProjections.ToCardDto,
			// just done in-memory since this handler builds its DTO via AutoMapper, not a
			// translated LINQ projection).
			var translation = branch.Translations.FirstOrDefault(t => t.LangCode == _languageProvider.Language);
			if (translation is not null)
			{
				branchDto = branchDto with
				{
					Name = translation.Name,
					City = translation.City ?? branchDto.City,
					Country = translation.Country ?? branchDto.Country,
				};
			}

			return Result<BranchDto>.Success(branchDto, _operationType);

		}
	}
}
