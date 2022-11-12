using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Rpc.TeX.Library;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Rpc.Bulletin.Maker.ViewModels;

public enum View
{
	SourceFile = 0,
	Morning = 1,
	Evening = 2,
}

public class MainWindowViewModel : ViewModelBase
{
	private string _pathToSrcFile;
	private string _prevFileContent;
	private string _srcFileContent;
	private string _sundayDateAsStr;
	private DateTime _sundayDate;
	private DialogHost _dlgHost;
	private int _selectedViewIndex;

	public TeXPdfViewViewModel MorningViewModel { get; }
	public TeXPdfViewViewModel EveningViewModel { get; }
	public Command SaveSrcFileCmd { get; }
	public Command GeneratePdfCmd { get; }
	public Command OpenSrcFileCmd { get; }
	public Command OpenInDefaultPdfViewerCmd { get; }
	public Command OpenInTeXWorksCmd { get; }

	private ViewModelBase[] _viewModels;
	private ViewModelBase CurrentVwModel => _viewModels[_selectedViewIndex];

	/// -----------------------------------------------------------------------------------------------------------
	public MainWindowViewModel(DialogHost dlgHost,
		Action<string> confessionSinGeneratePdfOutputReadyProvider,
		Action<string, Action> confessionSinPdfLoadingProvider,
		Action<string> confessionFaithGeneratePdfOutputReadyProvider,
		Action<string, Action> confessionFaithPdfLoadingProvider)
	{
		if (!Directory.Exists(Properties.Settings.Default.SourceFilesFolder))
			Directory.CreateDirectory(Properties.Settings.Default.SourceFilesFolder);

		if (!Directory.Exists(Properties.Settings.Default.TargetFilesFolder))
			Directory.CreateDirectory(Properties.Settings.Default.TargetFilesFolder);

		if (!Directory.Exists(Properties.Settings.Default.TeXFileFolder))
			Directory.CreateDirectory(Properties.Settings.Default.TeXFileFolder);

		TryFindNewSourceFile();

		_dlgHost = dlgHost;
		
		MorningViewModel = new TeXPdfViewViewModel(BulletinType.AM, _sundayDateAsStr, _pathToSrcFile,
			dlgHost, confessionSinGeneratePdfOutputReadyProvider, confessionSinPdfLoadingProvider);
		
		EveningViewModel = new TeXPdfViewViewModel(BulletinType.PM, _sundayDateAsStr, _pathToSrcFile,
			dlgHost, confessionFaithGeneratePdfOutputReadyProvider, confessionFaithPdfLoadingProvider);

		_viewModels = new[] { (ViewModelBase)this, MorningViewModel, EveningViewModel };

		SaveSrcFileCmd = new Command(_ => SaveSourceFile(), _ => true);
		OpenSrcFileCmd = new Command(_ => OpenSourceFile(), _ => true);
		OpenInTeXWorksCmd = new Command(_ => OpenInTeXWorks(), _ => CanOpenTeXExternally);
		OpenInDefaultPdfViewerCmd = new Command(_ => OpenInDefaultPdfViewer(), _ => CanOpenPdfExternally);
		GeneratePdfCmd = new Command(async _ => await GeneratePdf(), _ => CanGeneratePdf);
	}

	/// -----------------------------------------------------------------------------------------------------------
	public int SelectedViewIndex
	{
		get => _selectedViewIndex;
		set
		{
			_selectedViewIndex = value;
			OnPropertyChanged();
			RefreshView();
			CurrentVwModel.RefreshView();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public string PathToSrcFile
	{
		get => _pathToSrcFile;
		set
		{
			_pathToSrcFile = value;
			OnPropertyChanged();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public string SrcFileContent
	{
		get => _srcFileContent;
		set
		{
			_srcFileContent = value;
			OnPropertyChanged();
		}
	}

	public override bool CanGeneratePdf => CurrentVwModel != this && CurrentVwModel.CanGeneratePdf;
	public override bool CanOpenPdfExternally => CurrentVwModel != this && CurrentVwModel.CanOpenPdfExternally;
	public override bool CanOpenTeXExternally => CurrentVwModel != this && CurrentVwModel.CanOpenTeXExternally;
	public override bool CanSetConfessionSinInfo => CurrentVwModel != this && CurrentVwModel.CanSetConfessionSinInfo;
	public override bool CanSetConfessionFaithInfo => CurrentVwModel != this && CurrentVwModel.CanSetConfessionFaithInfo;
	public override bool CanSetNiceneCreedMode => CurrentVwModel != this && CurrentVwModel.CanSetNiceneCreedMode;

	public bool CanOpenExternally => CanOpenPdfExternally || CanOpenTeXExternally;

	/// -----------------------------------------------------------------------------------------------------------
	public override bool InNiceneCreedMode
	{
		get => CurrentVwModel == this ? base.InNiceneCreedMode : CurrentVwModel.InNiceneCreedMode;
		set
		{
			if (CurrentVwModel != this)
				CurrentVwModel.InNiceneCreedMode = value;
	
			RefreshView();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public string SelectedConfessionSinFontSz
	{
		get
		{
			if (CurrentVwModel == this)
				return null;

			return CurrentVwModel.ConfessionSinMarkupInfo.UseSmallFont ? AvailableFontSizes[1] : AvailableFontSizes[0];
		}
		set
		{
			if (CurrentVwModel != this)
				CurrentVwModel.ConfessionSinMarkupInfo.UseSmallFont = value == AvailableFontSizes[1];
			
			OnPropertyChanged();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public string SelectedConfessionSinParaSpacing
	{
		get
		{
			if (CurrentVwModel == this)
				return null;

			return CurrentVwModel.ConfessionSinMarkupInfo.SpaceBetweenParagraphs;
		}
		set
		{
			if (CurrentVwModel != this)
				CurrentVwModel.ConfessionSinMarkupInfo.SpaceBetweenParagraphs = value;
			
			OnPropertyChanged();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public string SelectedConfessionFaithFontSz
	{
		get
		{
			if (CurrentVwModel == this)
				return null;

			return CurrentVwModel.ConfessionFaithMarkupInfo.UseSmallFont? AvailableFontSizes[1] : AvailableFontSizes[0];
		}
		set
		{
			if (CurrentVwModel != this)
				CurrentVwModel.ConfessionFaithMarkupInfo.UseSmallFont = value == AvailableFontSizes[1];
			
			OnPropertyChanged();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public string SelectedConfessionFaithParaSpacing
	{
		get
		{
			if (CurrentVwModel == this)
				return null;

			return CurrentVwModel.ConfessionFaithMarkupInfo.SpaceBetweenParagraphs;
		}
		set
		{
			if (CurrentVwModel != this)
				CurrentVwModel.ConfessionFaithMarkupInfo.SpaceBetweenParagraphs = value;
			
			OnPropertyChanged();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public override void RefreshView()
	{
		OnPropertyChanged(nameof(CanOpenExternally));
		OnPropertyChanged(nameof(CanGeneratePdf));
		OnPropertyChanged(nameof(CanOpenTeXExternally));
		OnPropertyChanged(nameof(CanOpenPdfExternally));
		OnPropertyChanged(nameof(CanSetConfessionSinInfo));
		OnPropertyChanged(nameof(CanSetConfessionFaithInfo));
		OnPropertyChanged(nameof(CanSetNiceneCreedMode));
		OnPropertyChanged(nameof(InNiceneCreedMode));
		OnPropertyChanged(nameof(SelectedConfessionSinFontSz));
		OnPropertyChanged(nameof(SelectedConfessionSinParaSpacing));
		OnPropertyChanged(nameof(SelectedConfessionFaithFontSz));
		OnPropertyChanged(nameof(SelectedConfessionFaithParaSpacing));
		
		GeneratePdfCmd.OnCanExecuteChanged();
		OpenInTeXWorksCmd.OnCanExecuteChanged();
		OpenInDefaultPdfViewerCmd.OnCanExecuteChanged();
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void TryFindNewSourceFile()
	{
		var from = DateTime.Today;

		for (var attempts = 0; attempts < 5; attempts++)
		{
			_sundayDate = Utils.GetFollowingSunday(from);
			_sundayDateAsStr = $"{_sundayDate:yyyy-MM-dd}";
			var fileName = $"{_sundayDateAsStr}.txt";
			PathToSrcFile = Path.Combine(Properties.Settings.Default.SourceFilesFolder, fileName);

			if (!File.Exists(PathToSrcFile))
				from = from.AddDays(-7);
			else
			{
				SrcFileContent = File.ReadAllText(PathToSrcFile);
				_prevFileContent = SrcFileContent;
				return;
			}
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void SaveSourceFile()
	{
		if (_prevFileContent == _srcFileContent)
			return;

		try
		{
			File.WriteAllText(PathToSrcFile, SrcFileContent);
			SnackbarMsgQueue.Enqueue($"File '{Path.GetFileName(PathToSrcFile)}' saved");
			_prevFileContent = SrcFileContent;
			MorningViewModel.LoadOrGenerateTeXFile(true);
			EveningViewModel.LoadOrGenerateTeXFile(true);
		}
		catch (Exception e)
		{
			DialogContent.Show($"Error: {e.Message}", DialogType.Ok, _dlgHost);
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void OpenSourceFile()
	{
		var openDlg = new OpenFileDialog
		{
			CheckFileExists = true,
			Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*",
			Title = "Open Bulletin Source File",
		};

		if (openDlg.ShowDialog() != true)
			return;

		if (PathToSrcFile != null && PathToSrcFile.ToLower() == openDlg.FileName.ToLower())
			return;

		var fileName = Path.GetFileNameWithoutExtension(openDlg.FileName).Replace("-", string.Empty);
		
		if (!DateTime.TryParseExact(fileName, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var sundayDate))
		{
			DialogContent.Show($"{fileName} has an invalid name.", DialogType.Ok, _dlgHost);
			return;
		}

		_sundayDate = sundayDate;
		_sundayDateAsStr = $"{_sundayDate:yyyy-MM-dd}";
		PathToSrcFile = openDlg.FileName;
		SrcFileContent = File.ReadAllText(PathToSrcFile);
		MorningViewModel.ResetSource(_sundayDateAsStr, PathToSrcFile, true);
		EveningViewModel.ResetSource(_sundayDateAsStr, PathToSrcFile, true);
		RefreshView();
	}

	/// -----------------------------------------------------------------------------------------------------------
	public override void OpenInTeXWorks()
	{
		if (CurrentVwModel != this)
			CurrentVwModel.OpenInTeXWorks();
	}
	
	/// -----------------------------------------------------------------------------------------------------------
	public override void OpenInDefaultPdfViewer()
	{
		if (CurrentVwModel != this)
			CurrentVwModel.OpenInDefaultPdfViewer();
	}

	/// -----------------------------------------------------------------------------------------------------------
	public override async Task<bool> GeneratePdf()
	{
		if (CurrentVwModel == this)
			return false;

		var result = await CurrentVwModel.GeneratePdf();
		RefreshView();
		return result;
	}
}
