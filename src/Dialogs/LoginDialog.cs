using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace project
{
	class LoginDialog : Dialog
	{
		private Mediatek Program;
		[UI] private Button BtnLogin = null; // this useless null def is just to get rid of the compiler warning
		[UI] private Entry TxtHost = null;
		[UI] private Entry TxtUsername = null;
		[UI] private Entry TxtPassword = null;
		[UI] private Entry TxtDatabase = null;

		public LoginDialog(Mediatek Program) : this(new Builder("LoginDialog.glade"))
		{
			this.Program = Program;
		}

		private LoginDialog(Builder builder) : base(builder.GetRawOwnedObject("LoginDialog"))
		{
			builder.Autoconnect(this);

			BtnLogin.Clicked += this.LoginActivated;
		}

		private void LoginActivated(object sender, EventArgs e)
		{
			this.Program.Login(this.TxtHost.Text, this.TxtUsername.Text, this.TxtPassword.Text, this.TxtDatabase.Text);
		}
	}
}
