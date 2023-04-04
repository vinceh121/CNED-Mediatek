using System;
using Gtk;

namespace project
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application.Init();

			Application app = new Application("me.vinceh121.mediatek", GLib.ApplicationFlags.None);
			app.Register(GLib.Cancellable.Current);

			MainWindow win = new MainWindow();
			app.AddWindow(win);

			win.Show();
			Application.Run();
		}
	}
}
