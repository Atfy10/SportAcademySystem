using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.BranchCommands.DeleteBranch
{
	public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result<bool>>
	{
		private readonly IBranchRepository _branchRepository;
		private readonly string _operationType = OperationType.Delete.ToString();

		public DeleteBranchCommandHandler(IBranchRepository branchRepository)
		{
			_branchRepository = branchRepository;
		}
		public async Task<Result<bool>> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
		{
			var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken)
				?? throw new BranchNotFoundException();

			cancellationToken.ThrowIfCancellationRequested();

			await _branchRepository.DeleteAsync(branch, cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();
			return Result<bool>.Success(true, _operationType);
		}
	}
}
