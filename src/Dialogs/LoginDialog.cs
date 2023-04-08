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

		private async void LoginActivated(object sender, EventArgs e)
		{
			this.Sensitive = false;
			try
			{
				await this.Program.Login(this.TxtHost.Text, this.TxtUsername.Text, this.TxtPassword.Text, this.TxtDatabase.Text);
				this.Destroy();
			}
			catch (Exception ex)
			{
				MessageDialog msg = new MessageDialog(this, DialogFlags.Modal, MessageType.Error,
					ButtonsType.Ok, true, "<b>Échec de la connexion a la base de donnés</b>\n{0}", new object[] { ex });
				msg.Show();
				msg.Run();
				msg.Destroy();
				this.Sensitive = true;
			}
		}
	}
}
