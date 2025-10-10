using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SportAcademy.Application.Commands.BranchCommands.CreateBranch;
using SportAcademy.Application.Commands.BranchCommands.UpdateBranch;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.BranchProfile
{
	public class BranchProfile : AutoMapper.Profile
	{
		public BranchProfile()
		{
			CreateMap<Branch, CreateBranchCommand>().ReverseMap();
			CreateMap<Branch, BranchDto>().ReverseMap();
			CreateMap<Branch,UpdateBranchCommand>().ReverseMap();
		}

	}
}
