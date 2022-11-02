using MaterialDesignThemes.Wpf;
using Rpc.TeX.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
		private readonly DialogHost _dlgHost;
		private readonly Action<string> _pdfLoadingProvider;
		private readonly Action<string> _generatePdfOutputReadyProvider;

		public BulletinType BulletinType { get; }

		public override bool CanGeneratePdf => File.Exists(_pathToTeXFile);
		public override bool CanOpenPdfExternally => File.Exists(_pathToPdfFile);
		public override bool CanOpenTeXExternally => File.Exists(_pathToTeXFile);
		public override bool CanSetConfessionSinInfo => BulletinType == BulletinType.AM;
		public override bool CanSetConfessionFaithInfo => BulletinType == BulletinType.AM && !InNiceneCreedMode;
		public override bool CanSetNiceneCreedMode => BulletinType == BulletinType.AM;

		/// -----------------------------------------------------------------------------------------------------------
		public TeXPdfViewViewModel(BulletinType bulletinType, string sundayDateAsStr, string pathToSrcFile,
			DialogHost dlgHost, Action<string> generatePdfOutputReadyProvider, Action<string> pdfLoadingProvider)
		{
			BulletinType = bulletinType;
			_pathToSrcFile = pathToSrcFile;
			_dlgHost = dlgHost;
			_sundayDateAsStr = sundayDateAsStr;
			_pdfLoadingProvider = pdfLoadingProvider;
			_generatePdfOutputReadyProvider = generatePdfOutputReadyProvider;

			TrySetPaths();
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
			get => _pathToPdfFile;
			set
			{
				_pathToPdfFile = value;
				OnPropertyChanged();
				_pdfLoadingProvider?.Invoke(value);
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

				PathToPdfFile = Path.Combine(Properties.Settings.Default.TeXFileFolder,
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
			if (!File.Exists(PathToPdfFile))
			{
				DialogContent.Show("You must first generate the PDF file.", DialogType.Ok, _dlgHost);
				return;
			}

			var startInfo = new ProcessStartInfo
			{
				FileName = $"\"{PathToPdfFile}\"",
				UseShellExecute = true,
			};

			var process = new Process { StartInfo = startInfo };
			process.Start();
		}

		///// -----------------------------------------------------------------------------------------------------------
		//public void SetConfessionMarkupInfo(ConfessionMarkupInfo cmi)
		//{
		//	if (cmi.ConfessionType == ConfessionType.Sin)
		//		ConfessionSinMarkupInfo = cmi;
		//	else
		//		_confessionFaithMarkupInfo = cmi;

		//	LoadOrGenerateTeXFile(true);
		//}

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
	}
}
