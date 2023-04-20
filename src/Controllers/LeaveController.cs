using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using MySqlConnector;

using Mediatek.Mapper;
using Mediatek.Entities;

namespace Mediatek.Controllers
{
	public class LeaveController : AbstractController<Leave>
	{
		public LeaveController(Mediatek mediatek) : base(mediatek, "absences") { }

		public async IAsyncEnumerable<Leave> FetchInMonth(DateTime month)
		{
			Debug.Assert(month.Day == 1 && month.Hour == 0 &&
				month.Minute == 0 && month.Second == 0, "DateTime doesn't represent only a month of a year");

			using MySqlCommand cmd = new MySqlCommand("SELECT * FROM absences " // this gives us a few duplicate ID columns, but shouldn't be too bad
				+ "INNER JOIN personnel ON personnel.idpersonnel = absences.idpersonnel "
				+ "INNER JOIN motif ON motif.idmotif = absences.idmotif "
				+ "WHERE @month <= datefin AND LAST_DAY(@month) >= datedebut", // simple interval intersection test between [month, LAST_DAY(month)] and [datedebut, datefin]
				this._connection);
			cmd.Parameters.AddWithValue("month", month);

			using MySqlDataReader reader = await cmd.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				Leave leave = EntityMapper.MapFromRow<Leave>(reader);
				leave.Staff = EntityMapper.MapFromRow<Staff>(reader);
				leave.Reason = EntityMapper.MapFromRow<Reason>(reader);

				yield return leave;
			}
		}

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
