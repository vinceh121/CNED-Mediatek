using System;
using System.Collections.Generic;
using Gtk;
using MySqlConnector;
using UI = Gtk.Builder.ObjectAttribute;

using Mediatek.Mapper;

namespace Mediatek
{
	class EditStaffDialog : Dialog
	{
		private Mediatek _mediatek;
		[UI] private Entry _txtLastName = null;
		[UI] private Entry _txtFirstName = null;
		[UI] private Entry _txtPhone = null;
		[UI] private Entry _txtEmail = null;
		[UI] private ComboBox _cbxService = null;
		[UI] private Button _btnEditStaff = null;
		[UI] private Button _btnCancel = null;
		private Dictionary<long, string> _serviceNames = new Dictionary<long, string>();
		private long _editingId;
		private Staff _editing;

		public EditStaffDialog(Mediatek _mediatek, long editingId) : this(_mediatek, editingId, new Builder("EditStaffDialog.glade")) { }

		private EditStaffDialog(Mediatek _mediatek, long editingId, Builder builder) : base(builder.GetRawOwnedObject("EditStaffDialog"))
		{
			this._mediatek = _mediatek;
			this._editingId = editingId;

			builder.Autoconnect(this);

			this.LoadServicesAndStaff();

			this._btnCancel.Clicked += (_, _) => this.Destroy();

			this._btnEditStaff.Clicked += (object sender, EventArgs args) =>
			{
				this.CreateStaffActivated(sender, args);

			};
		}

		private async void LoadServicesAndStaff()
		{
			ListStore store = new ListStore(GLib.GType.String, GLib.GType.String);

			using MySqlCommand cmd = new MySqlCommand("SELECT idservice, nom FROM service;", this._mediatek.GetConnection());
			using MySqlDataReader read = await cmd.ExecuteReaderAsync();
			while (await read.ReadAsync())
			{
				this._serviceNames.Add(read.GetInt64("idservice"), read.GetString("nom"));
				store.AppendValues(read.GetString("nom"), read.GetInt64("idservice").ToString());
			}
			this._cbxService.Model = store;

			CellRendererText txtRender = new CellRendererText();
			this._cbxService.PackStart(txtRender, true);
			this._cbxService.SetAttributes(txtRender, "text", 0);
			this._cbxService.AddAttribute(txtRender, "id", 1);

			this._cbxService.IdColumn = 1;

			this._cbxService.Sensitive = true;

			// need to dispose before the call to LoadStaff cause the
			// using statements won't trigger it until the return of LoadServicesAndStaff
			await read.DisposeAsync();
			await cmd.DisposeAsync();
			this.LoadStaff();
		}

		private async void LoadStaff() {
			using MySqlCommand cmdStaff = new MySqlCommand("SELECT * FROM personnel WHERE idpersonnel=@id;", this._mediatek.GetConnection());
			cmdStaff.Parameters.AddWithValue("id", this._editingId);
			using MySqlDataReader readStaff = await cmdStaff.ExecuteReaderAsync();
			await readStaff.ReadAsync();
			this._editing = EntityMapper.MapFromRow<Staff>(readStaff);

			this._txtLastName.Text = this._editing.LastName;
			this._txtFirstName.Text = this._editing.FirstName;
			this._txtPhone.Text = this._editing.Phone;
			this._txtEmail.Text = this._editing.Email;

			// Default service combobox to current service
			this._cbxService.ActiveId = this._editing.IdService.ToString();

			this.Sensitive = true;
		}

		private async void CreateStaffActivated(object sender, EventArgs args)
		{
			using MySqlCommand cmd = new MySqlCommand(
				"UPDATE personnel SET nom=@nom, prenom=@prenom, tel=@tel, mail=@mail, idservice=@idservice WHERE idpersonnel=@id;",
				this._mediatek.GetConnection());

			Staff staff = new Staff(this._editing.Id, this._txtLastName.Text, this._txtFirstName.Text,
				this._txtPhone.Text, this._txtEmail.Text, long.Parse(this._cbxService.ActiveId));

			staff.Service = this._serviceNames.GetValueOrDefault(staff.IdService);

			cmd.Parameters.AddWithValue("id", this._editing.Id);
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
			else // on success
			{
				this.Destroy();
			}
		}
	}
}
