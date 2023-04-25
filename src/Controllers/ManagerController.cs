using MySqlConnector;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

using Mediatek.Entities;
using Mediatek.Mapper;

namespace Mediatek.Controllers
{
	public class ManagerController : AbstractController<Manager>
	{
		public ManagerController(Mediatek mediatek) : base(mediatek, "responsable") { }

		public async Task<bool> VerifyAuth(string username, string password)
		{
			Manager manager = await this.Get(username);

			if (manager == null)
			{
				return false;
			}

			SHA256 sha = SHA256.Create();
			byte[] input = sha.ComputeHash(Encoding.ASCII.GetBytes(password));
			byte[] actual = Convert.FromHexString(manager.Password);

			bool equals = CryptographicOperations.FixedTimeEquals(input, actual);

			CryptographicOperations.ZeroMemory(input);
			CryptographicOperations.ZeroMemory(actual);

			return equals;
		}

		public async Task<Manager> Get(string username)
		{
			using MySqlCommand cmd = new MySqlCommand("SELECT * FROM responsable WHERE login=@username;", this._connection);
			cmd.Parameters.AddWithValue("username", username);

			using MySqlDataReader read = await cmd.ExecuteReaderAsync();

			// if the result has no rows, avoid trying to map
			if (!await read.ReadAsync())
			{
				return null;
			}

			Manager manager = EntityMapper.MapFromRow<Manager>(read);
			return manager;
		}

		public override Task<MySqlCommand> Insert(Manager entity)
		{
			throw new System.NotImplementedException();
		}

		public override Task<MySqlCommand> Update(Manager entity)
		{
			throw new System.NotImplementedException();
		}
	}
}
