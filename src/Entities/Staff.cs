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