using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCount
{
    public class GetEnrollmentsCountQueryHandler: IRequestHandler<GetEnrollmentsCountQuery, Result<int>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        public GetEnrollmentsCountQueryHandler(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }
        public async Task<Result<int>> Handle(GetEnrollmentsCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _enrollmentRepository.CountAsync(cancellationToken);
            return Result<int>.Success(count, "Get Enrollments Count");
        }
    }
}
