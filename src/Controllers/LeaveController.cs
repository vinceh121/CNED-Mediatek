using System.Threading.Tasks;
using MySqlConnector;

using Mediatek.Entities;

namespace Mediatek.Controllers
{
	public class LeaveController : AbstractController<Leave>
	{
		public LeaveController(Mediatek mediatek) : base(mediatek, "absences") { }

		public override async Task<MySqlCommand> Insert(Leave entity)
		{
			MySqlCommand cmd = new MySqlCommand(
				"INSERT INTO absences (datedebut, datefin, idpersonnel, idmotif) VALUES "
				+ "(@datedebut, @datefin, @idpersonnel, @idmotif);",
				this._mediatek.GetConnection());

			cmd.Parameters.AddWithValue("datedebut", entity.Start);
			cmd.Parameters.AddWithValue("datefin", entity.End);
			cmd.Parameters.AddWithValue("idpersonnel", entity.IdStaff);
			cmd.Parameters.AddWithValue("idmotif", entity.IdReason);

			await cmd.ExecuteNonQueryAsync();

			return cmd;
		}

		public override async Task<MySqlCommand> Update(Leave entity)
		{
			MySqlCommand cmd = new MySqlCommand(
				"UPDATE absences SET datedebut=@datedebut, datefin=@datedebut, "
				+ "idpersonnel=@idpersonnel, idmotif=@idmotif "
				+ " WHERE id=@id;",
				this._mediatek.GetConnection());

			cmd.Parameters.AddWithValue("id", entity.Id);
			cmd.Parameters.AddWithValue("datedebut", entity.Start);
			cmd.Parameters.AddWithValue("datefin", entity.End);
			cmd.Parameters.AddWithValue("idpersonnel", entity.IdStaff);
			cmd.Parameters.AddWithValue("idmotif", entity.IdReason);

			await cmd.ExecuteNonQueryAsync();

			return cmd;
		}
	}
}
