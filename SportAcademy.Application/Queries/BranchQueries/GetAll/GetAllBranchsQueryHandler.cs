using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.BranchQueries.GetAll
{
	public class GetAllBranchsQueryHandler : IRequestHandler<GetAllBranchsQuery, Result<List<BranchDto>>>
	{
		private readonly IBranchRepository _branchRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.GetAll.ToString();

		public GetAllBranchsQueryHandler(IBranchRepository branchRepository, IMapper mapper)
		{
			_branchRepository = branchRepository;
			_mapper = mapper;
		}
		public async Task<Result<List<BranchDto>>> Handle(GetAllBranchsQuery request, CancellationToken cancellationToken)
		{
			var branches = await _branchRepository.GetAllAsync(cancellationToken)??[];
			var branchesDto = _mapper.Map<List<BranchDto>>(branches);
			return Result<List<BranchDto>>.Success(branchesDto, _operationType);
		}
	}
}
