using MaterialDesignThemes.Wpf;
using Rpc.TeX.Library;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Rpc.Bulletin.Maker.ViewModels
{
	public class TeXPdfViewViewModel : ViewModelBase
	{
		private string _sundayDateAsStr;
		private string _pathToSrcFile;
		private string _pathToPdfFile;
		private string _pathToTeXFile;
		private string _teXFileContent;
		private bool _isOutputPanelOpen;
		private bool _isPdfLoaded;
		private readonly DialogHost _dlgHost;
		private readonly Action<string, Action> _pdfLoadingProvider;
		private readonly Action<string> _generatePdfOutputReadyProvider;

		public BulletinType BulletinType { get; }
		public Command RefreshTeXCmd { get; }

		public override bool CanGeneratePdf => File.Exists(_pathToTeXFile);
		public override bool CanOpenPdfExternally => File.Exists(_pathToPdfFile) && _isPdfLoaded;
		public override bool CanOpenTeXExternally => File.Exists(_pathToTeXFile);
		public override bool CanSetConfessionSinInfo => BulletinType == BulletinType.AM;
		public override bool CanSetConfessionFaithInfo => BulletinType == BulletinType.AM && !InNiceneCreedMode;
		public override bool CanSetNiceneCreedMode => BulletinType == BulletinType.AM;

		/// -----------------------------------------------------------------------------------------------------------
		public TeXPdfViewViewModel(DialogHost dlgHost, BulletinType bulletinType, string sundayDateAsStr,
			string pathToSrcFile, Action<string> generatePdfOutputReadyProvider,
			Action<string, Action> pdfLoadingProvider) : base(dlgHost)
		{
			BulletinType = bulletinType;
			_dlgHost = dlgHost;
			_pdfLoadingProvider = pdfLoadingProvider;
			_generatePdfOutputReadyProvider = generatePdfOutputReadyProvider;

			ResetSource(sundayDateAsStr, pathToSrcFile);

			RefreshTeXCmd = new Command(_ => LoadOrGenerateTeXFile(true), _ => true);
		}

		/// -----------------------------------------------------------------------------------------------------------
		public void ResetSource(string sundayDateAsStr, string pathToSrcFile, bool refreshView = false)
		{
			_pathToSrcFile = pathToSrcFile;
			_sundayDateAsStr = sundayDateAsStr;

			TrySetPaths();

			if (refreshView)
			{
				IsOutputPanelOpen = false;
				LoadOrGenerateTeXFile();
				_isPdfLoaded = false;
				RefreshView();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public bool IsOutputPanelOpen
		{
			get => _isOutputPanelOpen;
			set
			{
				_isOutputPanelOpen = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsOutputPanelToggleChecked));
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public bool IsOutputPanelToggleChecked
		{
			get => !_isOutputPanelOpen;
			set
			{
				_isOutputPanelOpen = !_isOutputPanelOpen;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsOutputPanelOpen));
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string PathToTeXFile
		{
			get => _pathToTeXFile;
			set
			{
				_pathToTeXFile = value;
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string PathToPdfFile
		{
			get => _isPdfLoaded ? _pathToPdfFile : string.Empty;
			set
			{
				_pathToPdfFile = value;
				OnPropertyChanged();
				TryLoadPdf();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string TeXFileContent
		{
			get => _teXFileContent;
			set
			{
				_teXFileContent = value;
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		private void TrySetPaths()
		{
			if (!string.IsNullOrWhiteSpace(_sundayDateAsStr))
			{
				PathToTeXFile = Path.Combine(Properties.Settings.Default.TeXFileFolder,
					$"{_sundayDateAsStr}_{BulletinType}.tex");

				PathToPdfFile = Path.Combine(Properties.Settings.Default.TargetFilesFolder,
					$"{_sundayDateAsStr}_{BulletinType}.pdf");

				LoadOrGenerateTeXFile(true);
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public void LoadOrGenerateTeXFile(bool forceGenerate = false)
		{
			if (!forceGenerate && File.Exists(PathToTeXFile))
			{
				TryReadTeXFile();
				return;
			}

			var parser = new FileParser(_pathToSrcFile);
			var section = BulletinType == BulletinType.AM ? "MORNING" : "EVENING";

			var template = BulletinType == BulletinType.AM ?
				Properties.Settings.Default.TeXMorningBulletinTemplate :
				Properties.Settings.Default.TeXEveningBulletinTemplate;

			var processor = new XeLaTexBuilder(template, ConfessionSinMarkupInfo, ConfessionFaithMarkupInfo);

			parser.ReadAndProcessText(section, processor.ProcessText);
			TeXFileContent = processor.GetMarkup();
			File.WriteAllText(PathToTeXFile, TeXFileContent);

			SnackbarMsgQueue.Enqueue($"TeX File '{Path.GetFileName(PathToTeXFile)}' Recreated");
		}

		/// -----------------------------------------------------------------------------------------------------------
		private void TryReadTeXFile()
		{
			if (File.Exists(PathToTeXFile))
				TeXFileContent = File.ReadAllText(PathToTeXFile);
		}

		/// -----------------------------------------------------------------------------------------------------------
		private bool TrySaveTeXFile()
		{
			if (string.IsNullOrWhiteSpace(PathToTeXFile))
				return false;

			//var bldr = new StringBuilder(_txtTeXFileContent.Text);

			//if (_txtTeXFileContent.Text.StartsWith("%CONFESSION:"))
			//{
			//	var index = _txtTeXFileContent.Text.IndexOf()
			//}

			File.WriteAllText(PathToTeXFile, TeXFileContent);
			return true;
		}

		/// -----------------------------------------------------------------------------------------------------------
		public override void OpenInTeXWorks()
		{
			if (!TrySaveTeXFile())
				return;

			var startInfo = new ProcessStartInfo
			{
				FileName = Properties.Settings.Default.TeXWorksExe,
				Arguments = $"\"{PathToTeXFile}\"",
			};

			var process = new Process { StartInfo = startInfo };
			process.Start();
		}

		/// -----------------------------------------------------------------------------------------------------------
		public override void OpenInDefaultPdfViewer()
		{
			if (!File.Exists(_pathToPdfFile))
			{
				DialogContent.Show("You must first generate the PDF file.", DialogType.Ok, _dlgHost);
				return;
			}

			var startInfo = new ProcessStartInfo
			{
				FileName = $"\"{_pathToPdfFile}\"",
				UseShellExecute = true,
			};

			var process = new Process { StartInfo = startInfo };
			process.Start();
		}

		/// -----------------------------------------------------------------------------------------------------------
		public override async Task<bool> GeneratePdf()
		{
			if (!TrySaveTeXFile())
				return false;

			_generatePdfOutputReadyProvider(null);

			if (!IsOutputPanelOpen)
				IsOutputPanelOpen = true;

			var targetFile = Path.GetFileNameWithoutExtension(PathToTeXFile);

			var pathToPdfFile = await PdfGenerator.Generate(Properties.Settings.Default.PdfGeneratorExe,
				PathToTeXFile, Properties.Settings.Default.TargetFilesFolder, targetFile,
					text => _generatePdfOutputReadyProvider(text));

			if (!File.Exists(pathToPdfFile))
				return false;

			Dispatcher.CurrentDispatcher.Invoke(() => PathToPdfFile = pathToPdfFile);

			return true;
		}

		/// -----------------------------------------------------------------------------------------------------------
		public override void RefreshView()
		{
			if (File.Exists(_pathToPdfFile) && !_isPdfLoaded)
				TryLoadPdf();
		}

		/// -----------------------------------------------------------------------------------------------------------
		private void TryLoadPdf()
		{
			_isPdfLoaded = false;
			OnPropertyChanged(nameof(PathToPdfFile));

			_pdfLoadingProvider?.Invoke(_pathToPdfFile, () =>
			{
				_isPdfLoaded = true;
				OnPropertyChanged(nameof(PathToPdfFile));
			});
		}
	}
}
