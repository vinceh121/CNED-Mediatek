using System;
using Gtk;
using MySqlConnector;
using UI = Gtk.Builder.ObjectAttribute;

namespace project
{
	class MainWindow : ApplicationWindow
	{
		private Mediatek _mediatek;
		[UI] private Toolbar _toolbarStaff = null;
		[UI] private Grid _gridStaff = null;

		public MainWindow(Mediatek mediatek) : this(mediatek, new Builder("MainWindow.glade")) { }

		private MainWindow(Mediatek mediatek, Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
		{
			this._mediatek = mediatek;
			builder.Autoconnect(this);

			DeleteEvent += Window_DeleteEvent;

			var toolAddStaff = new ToolButton(null, "Ajouter personnel");
			toolAddStaff.Sensitive = true;
			toolAddStaff.IconName = "document-new";
			toolAddStaff.Clicked += StaffCreateActivated;
			_toolbarStaff.Add(toolAddStaff);

			this._mediatek.LoggedIn += LoggedInActivated;
		}

		private void StaffCreateActivated(object sender, EventArgs e)
		{
			CreateStaffDialog dialog = new CreateStaffDialog(this._mediatek);
			dialog.ShowAll();
		}

		private async void LoggedInActivated(object sender, EventArgs e)
		{
			this._toolbarStaff.Sensitive = true;

			using MySqlCommand cmd = new MySqlCommand("SELECT personnel.*, service.nom AS nomservice "
				+ " FROM personnel INNER JOIN service ON personnel.idservice = service.idservice;",
				this._mediatek.GetConnection());
			using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
			int row = 1;
			while (await reader.ReadAsync())
			{
				Staff staff = Staff.FromMySql(reader);
				this.InsertStaffRow(staff, row++);
			}
		}

		private void InsertStaffRow(Staff staff, int row)
		{
			int i = 0;
			this._gridStaff.Attach(TableLabel(staff.Id.ToString()), i++, row, 1, 1);
			this._gridStaff.Attach(TableLabel(staff.FirstName), i++, row, 1, 1);
			this._gridStaff.Attach(TableLabel(staff.LastName), i++, row, 1, 1);
			this._gridStaff.Attach(TableLabel(staff.Phone), i++, row, 1, 1);
			this._gridStaff.Attach(TableLabel(staff.Email), i++, row, 1, 1);
			this._gridStaff.Attach(TableLabel(staff.Service), i++, row, 1, 1);
		}

		private void Window_DeleteEvent(object sender, DeleteEventArgs a)
		{
			Application.Quit();
		}

		private static Label TableLabel(string text)
		{
			Label lbl = new Label(text);
			// huh??? why isn't this the default
			lbl.Visible = true;
			lbl.Selectable = true;
			lbl.Xalign = 0;
			return lbl;
		}
	}
}
