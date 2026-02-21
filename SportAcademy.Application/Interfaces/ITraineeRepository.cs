using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeRepository : IBaseRepository<Trainee, int>, IPersonRepository
    {
        Task<Trainee?> GetFullTrainee(int id, CancellationToken cancellationToken = default);
        Task<PagedData<TraineeOfSpecificDayDto>> GetAllTraineesOfSpecificDayAsync(DateTime date, PageRequest page, CancellationToken cancellationToken = default);
        Task<int> GetTraineesCountOfSpecificDayAsync(DateTime date, CancellationToken cancellationToken = default);
	}
}
