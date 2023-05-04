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
		private const char SALT_SEPARATOR = '$';

		public ManagerController(Mediatek mediatek) : base(mediatek, "responsable") { }

		public async Task<bool> VerifyAuth(string username, string password)
		{
			Manager manager = await this.Get(username);

			if (manager == null)
			{
				return false;
			}

			string[] parts = manager.Password.Split(SALT_SEPARATOR);

			string salt = parts[0];
			string b64Hash = parts[1];

			SHA256 sha = SHA256.Create();
			byte[] input = sha.ComputeHash(Encoding.ASCII.GetBytes(salt + password));
			byte[] actual = Convert.FromBase64String(b64Hash);

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

		/// <summary>Unused. I'm unsure if the exercise wants it to be this way or not.</summary>
		public async Task<MySqlCommand> Insert(string login, string password)
		{
			// SHA256 produces 32 bytes, the size of the hash will be increased by 33% by base64,
			// so it'll take 44 bytes, counting the padding equals.
			// This gives us 45 bytes already used counting both the b64 hash and the salt separator,
			// leaving us with 19 bytes for the actual salt.
			string salt = RandomString(19);

			SHA256 sha = SHA256.Create();
			byte[] saltedHash = sha.ComputeHash(Encoding.ASCII.GetBytes(salt + password));
			string b64SaltedHash = Convert.ToBase64String(saltedHash);
			string saltAndHash = salt + SALT_SEPARATOR + b64SaltedHash;

			Manager manager = new Manager(login, saltAndHash);
			return await this.Insert(manager);
		}

		private static string RandomString(int length)
		{
			ReadOnlySpan<char> SALT_CHARACTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./,?".AsSpan();
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				sb.Append(SALT_CHARACTERS[RandomNumberGenerator.GetInt32(SALT_CHARACTERS.Length)]);
			}
			return sb.ToString();
		}

		public override async Task<MySqlCommand> Insert(Manager entity)
		{
			MySqlCommand cmd = new MySqlCommand(
				"INSERT INTO responsable (login, pwd) VALUES "
				+ "(@login, @pwd);",
				this._mediatek.GetConnection());

			cmd.Parameters.AddWithValue("login", entity.Login);
			cmd.Parameters.AddWithValue("pwd", entity.Password);

			await cmd.ExecuteNonQueryAsync();

			return cmd;
		}

		public override Task<MySqlCommand> Update(Manager entity)
		{
			throw new System.NotImplementedException();
		}
	}
}
