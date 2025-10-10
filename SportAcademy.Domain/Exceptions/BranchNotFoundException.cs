using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class BranchNotFoundException : Exception
	{
		static readonly string _message = "We couldn’t find a matching branch. Please check your details and try again.";

		public BranchNotFoundException() : base(_message)
		{
		}
		public BranchNotFoundException(Exception innerException) : base(_message, innerException)
		{
		}
	}
}
