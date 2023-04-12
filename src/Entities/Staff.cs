using System;
using MySqlConnector;

namespace project
{
	public record Staff(
		long Id,
		string LastName,
		string FirstName,
		string Phone,
		string Email,
		long IdService
	)
	{
		public string Service { get; set; }

		/// <summary>
		/// Maps a Staff record from an SQL row, optionally with a service affectation join
		/// </summary>
		/// <param name="reader">A MySqlReader that needs to be opened and
		/// whose cursor is on a row.</param>
		public static Staff FromMySql(MySqlDataReader reader)
		{
			Staff staff = new Staff(
				reader.GetInt64("idpersonnel"),
				reader.GetString("nom"),
				reader.GetString("prenom"),
				reader.GetString("tel"),
				reader.GetString("mail"),
				reader.GetInt64("idservice"));

			try
			{
				staff.Service = reader.GetString("nomservice");
			}
			catch (IndexOutOfRangeException)
			{
				// silently ignore exception when the service join isn't used
			}

			return staff;
		}
	}
}