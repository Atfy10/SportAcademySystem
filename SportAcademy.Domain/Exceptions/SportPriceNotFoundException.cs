using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SportPriceNotFoundException : Exception
	{
		static readonly string _message = "We couldn’t find a matching sport price. Please check your details and try again.";
		public SportPriceNotFoundException() : base(_message)
		{
		}
		public SportPriceNotFoundException(Exception innerException) : base(_message, innerException)
		{
		}
	}
	
}
