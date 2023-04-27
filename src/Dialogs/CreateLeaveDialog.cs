using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

using Mediatek.Components;

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

		public CreateLeaveDialog(Mediatek program) : this(new Builder("CreateLeaveDialog.glade"))
		{
			this._program = program;
		}

		private CreateLeaveDialog(Builder builder) : base(builder.GetRawOwnedObject("CreateLeaveDialog"))
		{
			builder.Autoconnect(this);

			this._btnCancel.Clicked += (_, _) => this.Dispose();

			this._dateStart = new DateEntry();
			this._boxDateStart.Add(this._dateStart);

			this._dateEnd = new DateEntry();
			this._boxDateEnd.Add(this._dateEnd);

			this._btnCreate.Clicked += (_, _) =>
			{
				Console.WriteLine(this._dateStart.Date);
			};
		}
	}
}
