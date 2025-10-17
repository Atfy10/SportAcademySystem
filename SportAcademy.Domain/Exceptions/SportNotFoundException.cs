using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class SportNotFoundException : IdNotFoundException
	{
		static readonly string _entity = nameof(Sport);

		public SportNotFoundException(string id) : base(_entity, id)
		{

		}
		public SportNotFoundException(string id, Exception innerException) : base(_entity, id, innerException)
		{

		}
	}
}