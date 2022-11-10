using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Rpc.Bulletin.Maker;

public partial class TeXPdfView : UserControl
{
	private bool _pdfViewerLoaded;
	private string _pendingLoadPdfFile;
	private Action _pendingLoadCompleteCallback;

	/// -----------------------------------------------------------------------------------------------------------
	public TeXPdfView()
	{
		InitializeComponent();
	}

	/// -----------------------------------------------------------------------------------------------------------
	public void UpdateGeneratePdfOutput(string text)
	{
		InvokeOnUIThread(() =>
		{
			if (text == null)
				_txtGenerateOutput.Clear();
			else
			{
				_txtGenerateOutput.AppendText($"{text}\n\n");
				_txtGenerateOutput.ScrollToEnd();
			}
		});
	}

	/// -----------------------------------------------------------------------------------------------------------
	public void LoadPdfFile(string pathToPdfFile, Action pendingLoadCompleteCallback)
	{
		_pendingLoadPdfFile = pathToPdfFile;
		_pendingLoadCompleteCallback = pendingLoadCompleteCallback;

		InvokeOnUIThread(() =>
		{
			if (_pdfViewerLoaded)
				TryLoadPendingPdf();

			EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = null;

			handler += (s, e) =>
			{
				_webView.NavigationCompleted -= handler;
				_pdfViewerLoaded = true;
				TryLoadPendingPdf();
			};

			_webView.NavigationCompleted += handler;
		});
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void TryLoadPendingPdf()
	{
		if (_pendingLoadPdfFile == null || !_pdfViewerLoaded)
			return;

		InvokeOnUIThread(() =>
		{
			var uri = File.Exists(_pendingLoadPdfFile) ?
				$"file:///{_pendingLoadPdfFile.Replace('\\', '/')}" : "about:blank";

			_webView.CoreWebView2.Navigate(uri);
			_pendingLoadCompleteCallback?.Invoke();
			_pendingLoadPdfFile = null;
			_pendingLoadCompleteCallback = null;
		});
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void InvokeOnUIThread(Action action)
	{
		if (Dispatcher.Thread == Thread.CurrentThread)
			action();
		else
			Dispatcher.BeginInvoke(() => action());
	}
}
