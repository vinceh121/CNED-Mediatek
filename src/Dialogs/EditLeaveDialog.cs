using System.Threading.Tasks;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

using Mediatek.Components;
using Mediatek.Entities;

namespace Mediatek.Dialogs
{
	class EditLeaveDialog : Dialog
	{
		private Mediatek _program;
		[UI] private Button _btnEdit = null;
		[UI] private Button _btnCancel = null;
		[UI] private ComboBox _cbxReason = null;
		[UI] private FlowBoxChild _boxDateStart = null;
		[UI] private FlowBoxChild _boxDateEnd = null;
		[UI] private ComboBoxText _cbxStaff = null;
		private DateEntry _dateStart, _dateEnd;
		private Leave _leave;

		public EditLeaveDialog(Mediatek program, Leave leave) : this(program, new Builder("EditLeaveDialog.glade"), leave)
		{
		}

		private EditLeaveDialog(Mediatek program, Builder builder, Leave leave) : base(builder.GetRawOwnedObject("EditLeaveDialog"))
		{
			builder.Autoconnect(this);
			this._program = program;
			this._leave = leave; ;
			this._cbxStaff.Append(this._leave.IdStaff.ToString(), $"{this._leave.Staff.FirstName} {this._leave.Staff.LastName}");
			this._cbxStaff.Active = 0;
			this._cbxStaff.Sensitive = false;

			this.LoadReasons();

			this._btnCancel.Clicked += (_, _) => this.Dispose();

			this._dateStart = new DateEntry();
			this._boxDateStart.Add(this._dateStart);

			this._dateEnd = new DateEntry();
			this._boxDateEnd.Add(this._dateEnd);

			this._btnCancel.Clicked += (_, _) => this.Dispose();

			this._btnEdit.Clicked += (_, _) =>
			{
				this.EditLeave();
				this.Dispose();
			};

			// reason needs to be filled after the reasons are loaded
			this._dateStart.Date = leave.Start;
			this._dateEnd.Date = leave.End;
		}

		private async void EditLeave()
		{
			Leave leave = this._leave with { Start = this._dateStart.Date, End = this._dateEnd.Date, IdReason = long.Parse(this._cbxReason.ActiveId) };
			await this._program.GetLeaveController().Update(leave);
			await this._program.GetMainWindow().RefreshData();
		}

		private async void LoadReasons()
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

			this._cbxReason.ActiveId = this._leave.IdReason.ToString();

			this._cbxReason.Sensitive = true;
		}
	}
}
