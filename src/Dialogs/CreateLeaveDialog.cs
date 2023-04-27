using System;
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
		[UI] private Button _btnCancel = null;
		[UI] private Entry _txtStaff = null;
		[UI] private ComboBox _cbxReason = null;
		[UI] private FlowBoxChild _boxDateStart = null;
		[UI] private FlowBoxChild _boxDateEnd = null;
		private DateEntry _dateStart, _dateEnd;
		private Staff _staff;

		public CreateLeaveDialog(Mediatek program, Staff staff) : this(program, new Builder("CreateLeaveDialog.glade"), staff)
		{
		}

		private CreateLeaveDialog(Mediatek program, Builder builder, Staff staff) : base(builder.GetRawOwnedObject("CreateLeaveDialog"))
		{
			builder.Autoconnect(this);
			this._program = program;
			this._staff = staff;

			this._txtStaff.Text = this._staff.FirstName + " " + this._staff.LastName;

			this._btnCancel.Clicked += (_, _) => this.Dispose();

			this._dateStart = new DateEntry();
			this._boxDateStart.Add(this._dateStart);

			this._dateEnd = new DateEntry();
			this._boxDateEnd.Add(this._dateEnd);

			this._btnCreate.Clicked += (_, _) =>
			{
				Console.WriteLine(this._dateStart.Date);
			};

			this.LoadReasons();
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
			this._cbxReason.Active = 0;

			this._cbxReason.Sensitive = true;
		}
	}
}
