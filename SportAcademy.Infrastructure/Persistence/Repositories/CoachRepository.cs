using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using System.Data;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class CoachRepository : BaseRepository<Coach, int>, ICoachRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CoachRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            return await _context.Coachs.CountAsync(cancellationToken);
        }
        public async Task<double?> GetAverageRatingAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Coachs
                .Select(x => (double?)x.Rate)
                .AverageAsync(cancellationToken);
        }

        public async Task<PagedData<CoachCardDto>> SearchAsync(
            string term,
            PageRequest pageReq,
            CancellationToken cancellationToken)
        {
            var offset = (pageReq.Page - 1) * pageReq.PageSize;
            var fullTextTerm = BuildFullTextTerm(term);

            var sql = @"
                SELECT 
                    c.Id,
                    e.FirstName,
                    e.LastName,
                    e.Position,
                    b.Name AS BranchName,
                    e.Email,
                    e.IsWork,
                    e.PhoneNumber,
                    (e.City + ', ' + e.Street) AS Address,
                    e.HireDate,
                    c.TotalTrainees,
                    c.SkillLevel,
                    s.Name AS Sport
                FROM Coachs c
                INNER JOIN Employees e ON c.EmployeeId = e.Id
                INNER JOIN CONTAINSTABLE(
                    Employees,
                    (FirstName, LastName),
                    @term
                ) ft ON e.Id = ft.[KEY]
                INNER JOIN Branches b ON e.BranchId = b.Id
                INNER JOIN Sports s ON c.SportId = s.Id
                ORDER BY ft.RANK DESC, c.Id ASC
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM Coachs c
                INNER JOIN Employees e ON c.EmployeeId = e.Id
                INNER JOIN CONTAINSTABLE(
                    Employees,
                    (FirstName, LastName),
                    @term
                ) ft ON e.Id = ft.[KEY];
            ";

            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var multi = await connection.QueryMultipleAsync(
                sql,
                new
                {
                    term = fullTextTerm,
                    offset,
                    pageReq.PageSize
                }
            );

            var coaches = (await multi.ReadAsync<CoachCardDto>()).ToList();

            return coaches.ToPagedData(pageReq);
        }

        private static string BuildFullTextTerm(string term)
        {
            var tokens = term
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" AND ",
                tokens.Select(t => $"\"{t}*\""));
        }
    }
}
