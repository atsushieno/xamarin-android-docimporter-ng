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

		public TextWriter Verbose = TextWriter.Null;

		void Run (string [] args)
		{
			var options = CreateOptions (args);
			if (options.Verbose)
				Verbose = Console.Error;
			Verbose.WriteLine (options.DocumentDirectory);

			string referenceDocsTopDir = Path.Combine (options.DocumentDirectory, "reference");
			var htmlFiles = Directory.GetFiles (referenceDocsTopDir, "*.html", SearchOption.AllDirectories);

			var javaApi = new JavaApi ();

			foreach (var htmlFile in htmlFiles) {

				// skip irrelevant files.
				if  (excludes.Any (x => htmlFile.EndsWith (x, StringComparison.OrdinalIgnoreCase)))
					continue;

				Verbose.WriteLine ("-- " + htmlFile);
				var doc = new HtmlLoader ().GetJavaDocFile (htmlFile);
				var content = doc.Descendants ().FirstOrDefault (e => e.Name.LocalName == "div"
				                                        && ClassContains (e, "api")
				                                        && e.Nodes ().Any (
					                                        n => n.NodeType == System.Xml.XmlNodeType.Comment
					                                        && ((XComment) n).Value.Contains ("START OF CLASS DATA")));

				if (content == null)
					continue;

				var sigCodeElems = content.Element ("p") // first <p> element
							  .Elements ("code")
							  .Where (e => ClassContains (e, "api-signature"));
				var apiSignatureTokens = sigCodeElems.First ().Value.Split ('\n')
								     .Select (s => s.Trim ())
				                                     .Where (s => s.Length > 0);
				var extendsElem = sigCodeElems.FirstOrDefault (e => e.Value.Trim ().StartsWith ("extends", StringComparison.Ordinal)); // can be null (java.lang.Object)
				bool isClass = apiSignatureTokens.Contains ("class");
				string extendsJni = isClass ? GetTypeLinkJni (extendsElem?.Element ("a")) : null;
				var implementsElem = sigCodeElems.FirstOrDefault (e => e.Value.Trim ().StartsWith ("implements", StringComparison.Ordinal));
				var implementsJni = implementsElem != null ? implementsElem.Elements ("a")
											   .Select (a => GetTypeLinkJni (a)) : new string [0]; 
				Verbose.WriteLine (string.Join (" ", apiSignatureTokens));
				Verbose.WriteLine (" extends " + extendsJni);
				Verbose.WriteLine (" implements " + string.Join (" ", implementsJni));

				var packageName = Path.GetDirectoryName (htmlFile).Substring (referenceDocsTopDir.Length + 1).Replace ('/', '.');
				var javaPackage = javaApi.Packages.FirstOrDefault (p => p.Name == packageName);
				if (javaPackage == null) {
					javaPackage = new JavaPackage (javaApi) { Name = packageName };
					javaApi.Packages.Add (javaPackage);
				}

				var javaType = isClass ? (JavaType) new JavaClass (javaPackage) : new JavaInterface (javaPackage);
				javaType.Name = apiSignatureTokens.Last ();
				javaPackage.Types.Add (javaType);

				string sectionType = null;
				var sep = new char [] { ',' };
				foreach (var child in content.Elements ()) {
					if (child.Name == "h2" && ClassContains (child, "api-section"))
						sectionType = child.Value;
					else if (child.Name == "div" && ClassContains (child, "api")) {
						string name = child.Elements ("h3").First (e => ClassContains (e, "api-name")).Value;
						switch (sectionType) {
						case "Constants":
							// No need to handle in this tool.
							//var constant = new JavaField (javaType) { Name = name };
							break;
						case "Fields":
							// No need to handle in this tool.
							//var field = new JavaField (javaType) { Name = name };
							break;
						default:
							JavaMethodBase javaMethod = null;
							switch (sectionType) {
							case "Public constructors":
							case "Protected constructors":
								javaMethod = new JavaConstructor (javaType) { Name = name };
								break;
							case "Public methods":
							case "Protected methods":
								javaMethod = new JavaMethod (javaType) { Name = name };
								break;
							default:
								throw new NotSupportedException ("Unexpected section: " + sectionType);
							}
							javaType.Members.Add (javaMethod);

							var aNameElemValue = child.ElementsBeforeSelf ().Last ().Attribute ("name").Value;
							var pTable = child.XPathSelectElement ("table[tr/th/text()='Parameters']");
							var retTable = child.XPathSelectElement ("table[tr/th/text()='Returns']");
							var paramTypes = aNameElemValue.Substring (0, aNameElemValue.Length - 1)
										       .Substring (aNameElemValue.IndexOf ('(') + 1)
										       .Split (sep, StringSplitOptions.RemoveEmptyEntries)
										       .Select (s => s.Trim ());
							var paramNames = pTable != null ? pTable.XPathSelectElements ("tr/td[1]").Select (t => t.Value.Trim ()) : new string [0];
							foreach (var p in paramNames.Zip (paramTypes, (n, t) => new { Name = n, Type = t }))
								javaMethod.Parameters.Add (new JavaParameter (javaMethod) { Name = p.Name, Type = p.Type });
							break;
						}
					}
				}
			}

			var writer = XmlWriter.Create (Console.Out, new XmlWriterSettings { Indent = true });
			writer.WriteStartElement ("api");
			foreach (var pkg in javaApi.Packages) {
				writer.WriteStartElement ("package");
				writer.WriteAttributeString ("name", pkg.Name);
				foreach (var type in pkg.Types) {
					writer.WriteStartElement (type is JavaClass ? "class" : "interface");
					writer.WriteAttributeString ("name", type.Name);
					foreach (var method in type.Members.OfType<JavaMethodBase> ()) {
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
		}

		ImporterOptions CreateOptions (string [] args)
		{
			var ret = new ImporterOptions ();
			var options = new OptionSet () {"arguments:",
				{"input=", v => ret.DocumentDirectory = v },
				{"output=", v => ret.OutputFile = v },
				{"verbose", v => ret.Verbose = true },
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
	}
}
