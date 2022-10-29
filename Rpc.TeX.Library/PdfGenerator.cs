using System.Diagnostics;

namespace Rpc.TeX.Library
{
	public class PdfGenerator
	{
		/// -----------------------------------------------------------------------------------------------------------
		public static async Task<string> Generate(string pathToGeneratorExe, string pathToTeXFile,
			string targetFolder, string targetFile, Action<string> outputHandler)
		{
			if (targetFile.ToLower().EndsWith(".pdf"))
				targetFile = targetFile[..^4];

			var arguments =
				$"\"{pathToTeXFile}\" " +
				$"-output-directory=\"{targetFolder}\" " +
				$"-jobname=\"{targetFile}\"";

			var startInfo = new ProcessStartInfo
			{
				FileName = pathToGeneratorExe,
				Arguments = arguments,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};

			using var process = new Process();
			process.StartInfo = startInfo;

			process.OutputDataReceived += (s, e) =>
			{
				if (e.Data != null && outputHandler != null)
					outputHandler(e.Data);
			};

			process.Start();
			process.BeginOutputReadLine();
			await process.WaitForExitAsync();
			process.Close();

			return Path.Combine(targetFolder, $"{targetFile}.pdf");
		}
	}
}
