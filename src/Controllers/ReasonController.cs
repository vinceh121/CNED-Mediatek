using MySqlConnector;
using System.Threading.Tasks;

using Mediatek.Entities;

namespace Mediatek.Controllers
{
	public class ReasonController : AbstractController<Reason>
	{
		public ReasonController(Mediatek mediatek) : base(mediatek, "motif") { }

		public override Task<MySqlCommand> Insert(Reason entity)
		{
			throw new System.NotImplementedException();
		}

		public override Task<MySqlCommand> Update(Reason entity)
		{
			throw new System.NotImplementedException();
		}
	}
}
