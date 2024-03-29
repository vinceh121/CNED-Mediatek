using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gtk;
using MySqlConnector;
using UI = Gtk.Builder.ObjectAttribute;

using Mediatek.Entities;
using Mediatek.Dialogs;

namespace Mediatek
{
	/// <summary>
	/// GLib signals:
	/// <list type="bullet">
	/// <item>
	/// <term>staffDelete</term>
	/// <description>Shows the staff deletion dialog on the selected staff member</description>
	/// </item>
	/// <item>
	/// <term>staffEdit</term>
	/// <description>Shows the staff edition dialog on the selected staff member</description>
	/// </item>
	/// <item>
	/// <term>leaveCreate</term>
	/// <description>Shows the leave creation dialog for the selected staff member</description>
	/// </item>
	/// <item>
	/// <term>leaveEdit</term>
	/// <description>Shows the edit leave dialog for the selected leave</description>
	/// </item>
	/// <item>
	/// <term>leaveDelete</term>
	/// <description>Shows the leave deletion dialog for the selected leave</description>
	/// </item>
	/// <item>
	/// <term>refresh</term>
	/// <description>Refreshes the staff list and leave calendar</description>
	/// </item>
	/// </list>
	/// </summary>
	public class MainWindow : ApplicationWindow
	{
		private Mediatek _mediatek;
		[UI] private Notebook _notebook = null;
		[UI] private Toolbar _toolbarStaff = null;
		[UI] private Toolbar _toolbarLeave = null;
		[UI] private TreeView _staffTree = null;
		[UI] private Calendar _leaveCalendar = null;
		// arrays of sets to store staff names on leave for each day of the month
		// a set is used to prevent duplicates
		private ISet<string>[] _calendarDetails;
		// need to store this so the DetailFunc can avoid returning the string for another month
		private DateTime _currentMonth;

		public MainWindow(Mediatek mediatek) : this(mediatek, new Builder("MainWindow.glade")) { }

		private MainWindow(Mediatek mediatek, Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
		{
			this._mediatek = mediatek;
			this.Application = mediatek.GetApplication();
			builder.Autoconnect(this);

			this.CreateColumns();

			DeleteEvent += Window_DeleteEvent;

			this.CreateToolBars();

			this._mediatek.LoggedIn += LoggedInActivated;

			GLib.SimpleAction staffDeleteAction = new GLib.SimpleAction("staffDelete", null);
			staffDeleteAction.Activated += StaffDeleteActivated;
			staffDeleteAction.Enabled = false;
			this.Application.SetAccelsForAction("win.staffDelete", new string[] { "Delete", "<Ctrl>D" });
			this.AddAction(staffDeleteAction);

			GLib.SimpleAction actionEditStaff = new GLib.SimpleAction("staffEdit", null);
			actionEditStaff.Activated += StaffEditActivated;
			actionEditStaff.Enabled = false;
			this.Application.SetAccelsForAction("win.staffEdit", new string[] { "<Ctrl>E" });
			this.AddAction(actionEditStaff);

			GLib.SimpleAction actionCreateLeave = new GLib.SimpleAction("leaveCreate", null);
			actionCreateLeave.Activated += LeaveCreateActivated;
			actionCreateLeave.Enabled = false;
			this.Application.SetAccelsForAction("win.leaveCreate", new string[] { "<Ctrl><Shift>N" });
			this.AddAction(actionCreateLeave);

			GLib.SimpleAction actionEditLeave = new GLib.SimpleAction("leaveEdit", null);
			actionEditLeave.Activated += LeaveEditActivated;
			actionEditLeave.Enabled = false;
			this.Application.SetAccelsForAction("win.leaveEdit", new string[] { "<Ctrl><Shift>E" });
			this.AddAction(actionEditLeave);

			GLib.SimpleAction actionDeleteLeave = new GLib.SimpleAction("leaveDelete", null);
			actionDeleteLeave.Activated += LeaveDeleteActivated;
			actionDeleteLeave.Enabled = false;
			this.Application.SetAccelsForAction("win.leaveDelete", new string[] { "<Shift>Delete", "<Ctrl><Shift>D" });
			this.AddAction(actionDeleteLeave);

			GLib.SimpleAction actionRefresh = new GLib.SimpleAction("refresh", null);
			actionRefresh.Activated += async (_, _) => await this.RefreshData();
			actionRefresh.Enabled = false;
			this.Application.SetAccelsForAction("win.refresh", new string[] { "F5", "<Ctrl>r" });
			this.AddAction(actionRefresh);
		}

		private void CreateToolBars()
		{
			// staff
			ToolButton toolAddStaff = new ToolButton(null, "Ajouter personnel");
			toolAddStaff.Sensitive = true;
			toolAddStaff.IconName = "document-new";
			toolAddStaff.ActionName = "app.staffCreate";
			_toolbarStaff.Add(toolAddStaff);

			ToolButton toolEditStaff = new ToolButton(null, "Modifier personnel");
			toolEditStaff.Sensitive = true;
			toolEditStaff.IconName = "document-edit";
			toolEditStaff.ActionName = "win.staffEdit";
			_toolbarStaff.Add(toolEditStaff);

			ToolButton toolDeleteStaff = new ToolButton(null, "Supprimer personnel");
			toolDeleteStaff.Sensitive = true;
			toolDeleteStaff.IconName = "edit-delete";
			toolDeleteStaff.ActionName = "win.staffDelete";
			_toolbarStaff.Add(toolDeleteStaff);

			// leave
			ToolButton toolAddLeave = new ToolButton(null, "Ajouter absence");
			toolAddLeave.Sensitive = true;
			toolAddLeave.IconName = "document-new";
			toolAddLeave.ActionName = "win.leaveCreate";
			_toolbarLeave.Add(toolAddLeave);

			ToolButton toolEditLeave = new ToolButton(null, "Modifier absence");
			toolEditLeave.Sensitive = true;
			toolEditLeave.IconName = "document-edit";
			toolEditLeave.ActionName = "win.leaveEdit";
			_toolbarLeave.Add(toolEditLeave);

			ToolButton toolDeleteLeave = new ToolButton(null, "Supprimer absence");
			toolDeleteLeave.Sensitive = true;
			toolDeleteLeave.IconName = "edit-delete";
			toolDeleteLeave.ActionName = "win.leaveDelete";
			_toolbarLeave.Add(toolDeleteLeave);
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
			CellRendererToggle tgl = new CellRendererToggle();
			tgl.Toggled += LeaveDayToggle;
			this._staffTree.AppendColumn(new TreeViewColumn("Absent ajd.", tgl, "active", i++));
		}

		private async void LeaveDayToggle(object sender, ToggledArgs args)
		{
			CellRendererToggle tgl = sender as CellRendererToggle;

			TreeIter iter;
			this._staffTree.Model.GetIterFromString(out iter, args.Path);
			long id = (long)this._staffTree.Model.GetValue(iter, 0);

			if (tgl.Active)
			{ // staff is set to be missing today, cut all today's leave
			  // need to accumulate the entire list into, because MySqlConnector's "one command at a time rule" will fail otherwise with an await-foreach
				List<Leave> leaves = await Utils.ToArray(this._mediatek.GetLeaveController().FetchForDay(DateTime.Today, id));
				foreach (Leave l in leaves)
				{
					await this._mediatek.GetLeaveController().RemoveDay(DateTime.Today, l);
				}
			}
			else
			{ // staff is set to be here today, add a leave for today
				List<Reason> reasons = new List<Reason>();
				await foreach (Reason reason in this._mediatek.GetReasonController().FetchAll())
					reasons.Add(reason);

				ReasonSelectDialog resDiag = new ReasonSelectDialog(this._mediatek, reasons);
				resDiag.Run();

				if (resDiag.SelectedReason == null)
				{
					return;
				}

				Leave leave = new Leave(-1, DateTime.Today, DateTime.Today.AddDays(1).AddMinutes(-1), id, resDiag.SelectedReason.Id);
				await this._mediatek.GetLeaveController().Insert(leave);
			}

			await this.RefreshData();
		}

		private async void LeaveCreateActivated(object sender, EventArgs e)
		{
			CreateLeaveDialog diag;

			if (this._notebook.Page == 0)
			{ // if we have the staff list open, use it for staff selection
				List<long> ids = this.GetSelectedIds();

				if (ids.Count != 1)
				{
					MessageDialog errDiag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error, ButtonsType.Ok,
						false, "Sélectionnez une personne seulement",
						new object[0]);
					errDiag.Run();
					errDiag.Destroy();
					return;
				}

				Staff staff = await this._mediatek.GetStaffController().Get(ids[0]);
				diag = new CreateLeaveDialog(this._mediatek, staff);
			}
			else
			{ // else use in-dialog combobox
				diag = new CreateLeaveDialog(this._mediatek, null);
			}
			diag.ShowAll();
			diag.TransientFor = this;
		}

		private async void LeaveEditActivated(object sender, EventArgs e)
		{
			List<Leave> leaves = new List<Leave>();
			await foreach (Leave l in this._mediatek.GetLeaveController().FetchForDay(this._leaveCalendar.Date)) leaves.Add(l);

			Leave leave;
			if (leaves.Count == 0)
			{
				MessageDialog diag = new MessageDialog(this, DialogFlags.Modal, MessageType.Error,
					ButtonsType.Ok, "Aucune absence pour le jour sélectionné", new object[] { });
				diag.Run();
				diag.Destroy();
				return;
			}
			else if (leaves.Count == 1)
			{
				leave = leaves[0];
			}
			else
			{
				LeaveSelectDialog diag = new LeaveSelectDialog(this._mediatek, leaves);
				diag.ShowAll();
				diag.Run();

				leave = diag.SelectedLeave;
			}

			EditLeaveDialog editDiag = new EditLeaveDialog(this._mediatek, leave);
			editDiag.ShowAll();
			editDiag.Run();
		}

		private async void LeaveDeleteActivated(object sender, EventArgs e)
		{
			List<Leave> leaves = new List<Leave>();
			await foreach (Leave l in this._mediatek.GetLeaveController().FetchForDay(this._leaveCalendar.Date)) leaves.Add(l);

			Leave leave;
			if (leaves.Count == 0)
			{
				MessageDialog diag = new MessageDialog(this, DialogFlags.Modal, MessageType.Error,
					ButtonsType.Ok, "Aucune absence pour le jour sélectionné", new object[] { });
				diag.Run();
				diag.Destroy();
				return;
			}
			else if (leaves.Count == 1)
			{
				leave = leaves[0];
			}
			else
			{
				LeaveSelectDialog diag = new LeaveSelectDialog(this._mediatek, leaves);
				diag.ShowAll();
				diag.Run();

				if (diag.SelectedLeave == null)
				{
					return;
				}

				leave = diag.SelectedLeave;
			}

			MessageDialog confirm = new MessageDialog(this, DialogFlags.Modal, MessageType.Question, ButtonsType.OkCancel,
				true, "Supprimer l'absence de {0} {1} de {2} à {3} pour cause de {4} ?",
				new object[] { leave.Staff.LastName, leave.Staff.FirstName, leave.Start, leave.End, leave.Reason.label });
			int res = confirm.Run();
			confirm.Dispose();
			if (res == -5)
			{
				await this._mediatek.GetLeaveController().Delete(leave);
				await this.RefreshData();
			}
		}

		private void StaffEditActivated(object sender, EventArgs e)
		{
			List<long> ids = this.GetSelectedIds();

			if (ids.Count != 1)
			{
				MessageDialog diag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error, ButtonsType.Ok,
					false, "Sélectionnez une personne seulement",
					new object[0]);
				diag.Run();
				diag.Destroy();
				return;
			}

			EditStaffDialog dialog = new EditStaffDialog(this._mediatek, ids[0]);
			dialog.ShowAll();
			dialog.TransientFor = this;
		}

		private async void StaffDeleteActivated(object sender, EventArgs args)
		{
			List<long> ids = this.GetSelectedIds();

			if (ids.Count == 0)
			{
				MessageDialog diag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error, ButtonsType.Ok,
					false, "Sélectionnez au moins une personne",
					new object[0]);
				diag.Run();
				diag.Destroy();
				return;
			}

			try
			{
				MySqlCommand cmd = await this._mediatek.GetStaffController().Delete(ids);
			}
			catch (Exception e)
			{
				MessageDialog diag = new MessageDialog(this, DialogFlags.UseHeaderBar, MessageType.Error, ButtonsType.Ok,
					false, "Error: {0}",
					new object[] { e });
				diag.Run();
				diag.Destroy();
				return;
			}

			// manually deleting the tree items works badly with GtkSharp.
			// in fact the entire binding of the RB Tree is buggy.
			await this.RefreshData();
		}

		private async void LoggedInActivated(object sender, EventArgs e)
		{
			await this.RefreshData();

			// add calender event listeners only after we have a connection
			this._leaveCalendar.MonthChanged += async (_, _) => await this.RefreshLeaveCalendar();
			this._leaveCalendar.DetailFunc = this.CalendarDetail;
		}

		public async Task RefreshData()
		{
			await this.RefreshStaffList();
			await this.RefreshLeaveCalendar();
		}

		public async Task RefreshStaffList()
		{
			ListStore model = new ListStore(GLib.GType.Int64, GLib.GType.String, GLib.GType.String, GLib.GType.String,
				GLib.GType.String, GLib.GType.String, GLib.GType.Boolean);
			this._staffTree.Model = model;

			await foreach (object staff in this._mediatek.GetStaffController().FetchAllWithServiceLeave())
			{
				this.AppendStaff((Staff)staff);
			}
		}

		private string CalendarDetail(Calendar calendar, uint year, uint month, uint day)
		{
			if (this._calendarDetails == null
				|| day >= this._calendarDetails.Length
				|| this._calendarDetails[day - 1] == null
				|| this._currentMonth.Month != month + 1) // GTK month is 0-based, but C#'s is 1-based!
			{
				return null;
			}
			return String.Join("\n", this._calendarDetails[day - 1]);
		}

		public async Task RefreshLeaveCalendar()
		{
			this._leaveCalendar.Sensitive = false;

			this._leaveCalendar.ClearMarks();

			DateTime selected = this._leaveCalendar.Date;
			this._currentMonth = new DateTime(selected.Year, selected.Month, 1);

			int daysInMonth = DateTime.DaysInMonth(this._currentMonth.Year, this._currentMonth.Month);
			DateTime lastDayOfMonth = this._currentMonth.AddDays(daysInMonth);

			this._calendarDetails = new ISet<string>[daysInMonth];

			IAsyncEnumerable<Leave> leaves = this._mediatek.GetLeaveController().FetchInMonth(this._currentMonth);

			await foreach (Leave leave in leaves)
			{
				// leave start as day of current month
				int monthStart;
				if (leave.Start < this._currentMonth)
				{ // if the leave starts before this month
					monthStart = 1; // highlight everyday until end
				}
				else
				{
					monthStart = leave.Start.Day;
				}

				// ditto for end
				int monthEnd;
				if (leave.End > lastDayOfMonth)
				{
					monthEnd = daysInMonth;
				}
				else
				{
					monthEnd = leave.End.Day;
				}

				for (int i = monthStart; i <= monthEnd; i++)
				{
					this._leaveCalendar.MarkDay((uint)i);

					if (this._calendarDetails[i - 1] == null)
					{
						this._calendarDetails[i - 1] = new HashSet<string>();
					}
					this._calendarDetails[i - 1].Add(leave.Staff.LastName + " " + leave.Staff.FirstName);
				}
			}

			this._leaveCalendar.Sensitive = true;
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
			model.SetValue(iter, i++, staff.IsCurrentlyOut);
		}

		private void Window_DeleteEvent(object sender, DeleteEventArgs a)
		{
			Application.Quit();
		}

		/// <summary>
		/// Returns the IDs of staff selected in the TreeView. Empty list when none are selected.
		/// </summary>
		public List<long> GetSelectedIds()
		{
			List<long> ids = new List<long>();

			TreePath[] paths = this._staffTree.Selection.GetSelectedRows();

			foreach (TreePath p in paths)
			{
				// since all our "tree" nodes are root nodes, the paths here should only have a depth of 1
				TreeIter iter;
				this._staffTree.Model.GetIter(out iter, p);
				ids.Add((long)this._staffTree.Model.GetValue(iter, 0));
			}

			return ids;
		}
	}
}
