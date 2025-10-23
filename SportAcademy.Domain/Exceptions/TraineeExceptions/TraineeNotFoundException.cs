using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions.TraineeExceptions
{
	public class TraineeNotFoundException : IdNotFoundException
	{
		static readonly string _entity = nameof(Trainee);

		public TraineeNotFoundException(string id) : base(_entity, id)
		{
		}
		public TraineeNotFoundException(string id, Exception innerException) : base(_entity, id, innerException)
		{
		}
	}
}
