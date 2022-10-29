using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;

namespace Rpc.Bulletin.Maker;

public enum TeXType
{
	MorningBulletin,
	EveningBulletin,
}

public partial class PdfWebViewer : Window
{
	private static Dictionary<TeXType, PdfWebViewer> s_viewers = new();
	private string _pathToPdfFile;
	private TeXType _teXType;
	private Timer _timer;

	/// -----------------------------------------------------------------------------------------------------------
	public static void Show(string pathToPdfFile, TeXType teXType)
	{
		if (s_viewers.TryGetValue(teXType, out var viewer))
		{
			viewer.InitializeFilePath(pathToPdfFile);
			viewer.Activate();
		}
		else
		{
			viewer = new PdfWebViewer(pathToPdfFile, teXType);
			s_viewers[teXType] = viewer;
			viewer.Show();
		}
	}

	/// -----------------------------------------------------------------------------------------------------------
	public static void Shutdown()
	{
		foreach (var viewer in s_viewers.Values)
			viewer.Close();
	}

	/// -----------------------------------------------------------------------------------------------------------
	private PdfWebViewer(string pathToPdfFile, TeXType teXType) : this()
	{
		_teXType = teXType;
		InitializeFilePath(pathToPdfFile);
	}

	/// -----------------------------------------------------------------------------------------------------------
	public PdfWebViewer()
	{
		InitializeComponent();
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void InitializeFilePath(string pathToFile)
	{
		_txtFilePath.Text = pathToFile;
		_pathToPdfFile = $"file:///{pathToFile.Replace('\\', '/')}";
	}

	/// -----------------------------------------------------------------------------------------------------------
	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);

		_timer = new Timer(100);
		_timer.Elapsed += HandleTimerTick;
		_timer.Start();
	}

	/// -----------------------------------------------------------------------------------------------------------
	private void HandleTimerTick(object s, ElapsedEventArgs e)
	{
		Dispatcher.Invoke(() =>
		{
			if (_webView.CoreWebView2 != null)
			{
				_timer.Stop();
				_timer.Dispose();
				_timer = null;
				_webView.CoreWebView2.Navigate(_pathToPdfFile);
			}
		});
	}

	/// -----------------------------------------------------------------------------------------------------------
	protected override void OnClosing(CancelEventArgs e)
	{
		if (s_viewers.ContainsKey(_teXType))
			s_viewers.Remove(_teXType);

		_webView?.Dispose();
		base.OnClosing(e);
	}
}
