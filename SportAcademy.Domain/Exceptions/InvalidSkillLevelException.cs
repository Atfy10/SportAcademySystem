using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class InvalidSkillLevelException : Exception
	{
		static readonly string _message = "The selected SkillLevel option is not valid. Please choose a valid option.";

		public InvalidSkillLevelException() : base(_message)
		{

		}
		public InvalidSkillLevelException(Exception innerException) : base(_message, innerException)
		{

		}
	}
}
