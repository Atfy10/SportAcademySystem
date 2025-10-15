using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SportTraineeNotFoundException: Exception
	{
		static readonly string _message = "A record for this sport and trainee not found.";

		public SportTraineeNotFoundException() : base(_message)
		{

		}
		public SportTraineeNotFoundException(Exception innerException) : base(_message, innerException)
		{

		}
	}
}
