using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Controls;

namespace Rpc.Bulletin.Maker;

public enum DialogType
{
	Ok,
	OkCancel,
	YesNo,
}

public partial class DialogContent : UserControl
{
	private DialogHost _host;
	private Action<bool?> _dismissedHandler { get; set; }

	/// -----------------------------------------------------------------------------------------------------------
	public static void Show(string msg, DialogType dlgType, DialogHost host, Action<bool?> dismissedHandler = null)
	{
		var content = new DialogContent();
		content._btnLeft.Click += (s, e) => content.HandleDialogClosing(true);
		content._btnRight.Click += (s, e) => content.HandleDialogClosing(false);
		content._txtDialog.Text = msg;
		content._btnLeft.Content = dlgType is DialogType.Ok or DialogType.OkCancel ? "OK" : "Yes";

		if (dlgType == DialogType.Ok)
			content._btnRight.Visibility = System.Windows.Visibility.Collapsed;
		else
			content._btnRight.Content = dlgType == DialogType.OkCancel ? "Cancel" : "No";

		content._dismissedHandler = dismissedHandler;
		content._host = host;
		host.ShowDialog(content);
	}

	/// -----------------------------------------------------------------------------------------------------------
	public DialogContent()
	{
		InitializeComponent();
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void HandleDialogClosing(bool okOrYes)
	{
		_host.IsOpen = false;
		_dismissedHandler?.Invoke(okOrYes);
	}
}
