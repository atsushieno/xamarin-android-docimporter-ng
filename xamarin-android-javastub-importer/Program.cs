using System;
using System.IO;
using System.IO.Compression;
using Mono.Options;

namespace Xamarin.Android.Tools.JavaStubImporter
{
	class Driver
	{
		public static void Main (string [] args)
		{
			new Driver ().Run (args);
		}

		public void Run (string [] args)
		{
			var options = CreateOptions (args);
			new Importer ().Import (options);
		}

		Importer.ImporterOptions CreateOptions (string [] args)
		{
			var ret = new Importer.ImporterOptions ();
			var options = new OptionSet () {"arguments:",
				{"input=", v => ret.InputZipArchive = v },
				{"output=", v => ret.OutputFile = v },
				//{"output-only-parameters", v => ret.OutputType = Importer.OutputType.ParameterNames },
				{"simple-format", v => ret.ParameterNamesFormat = Importer.ParameterNamesFormat.SimpleText },
				{"verbose", v => ret.DiagnosticWriter = Console.Error },
			};
			options.Parse (args);
			return ret;
		}
	}
}
