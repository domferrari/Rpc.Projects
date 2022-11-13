using MaterialDesignThemes.Wpf;
using Rpc.TeX.Library;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Rpc.Bulletin.Maker.ViewModels;

public enum BulletinType
{
	NA,
	AM,
	PM,
}

public abstract class ViewModelBase : INotifyPropertyChanged
{
	protected DialogHost _dlgHost;

	public string[] AvailableFontSizes { get; } = new[] { "Normal", "Small" };
	public string[] AvailableParaSpacing { get; } = new[] { "8pt", "7pt", "6pt", "5pt", "4pt", "3pt" };

	public virtual SnackbarMessageQueue SnackbarMsgQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
	public virtual bool CanGeneratePdf { get; set; }
	public virtual bool CanOpenPdfExternally { get; set; }
	public virtual bool CanOpenTeXExternally { get; set; }
	public virtual bool CanSetConfessionSinInfo { get; set; }
	public virtual bool CanSetConfessionFaithInfo { get; set; }
	public virtual bool CanSetNiceneCreedMode { get; set; }
	public virtual ConfessionMarkupInfo ConfessionSinMarkupInfo { get; set; } = new ConfessionMarkupInfo(ConfessionType.Sin);
	public virtual ConfessionMarkupInfo ConfessionFaithMarkupInfo { get; set; } = new ConfessionMarkupInfo(ConfessionType.Faith);

	public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	public virtual bool InNiceneCreedMode
	{
		get => ConfessionFaithMarkupInfo?.NiceneCreedMode == true;
		set
		{
			if (ConfessionFaithMarkupInfo != null)
				ConfessionFaithMarkupInfo.NiceneCreedMode = value;
		}
	}

	public abstract void OpenInTeXWorks();
	public abstract void OpenInDefaultPdfViewer();
	public abstract Task<bool> GeneratePdf();
	public abstract void RefreshView();

	public ViewModelBase(DialogHost dlgHost)
	{
		_dlgHost = dlgHost;
	}
}