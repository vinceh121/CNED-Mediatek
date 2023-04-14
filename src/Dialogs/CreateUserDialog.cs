using System;
using System.Collections;
using Gtk;
using MySqlConnector;
using UI = Gtk.Builder.ObjectAttribute;

namespace project
{
	class CreateUserDialog : Dialog
	{
		private Mediatek _mediatek;
		[UI] private Entry _txtLastName = null;
		[UI] private Entry _txtFirstName = null;
		[UI] private Entry _txtPhone = null;
		[UI] private Entry _txtEmail = null;
		[UI] private ComboBox _cbxService = null;
		[UI] private Button _btnCreateStaff = null;
		[UI] private Button _btnCreateStaffAndClose = null;
		private IList _serviceIds = new ArrayList();

		public CreateUserDialog(Mediatek _mediatek) : this(_mediatek, new Builder("CreateUserDialog.glade")) { }

		private CreateUserDialog(Mediatek _mediatek, Builder builder) : base(builder.GetRawOwnedObject("CreateUserDialog"))
		{
			this._mediatek = _mediatek;

			builder.Autoconnect(this);

			this.LoadServices();

			this._btnCreateStaff.Clicked += CreateStaffActivated;

			this._btnCreateStaffAndClose.Clicked += (object sender, EventArgs args) =>
			{
				this.CreateStaffActivated(sender, args);
				this.Destroy();
			};
		}

		private async void LoadServices()
		{
			// first column is the text, second is the service
			// for some reason GTK requires the IdColumn to be a string, so we ToString and then Parse the ID
			ListStore store = new ListStore(GLib.GType.String, GLib.GType.String);

			using MySqlCommand cmd = new MySqlCommand("SELECT idservice, nom FROM service;", this._mediatek.GetConnection());
			using MySqlDataReader read = await cmd.ExecuteReaderAsync();
			while (await read.ReadAsync())
			{
				store.AppendValues(read.GetString("nom"), read.GetInt64("idservice").ToString());
			}
			this._cbxService.Model = store;

			CellRendererText txtRender = new CellRendererText();
			this._cbxService.PackStart(txtRender, true);
			this._cbxService.SetAttributes(txtRender, "text", 0);
			this._cbxService.AddAttribute(txtRender, "id", 1);

			this._cbxService.IdColumn = 1;

			this._cbxService.Sensitive = true;
		}

		private async void CreateStaffActivated(object sender, EventArgs args)
		{
			using MySqlCommand cmd = new MySqlCommand(
				"INSERT INTO personnel (nom, prenom, tel, mail, idservice) VALUES "
				+ "(@nom, @prenom, @tel, @mail, @idservice);",
				this._mediatek.GetConnection());

			cmd.Parameters.AddWithValue("nom", this._txtLastName.Text);
			cmd.Parameters.AddWithValue("prenom", this._txtFirstName.Text);
			cmd.Parameters.AddWithValue("tel", this._txtPhone.Text);
			cmd.Parameters.AddWithValue("mail", this._txtEmail.Text);
			cmd.Parameters.AddWithValue("idservice", long.Parse(this._cbxService.ActiveId));

			int affectedRows = await cmd.ExecuteNonQueryAsync();
			if (affectedRows != 1)
			{
				MessageDialog diag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error, ButtonsType.Ok, false, "Error: inserted wrong number of rows", new object[0]);
				diag.ShowAll();
			}
		}
	}
}
