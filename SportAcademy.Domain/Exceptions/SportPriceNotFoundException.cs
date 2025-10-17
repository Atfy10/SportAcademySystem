using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SportPriceNotFoundException : IdNotFoundException
	{
		static readonly string _entity = nameof(SportPrice);
		public SportPriceNotFoundException(string id) : base(_entity, id) { }

		public SportPriceNotFoundException(string id, Exception innerException)
			: base(_entity, id, innerException) { }
	}
	
}
