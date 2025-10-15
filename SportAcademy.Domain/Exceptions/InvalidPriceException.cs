using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class InvalidPriceException : Exception
	{
		static readonly string _message = "The price must be greater than zero. Please enter a valid price and try again.";
		public InvalidPriceException() : base(_message)
		{
		}
		public InvalidPriceException(Exception innerException) : base(_message, innerException)
		{
		}
	}
}
