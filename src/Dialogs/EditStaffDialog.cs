using System;
using System.Collections.Generic;
using Gtk;
using MySqlConnector;
using UI = Gtk.Builder.ObjectAttribute;

using Mediatek.Mapper;
using Mediatek.Entities;

namespace Mediatek.Dialogs
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
				try
				{
					this.CreateStaffActivated(sender, args);
				}
				catch (MySqlException e)
				{
					MessageDialog diag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error,
						ButtonsType.Ok, false, "N'a pas pu modifier le personnel: {0}", new object[] { e });
					diag.ShowAll();
				}

			};
		}

		private async void LoadServicesAndStaff()
		{
			ListStore store = new ListStore(GLib.GType.String, GLib.GType.String);

			await foreach (Service service in this._mediatek.GetServiceController().FetchAll())
			{
				this._serviceNames.Add(service.Id, service.Name);
				store.AppendValues(service.Name, service.Id.ToString());
			}
			this._cbxService.Model = store;

			CellRendererText txtRender = new CellRendererText();
			this._cbxService.PackStart(txtRender, true);
			this._cbxService.SetAttributes(txtRender, "text", 0);
			this._cbxService.AddAttribute(txtRender, "id", 1);

			this._cbxService.IdColumn = 1;

			this._cbxService.Sensitive = true;

			this.LoadStaff();
		}

		private async void LoadStaff()
		{
			this._editing = await this._mediatek.GetStaffController().Get(this._editingId);

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
			Staff staff = new Staff(this._editing.Id, this._txtLastName.Text, this._txtFirstName.Text,
				this._txtPhone.Text, this._txtEmail.Text, long.Parse(this._cbxService.ActiveId));

			staff.Service = this._serviceNames.GetValueOrDefault(staff.IdService);

			using MySqlCommand cmd = await this._mediatek.GetStaffController().Update(staff);

			this.Destroy();
		}
	}
}
