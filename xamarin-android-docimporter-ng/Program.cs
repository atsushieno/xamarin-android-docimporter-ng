using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Mono.Options;
using Xamarin.Android.Tools.ApiXmlAdjuster;

namespace Xamarin.Android.Tools.JavadocImporterNG
{
	public class Driver
	{
		public static void Main (string [] args)
		{
			var options = CreateOptions (args);
			new DroidDocImporter ().Import (options);
		}

		static ImporterOptions CreateOptions (string [] args)
		{
			var ret = new ImporterOptions ();
			var options = new OptionSet () {"arguments:",
				{"input=", v => ret.DocumentDirectory = v },
				{"output=", v => ret.OutputFile = v },
				{"verbose", v => ret.DiagnosticWriter = Console.Error },
				{"simple-format", v => ret.ParameterNamesFormat = ParameterNamesFormat.SimpleText },
				{"framework-only", v => ret.FrameworkOnly = true },
			};
			options.Parse (args);
			return ret;
		}
	}
}
