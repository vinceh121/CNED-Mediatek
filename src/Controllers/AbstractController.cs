using System.Threading.Tasks;
using MySqlConnector;

namespace Mediatek.Controllers
{
	public abstract class AbstractController<T>
	{
		protected Mediatek _mediatek;
		protected MySqlConnection _connection;

		public AbstractController(Mediatek mediatek)
		{
			this._mediatek = mediatek;
			this._connection = mediatek.GetConnection();
		}

		public abstract Task<MySqlCommand> Insert(T entity);
	}
}
