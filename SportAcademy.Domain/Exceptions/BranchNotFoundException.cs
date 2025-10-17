using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Domain.Exceptions
{
	public class BranchNotFoundException : IdNotFoundException
	{
		static readonly string _entity = nameof(Branch);

		public BranchNotFoundException(string id) : base(_entity, id) { }

		public BranchNotFoundException(string id, Exception innerException)
			: base(_entity, id, innerException) { }
	}
}
