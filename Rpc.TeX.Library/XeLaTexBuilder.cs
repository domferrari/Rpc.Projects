using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Rpc.TeX.Library
{
	public class XeLaTexBuilder
	{
		private string _markupText;
		private bool _dateFound;
		private ConfessionMarkupInfo _sinConfessionMarkupInfo;
		private ConfessionMarkupInfo _faithConfessionMarkupInfo;
		private const string kDateField = "DATE";
		
		/// -----------------------------------------------------------------------------------------------------------
		public XeLaTexBuilder(string pathToTemplate, ConfessionMarkupInfo sinConfessionMarkupInfo,
			ConfessionMarkupInfo faithConfessionMarkupInfo)
		{
			_sinConfessionMarkupInfo = sinConfessionMarkupInfo;
			_faithConfessionMarkupInfo = faithConfessionMarkupInfo;
			_markupText = File.ReadAllText(pathToTemplate);
		}

		/// -----------------------------------------------------------------------------------------------------------
		public void ProcessText(string field, string text)
		{
			var pattern = $"<<{field.Trim()}>>";

			if (field.StartsWith("CONFESSION") && !field.EndsWith("ATTRIBUTION"))
				text = GetConfessionMarkup(field, text);
			else if (field.StartsWith("COLLECTION") && text.ToUpper() == "Y")
			{
				// In this case, we don't want to insert any text in the .tex file, we want to
				// remove some to the markup becomes active (i.e. not commented out).
				pattern = "%<<COLLECTION>>";
				text = string.Empty;
			}

			_dateFound = _dateFound || (field == kDateField && !string.IsNullOrWhiteSpace(text));
			_markupText = _markupText.Replace(pattern, text.Trim());
		}

		/// -----------------------------------------------------------------------------------------------------------
		private string GetConfessionMarkup(string pattern, string text)
		{
			var paragraphSpacing = pattern.Contains("SIN") ?
				_sinConfessionMarkupInfo.SpaceBetweenParagraphs :
				_faithConfessionMarkupInfo.SpaceBetweenParagraphs;

			var parMarkup = $"\\par\\vspace{{{paragraphSpacing}}}";

			text = text.Trim();
			text = text.Replace("  ", " ");
			text = text.Replace($"{Environment.NewLine}{Environment.NewLine}", parMarkup);
			text = text.Replace($"\n\n", parMarkup);
			text = text.Replace($"\r\r", parMarkup);
			text = text.Replace($"{Environment.NewLine}", " ");

			text = TryAddBoldMarkup("Leader:", text);
			text = TryAddBoldMarkup("Minister:", text);
			text = TryAddBoldMarkup("Congregation:", text);

			return $"{text}\\par";

			//var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			//var bldr = new StringBuilder();

			//foreach (var line in lines)
			//{
			//	var confessionLine = $"{line.Trim()} ";
			//	confessionLine = TryAddBoldMarkup("Leader:", confessionLine);
			//	confessionLine = TryAddBoldMarkup("Minister:", confessionLine);
			//	confessionLine = TryAddBoldMarkup("Congregation:", confessionLine);

			//	bldr.Append(confessionLine);
			//	bldr.AppendLine(@"\par");

			//	if (line != lines.Last())
			//	{
			//		var paragraphSpacing = pattern.Contains("SIN") ?
			//			_sinConfessionMarkupInfo.SpaceBetweenParagraphs :
			//			_faithConfessionMarkupInfo.SpaceBetweenParagraphs;

			//		bldr.AppendLine($"\\vspace{{{paragraphSpacing}}}");
			//	}
			//}

			//return bldr.ToString();
		}


		/// -----------------------------------------------------------------------------------------------------------
		private string TryAddBoldMarkup(string textToEmbolden, string fullText)
		{
			return fullText.Replace(textToEmbolden, $"\\textbf{{{textToEmbolden}}}");
		}


		///// -----------------------------------------------------------------------------------------------------------
		//private string TryAddBoldMarkup(string textToEmbolden, string confessionLine)
		//{
		//	if (!confessionLine.ToLower().StartsWith(textToEmbolden.ToLower()))
		//		return confessionLine;

		//	confessionLine = confessionLine.Substring(textToEmbolden.Length);
		//	return $"\\textbf{{{textToEmbolden}}}{confessionLine}";
		//}

		/// -----------------------------------------------------------------------------------------------------------
		public string GetMarkup()
		{
			if (!_dateFound)
			{
				var nextSunday = Utils.GetNextSunday();
				var pattern = $"<<{kDateField}>>";
				_markupText = _markupText.Replace(pattern, nextSunday.ToString("dddd, dd MMMM yyyy"));
			}

			var text = _sinConfessionMarkupInfo.UseSmallFont ? @"\small" : string.Empty;
			_markupText = _markupText.Replace("<<MAKE-CS-SMALL>>", text);

			text = _faithConfessionMarkupInfo.UseSmallFont ? @"\small" : string.Empty;
			_markupText = _markupText.Replace("<<MAKE-CF-SMALL>>", text);

			text = _faithConfessionMarkupInfo.SpaceBetweenParagraphs;
			_markupText = _markupText.Replace("<<CF-PARA-SPACING>>", text);

			text = _faithConfessionMarkupInfo.NiceneCreedMode ? "%NICENE-CREED" : "%NO-NICENE-CREED";
			_markupText = _markupText.Replace(text, string.Empty);

			_markupText = _markupText.Replace("<<CONFESSION-SIN-ATTRIBUTION>>", string.Empty);
			_markupText = _markupText.Replace("<<CONFESSION-FAITH-ATTRIBUTION>>", string.Empty);

			return _markupText;
		}
	}
}
