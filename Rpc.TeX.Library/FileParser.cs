using System.Text.RegularExpressions;

namespace Rpc.TeX.Library
{
	public class FileParser
	{
		private string _pathToFile;

		public string FileContents { get; }

		/// -----------------------------------------------------------------------------------------------------------
		public FileParser(string pathToFile)
		{
			_pathToFile = pathToFile;
			FileContents = ReadFile();
		}

		/// -----------------------------------------------------------------------------------------------------------
		private string ReadFile()
		{
			if (File.Exists(_pathToFile))
				return File.ReadAllText(_pathToFile);
		
			throw new FileNotFoundException($"Input file {_pathToFile} not found.", _pathToFile);
		}

		/// -----------------------------------------------------------------------------------------------------------
		public void ReadAndProcessText(string worshipServiceSection, Action<string, string> textProcessor)
		{
			var sectionContent = GetSection(worshipServiceSection);

			if (sectionContent == XeLaTexBuilder.EMPTY_SRC_DATA)
                textProcessor(sectionContent, null);
			else
			{
                var regEx = new Regex(@"^(?<field>[-()/A-Z1-3]+)\s*:\s*<<\s*(?<text>[^>]+)>>",
                    RegexOptions.Multiline | RegexOptions.Compiled);

                foreach (var match in regEx.Matches(sectionContent).Where(m => m.Success))
                    textProcessor(match.Groups["field"].Value, match.Groups["text"].Value);
            }
        }

		/// -----------------------------------------------------------------------------------------------------------
		private string GetSection(string section)
		{
			var pattern = @"^{0}\s*%(?<bulletin>[^%]+)%";
			var regEx = new Regex(string.Format(pattern, section), RegexOptions.Multiline | RegexOptions.Compiled);
			var match = regEx.Match(FileContents);

			if (match.Success && match.Groups["bulletin"].Success)
				return match.Groups["bulletin"].Value;

			return XeLaTexBuilder.EMPTY_SRC_DATA;
		}
	}
}
