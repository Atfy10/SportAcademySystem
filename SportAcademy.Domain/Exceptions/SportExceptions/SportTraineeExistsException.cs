using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.SportExceptions
{
	public class SportTraineeExistsException : IdNotFoundException
	{
		static readonly string _entity = nameof(SportTrainee);

		public SportTraineeExistsException(string id) : base(_entity, id)
		{

		}
		public SportTraineeExistsException(string id, Exception innerException) : base(_entity, id, innerException)
		{

		}

	}
}
