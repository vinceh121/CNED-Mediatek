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
		[UI] private TreeView _staffTree = null;

		public MainWindow(Mediatek mediatek) : this(mediatek, new Builder("MainWindow.glade")) { }

		private MainWindow(Mediatek mediatek, Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
		{
			this._mediatek = mediatek;
			this.Application = mediatek.GetApplication();
			builder.Autoconnect(this);

			this.CreateColumns();

			DeleteEvent += Window_DeleteEvent;

			ToolButton toolAddStaff = new ToolButton(null, "Ajouter personnel");
			toolAddStaff.Sensitive = true;
			toolAddStaff.IconName = "document-new";
			toolAddStaff.ActionName = "app.staffCreate";
			_toolbarStaff.Add(toolAddStaff);

			this._mediatek.LoggedIn += LoggedInActivated;
		}

		private void CreateColumns()
		{
			int i = 0;
			this._staffTree.AppendColumn(new TreeViewColumn("ID", new CellRendererText() { Weight = 100 }, "text", i++) { Reorderable = true });
			this._staffTree.AppendColumn(new TreeViewColumn("Prénom", new CellRendererText(), "text", i++));
			this._staffTree.AppendColumn(new TreeViewColumn("Nom", new CellRendererText(), "text", i++));
			this._staffTree.AppendColumn(new TreeViewColumn("Tél.", new CellRendererText(), "text", i++));
			this._staffTree.AppendColumn(new TreeViewColumn("EMail", new CellRendererText(), "text", i++));
			this._staffTree.AppendColumn(new TreeViewColumn("Service", new CellRendererText(), "text", i++));
			this._staffTree.AppendColumn(new TreeViewColumn("Absent ajd.", new CellRendererToggle() { Activatable = false }, "active", i++));
		}

		private async void LoggedInActivated(object sender, EventArgs e)
		{
			using MySqlCommand cmd = new MySqlCommand("SELECT personnel.*, service.nom AS nomservice "
				+ " FROM personnel INNER JOIN service ON personnel.idservice = service.idservice;",
				this._mediatek.GetConnection());
			using MySqlDataReader reader = await cmd.ExecuteReaderAsync();

			ListStore model = new ListStore(GLib.GType.Int64, GLib.GType.String, GLib.GType.String, GLib.GType.String,
				GLib.GType.String, GLib.GType.String, GLib.GType.Boolean);

			while (await reader.ReadAsync())
			{
				Staff staff = Staff.FromMySql(reader);
				int i = 0;
				TreeIter iter = model.Append();
				model.SetValue(iter, i++, staff.Id);
				model.SetValue(iter, i++, staff.FirstName);
				model.SetValue(iter, i++, staff.LastName);
				model.SetValue(iter, i++, staff.Phone);
				model.SetValue(iter, i++, staff.Email);
				model.SetValue(iter, i++, staff.Service);
				// todo leave column
				model.SetValue(iter, i++, new Random().NextSingle() > 0.5f);
			}

			this._staffTree.Model = model;
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
