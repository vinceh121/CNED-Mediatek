using MySqlConnector;

using Mediatek.Entities;
using System.Threading.Tasks;

namespace Mediatek.Controllers
{
	public class StaffController : AbstractController<Staff>
	{
		public StaffController(Mediatek mediatek) : base(mediatek) { }

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
	}
}
