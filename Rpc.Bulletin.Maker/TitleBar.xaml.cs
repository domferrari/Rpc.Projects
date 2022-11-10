using System;
using System.Windows;
using System.Windows.Controls;

namespace Rpc.Bulletin.Maker
{
	/// <summary>
	/// Taken from https://engy.us/blog/2020/01/01/implementing-a-custom-window-title-bar-in-wpf/
	/// </summary>
	public partial class TitleBar : UserControl
	{
		private Window _mainWindow;

		private void HandleMainWindowStateChanged(object sender, EventArgs e) => RefreshMaximizeRestoreButton();
		private void OnCloseButtonClick(object sender, RoutedEventArgs e) => MainWindow.Close();
		private void OnMinimizeButtonClick(object sender, RoutedEventArgs e) => MainWindow.WindowState = WindowState.Minimized;

		/// -----------------------------------------------------------------------------------------------------------
		public TitleBar()
		{
			InitializeComponent();
		}

		/// -----------------------------------------------------------------------------------------------------------
		public Window MainWindow
		{
			get => _mainWindow;
			set
			{
				if (_mainWindow != null)
					_mainWindow.StateChanged -= HandleMainWindowStateChanged;
				
				_mainWindow = value;
				
				if (_mainWindow != null)
					_mainWindow.StateChanged += HandleMainWindowStateChanged;
				
				RefreshMaximizeRestoreButton();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
		{
			MainWindow.WindowState = MainWindow.WindowState == WindowState.Maximized ?
				WindowState.Normal : WindowState.Maximized;
		}

		/// -----------------------------------------------------------------------------------------------------------
		private void RefreshMaximizeRestoreButton()
		{
			_btnMaximize.Visibility = MainWindow.WindowState == WindowState.Maximized ?
				Visibility.Collapsed : Visibility.Visible;
			
			_btnRestore.Visibility = MainWindow.WindowState == WindowState.Maximized ? 
				Visibility.Visible : Visibility.Collapsed;
		}
	}
}
