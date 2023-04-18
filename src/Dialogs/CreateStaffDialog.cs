using System;
using System.Collections.Generic;
using Gtk;
using MySqlConnector;
using UI = Gtk.Builder.ObjectAttribute;

using Mediatek.Entities;

namespace Mediatek.Dialogs
{
	class CreateStaffDialog : Dialog
	{
		private Mediatek _mediatek;
		[UI] private Entry _txtLastName = null;
		[UI] private Entry _txtFirstName = null;
		[UI] private Entry _txtPhone = null;
		[UI] private Entry _txtEmail = null;
		[UI] private ComboBox _cbxService = null;
		[UI] private Button _btnCreateStaff = null;
		[UI] private Button _btnCreateStaffAndClose = null;
		[UI] private Button _btnCancel = null;
		private Dictionary<long, string> _serviceNames = new Dictionary<long, string>();

		public CreateStaffDialog(Mediatek _mediatek) : this(_mediatek, new Builder("CreateStaffDialog.glade")) { }

		private CreateStaffDialog(Mediatek _mediatek, Builder builder) : base(builder.GetRawOwnedObject("CreateStaffDialog"))
		{
			this._mediatek = _mediatek;

			builder.Autoconnect(this);

			this.LoadServices();

			this._btnCancel.Clicked += (_, _) => this.Destroy();
			this._btnCreateStaff.Clicked += (object sender, EventArgs args) =>
			{
				try
				{
					this.CreateStaffActivated(sender, args);
				}
				catch (MySqlException e)
				{
					this.InsertErr(e);
				}
			};

			this._btnCreateStaffAndClose.Clicked += (object sender, EventArgs args) =>
			{
				try
				{
					this.CreateStaffActivated(sender, args);
					this.Destroy();
				}
				catch (MySqlException e)
				{
					this.InsertErr(e);
				}
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
				this._serviceNames.Add(read.GetInt64("idservice"), read.GetString("nom"));
				store.AppendValues(read.GetString("nom"), read.GetInt64("idservice").ToString());
			}
			this._cbxService.Model = store;

			CellRendererText txtRender = new CellRendererText();
			this._cbxService.PackStart(txtRender, true);
			this._cbxService.SetAttributes(txtRender, "text", 0);
			this._cbxService.AddAttribute(txtRender, "id", 1);

			this._cbxService.IdColumn = 1;
			// force the first index to be selected to prevent null selection
			this._cbxService.Active = 0;

			this._cbxService.Sensitive = true;
		}

		private async void CreateStaffActivated(object sender, EventArgs args)
		{
			Staff staff = new Staff(-1, this._txtLastName.Text, this._txtFirstName.Text,
				this._txtPhone.Text, this._txtEmail.Text, long.Parse(this._cbxService.ActiveId));

			// I wanted to only use the ComboBox's ListStore to avoid duplicating the dictionary,
			// but the GTK# binding is broken and iterating it using the IEnumator causes
			// what looks like an invalid linked tree read.
			// See https://github.com/GtkSharp/GtkSharp/issues/399
			// We set the service string here because we display this staff record in the main table.
			staff.Service = this._serviceNames.GetValueOrDefault(staff.IdService);

			using MySqlCommand cmd = await this._mediatek.GetStaffController().Insert(staff);

			// set ID in record, which is still -1, to the auto increment
			this._mediatek.GetMainWindow().AppendStaff(staff with { Id = cmd.LastInsertedId });
		}

		private void InsertErr(object msg)
		{
			MessageDialog diag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error, ButtonsType.Ok, false, "N'a pas pu insérer le personnel: {0}", new object[] { msg });
			diag.ShowAll();
		}
	}
}
