using System.Threading.Tasks;
using System.Collections.Generic;
using MySqlConnector;

using Mediatek.Mapper;

namespace Mediatek.Controllers
{
	public abstract class AbstractController<T>
	{
		protected Mediatek _mediatek;
		protected MySqlConnection _connection;
		protected string _table;

		public AbstractController(Mediatek mediatek, string table)
		{
			this._mediatek = mediatek;
			this._connection = mediatek.GetConnection();
			this._table = table;
		}

		public async IAsyncEnumerable<T> FetchAll()
		{
			// table name can't be safetly inserted using a prepared statement
			// since it's only in-code constants it should be fine
			using MySqlCommand cmd = new MySqlCommand("SELECT * FROM " + this._table + ";", this._connection);

			using MySqlDataReader read = await cmd.ExecuteReaderAsync();
			while (await read.ReadAsync())
			{
				yield return EntityMapper.MapFromRow<T>(read);
			}
		}

		public abstract Task<MySqlCommand> Insert(T entity);

		public abstract Task<MySqlCommand> Update(T entity);
	}
}
