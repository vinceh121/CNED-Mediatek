using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace project
{
	class LoginDialog : Dialog
	{
		private Mediatek _program;
		[UI] private Button _btnLogin = null; // this useless null def is just to get rid of the compiler warning
		[UI] private Button _btnCancel = null;
		[UI] private Entry _txtHost = null;
		[UI] private Entry _txtUsername = null;
		[UI] private Entry _txtPassword = null;
		[UI] private Entry _txtDatabase = null;

		public LoginDialog(Mediatek program) : this(new Builder("LoginDialog.glade"))
		{
			this._program = program;
		}

		private LoginDialog(Builder builder) : base(builder.GetRawOwnedObject("LoginDialog"))
		{
			builder.Autoconnect(this);

			_btnLogin.Clicked += this.LoginActivated;
			_btnCancel.Clicked += (_, _) => this.Destroy();
		}

		private async void LoginActivated(object sender, EventArgs e)
		{
			this.Sensitive = false;
			try
			{
				await this._program.Login(this._txtHost.Text, this._txtUsername.Text, this._txtPassword.Text, this._txtDatabase.Text);
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
