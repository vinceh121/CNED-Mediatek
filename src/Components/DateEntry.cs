using System;
using Gtk;

namespace Mediatek.Components
{
	public class DateEntry : Entry
	{
		private Popover _popover;
		private Calendar _calendar;

		public DateEntry() : base()
		{
			this._popover = new Popover(this);
			this._calendar = new Calendar();
			this._popover.Add(this._calendar);

			this.SecondaryIconName = "appointment-soon";
			this.SecondaryIconSensitive = true;
			this.IconPress += (_, _) =>
			{
				this._popover.ShowAll();
			};

			this._calendar.DaySelectedDoubleClick += (_, _) =>
			{
				this._popover.Hide();
				this.Text = this._calendar.Date.ToString();
			};
		}

		public DateTime Date
		{
			get
			{
				return DateTime.Parse(this.Text);
			}
		}
	}
}
