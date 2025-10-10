using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SportAcademy.Application.Commands.BranchCommands.CreateBranch;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings.BranchProfile
{
	public class BranchProfile : AutoMapper.Profile
	{
		public BranchProfile()
		{
			CreateMap<Branch, CreateBranchCommand>().ReverseMap();
		}

	}
}
