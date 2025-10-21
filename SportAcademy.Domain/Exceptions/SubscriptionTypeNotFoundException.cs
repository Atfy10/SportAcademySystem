using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SubscriptionTypeNotFoundException : IdNotFoundException
	{
		static readonly string _entity = nameof(SubscriptionType);

		public SubscriptionTypeNotFoundException(string id) : base(_entity, id) { }
		public SubscriptionTypeNotFoundException(string id, Exception innerException) 
			: base(_entity, id, innerException) { }
	}
}
