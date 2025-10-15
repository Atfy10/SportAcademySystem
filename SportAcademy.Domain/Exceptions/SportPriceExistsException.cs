
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SportPriceExistsException : Exception
	{
		static readonly string _message = "A price record for this branch, sport and subscription already exists.";

		public SportPriceExistsException() : base(_message)
		{

		}
		public SportPriceExistsException(Exception innerException) : base(_message, innerException)
		{

		}

	}
}
