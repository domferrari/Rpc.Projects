namespace Rpc.TeX.Library
{
	public class XeLaTexBuilder
	{
		public const string EMPTY_SRC_DATA = "*** SECTION MISSING ***";

		private const string kDateField = "DATE";

		private string _markupText;
		private DateTime _sundayDate;
		private ConfessionMarkupInfo _sinConfessionMarkupInfo;
		private ConfessionMarkupInfo _faithConfessionMarkupInfo;
		
		/// -----------------------------------------------------------------------------------------------------------
		public XeLaTexBuilder(string pathToTemplate, DateTime sundayDate,
			ConfessionMarkupInfo sinConfessionMarkupInfo, ConfessionMarkupInfo faithConfessionMarkupInfo)
		{
			_sundayDate = sundayDate;
			_sinConfessionMarkupInfo = sinConfessionMarkupInfo;
			_faithConfessionMarkupInfo = faithConfessionMarkupInfo;
			_markupText = File.ReadAllText(pathToTemplate);
		}

		/// -----------------------------------------------------------------------------------------------------------
		public void ProcessText(string field, string text)
		{
			if (field == EMPTY_SRC_DATA)
				_markupText = EMPTY_SRC_DATA;
			else
			{
                var pattern = $"<<{field.Trim()}>>";

                if (field.Trim() == "CONFESSION-SIN-ATTRIBUTION-2")
                    _sinConfessionMarkupInfo.HasTwoLineAttribution = true;
                if (field.Trim() == "CONFESSION-FAITH-ATTRIBUTION-2")
                    _faithConfessionMarkupInfo.HasTwoLineAttribution = true;
                else if (field.StartsWith("CONFESSION") && !field.Contains("ATTRIBUTION"))
                    text = GetConfessionMarkup(field, text);
                else if (field.StartsWith("COLLECTION") && text.ToUpper() == "Y")
                {
                    // In this case, we don't want to insert any text in the .tex file, we want to
                    // remove some so the markup becomes active (i.e. not commented out).
                    pattern = "%<<REM-COLLECTION>>";
                    text = string.Empty;
                }

                _markupText = _markupText.Replace(pattern, text.Trim());
            }
        }

		/// -----------------------------------------------------------------------------------------------------------
		private string GetConfessionMarkup(string pattern, string text)
		{
			var paragraphSpacing = pattern.Contains("SIN") ?
				_sinConfessionMarkupInfo.SpaceBetweenParagraphs :
				_faithConfessionMarkupInfo.SpaceBetweenParagraphs;

			var parMarkup = $" \\par\\vspace{{{paragraphSpacing}}}";

			text = text.Trim();
			text = text.Replace("  ", " ");
			text = text.Replace($"{Environment.NewLine}", "@");
			text = text.Replace($"\n", "@");
			text = text.Replace($"\r", "@");
			text = text.Replace("@@", parMarkup);
			text = text.Replace('@', ' ');

			text = TryAddBoldMarkup("Leader:", text);
			text = TryAddBoldMarkup("Minister:", text);
			text = TryAddBoldMarkup("Congregation:", text);
			text = TryAddBoldMarkup("People:", text);

			return $"{text}\\par";
		}

		/// -----------------------------------------------------------------------------------------------------------
		private string TryAddBoldMarkup(string textToEmbolden, string fullText)
		{
			return fullText.Replace(textToEmbolden, $"\\textbf{{{textToEmbolden}}}");
		}

		/// -----------------------------------------------------------------------------------------------------------
		public string GetMarkup()
		{
			var nextSunday = _sundayDate == default ? Utils.GetFollowingSunday() : _sundayDate;
			var pattern = $"<<{kDateField}>>";
			_markupText = _markupText.Replace(pattern, nextSunday.ToString("dddd, dd MMMM yyyy"));

			var text = _sinConfessionMarkupInfo.UseSmallFont ? @"\small" : string.Empty;
			_markupText = _markupText.Replace("<<MAKE-CS-SMALL>>", text);

			text = _faithConfessionMarkupInfo.UseSmallFont ? @"\small" : string.Empty;
			_markupText = _markupText.Replace("<<MAKE-CF-SMALL>>", text);

			text = _faithConfessionMarkupInfo.SpaceBetweenParagraphs;
			_markupText = _markupText.Replace("<<CF-PARA-SPACING>>", text);

			text = _faithConfessionMarkupInfo.NiceneCreedMode ? "%NICENE-CREED" : "%NO-NICENE-CREED";
			_markupText = _markupText.Replace(text, string.Empty);

			text = $"%{(_sinConfessionMarkupInfo.HasTwoLineAttribution ? "2" : "1")}-LINE-CS-ATTRIBUTION";
			_markupText = _markupText.Replace(text, string.Empty);

			if (!_faithConfessionMarkupInfo.NiceneCreedMode)
			{
				text = $"%{(_faithConfessionMarkupInfo.HasTwoLineAttribution ? "2" : "1")}-LINE-CF-ATTRIBUTION";
				_markupText = _markupText.Replace(text, string.Empty);
			}

			_markupText = _markupText.Replace("<<CONFESSION-SIN-ATTRIBUTION-1>>", string.Empty);
			_markupText = _markupText.Replace("<<CONFESSION-SIN-ATTRIBUTION-2>>", string.Empty);
			_markupText = _markupText.Replace("<<CONFESSION-FAITH-ATTRIBUTION-1>>", string.Empty);
			_markupText = _markupText.Replace("<<CONFESSION-FAITH-ATTRIBUTION-2>>", string.Empty);

			if (!_markupText.Contains("<<HYMN-SING-3-NUMBER>>"))
				_markupText = _markupText.Replace("%<<REM-HYMN3>>", string.Empty);

			if (!_markupText.Contains("<<CONFESSION-FAITH>>"))
				_markupText = _markupText.Replace("%<<REM-CONFESSION>>", string.Empty);

			return _markupText;
		}
	}
}
