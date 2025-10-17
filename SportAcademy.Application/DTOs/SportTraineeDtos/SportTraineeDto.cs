using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.DTOs.SportTraineeDtos
{
	public class SportTraineeDto
	{
		public int SportId { get; set; }
		public int TraineeId { get; set; }
		public string SkillLevel { get; set; } = null!;
		public string SportName { get; set; } = null!;
		public string TraineeName { get; set; } = null!;
	}

}
