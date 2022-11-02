using Rpc.Bulletin.Maker.ViewModels;
using Rpc.TeX.Library;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Rpc.Bulletin.Maker;

public partial class MainWindow : Window
{
	private readonly string _sundayDate;
	private readonly string _pathToSourceFile;

	/// -----------------------------------------------------------------------------------------------------------
	public MainWindow()
	{
		InitializeComponent();

		DataContext = new MainWindowViewModel(_dlgHost,
			_tpMorningView.UpdateGeneratePdfOutput, _tpMorningView.LoadPdfFile,
			_tpEveningView.UpdateGeneratePdfOutput, _tpEveningView.LoadPdfFile);
	}



///// -----------------------------------------------------------------------------------------------------------
//private void TryFindNewSourceFile()
//{
//	var upcomingSun = Utils.GetNextSunday();
//	_sundayDate = $"{upcomingSun:yyyy-MM-dd}";
//	var path = $"{_sundayDate}.txt";
//	path = Path.Combine(Properties.Settings.Default.SourceFilesFolder, path);

//	_tpMorningView.SundayDate = upcomingSun;
//	_tpEveningView.SundayDate = upcomingSun;
//	_tpMorningView.SundayDateAsStr = _sundayDate;
//	_tpEveningView.SundayDateAsStr = _sundayDate;

//	if (File.Exists(path))
//	{
//		LoadSourceFile(path);
//		_pathToSourceFile = path;
//		_tpMorningView.PathToSourceFile = path;
//		_tpEveningView.PathToSourceFile = path;
//	}
//}

///// -----------------------------------------------------------------------------------------------------------
//private void LoadSourceFile(string pathToSrcFile)
//{
//	if (File.Exists(pathToSrcFile))
//	{
//		_txtSrcFilePath.Text = pathToSrcFile;
//		_txtSourceFile.Text = File.ReadAllText(pathToSrcFile);
//	}
//}

/// -----------------------------------------------------------------------------------------------------------
	private async void HandleGenerateClick(object sender, RoutedEventArgs e)
	{
		//try
		//{
		//	await (_tabCtrl.SelectedContent as TeXPdfView)?.GeneratePdf();
		//}
		//catch (Exception ex)
		//{
		//	DialogContent.Show(ex.Message, DialogType.Ok, _dialog);
		//}
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void HandleOpenInTeXWorksClick(object sender, RoutedEventArgs e)
	{
		//(_tabCtrl.SelectedContent as TeXPdfView)?.OpenInTeXWorks();
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void HandleOpenInDefaultPdfViewerClick(object sender, RoutedEventArgs e)
	{
		//(_tabCtrl.SelectedContent as TeXPdfView)?.OpenInDefaultPdfViewer(_dialog);
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

	///// -----------------------------------------------------------------------------------------------------------
	//private void HandleSaveButtonClick(object sender, RoutedEventArgs e)
	//{
	//	if (File.Exists(_txtSrcFilePath.Text))
	//		File.WriteAllText(_txtSrcFilePath.Text, _txtSourceFile.Text);
	//}

	///// -----------------------------------------------------------------------------------------------------------
	//private void HandleTabSelectionChanged(object sender, SelectionChangedEventArgs e)
	//{
	//	var view = _tabCtrl.SelectedContent as TeXPdfView;
		
	//	_btnGeneratePdf.IsEnabled = view != null;
	//	_mnuOpen.IsEnabled = view != null;

	//	_txtConfessionSin.IsEnabled = view?.CanSetConfessionSinSpecs == true;
	//	_cboFontSzSin.IsEnabled = view?.CanSetConfessionSinSpecs == true;
	//	_cboSpacingSin.IsEnabled = view?.CanSetConfessionSinSpecs == true;
	//	RefreshConfessionFaithControls(view);
		
	//	view?.LoadOrGenerateTeXFile(_pathToSourceFile);
	//}

	///// -----------------------------------------------------------------------------------------------------------
	//private void HandleConfessionComboSelectionChanged(object sender, SelectionChangedEventArgs e)
	//{
	//	RefreshConfessionInfo();
	//}

	///// -----------------------------------------------------------------------------------------------------------
	//private void HandleNiceneCreedModeClicked(object sender, RoutedEventArgs e)
	//{
	//	RefreshConfessionInfo();
	//	RefreshConfessionFaithControls(_tabCtrl.SelectedContent as TeXPdfView);
	//}

	///// -----------------------------------------------------------------------------------------------------------
	//private void RefreshConfessionFaithControls(TeXPdfView view)
	//{
	//	_txtConfessionFaith.IsEnabled = view?.CanSetConfessionFaithSpecs == true && !InNiceneCreedMode;
	//	_cboFontSzFaith.IsEnabled = view?.CanSetConfessionFaithSpecs == true && !InNiceneCreedMode;
	//	_cboSpacingFaith.IsEnabled = view?.CanSetConfessionFaithSpecs == true && !InNiceneCreedMode;
	//	_chkNiceneCreedMode.IsEnabled = view?.CanSetConfessionFaithSpecs == true;
	//}

	///// -----------------------------------------------------------------------------------------------------------
	//private void RefreshConfessionInfo()
	//{
	//	if (_tabCtrl?.SelectedContent is not TeXPdfView view)
	//		return;

	//	var fontSz = ((ComboBoxItem)_cboFontSzSin.SelectedItem).Content as string;
	//	var paraSpacing = ((ComboBoxItem)_cboSpacingSin.SelectedItem).Content as string;
	//	var cmi = new ConfessionMarkupInfo(ConfessionType.Sin, fontSz == "Small", paraSpacing);
	//	view.SetConfessionMarkupInfo(cmi);

	//	fontSz = ((ComboBoxItem)_cboFontSzFaith.SelectedItem).Content as string;
	//	paraSpacing = ((ComboBoxItem)_cboSpacingFaith.SelectedItem).Content as string;
	//	cmi = _chkNiceneCreedMode.IsChecked == true ?
	//		ConfessionMarkupInfo.CreateForNiceneCreed() :
	//		new ConfessionMarkupInfo(ConfessionType.Faith, fontSz == "Small", paraSpacing);

	//	view.SetConfessionMarkupInfo(cmi);
	//}
}
