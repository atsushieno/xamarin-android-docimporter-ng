using System;
using System.IO;
using System.IO.Compression;
using Irony.Parsing;
using Xamarin.Android.Tools.ApiXmlAdjuster;

namespace Xamarin.Android.Tools.JavaStubImporter
{
	public class Importer
	{
		public Importer ()
		{
		}


		public void Import (ImporterOptions options)
		{
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

		JavaStubGrammar grammar = new JavaStubGrammar () { LanguageFlags = LanguageFlags.Default | LanguageFlags.CreateAst };
		JavaApi api = new JavaApi ();

		bool ParseJava (string javaSourceText)
		{
			var parser = new Irony.Parsing.Parser (grammar);
			var result = parser.Parse (javaSourceText);
			foreach (var m in result.ParserMessages)
				Console.WriteLine ($"{m.Level} {m.Location} {m.Message}");
			return !result.HasErrors ();
		}

		public class ImporterOptions
		{
			public string InputZipArchive { get; set; }
			public string OutputFile { get; set; }
			public TextWriter DiagnosticWriter { get; set; } = TextWriter.Null;
		}
	}
}
