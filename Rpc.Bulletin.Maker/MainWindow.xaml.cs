using Rpc.Bulletin.Maker.ViewModels;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Rpc.Bulletin.Maker;

public partial class MainWindow : Window
{
	/// -----------------------------------------------------------------------------------------------------------
	public MainWindow()
	{
		InitializeComponent();

		_titleBar.MainWindow = this;

		DataContext = new MainWindowViewModel(_dlgHost,
			_tpMorningView.UpdateGeneratePdfOutput, _tpMorningView.LoadPdfFile,
			_tpEveningView.UpdateGeneratePdfOutput, _tpEveningView.LoadPdfFile);
	}

	/// -----------------------------------------------------------------------------------------------------------
	protected override void OnClosing(CancelEventArgs e)
	{
		DeleteExtraFiles("*.log");
		DeleteExtraFiles("*.aux");

		PdfWebViewer.Shutdown();
		base.OnClosing(e);
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void DeleteExtraFiles(string pattern)
	{
		var folders = new[]
		{
			Properties.Settings.Default.TeXFileFolder,
			Properties.Settings.Default.TargetFilesFolder,
		};

		foreach (var fldr in folders)
		{
			foreach (var file in Directory.EnumerateFiles(fldr, pattern))
			{
				try { File.Delete(file); }
				catch { }
			}
		}
	}
}
