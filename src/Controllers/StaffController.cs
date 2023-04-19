using MySqlConnector;
using System.Threading.Tasks;
using System.Collections.Generic;

using Mediatek.Entities;
using Mediatek.Mapper;

namespace Mediatek.Controllers
{
	public class StaffController : AbstractController<Staff>
	{
		public StaffController(Mediatek mediatek) : base(mediatek, "personnel") { }

		public async IAsyncEnumerable<Staff> FetchAllWithServiceLeave()
		{
			using MySqlCommand cmd = new MySqlCommand("SELECT personnel.*, service.nom AS nomservice "
				+ " FROM personnel INNER JOIN service ON personnel.idservice = service.idservice;", this._connection);

			using MySqlDataReader read = await cmd.ExecuteReaderAsync();
			while (await read.ReadAsync()) {
				yield return EntityMapper.MapFromRow<Staff>(read);
			}
		}

		public override async Task<MySqlCommand> Insert(Staff entity)
		{
			// no using here because we don't want the command to be disposed
			// before the return, otherwise business logic won't work properly
			MySqlCommand cmd = new MySqlCommand(
				"INSERT INTO personnel (nom, prenom, tel, mail, idservice) VALUES "
				+ "(@nom, @prenom, @tel, @mail, @idservice);",
				this._mediatek.GetConnection());

			cmd.Parameters.AddWithValue("nom", entity.LastName);
			cmd.Parameters.AddWithValue("prenom", entity.FirstName);
			cmd.Parameters.AddWithValue("tel", entity.Phone);
			cmd.Parameters.AddWithValue("mail", entity.Email);
			cmd.Parameters.AddWithValue("idservice", entity.IdService);

			await cmd.ExecuteNonQueryAsync();

			return cmd;
		}

		public override async Task<MySqlCommand> Update(Staff entity)
		{
			MySqlCommand cmd = new MySqlCommand(
				"UPDATE personnel SET nom=@nom, prenom=@prenom, tel=@tel, mail=@mail, idservice=@idservice "
				+ "WHERE idpersonnel=@id;",
				this._mediatek.GetConnection());

			cmd.Parameters.AddWithValue("nom", entity.LastName);
			cmd.Parameters.AddWithValue("prenom", entity.FirstName);
			cmd.Parameters.AddWithValue("tel", entity.Phone);
			cmd.Parameters.AddWithValue("mail", entity.Email);
			cmd.Parameters.AddWithValue("idservice", entity.IdService);

			await cmd.ExecuteNonQueryAsync();

			return cmd;
		}
	}
}
