using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class CoordinateExistException : Exception
	{
		static readonly string _message = "This coordinate is not available. Please choose a different one.";
		public CoordinateExistException():base(_message) { }
		public CoordinateExistException(Exception innerException) : base(_message, innerException)
		{ 

		}
	}
}
