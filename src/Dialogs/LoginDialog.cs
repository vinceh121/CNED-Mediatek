using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Mediatek.Dialogs
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
		[UI] private ComboBox _cbxSslMode = null;
		[UI] private Entry _txtManagerUsername = null;
		[UI] private Entry _txtManagerPassword = null;

		public LoginDialog(Mediatek program) : this(new Builder("LoginDialog.glade")) // GtkBuilder reads and creates
																					  // the objects defined in the Glade XML
		{
			this._program = program;
		}

		private LoginDialog(Builder builder) : base(builder.GetRawOwnedObject("LoginDialog")) // Get the pointer of the native Dialog,
																							  // and make it correspond to this C# class
		{
			builder.Autoconnect(this); // calls for the fields with the UI attribute to be filled

			ListStore model = new ListStore(GLib.GType.String, GLib.GType.String);

			Type sslModeEnum = typeof(MySqlConnector.MySqlSslMode);
			Array values = sslModeEnum.GetEnumValues();
			// skip the first enum constant, because there's both None and
			// Disabled, we get twice 0 and twich "None" returned by GetEnumName
			for (int i = 1; i < values.Length; i++)
			{
				int val = (int)values.GetValue(i);
				string name = sslModeEnum.GetEnumName(val);
				model.AppendValues(val.ToString(), name);
			}

			this._cbxSslMode.Model = model;

			CellRendererText txtRender = new CellRendererText();
			this._cbxSslMode.PackStart(txtRender, true);
			this._cbxSslMode.SetAttributes(txtRender, "id", 0);
			this._cbxSslMode.AddAttribute(txtRender, "text", 1);

			this._cbxSslMode.IdColumn = 0;
			// default to None
			this._cbxSslMode.Active = 0;

			_btnLogin.Clicked += this.LoginActivated;
			_btnCancel.Clicked += (_, _) => this.Destroy();
		}

		private async void LoginActivated(object sender, EventArgs e)
		{
			this.Sensitive = false;
			try
			{
				await this._program.Login(this._txtHost.Text, this._txtUsername.Text, this._txtPassword.Text,
					this._txtDatabase.Text, (MySqlConnector.MySqlSslMode)int.Parse(this._cbxSslMode.ActiveId),
					this._txtManagerUsername.Text, this._txtManagerPassword.Text);
				this.Destroy();
			}
			catch (UnauthorizedAccessException ex)
			{
				Console.WriteLine(ex);
				MessageDialog msg = new MessageDialog(this, DialogFlags.Modal, MessageType.Error,
									ButtonsType.Ok, true, "<b>{0}</b>", new object[] { ex.Message });
				msg.Show();
				msg.Run();
				msg.Destroy();
				this.Sensitive = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
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
