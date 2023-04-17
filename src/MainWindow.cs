using System;
using System.Collections.Generic;
using Gtk;
using MySqlConnector;
using UI = Gtk.Builder.ObjectAttribute;

namespace mediatek
{
	public class MainWindow : ApplicationWindow
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

			GLib.SimpleAction staffDeleteAction = new GLib.SimpleAction("staffDelete", null);
			staffDeleteAction.Activated += StaffDeleteActivated;
			staffDeleteAction.Enabled = false;
			this.Application.SetAccelsForAction("win.staffDelete", new string[] { "Delete" });
			this.AddAction(staffDeleteAction);
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

		private async void StaffDeleteActivated(object sender, EventArgs args)
		{
			List<long> ids = new List<long>();

			TreePath[] paths = this._staffTree.Selection.GetSelectedRows();

			if (paths.Length == 0)
			{
				MessageDialog diag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error, ButtonsType.Ok,
					false, "Sélectionnez au moins une personne",
					new object[0]);
				diag.Run();
				diag.Destroy();
				return;
			}

			foreach (TreePath p in paths)
			{
				// since all our "tree" nodes are root nodes, the paths here should only have a depth of 1
				TreeIter iter;
				this._staffTree.Model.GetIter(out iter, p);
				ids.Add((long)this._staffTree.Model.GetValue(iter, 0));
			}

			// apparently ADO.NET doesn't have array parameters
			// https://www.mikesdotnetting.com/article/116/parameterized-in-clauses-with-ado-net-and-linq
			string[] parameters = new string[ids.Count];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = "@id" + i;
			}

			using MySqlCommand cmd = new MySqlCommand("DELETE FROM personnel WHERE idpersonnel IN ("
				+ String.Join(", ", parameters) + ");", this._mediatek.GetConnection());
			for (int i = 0; i < ids.Count; i++)
			{
				cmd.Parameters.AddWithValue("@id" + i, ids[i]);
			}
			int affectedRows = await cmd.ExecuteNonQueryAsync();
			if (affectedRows != ids.Count)
			{
				MessageDialog diag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error, ButtonsType.Ok,
					false, "Error: deleted wrong number of rows, should've deleted {0} but actually deleted {1}",
					new object[] { ids.Count, affectedRows });
				diag.Run();
				diag.Destroy();
				return;
			}

			// delete in db worked, now delete in ui
			foreach (TreePath p in paths)
			{
				this._staffTree.Model.EmitRowDeleted(p);
			}
		}

		private async void LoggedInActivated(object sender, EventArgs e)
		{
			using MySqlCommand cmd = new MySqlCommand("SELECT personnel.*, service.nom AS nomservice "
				+ " FROM personnel INNER JOIN service ON personnel.idservice = service.idservice;",
				this._mediatek.GetConnection());
			using MySqlDataReader reader = await cmd.ExecuteReaderAsync();

			ListStore model = new ListStore(GLib.GType.Int64, GLib.GType.String, GLib.GType.String, GLib.GType.String,
				GLib.GType.String, GLib.GType.String, GLib.GType.Boolean);
			this._staffTree.Model = model;

			while (await reader.ReadAsync())
			{
				Staff staff = EntityMapper.MapFromRow<Staff>(reader);
				this.AppendStaff(staff);
			}
		}

		/// <summary>
		/// Appends a new row to the staff TreeView
		/// </summary>
		/// <param name="staff">Staff record to display</param>
		public void AppendStaff(Staff staff)
		{
			ListStore model = this._staffTree.Model as ListStore;
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
