using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SportExceptions
{
	public class SportExistsException : Exception
	{
		static readonly string _message = "A sport with the same name already exists. Please choose a different name.";
		public SportExistsException() : base(_message)
		{
		}
		public SportExistsException(Exception innerException) : base(_message, innerException)
		{
		}
	}
}
