using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SportNotFoundException : Exception
	{
		static readonly string _message = "We couldn’t find a Sport with that id. Please check and try again.";

		public SportNotFoundException() : base(_message)
		{

		}
		public SportNotFoundException(Exception innerException) : base(_message, innerException)
		{

		}
	}
}