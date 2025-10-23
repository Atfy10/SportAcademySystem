using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class InvalidCategoryException : Exception
	{
		static readonly string _message = "The selected Category option is not valid. Please choose a valid option.";
		public InvalidCategoryException() : base(_message) { }
		public InvalidCategoryException(Exception innerException)
			: base(_message, innerException) { }
	}
}
