using MySqlConnector;
using System;
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
			using MySqlCommand cmd = new MySqlCommand("SELECT personnel.*, service.nom AS nomservice, (absences.idabsence IS NOT NULL) AS absent "
				+ "FROM personnel INNER JOIN service ON personnel.idservice = service.idservice "
				+ "LEFT JOIN absences ON personnel.idpersonnel = absences.idpersonnel "
					+ "AND absences.datedebut <= CURDATE() AND absences.datefin >= CURDATE() " // joins leaves for today written in `absent`
				+ "GROUP BY personnel.idpersonnel;", // this prevents duplicates caused when a staff has multiple leaves marked for today
				this._connection);

			using MySqlDataReader read = await cmd.ExecuteReaderAsync();
			while (await read.ReadAsync())
			{
				yield return EntityMapper.MapFromRow<Staff>(read);
			}
		}

		public async Task<Staff> Get(long id)
		{
			using MySqlCommand cmd = new MySqlCommand("SELECT * FROM personnel WHERE idpersonnel=@id;", this._connection);
			cmd.Parameters.AddWithValue("id", id);

			using MySqlDataReader read = await cmd.ExecuteReaderAsync();
			if (!await read.ReadAsync())
			{
				return null;
			}
			return EntityMapper.MapFromRow<Staff>(read);
		}

		public async Task<MySqlCommand> Delete(List<long> ids)
		{
			// apparently ADO.NET doesn't have array parameters
			// https://www.mikesdotnetting.com/article/116/parameterized-in-clauses-with-ado-net-and-linq
			string[] parameters = new string[ids.Count];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = "@id" + i;
			}

			using MySqlCommand cmd = new MySqlCommand("DELETE FROM personnel WHERE idpersonnel IN ("
				+ String.Join(", ", parameters) + ");", this._connection);
			for (int i = 0; i < ids.Count; i++)
			{
				cmd.Parameters.AddWithValue("@id" + i, ids[i]);
			}

			int affectedRows = await cmd.ExecuteNonQueryAsync();

			if (affectedRows != ids.Count)
			{
				throw new OperationCanceledException("Should have affected " + ids.Count + " rows, but actually affected " + affectedRows);
			}

			return cmd;
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

			cmd.Parameters.AddWithValue("id", entity.Id);
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
