using System;
using System.Linq;
using Irony.Parsing;
using Irony.Ast;
using System.Collections.Generic;
using Xamarin.Android.Tools.ApiXmlAdjuster;

namespace Xamarin.Android.Tools.JavaStubImporter
{
	[Language ("JavaStub", "1.0", "Java Stub grammar in Android SDK (android-stubs-src.jar)")]
	public partial class JavaStubGrammar : Grammar
	{
		NonTerminal DefaultNonTerminal (string label)
		{
			var nt = new NonTerminal (label);
			nt.AstConfig.NodeCreator = delegate { throw new NotImplementedException (label); };
			return nt;
		}

		KeyTerm Keyword (string label)
		{
			var ret = ToTerm (label);
			ret.AstConfig.NodeCreator = delegate (AstContext ctx, ParseTreeNode node) {
				node.AstNode = node.Token.ValueString;
			};
			return ret;
		}

		AstNodeCreator CreateArrayCreator<T> ()
		{
			return delegate (AstContext ctx, ParseTreeNode node) {
				ProcessChildren (ctx, node);
				node.AstNode = (from n in node.ChildNodes select (T)n.AstNode).ToArray ();
			};
		}

		AstNodeCreator CreateStringFlattener ()
		{
			return delegate (AstContext ctx, ParseTreeNode node) {
				ProcessChildren (ctx, node);
				node.AstNode = string.Concat (node.ChildNodes.Select (n => n.AstNode?.ToString ()));
			};
		}

		void SelectSingleChild (AstContext ctx, ParseTreeNode node)
		{
			ProcessChildren (ctx, node);
			if (node.ChildNodes.Count == 1)
				node.AstNode = node.ChildNodes.First ().AstNode;
		}

		void ProcessChildren (AstContext ctx, ParseTreeNode node)
		{
			foreach (var cn in node.ChildNodes) {
				if (cn.Term.AstConfig.NodeCreator != null)
					cn.Term.AstConfig.NodeCreator (ctx, cn);
			}
		}

		AstNodeCreator SelectChildValueAt (int index)
		{
			return delegate (AstContext ctx, ParseTreeNode node) {
				ProcessChildren (ctx, node);
				node.AstNode = node.ChildNodes [index].AstNode;
			};
		}

		void DoNothing (AstContext ctx, ParseTreeNode node)
		{
			ProcessChildren (ctx, node);
			// do nothing except for processing children.
		}

		public JavaStubGrammar ()
		{
			CommentTerminal single_line_comment = new CommentTerminal ("SingleLineComment", "//", "\r", "\n");
			CommentTerminal delimited_comment = new CommentTerminal ("DelimitedComment", "/*", "*/");

			NonGrammarTerminals.Add (single_line_comment);
			NonGrammarTerminals.Add (delimited_comment);

			IdentifierTerminal identifier = new IdentifierTerminal ("identifier");

			KeyTerm keyword_package = Keyword ("package");
			KeyTerm keyword_import = Keyword ("import");
			KeyTerm keyword_public = Keyword ("public");
			KeyTerm keyword_protected = Keyword ("protected");
			KeyTerm keyword_static = Keyword ("static");
			KeyTerm keyword_final = Keyword ("final");
			KeyTerm keyword_abstract = Keyword ("abstract");
			KeyTerm keyword_synchronized = Keyword ("synchronized");
			KeyTerm keyword_default = Keyword ("default");
			KeyTerm keyword_native = Keyword ("native");
			KeyTerm keyword_volatile = Keyword ("volatile");
			KeyTerm keyword_transient = Keyword ("transient");
			KeyTerm keyword_enum = Keyword ("enum");
			KeyTerm keyword_class = Keyword ("class");
			KeyTerm keyword_interface = Keyword ("interface");
			KeyTerm keyword_at_interface = Keyword ("@interface");
			KeyTerm keyword_extends = Keyword ("extends");
			KeyTerm keyword_implements = Keyword ("implements");
			KeyTerm keyword_throws = Keyword ("throws");
			KeyTerm keyword_null = Keyword ("null");
			KeyTerm keyword_super = Keyword ("super");
			KeyTerm keyword_true = Keyword ("true");
			KeyTerm keyword_false = Keyword ("false");

			var compile_unit = DefaultNonTerminal ("compile_unit");
			var opt_package_decl = DefaultNonTerminal ("opt_package_declaration");
			var package_decl = DefaultNonTerminal ("package_declaration");
			var imports = DefaultNonTerminal ("imports");
			var import = DefaultNonTerminal ("import");
			var type_decls = DefaultNonTerminal ("type_decls");
			var type_decl = DefaultNonTerminal ("type_decl");
			var enum_decl = DefaultNonTerminal ("enum_decl");
			var class_decl = DefaultNonTerminal ("class_decl");
			var opt_generic_arg_decl = DefaultNonTerminal ("opt_generic_arg_decl");
			var opt_extends_decl = DefaultNonTerminal ("opt_extends_decl");
			var opt_implements_decl = DefaultNonTerminal ("opt_implements_decl");
			var implements_decl = DefaultNonTerminal ("implements_decl");
			var interface_decl = DefaultNonTerminal ("interface_decl");
			var type_body = DefaultNonTerminal ("type_body");
			var type_members = DefaultNonTerminal ("type_members");
			var type_member = DefaultNonTerminal ("type_member");
			var ctor_decl = DefaultNonTerminal ("ctor_decl");
			var method_decl = DefaultNonTerminal ("method_decl");
			var field_decl = DefaultNonTerminal ("field_decl");
			var static_ctor_decl = DefaultNonTerminal ("static_ctor_decl");
			var enum_member_initializers = DefaultNonTerminal ("enum_member_initializers");
			var enum_member_initializer = DefaultNonTerminal ("enum_member_initializer");
			var terminate_decl_or_body = DefaultNonTerminal ("terminate_decl_or_body");
			var assignments = DefaultNonTerminal ("assignments");
			var assign_expr = DefaultNonTerminal ("assign_expr");
			var annotations = DefaultNonTerminal ("annotations");
			var annotation = DefaultNonTerminal ("annotation");
			var annot_assign_expr = DefaultNonTerminal ("annot_assign_expr");
			var modifiers_then_opt_generic_arg = DefaultNonTerminal ("modifiers_then_opt_generic_arg");
			var modifier_or_generic_arg = DefaultNonTerminal ("modifier_or_generic_arg");
			var modifiers = DefaultNonTerminal ("modifiers");
			var modifier = DefaultNonTerminal ("modifier");
			var argument_decls = DefaultNonTerminal ("argument_decls");
			var argument_decl = DefaultNonTerminal ("argument_decl");
			var comma_separated_types = DefaultNonTerminal ("comma_separated_types");
			var throws_decl = DefaultNonTerminal ("throws_decl");
			var opt_throws_decl = DefaultNonTerminal ("opt_throws_decl");
			var type_name = DefaultNonTerminal ("type_name");
			var dotted_identifier = DefaultNonTerminal ("dotted_identifier");
			var array_type = DefaultNonTerminal ("array_type");
			var vararg_type = DefaultNonTerminal ("vararg_type");
			var generic_type = DefaultNonTerminal ("generic_type");
			var generic_instance_arguments_spec = DefaultNonTerminal ("generic_arguments_spec");
			var generic_instance_arguments = DefaultNonTerminal ("generic_arguments");
			var expressions = DefaultNonTerminal ("expressions");
			var call_super = DefaultNonTerminal ("call_super");
			var default_value_expr = DefaultNonTerminal ("default_value_expr");
			var default_value_literal = DefaultNonTerminal ("default_value_literal");
			var runtime_exception = DefaultNonTerminal ("runtime_exception");
			var numeric_terminal = TerminalFactory.CreateCSharpNumber ("numeric_value_literal");
			var numeric_literal = (Empty | "-") + numeric_terminal + (Empty | "L" | "f");
			numeric_literal |= "(" + numeric_literal + "/" + numeric_literal + ")";
			var string_literal = TerminalFactory.CreateCSharpString ("string_literal");
			var value_literal = string_literal | numeric_literal | keyword_null;

			// <construction_rules>

			compile_unit.Rule = opt_package_decl + imports + type_decls;
			imports.Rule = MakeStarRule (imports, null, import);
			opt_package_decl.Rule = package_decl | Empty;
			package_decl.Rule = keyword_package + type_name + ";";
			imports.Rule = MakeStarRule (imports, null, import);
			import.Rule = keyword_import + type_name + ";";
			type_decls.Rule = MakeStarRule (type_decls, type_decl);

			type_decl.Rule = class_decl | interface_decl | enum_decl;
			enum_decl.Rule = annotations + modifiers_then_opt_generic_arg + keyword_enum + identifier + opt_implements_decl + "{" + (Empty | enum_member_initializers) + type_members + "}";
			class_decl.Rule = annotations + modifiers_then_opt_generic_arg + keyword_class + identifier + opt_generic_arg_decl + opt_extends_decl + opt_implements_decl + type_body;
			interface_decl.Rule = annotations + modifiers_then_opt_generic_arg + (keyword_interface | keyword_at_interface) + identifier + opt_generic_arg_decl + opt_extends_decl + opt_implements_decl + type_body;
			opt_generic_arg_decl.Rule = Empty | ("<" + generic_instance_arguments + ">");
			opt_extends_decl.Rule = Empty | keyword_extends + implements_decl; // when it is used with an interface, it can be more than one...
			opt_implements_decl.Rule = Empty | keyword_implements + implements_decl;
			implements_decl.Rule = MakePlusRule (implements_decl, ToTerm (","), type_name);
			type_body.Rule = "{" + type_members + "}";
			annotations.Rule = MakeStarRule (DefaultNonTerminal ("annotation_list"), annotation);
			annotation.Rule = "@" + type_name + (Empty | "(" + MakeStarRule (DefaultNonTerminal ("annotinitlist"), ToTerm (","), annot_assign_expr) + ")");
			annot_assign_expr.Rule = assign_expr | identifier + "=" + "{" + MakeStarRule (DefaultNonTerminal ("inner_annot_list"), ToTerm (","), annotations) + "}";

			// HACK: I believe this is an Irony bug that adding opt_generic_arg_decl here results in shift-reduce conflict, but it's too complicated to investigate the actual issue.
			// As a workaround I add generic arguments as part of this "modifier" so that it can be safely added to a generic method declaration.
			modifiers_then_opt_generic_arg.Rule = MakeStarRule (modifiers_then_opt_generic_arg, modifier_or_generic_arg);
			modifiers.Rule = MakeStarRule (modifiers, modifier);
			modifier_or_generic_arg.Rule = modifier | generic_instance_arguments_spec;
			modifier.Rule = keyword_public | keyword_protected | keyword_final | keyword_abstract | keyword_synchronized | keyword_default | keyword_native | keyword_volatile | keyword_transient | keyword_static;

			type_members.Rule = MakeStarRule (DefaultNonTerminal ("type_member_list"), type_member);
			type_member.Rule = type_decl | ctor_decl | method_decl | field_decl | static_ctor_decl;
			enum_member_initializers.Rule = MakeStarRule (enum_member_initializers, ToTerm (","), enum_member_initializer) + ";";
			enum_member_initializer.Rule = identifier + "(" + ")";
			static_ctor_decl.Rule = annotations + keyword_static + "{" + assignments + "}";
			assignments.Rule = MakeStarRule (assignments, assign_expr + ";");
			assign_expr.Rule = identifier + "=" + (value_literal | type_name | "{" + MakeStarRule (DefaultNonTerminal ("values"), ToTerm (","), value_literal) + "}");

			field_decl.Rule = annotations + modifiers_then_opt_generic_arg + type_name + identifier + (Empty | "=" + value_literal) + ";";
			terminate_decl_or_body.Rule = ";" | ("{" + expressions + "}") | (keyword_default + default_value_literal + ";");

			ctor_decl.Rule = annotations + modifiers_then_opt_generic_arg + Empty + Empty + identifier + "(" + argument_decls + ")" + opt_throws_decl + terminate_decl_or_body; // these Empties can make the structure common to method_decl.

			method_decl.Rule = annotations + modifiers_then_opt_generic_arg + /*opt_generic_arg_decl*/ Empty + type_name + identifier + "(" + argument_decls + ")" + opt_throws_decl + terminate_decl_or_body;

			expressions.Rule = MakeStarRule (expressions, call_super | runtime_exception);
			call_super.Rule = keyword_super + "(" + MakeStarRule (DefaultNonTerminal ("super_args"), ToTerm (","), default_value_expr) + ")" + ";";
			default_value_expr.Rule = keyword_null | ("(" + type_name + ")" + keyword_null) | default_value_literal;
			default_value_literal.Rule = numeric_terminal | "\"\"" | ("{" + "}") | keyword_true | keyword_false;
			runtime_exception.Rule = ToTerm ("throw") + "new" + "RuntimeException" + "(\"Stub!\"" + ")" + ";";

			argument_decls.Rule = annotations | MakeStarRule (argument_decls, ToTerm (","), argument_decl);

			argument_decl.Rule = annotations + type_name + identifier;

			throws_decl.Rule = keyword_throws + comma_separated_types;
			comma_separated_types.Rule = MakeStarRule (opt_throws_decl, ToTerm (","), type_name);
			opt_throws_decl.Rule = Empty | throws_decl;

			type_name.Rule = dotted_identifier | array_type | vararg_type | generic_type;

			vararg_type.Rule = type_name + "...";
			array_type.Rule = type_name + "[" + "]";

			generic_type.Rule = dotted_identifier + generic_instance_arguments_spec;
			generic_instance_arguments_spec.Rule = "<" + generic_instance_arguments + ">";
			generic_instance_arguments.Rule = MakePlusRule (generic_instance_arguments, ToTerm (","), (type_name | "?") + (Empty | keyword_extends + MakePlusRule (DefaultNonTerminal ("types"), ToTerm ("&"), type_name) | keyword_super + type_name));

			dotted_identifier.Rule = MakePlusRule (dotted_identifier, ToTerm ("."), identifier);

			// Define node creators

			identifier.AstConfig.NodeCreator = CreateStringFlattener ();
			compile_unit.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				node.AstNode = new JavaPackage (null) { Name = (string)node.ChildNodes [0].AstNode, Types = ((IEnumerable<JavaType>) node.ChildNodes [2].AstNode).ToList () };
			};
			opt_package_decl.AstConfig.NodeCreator = SelectSingleChild;
			package_decl.AstConfig.NodeCreator = SelectChildValueAt (1);
			imports.AstConfig.NodeCreator = CreateArrayCreator<object> ();
			import.AstConfig.NodeCreator = SelectChildValueAt (1);
			type_decls.AstConfig.NodeCreator = CreateArrayCreator<JavaType> ();
			type_decl.AstConfig.NodeCreator = SelectSingleChild;
			opt_generic_arg_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				node.AstNode = node.ChildNodes.Count == 0  ? null : node.ChildNodes [1].AstNode;
			};
			opt_extends_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				node.AstNode = node.ChildNodes.Count == 0 ? null : node.ChildNodes [1].AstNode;
			};
			opt_implements_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				node.AstNode = node.ChildNodes.Count == 0 ? null : node.ChildNodes [1].AstNode;
			};
			implements_decl.AstConfig.NodeCreator = CreateArrayCreator<string> ();
			Action<ParseTreeNode,JavaType> fillType = (node, type) => {
				var mods = (IEnumerable<string>)node.ChildNodes [1].AstNode;
				var tps = ((IEnumerable<string>)node.ChildNodes [4].AstNode);
				type.Abstract = mods.Contains ("abstract");
				type.Static = mods.Contains ("static");
				type.Final = mods.Contains ("final");
				type.Visibility = mods.FirstOrDefault (s => s == "public" || s == "protected");
				type.Name = (string)node.ChildNodes [3].AstNode;
				type.Deprecated = ((IEnumerable<string>)node.ChildNodes [0].AstNode).FirstOrDefault (v => v == "Deprecated");
				// HACK: since modifiers_then_opt_generic_args contains both modifiers and generic args,
				// it needs to distinguish type names from modifiers.
				type.TypeParameters = new JavaTypeParameters ((JavaMethod)null) {
					TypeParameters = tps
							?.Where (s => s.Contains ('.'))
							?.Select (s => new JavaTypeParameter (null) { Name = s })
							?.ToArray ()
				};
				type.Members = (IList<JavaMember>) node.ChildNodes [7].AstNode;
			};
			enum_decl.AstConfig.NodeCreator = (ctx, node) => {
				//annotations + modifiers_then_opt_generic_arg + keyword_enum + identifier + opt_implements_decl + "{" + (Empty | enum_member_initializers) + type_members + "}";
				ProcessChildren (ctx, node);
				var mods = (IEnumerable<string>)node.ChildNodes [1].AstNode;
				var type = new JavaClass (null);
				fillType (node, type);
				if (node.ChildNodes [6].Term != Empty)
					type.Members = ((IEnumerable<string>)node.ChildNodes [6].AstNode).Select (s => new JavaField (null) { Name = s }).Concat (type.Members).ToArray ();
				node.AstNode = type;
			};
			class_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				var mods = (IEnumerable<string>) node.ChildNodes [1].AstNode;
				var exts = ((IEnumerable<string>) node.ChildNodes [5].AstNode);
				var impls = ((IEnumerable<string>) node.ChildNodes [6].AstNode);
				var type = new JavaClass (null) {
					Extends = exts?.FirstOrDefault (),
					Implements = impls
						?.Select (s => new JavaImplements { Name = s })
						?.ToArray (),
				};
				fillType (node, type);
				node.AstNode = type;
			};
			interface_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				var type = new JavaInterface (null) {
					Implements = ((IEnumerable<string>)node.ChildNodes [5].AstNode)
						.Concat ((IEnumerable<string>)node.ChildNodes [6].AstNode)
						.Select (s => new JavaImplements { Name = s })
						.ToArray (),
				};
				fillType (node, type);
				node.AstNode = type;
			};
			type_body.AstConfig.NodeCreator = SelectChildValueAt (1);
			type_members.AstConfig.NodeCreator = CreateArrayCreator<JavaMember> ();
			type_member.AstConfig.NodeCreator = SelectSingleChild;
			Action<ParseTreeNode, JavaMethodBase> fill = (node, method) => {
				var mods = (IEnumerable<string>)node.ChildNodes [1].AstNode;
				method.Static = mods.Contains ("static");
				method.Visibility = mods.FirstOrDefault (s => s == "public" || s == "protected");
				method.Name = (string)node.ChildNodes [4].AstNode;
				method.Parameters = ((IEnumerable<JavaParameter>)node.ChildNodes [5].AstNode).ToArray ();
				method.ExtendedSynthetic = mods.Contains ("synthetic");
				method.Exceptions = ((IEnumerable<string>)node.ChildNodes [8].AstNode).Select (s => new JavaException { Type = s }).ToArray ();
				method.Deprecated = ((IEnumerable<string>)node.ChildNodes [0].AstNode).FirstOrDefault (v => v == "Deprecated");
				method.Final = mods.Contains ("final");
				method.TypeParameters = new JavaTypeParameters ((JavaMethod) null) {
					TypeParameters = ((IEnumerable<string>) node.ChildNodes [1].AstNode)
							.Where (s => s.Contains ('.'))
							.Select (s => new JavaTypeParameter (null) { Name = s })
							.ToArray ()
				};
			};
			ctor_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				var annots = node.ChildNodes [0].AstNode;
				var mods = (IEnumerable<string>)node.ChildNodes [1].AstNode;
				var ctor = new JavaConstructor (null);
				fill (node, ctor);
				node.AstNode = ctor;
			};
			method_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				var annots = node.ChildNodes [0].AstNode;
				var mods = (IEnumerable<string>)node.ChildNodes [1].AstNode;
				var method = new JavaMethod (null) {
					Return = (string)node.ChildNodes [3].AstNode,
					Abstract = mods.Contains ("abstract"),
					Native = mods.Contains ("native"),
					Synchronized = mods.Contains ("synchronized"),
					ExtendedSynthetic = mods.Contains ("synthetic"),
				};
				fill (node, method);
				node.AstNode = method;
			};
			field_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				var annots = node.ChildNodes [0].AstNode;
				var mods = (IEnumerable<string>)node.ChildNodes [1].AstNode;
				node.AstNode = new JavaField (null) {
					Static = mods.Contains ("static"),
					Visibility = mods.FirstOrDefault (s => s == "public" || s == "protected"),
					Type = (string) node.ChildNodes [2].AstNode,
					Name = (string) node.ChildNodes [3].AstNode,
					Deprecated = ((IEnumerable<string>) node.ChildNodes [0].AstNode).FirstOrDefault (v => v == "Deprecated"),
					Value = node.ChildNodes [4].AstNode?.ToString (),
					Volatile = mods.Contains ("volatile"),
					Final = mods.Contains ("final"),
					Transient = mods.Contains ("transient"),
				};
			};
			static_ctor_decl.AstConfig.NodeCreator = DoNothing; // static constructors are ignorable.
			enum_member_initializers.AstConfig.NodeCreator = CreateArrayCreator<string> ();
			enum_member_initializer.AstConfig.NodeCreator = SelectChildValueAt (0);
			terminate_decl_or_body.AstConfig.NodeCreator = DoNothing; // method/ctor body doesn't matter.
			assignments.AstConfig.NodeCreator = CreateArrayCreator<object> ();
			assign_expr.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				node.AstNode = new KeyValuePair<string, string> ((string)node.ChildNodes [0].AstNode, node.ChildNodes [2].AstNode?.ToString ());
			};
			annotations.AstConfig.NodeCreator = CreateArrayCreator<string> ();
			annotation.AstConfig.NodeCreator = (ctx, node) => SelectChildValueAt (1); // we don't care about annotation usages so far.
			annot_assign_expr.AstConfig.NodeCreator = DoNothing;
			modifiers_then_opt_generic_arg.AstConfig.NodeCreator = CreateArrayCreator<string> ();
			modifier_or_generic_arg.AstConfig.NodeCreator = (ctx, node) => CreateStringFlattener ();
			modifiers.AstConfig.NodeCreator = CreateArrayCreator<string> ();
			modifier.AstConfig.NodeCreator = (ctx, node) => CreateStringFlattener ();
			argument_decls.AstConfig.NodeCreator = CreateArrayCreator<object> ();
			argument_decl.AstConfig.NodeCreator = (ctx, node) => {
				ProcessChildren (ctx, node);
				node.AstNode = new JavaParameter (null) { Type = (string) node.ChildNodes [1].AstNode, Name = (string) node.ChildNodes [2].AstNode };
			};
			opt_throws_decl.AstConfig.NodeCreator = SelectSingleChild;
			throws_decl.AstConfig.NodeCreator = SelectChildValueAt (1);
			comma_separated_types.AstConfig.NodeCreator = CreateArrayCreator<object> ();
			type_name.AstConfig.NodeCreator = SelectSingleChild;
			dotted_identifier.AstConfig.NodeCreator = CreateStringFlattener ();
			array_type.AstConfig.NodeCreator = CreateStringFlattener ();
			vararg_type.AstConfig.NodeCreator = CreateStringFlattener ();
			generic_type.AstConfig.NodeCreator = CreateStringFlattener ();
			generic_instance_arguments_spec.AstConfig.NodeCreator = SelectChildValueAt (1);
			generic_instance_arguments.AstConfig.NodeCreator = CreateArrayCreator<object> ();
			expressions.AstConfig.NodeCreator = CreateArrayCreator<object> ();
			// each expression item is not seriously processed.
			// They are insignificant except for consts, and for consts they are just string values.
			call_super.AstConfig.NodeCreator = CreateStringFlattener ();
			default_value_expr.AstConfig.NodeCreator = CreateStringFlattener ();
			default_value_literal.AstConfig.NodeCreator = CreateStringFlattener ();
			runtime_exception.AstConfig.NodeCreator = CreateStringFlattener ();
			numeric_terminal.AstConfig.NodeCreator = (ctx, node) => node.AstNode = node.Token.ValueString;
			numeric_literal.AstConfig.NodeCreator = CreateStringFlattener ();
			string_literal.AstConfig.NodeCreator = (ctx, node) => node.AstNode = node.Token.ValueString;
			value_literal.AstConfig.NodeCreator = SelectSingleChild;

			this.Root = compile_unit;
		}
	}
}

