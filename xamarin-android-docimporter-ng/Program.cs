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
			new Driver ().Run (args);
		}

		static bool ClassContains (XElement e, string cls)
		{
			return e.Attribute ("class")?.Value?.Split (' ')?.Contains (cls) == true;
		}

		string GetTypeLinkJni (XElement a)
		{
			if (a == null) // android.support.test.InstrumentationRegistry falls in this path.
				return null;
			var extendsHref = a.Attribute ("href").Value;
			var extendsJni = extendsHref.Substring (0, extendsHref.Length - ".html".Length);
			extendsJni = extendsJni.Substring (extendsJni.IndexOf ("/reference/", StringComparison.Ordinal) + "/reference/".Length);
			return extendsJni;
		}

		string [] excludes = new string [] {
			"classes.html",
			"hierarchy.html",
			"index.html",
			"package-summary.html",
			"packages-wearable-support.html",
			"packages-wearable-support.html",
		};
		string [] non_frameworks = new string [] {
			"android.support.",
			"com.google.android.gms.",
			"renderscript."
		};

		public TextWriter Verbose = TextWriter.Null;

		void Run (string [] args)
		{
			var options = CreateOptions (args);
			if (options.Verbose)
				Verbose = Console.Error;
			Verbose.WriteLine (options.DocumentDirectory);

			string referenceDocsTopDir = Path.Combine (options.DocumentDirectory, "reference");
			var htmlFiles = Directory.GetDirectories (referenceDocsTopDir).SelectMany (d => Directory.GetFiles (d, "*.html", SearchOption.AllDirectories));

			var javaApi = new JavaApi ();

			foreach (var htmlFile in htmlFiles) {

				// skip irrelevant files.
				if  (excludes.Any (x => htmlFile.EndsWith (x, StringComparison.OrdinalIgnoreCase)))
					continue;
				var packageName = Path.GetDirectoryName (htmlFile).Substring (referenceDocsTopDir.Length + 1).Replace ('/', '.');
				if (options.FrameworkOnly && non_frameworks.Any (n => packageName.StartsWith (n, StringComparison.Ordinal)))
					continue;

				Verbose.WriteLine ("-- " + htmlFile);
				var doc = new HtmlLoader ().GetJavaDocFile (htmlFile);
				var docCol = doc.Descendants ().FirstOrDefault (e => e.Name.LocalName == "div"
				                                                 && e.Attribute ("id")?.Value == "doc-col"
				                                                 && e.Nodes ().Any (
					                                                 n => n.NodeType == System.Xml.XmlNodeType.Comment
					                                                 && ((XComment) n).Value.Contains ("START OF CLASS DATA")));

				if (docCol == null)
					continue;

				var header = docCol.Descendants ().FirstOrDefault (e => e.Attribute ("id")?.Value == "jd-header");
				var content = docCol.Descendants ().FirstOrDefault (e => e.Attribute ("id")?.Value == "jd-content");

				var apiSignatureTokens = header.Value.Replace ('\r', ' ').Replace ('\n', ' ').Replace ('\t', ' ').Trim ();
				if (apiSignatureTokens.Contains ("extends "))
					apiSignatureTokens = apiSignatureTokens.Substring (0, apiSignatureTokens.IndexOf ("extends ", StringComparison.Ordinal)).Trim ();
				bool isClass = apiSignatureTokens.Contains ("class");
				Verbose.WriteLine (apiSignatureTokens);

				var javaPackage = javaApi.Packages.FirstOrDefault (p => p.Name == packageName);
				if (javaPackage == null) {
					javaPackage = new JavaPackage (javaApi) { Name = packageName };
					javaApi.Packages.Add (javaPackage);
				}

				var javaType = isClass ? (JavaType) new JavaClass (javaPackage) : new JavaInterface (javaPackage);
				javaType.Name = apiSignatureTokens.Substring (apiSignatureTokens.LastIndexOf (' ') + 1);
				javaPackage.Types.Add (javaType);

				string sectionType = null;
				var sep = new string [] { ", " };
				var ssep = new char [] { ' ' };
				foreach (var child in content.Elements ()) {
					if (child.Name == "h2") {
						sectionType = child.Value;
						continue;
					}
					switch (sectionType) {
					case "Public Constructors":
					case "Protected Constructors":
					case "Public Methods":
					case "Protected Methods":
						break;
					default:
						continue;
					}
					if (child.Name != "a" || child.Attribute ("name") == null)
						continue;
					
					var h4 = (child.XPathSelectElement ("following-sibling::div") as XElement)?.Elements ("h4")?.FirstOrDefault (e => ClassContains (e, "jd-details-title"));
					if (h4 == null)
						continue;

					string sig = h4.Value.Replace ('\n', ' ').Replace ('\r', ' ').Trim ();
					if (!sig.Contains ('('))
						continue;
					JavaMethodBase javaMethod = null;
					string name = sig.Substring (0, sig.IndexOf ('(')).Split (ssep, StringSplitOptions.RemoveEmptyEntries).Last ();
					switch (sectionType) {
					case "Public Constructors":
					case "Protected Constructors":
						javaMethod = new JavaConstructor (javaType) { Name = name };
						break;
					case "Public Methods":
					case "Protected Methods":
						string mname = sig.Substring (0, sig.IndexOf ('('));
						javaMethod = new JavaMethod (javaType) { Name = name };
						break;
					}
					javaType.Members.Add (javaMethod);

					var parameters = sig.Substring (sig.IndexOf ('(') + 1).TrimEnd (')')
							    .Split (sep, StringSplitOptions.RemoveEmptyEntries)
					                    .Select (s => s.Trim ())
					                    .ToArray ();
					foreach (var p in parameters.Select (pTN => pTN.Split (' ')))
						javaMethod.Parameters.Add (new JavaParameter (javaMethod) { Name = p [1], Type = p [0] });
				}
			}

			var output = options.OutputFile != null ? File.CreateText (options.OutputFile) : Console.Out;

			var writer = XmlWriter.Create (output, new XmlWriterSettings { Indent = true });
			writer.WriteStartElement ("api");
			foreach (var pkg in javaApi.Packages) {
				writer.WriteStartElement ("package");
				writer.WriteAttributeString ("name", pkg.Name);
				foreach (var type in pkg.Types) {
					writer.WriteStartElement (type is JavaClass ? "class" : "interface");
					writer.WriteAttributeString ("name", type.Name);
					foreach (var method in type.Members.OfType<JavaMethodBase> ().Where (m => m.Parameters.Any ())) {
						writer.WriteStartElement (method is JavaConstructor ? "constructor" : "method");
						writer.WriteAttributeString ("name", method.Name);
						foreach (var p in method.Parameters) {
							writer.WriteStartElement ("parameter");
							writer.WriteAttributeString ("name", p.Name);
							writer.WriteAttributeString ("type", p.Type);
							writer.WriteEndElement ();
						}
						writer.WriteEndElement ();
					}
					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();
			writer.Close ();

			output.Close ();
		}

		ImporterOptions CreateOptions (string [] args)
		{
			var ret = new ImporterOptions ();
			var options = new OptionSet () {"arguments:",
				{"input=", v => ret.DocumentDirectory = v },
				{"output=", v => ret.OutputFile = v },
				{"verbose", v => ret.Verbose = true },
				{"framework-only", v => ret.FrameworkOnly = true },
			};
			options.Parse (args);
			return ret;
		}
	}

	public class ImporterOptions
	{
		public string DocumentDirectory { get; set; }
		public string OutputFile { get; set; }
		public bool Verbose { get; set; }
		public bool FrameworkOnly { get; set; }
	}
}
