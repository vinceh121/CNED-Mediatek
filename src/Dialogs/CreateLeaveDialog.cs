using System.Threading.Tasks;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

using Mediatek.Components;
using Mediatek.Entities;

namespace Mediatek.Dialogs
{
	class CreateLeaveDialog : Dialog
	{
		private Mediatek _program;
		[UI] private Button _btnCreate = null;
		[UI] private Button _btnCreateAndClose = null;
		[UI] private Button _btnCancel = null;
		[UI] private ComboBox _cbxReason = null;
		[UI] private FlowBoxChild _boxDateStart = null;
		[UI] private FlowBoxChild _boxDateEnd = null;
		[UI] private ComboBoxText _cbxStaff = null;
		private DateEntry _dateStart, _dateEnd;
		private Staff _staff;

		/// <param name="staff">
		/// 	Nullable staff entity. If null, staff selection will be prompted,
		/// 	else, the given staff will be used and selection will be disabled.
		/// </param>
		public CreateLeaveDialog(Mediatek program, Staff staff) : this(program, new Builder("CreateLeaveDialog.glade"), staff)
		{
		}

		private CreateLeaveDialog(Mediatek program, Builder builder, Staff staff) : base(builder.GetRawOwnedObject("CreateLeaveDialog"))
		{
			builder.Autoconnect(this);
			this._program = program;
			this._staff = staff;

			if (staff != null)
			{
				this._cbxStaff.Sensitive = false;
				this._cbxStaff.Append(this._staff.Id.ToString(), $"{this._staff.FirstName} {this._staff.LastName}");
				this.LoadReasons().Start();
			}
			else
			{
				this.LoadAll();
			}
			this._cbxStaff.Active = 1; // select first by default

			this._btnCancel.Clicked += (_, _) => this.Dispose();

			this._dateStart = new DateEntry();
			this._boxDateStart.Add(this._dateStart);

			this._dateEnd = new DateEntry();
			this._boxDateEnd.Add(this._dateEnd);

			this._btnCancel.Clicked += (_, _) => this.Dispose();

			this._btnCreate.Clicked += (_, _) => this.CreateLeave();

			this._btnCreateAndClose.Clicked += (_, _) =>
			{
				this.CreateLeave();
				this.Dispose();
			};
		}

		private async void CreateLeave()
		{
			if (this._cbxStaff.ActiveId == null) {
				MessageDialog diag = new MessageDialog(this, DialogFlags.Modal, MessageType.Error,
					ButtonsType.Ok, false, "Sélectionnez un personnel", new object[0]);
				diag.Run();
				diag.Dispose();
				return;
			}
			Leave leave = new Leave(-1, this._dateStart.Date, this._dateEnd.Date,
				long.Parse(this._cbxStaff.ActiveId), long.Parse(this._cbxReason.ActiveId));
			await this._program.GetLeaveController().Insert(leave);
			await this._program.GetMainWindow().RefreshData();
		}

		private async void LoadAll()
		{
			await this.LoadStaff();
			await this.LoadReasons();
		}

		private async Task LoadStaff()
		{
			await foreach (Staff staff in this._program.GetStaffController().FetchAll())
			{
				this._cbxStaff.Append(staff.Id.ToString(), $"({staff.Id}) {staff.FirstName} {staff.LastName}");
			}
		}

		private async Task LoadReasons()
		{
			ListStore store = new ListStore(GLib.GType.String, GLib.GType.String);

			await foreach (Reason res in this._program.GetReasonController().FetchAll())
			{
				store.AppendValues(res.label, res.Id);
			}
			this._cbxReason.Model = store;

			CellRendererText txtRender = new CellRendererText();
			this._cbxReason.PackStart(txtRender, true);
			this._cbxReason.SetAttributes(txtRender, "text", 0);
			this._cbxReason.AddAttribute(txtRender, "id", 1);

			this._cbxReason.IdColumn = 1;
			this._cbxReason.Active = 0;

			this._cbxReason.Sensitive = true;
		}
	}
}
