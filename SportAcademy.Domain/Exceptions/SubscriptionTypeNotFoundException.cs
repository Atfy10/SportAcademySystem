using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SubscriptionTypeNotFoundException : Exception
	{
		static readonly string _message = "We couldn’t find an SubscriptionType with that id. Please check and try again.";

		public SubscriptionTypeNotFoundException() : base(_message)
		{

		}
		public SubscriptionTypeNotFoundException(Exception innerException) : base(_message, innerException)
		{

		}
	}
}
