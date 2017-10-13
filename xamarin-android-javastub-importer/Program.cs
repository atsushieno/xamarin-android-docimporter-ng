using System;
using System.IO;
using System.IO.Compression;
using Mono.Options;
using Xamarin.Android.Tools.ApiXmlAdjuster;

namespace Xamarin.Android.Tools.JavaStubImporter
{
	class Driver
	{
		public static void Main (string [] args)
		{
			var options = CreateOptions (args);
			new Importer ().Import (options);
		}

		static ImporterOptions CreateOptions (string [] args)
		{
			var ret = new ImporterOptions ();
			var options = new OptionSet () {"arguments:",
				{"input=", v => ret.InputZipArchive = v },
				{"output=", v => ret.OutputFile = v },
				{"simple-format", v => ret.ParameterNamesFormat = ParameterNamesFormat.SimpleText },
				{"verbose", v => ret.DiagnosticWriter = Console.Error },
			};
			options.Parse (args);
			return ret;
		}
	}
}
