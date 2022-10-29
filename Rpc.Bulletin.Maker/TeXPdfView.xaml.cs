using MaterialDesignThemes.Wpf;
using Rpc.TeX.Library;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Rpc.Bulletin.Maker;

public enum BulletinType
{
	NA,
	AM,
	PM,
}

public partial class TeXPdfView : UserControl
{
	private string _sundayDateAsStr;
	private BulletinType _bulletinType;

	public string PathToSourceFile { get; set; }
	public DateTime SundayDate { get; set; }
	public bool CanSetConfessionSinSpecs { get; set; }
	public bool CanSetConfessionFaithSpecs { get; set; }
	public string ConfessionSinFontSz { get; set; }
	public string ConfessionSinParaSpacing { get; set; }
	public string PathToTeXFile => _txtTeXFilePath?.Text;
	public string PathToPdfFile => _txtPdfFilePath?.Text;

	public ConfessionMarkupInfo _confessionSinMarkupInfo = new ConfessionMarkupInfo(ConfessionType.Sin);
	public ConfessionMarkupInfo _confessionFaithMarkupInfo = new ConfessionMarkupInfo(ConfessionType.Faith);

	/// -----------------------------------------------------------------------------------------------------------
	public TeXPdfView()
	{
		InitializeComponent();
	}

	/// -----------------------------------------------------------------------------------------------------------
	public string SundayDateAsStr
	{
		get => _sundayDateAsStr;
		set
		{
			_sundayDateAsStr = value;
			TrySetPaths();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public BulletinType BulletinType
	{
		get => _bulletinType;
		set
		{
			_bulletinType = value;
			TrySetPaths();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void TrySetPaths()
	{
		if (!string.IsNullOrWhiteSpace(_sundayDateAsStr))
		{
			_txtTeXFilePath.Text = Path.Combine(Properties.Settings.Default.TeXFileFolder,
				$"{_sundayDateAsStr}_{BulletinType}.tex");

			_txtPdfFilePath.Text = Path.Combine(Properties.Settings.Default.TeXFileFolder,
				$"{_sundayDateAsStr}_{BulletinType}.pdf");
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public void LoadOrGenerateTeXFile(string pathToSourceFile, bool forceGenerate = false)
	{
		PathToSourceFile ??= pathToSourceFile;

		if (!forceGenerate && File.Exists(PathToTeXFile))
		{
			TryReadTeXFile();
			return;
		}

		var parser = new FileParser(PathToSourceFile);
		var section = BulletinType == BulletinType.AM ? "MORNING" : "EVENING";

		var template = BulletinType == BulletinType.AM ?
			Properties.Settings.Default.TeXMorningBulletinTemplate :
			Properties.Settings.Default.TeXEveningBulletinTemplate;

		var processor = new XeLaTexBuilder(template, _confessionSinMarkupInfo, _confessionFaithMarkupInfo);

		parser.ReadAndProcessText(section, processor.ProcessText);
		_txtTeXFileContent.Text = processor.GetMarkup();
		File.WriteAllText(PathToTeXFile, _txtTeXFileContent.Text);
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void TryReadTeXFile()
	{
		if (File.Exists(PathToTeXFile))
			_txtTeXFileContent.Text = File.ReadAllText(PathToTeXFile);
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

		File.WriteAllText(PathToTeXFile, _txtTeXFileContent.Text);
		return true;
	}

	/// -----------------------------------------------------------------------------------------------------------
	public void OpenInTeXWorks()
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
	public void OpenInDefaultPdfViewer(DialogHost dlgHost)
	{
		if (!File.Exists(PathToPdfFile))
		{
			DialogContent.Show("You must first generate the PDF file.", DialogType.Ok, dlgHost);
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

	/// -----------------------------------------------------------------------------------------------------------
	public async Task<bool> GeneratePdf()
	{
		if (!TrySaveTeXFile())
			return false;

		_txtGenerateOutput.Clear();

		if (!_drawerHost.IsBottomDrawerOpen)
		{
			_toggleOutputPanel.IsChecked = false;
			_drawerHost.IsBottomDrawerOpen = true;
		}

		var targetFile = Path.GetFileNameWithoutExtension(PathToTeXFile);

		var pathToPdfFile = await PdfGenerator.Generate(Properties.Settings.Default.PdfGeneratorExe,
			PathToTeXFile, Properties.Settings.Default.TargetFilesFolder, targetFile, text =>
			{
				Dispatcher.BeginInvoke(() =>
				{
					_txtGenerateOutput.AppendText($"{text}\n\n");
					_txtGenerateOutput.ScrollToEnd();
				});
			});

		if (!File.Exists(pathToPdfFile))
			return false;

		await Dispatcher.BeginInvoke(() =>
		{
			_txtPdfFilePath.Text = pathToPdfFile;
			LoadPdf(pathToPdfFile);
		});

		return true;
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void LoadPdf(string pathToPdf)
	{
		if (_webView.CoreWebView2 != null)
		{
			var uri = File.Exists(pathToPdf) ? $"file:///{pathToPdf.Replace('\\', '/')}" : "about:blank";
			_webView.CoreWebView2.Navigate(uri);
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public void SetConfessionMarkupInfo(ConfessionMarkupInfo cmi)
	{
		if (cmi.ConfessionType == ConfessionType.Sin)
			_confessionSinMarkupInfo = cmi;
		else
			_confessionFaithMarkupInfo = cmi;

		LoadOrGenerateTeXFile(null, true);
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void HandleToggleOutputPanel(object sender, RoutedEventArgs e)
	{
		_drawerHost.IsBottomDrawerOpen = _toggleOutputPanel.IsChecked == false;
	}
}
