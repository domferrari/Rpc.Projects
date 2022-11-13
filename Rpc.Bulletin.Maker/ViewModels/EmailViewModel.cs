using MailKit.Net.Smtp;
using MaterialDesignThemes.Wpf;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Rpc.Bulletin.Maker.ViewModels
{
	public class EmailViewModel : ViewModelBase
	{
		private bool _sendForReview = true;
		private string _bcc;
		private string _cc;
		private Func<(string morningPdf, string eveningPdf)> _pdfFileNameProvider;
		private Func<string> _emailPasswordProvider;

		public string MorningPdfFileName => Path.GetFileName(_pdfFileNameProvider?.Invoke().morningPdf);
		public string EveningPdfFileName => Path.GetFileName(_pdfFileNameProvider?.Invoke().eveningPdf);

		public Visibility MorningPdfFileNameVisible =>
			string.IsNullOrWhiteSpace(MorningPdfFileName) ? Visibility.Collapsed : Visibility.Visible;

		public Visibility EveningPdfFileNameVisible =>
			string.IsNullOrWhiteSpace(EveningPdfFileName) ? Visibility.Collapsed : Visibility.Visible;

		public Command SendEmailCmd { get; }

		/// -----------------------------------------------------------------------------------------------------------
		public EmailViewModel(DialogHost dlgHost, Func<(string morningPdf, string eveningPdf)> pdfFileNameProvider,
			Func<string> emailPasswordProvider) : base(dlgHost)
		{
			_pdfFileNameProvider = pdfFileNameProvider;
			_emailPasswordProvider = emailPasswordProvider;

			SendEmailCmd = new Command(_ => SendEmail(), _ => true);
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string EmailSmtpServer
		{
			get => Properties.Settings.Default.EmailSmtpServer;
			set
			{
				Properties.Settings.Default.EmailSmtpServer = value;
				Properties.Settings.Default.Save();
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string EmailPort
		{
			get => Properties.Settings.Default.EmailPort;
			set
			{
				Properties.Settings.Default.EmailPort = value;
				Properties.Settings.Default.Save();
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public bool UseSsl
		{
			get => Properties.Settings.Default.UseSsl;
			set
			{
				Properties.Settings.Default.UseSsl = value;
				Properties.Settings.Default.Save();
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string EmailUserId
		{
			get => Properties.Settings.Default.EmailUserId;
			set
			{
				Properties.Settings.Default.EmailUserId = value;
				Properties.Settings.Default.Save();
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string Cc
		{
			get => _cc;
			set
			{
				_cc = value;
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string Bcc
		{
			get => _bcc;
			set
			{
				_bcc = value;
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public bool SendForReview
		{
			get => _sendForReview;
			set
			{
				_sendForReview = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(EmailRecipients));
				OnPropertyChanged(nameof(EmailSubject));
				OnPropertyChanged(nameof(EmailBody));
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string EmailFrom
		{
			get => Properties.Settings.Default.EmailFrom;
			set
			{
				Properties.Settings.Default.EmailFrom = value;
				Properties.Settings.Default.Save();
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string EmailSubject
		{
			get => _sendForReview ? Properties.Settings.Default.ReviewEmailSubject : Properties.Settings.Default.EmailSubject;
			set
			{
				if (_sendForReview)
					Properties.Settings.Default.ReviewEmailSubject = value;
				else
					Properties.Settings.Default.EmailSubject = value;

				Properties.Settings.Default.Save();
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string EmailBody
		{
			get => _sendForReview ? Properties.Settings.Default.ReviewEmailBody : Properties.Settings.Default.EmailBody;
			set
			{
				if (_sendForReview)
					Properties.Settings.Default.ReviewEmailBody = value;
				else
					Properties.Settings.Default.EmailBody = value;

				Properties.Settings.Default.Save();
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string EmailRecipients
		{
			get
			{
				return _sendForReview ?
					Properties.Settings.Default.ReviewEmailRecipients : Properties.Settings.Default.EmailRecipients;
			}
			set
			{
				if (_sendForReview)
					Properties.Settings.Default.ReviewEmailRecipients = value;
				else
					Properties.Settings.Default.EmailRecipients = value;

				Properties.Settings.Default.Save();
				OnPropertyChanged();
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		private ICollection<string> GetEmailRecipients()
		{
			var recipients = _sendForReview ?
				Properties.Settings.Default.ReviewEmailRecipients :
				Properties.Settings.Default.EmailRecipients;

			var options = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
			return recipients.Split(';', options);
		}

		/// -----------------------------------------------------------------------------------------------------------
		public override void RefreshView()
		{
			OnPropertyChanged(nameof(MorningPdfFileName));
			OnPropertyChanged(nameof(EveningPdfFileName));
			OnPropertyChanged(nameof(MorningPdfFileNameVisible));
			OnPropertyChanged(nameof(EveningPdfFileNameVisible));
			OnPropertyChanged(nameof(EmailRecipients));
			OnPropertyChanged(nameof(EmailSubject));
			OnPropertyChanged(nameof(EmailBody));
		}

		/// -----------------------------------------------------------------------------------------------------------
		private void SendEmail()
		{
			try
			{
				var bldr = new BodyBuilder { TextBody = EmailBody };
				var contentType = new ContentType("application", "pdf");
				var fileNames = _pdfFileNameProvider();

				if (!string.IsNullOrWhiteSpace(fileNames.morningPdf))
				{
					var bytes = File.ReadAllBytes(fileNames.morningPdf);
					bldr.Attachments.Add(Path.GetFileName(fileNames.morningPdf), bytes, contentType);
				}

				if (!string.IsNullOrWhiteSpace(fileNames.eveningPdf))
				{
					var bytes = File.ReadAllBytes(fileNames.eveningPdf);
					bldr.Attachments.Add(Path.GetFileName(fileNames.eveningPdf), bytes, contentType);
				}

				var message = new MimeMessage();
				message.From.Add(new MailboxAddress(string.Empty, EmailFrom));
				message.To.AddRange(GetEmailRecipients().Select(r => new MailboxAddress(string.Empty, r)));
				message.Subject = EmailSubject;
				message.Body = bldr.ToMessageBody();

				using var client = new SmtpClient();
				client.Connect(EmailSmtpServer, int.Parse(EmailPort), UseSsl);
				client.Authenticate(EmailUserId, _emailPasswordProvider());
				client.Send(message);
				client.Disconnect(true);

				SnackbarMsgQueue.Enqueue("Email has been sent");
			}
			catch (Exception e)
			{
				DialogContent.Show(e.Message, DialogType.Ok, _dlgHost);
			}
		}

		public override void OpenInTeXWorks() => throw new NotImplementedException();
		public override void OpenInDefaultPdfViewer() => throw new NotImplementedException();
		public override Task<bool> GeneratePdf() => throw new NotImplementedException();
	}
}
