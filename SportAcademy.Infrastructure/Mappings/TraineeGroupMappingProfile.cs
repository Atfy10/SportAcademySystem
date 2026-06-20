using AutoMapper;
using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Mappings
{
    public class TraineeGroupMappingProfile : AutoMapper.Profile
    {
        public TraineeGroupMappingProfile()
        {
            CreateMap<TraineeGroup, TraineeGroupDetailDto>()
                .ConstructUsing((src, _) => new TraineeGroupDetailDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    SkillLevel = src.SkillLevel,
                    Gender = src.Gender,
                    MaximumCapacity = src.MaximumCapacity,
                    DurationInMinutes = src.DurationInMinutes,
                    SportName = src.Coach.Sport.Name,
                    CoachName = src.Coach.Employee.FirstName,
                    BranchName = src.Branch.Name,
                    TraineesCount = src.Enrollments.Count,
                    Schedules = src.GroupSchedules
                        .Select(gs => new GroupScheduleDto
                        {
                            Id = gs.Id,
                            DayOfWeek = gs.Day,
                            StartTime = gs.StartTime,
                            EndTime = gs.StartTime.Add(TimeSpan.FromMinutes(src.DurationInMinutes))
                        }).ToList()
                });

            CreateMap<TraineeGroup, TraineeGroupCardDto>()
                .ConstructUsing((src, _) => new TraineeGroupCardDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    SportName = src.Coach.Sport.Name,
                    CoachName = src.Coach.Employee.FirstName,
                    BranchName = src.Branch.Name,
                    DurationInMinutes = src.DurationInMinutes,
                    TraineesCount = src.Enrollments.Count,
                    Schedules = src.GroupSchedules
                        .Select(gs => new GroupSchedulesTimesDto
                        {
                            DayOfWeek = gs.Day,
                            StartTime = gs.StartTime
                        }).ToList()
                });

            CreateMap<TraineeGroup, TraineeGroupDto>()
                .ConstructUsing(src => new TraineeGroupDto(
                    src.Id,
                    src.SkillLevel,
                    src.MaximumCapacity,
                    src.DurationInMinutes,
                    src.Gender,
                    src.BranchId,
                    src.CoachId
                ));

            CreateMap<TraineeGroup, ListTraineeGroupDto>()
                .ConstructUsing((src, _) => new ListTraineeGroupDto(
                    src.Id,
                    src.Coach.Sport.Name,
                    src.Coach.Employee.FirstName,
                    src.Branch.Name,
                    src.DurationInMinutes,
                    src.Enrollments.Count,
                    src.GroupSchedules
                        .Select(gs => new GroupScheduleItemDto
                        {
                            DayOfWeek = gs.Day.ToString(),
                            StartTime = gs.StartTime.ToString("HH:mm:ss")
                        }).ToList()));

            CreateMap<TraineeGroup, TraineeGroupDropdownDto>()
                .ConstructUsing(src => new TraineeGroupDropdownDto(src.Id, src.Name));

            CreateMap<TraineeGroup, GroupsViewDto>()
                .ConstructUsing((src, _) => new GroupsViewDto
                {
                    TraineeGroupId = src.Id,
                    SkillLevel = src.SkillLevel,
                    MaximumCapacity = src.MaximumCapacity,
                    DurationInMinutes = src.DurationInMinutes,
                    Gender = src.Gender,
                    BranchName = src.Branch.Name,
                    CoachName = src.Coach.Employee.FirstName
                });
        }
    }
}
