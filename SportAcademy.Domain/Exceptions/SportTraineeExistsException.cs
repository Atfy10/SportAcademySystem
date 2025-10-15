using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SportTraineeExistsException : Exception
	{
		static readonly string _message = "A record for this sport and trainee already exists.";

		public SportTraineeExistsException() : base(_message)
		{

		}
		public SportTraineeExistsException(Exception innerException) : base(_message, innerException)
		{

		}

	}
}
