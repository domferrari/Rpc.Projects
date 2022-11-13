using System.Windows.Controls;

namespace Rpc.Bulletin.Maker
{
	public partial class EmailView : UserControl
	{
		public EmailView()
		{
			InitializeComponent();

			_txtPassword.Password = Properties.Settings.Default.EmailPassword;
		}

		public string Password => _txtPassword.Password;

		private void HandlePasswordLostFocus(object sender, System.Windows.RoutedEventArgs e)
		{
			Properties.Settings.Default.EmailPassword = _txtPassword.Password;
			Properties.Settings.Default.Save();
		}
	}
}
