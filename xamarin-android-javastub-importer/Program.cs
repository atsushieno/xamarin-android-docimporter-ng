﻿using System;
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
			ZipArchive zip;
			using (var stream = File.OpenRead (options.InputZipArchive)) {
				zip = new ZipArchive (stream);
				foreach (var ent in zip.Entries) {
					options.DiagnosticWriter.WriteLine (ent.FullName);
					if (!ent.Name.EndsWith (".java", StringComparison.OrdinalIgnoreCase))
						continue;
					var java = new StreamReader (ent.Open ()).ReadToEnd ();
					if (!ParseJava (java))
						break;
				}
			}
		}

		JavaStubGrammar grammar = new JavaStubGrammar ();

		bool ParseJava (string javaSourceText)
		{
			var parser = new Irony.Parsing.Parser (grammar);
			var result = parser.Parse (javaSourceText);
			foreach (var m in result.ParserMessages)
				Console.WriteLine ($"{m.Level} {m.Location} {m.Message}");
			return !result.HasErrors ();
		}

		Importer.ImporterOptions CreateOptions (string [] args)
		{
			var ret = new Importer.ImporterOptions ();
			var options = new OptionSet () {"arguments:",
				{"input=", v => ret.InputZipArchive = v },
				{"output=", v => ret.OutputFile = v },
				{"verbose", v => ret.DiagnosticWriter = Console.Error },
			};
			options.Parse (args);
			return ret;
		}
	}
}
