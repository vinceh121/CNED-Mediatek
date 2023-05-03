using System.Collections.Generic;

using UI = Gtk.Builder.ObjectAttribute;
using Gtk;

using Mediatek.Entities;

namespace Mediatek.Dialogs
{
	public class LeaveSelectDialog : Dialog
	{
		[UI] private ComboBoxText _cbxLeaves = null;
		[UI] private Button _btnCancel = null;
		[UI] private Button _btnOk = null;
		private List<Leave> _leaves;
		private Dictionary<long, Leave> _leavesDict = new Dictionary<long, Leave>();

		public Leave SelectedLeave { get; private set; }

		public LeaveSelectDialog(Mediatek program, List<Leave> leaves) : this(program, new Builder("LeaveSelectDialog.glade"), leaves) { }

		private LeaveSelectDialog(Mediatek program, Builder builder, List<Leave> leaves) : base(builder.GetRawOwnedObject("LeaveSelectDialog"))
		{
			builder.Autoconnect(this);

			this._leaves = leaves;

			foreach (Leave l in leaves)
			{
				this._cbxLeaves.Append(l.Id.ToString(), $"{l.Staff.LastName} {l.Staff.FirstName} - {l.Start} à {l.End}");
				this._leavesDict.Add(l.Id, l);
			}

			this._btnCancel.Clicked += (_, _) => this.Dispose();
			this._btnOk.Clicked += (_, _) =>
			{
				this.SelectedLeave = this._leavesDict.GetValueOrDefault(long.Parse(this._cbxLeaves.ActiveId));
			};
		}
	}
}
