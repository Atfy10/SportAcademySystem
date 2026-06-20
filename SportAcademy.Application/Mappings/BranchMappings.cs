using SportAcademy.Application.Commands.BranchCommands.CreateBranch;
using SportAcademy.Application.Commands.BranchCommands.UpdateBranch;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings
{
    public static class BranchMappings
    {
        public static Branch ToBranch(this CreateBranchCommand cmd)
        {
            return Branch.Create(
                cmd.Name,
                cmd.City,
                cmd.Country,
                cmd.PhoneNumber,
                cmd.Email,
                cmd.CoX,
                cmd.CoY);
        }

        public static void ApplyUpdate(this Branch branch, UpdateBranchCommand cmd)
        {
            branch.Update(
                cmd.Name,
                cmd.City,
                cmd.Country,
                cmd.PhoneNumber,
                cmd.Email,
                cmd.CoX,
                cmd.CoY,
                cmd.IsActive);
        }

        public static BranchDto ToDto(this Branch branch)
        {
            return new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                City = branch.City,
                Country = branch.Country,
                PhoneNumber = branch.PhoneNumber,
                Email = branch.Email,
                CoX = branch.CoX,
                CoY = branch.CoY,
                IsActive = branch.IsActive
            };
        }
    }
}
