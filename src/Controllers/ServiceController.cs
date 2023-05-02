using MySqlConnector;
using System.Threading.Tasks;

using Mediatek.Entities;

namespace Mediatek.Controllers
{
	public class ServiceController : AbstractController<Service>
	{
		public ServiceController(Mediatek mediatek) : base(mediatek, "service") { }

		public override Task<MySqlCommand> Insert(Service entity)
		{
			throw new System.NotImplementedException();
		}

		public override Task<MySqlCommand> Update(Service entity)
		{
			throw new System.NotImplementedException();
		}
	}
}
