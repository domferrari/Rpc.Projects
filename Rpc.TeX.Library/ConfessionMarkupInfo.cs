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
		public ConfessionType ConfessionType;
		public bool UseSmallFont { get; private set; }
		public bool NiceneCreedMode { get; set; }

		/// -----------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Normally use '8pt'. For long confessions, try '6pt'
		/// </summary>
		/// -----------------------------------------------------------------------------------------------------------
		public string SpaceBetweenParagraphs { get; private set; }

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
		public static ConfessionMarkupInfo CreateForNiceneCreed()
		{
			return new ConfessionMarkupInfo(ConfessionType.Faith, true, "3pt") {  NiceneCreedMode = true };
		}

		/// -----------------------------------------------------------------------------------------------------------
		public override string ToString()
		{
			return $"CONFESSION: {ConfessionType},{UseSmallFont},{SpaceBetweenParagraphs},{NiceneCreedMode}";
		}
	}
}
