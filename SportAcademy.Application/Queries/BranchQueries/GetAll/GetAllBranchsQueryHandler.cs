using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.BranchQueries.GetAll
{
	public class GetAllBranchsQueryHandler : IRequestHandler<GetAllAttendancesQuery, Result<List<BranchDropDownListDto>>>
	{
		private readonly IBranchRepository _branchRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.GetAll.ToString();

		public GetAllBranchsQueryHandler(IBranchRepository branchRepository, IMapper mapper)
		{
			_branchRepository = branchRepository;
			_mapper = mapper;
		}

		public async Task<Result<List<BranchDropDownListDto>>> Handle(GetAllAttendancesQuery request, CancellationToken cancellationToken)
		{
			var branchesDto = await _branchRepository.GetAllBranchsBase(cancellationToken)??[];

			return Result<List<BranchDropDownListDto>>.Success(branchesDto, nameof(GetAllAttendancesQuery));
		}
	}
}
