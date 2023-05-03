using System.Collections.Generic;

using UI = Gtk.Builder.ObjectAttribute;
using Gtk;

using Mediatek.Entities;

namespace Mediatek.Dialogs
{
	public class ReasonSelectDialog : Dialog
	{
		[UI] private ComboBoxText _cbxLeaves = null;
		[UI] private Button _btnCancel = null;
		[UI] private Button _btnOk = null;
		private List<Reason> _reasons;
		private Dictionary<long, Reason> _reasonsDict = new Dictionary<long, Reason>();

		public Reason SelectedReason { get; private set; }

		public ReasonSelectDialog(Mediatek program, List<Reason> reasons) : this(program, new Builder("ReasonSelectDialog.glade"), reasons) { }

		private ReasonSelectDialog(Mediatek program, Builder builder, List<Reason> reasons) : base(builder.GetRawOwnedObject("ReasonSelectDialog"))
		{
			builder.Autoconnect(this);

			this._reasons = reasons;

			foreach (Reason r in reasons)
			{
				this._cbxLeaves.Append(r.Id.ToString(), r.label);
				this._reasonsDict.Add(r.Id, r);
			}

			this._btnCancel.Clicked += (_, _) => this.Dispose();
			this._btnOk.Clicked += (_, _) =>
			{
				this.SelectedReason = this._reasonsDict.GetValueOrDefault(long.Parse(this._cbxLeaves.ActiveId));
				this.Destroy();
			};
		}
	}
}
