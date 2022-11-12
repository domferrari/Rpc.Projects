using System.Text.RegularExpressions;

namespace Rpc.TeX.Library
{
	public enum ConfessionType
	{
		Sin,
		Faith,
	}

	public class ConfessionMarkupInfo
	{
		private string _spaceBetweenParagraphs = "8pt";
		private bool _useSmallFont;

		public ConfessionType ConfessionType;
		public bool HasTwoLineAttribution { get; set; }
		public bool NiceneCreedMode { get; set; }

		/// -----------------------------------------------------------------------------------------------------------
		public bool UseSmallFont
		{
			get => NiceneCreedMode || _useSmallFont;
			set => _useSmallFont = value;
		}

		/// -----------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Normally use '8pt'. For long confessions, try '6pt'
		/// </summary>
		/// -----------------------------------------------------------------------------------------------------------
		public string SpaceBetweenParagraphs
		{
			get => NiceneCreedMode ? "3pt" : _spaceBetweenParagraphs;
			set => _spaceBetweenParagraphs = value;
		}

		/// -----------------------------------------------------------------------------------------------------------
		private ConfessionMarkupInfo()
		{
		}

		/// -----------------------------------------------------------------------------------------------------------
		public ConfessionMarkupInfo(ConfessionType confessionType, bool useSmallFont = false,
			string spaceBetweenParagraphs = "8pt")
		{
			ConfessionType = confessionType;
			UseSmallFont = useSmallFont;
			SpaceBetweenParagraphs = spaceBetweenParagraphs;
		}

		/// -----------------------------------------------------------------------------------------------------------
		public static IEnumerable<ConfessionMarkupInfo> TryCreateFromConfigStrings(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				yield break;

			var pattern = @"^%CONFESSION:\s*(?<type>(Sin|Faith)),(?<smallFont>(True|true|False|false)),(?<paraSpacing>[^,]+),(?<niceneMode>(True|true|False|false))%";
			var regEx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);

			foreach (var match in regEx.Matches(text).Where(m => m.Success))
			{
				yield return new ConfessionMarkupInfo
				{
					ConfessionType = Enum.Parse<ConfessionType>(match.Groups["type"].Value, true),
					UseSmallFont = bool.Parse(match.Groups["smallFont"].Value),
					SpaceBetweenParagraphs = match.Groups["paraSpacing"].Value,
					NiceneCreedMode = bool.Parse(match.Groups["niceneMode"].Value),
				};
			}
		}

		/// -----------------------------------------------------------------------------------------------------------
		public override string ToString()
		{
			return $"CONFESSION: {ConfessionType},{UseSmallFont},{SpaceBetweenParagraphs},{NiceneCreedMode}";
		}
	}
}
