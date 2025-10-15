using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class TraineeNotFoundException : Exception
	{
		static readonly string _message = "We couldn’t find a matching trainee. Please check your details and try again.";

		public TraineeNotFoundException() : base(_message)
		{
		}
		public TraineeNotFoundException(Exception innerException) : base(_message, innerException)
		{
		}
	}
}
