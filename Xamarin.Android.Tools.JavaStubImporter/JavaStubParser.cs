using System;
using System.Linq;
using Irony.Parsing;
using Irony.Ast;

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

		void DoNothing (AstContext ctx, ParseTreeNode node)
		{
			// do nothing.
		}

		public JavaStubGrammar ()
		{
			CommentTerminal single_line_comment = new CommentTerminal ("SingleLineComment", "//", "\r", "\n");
			CommentTerminal delimited_comment = new CommentTerminal ("DelimitedComment", "/*", "*/");

			NonGrammarTerminals.Add (single_line_comment);
			NonGrammarTerminals.Add (delimited_comment);

			IdentifierTerminal identifier = new IdentifierTerminal ("identifier");// TerminalFactory.CreateCSharpIdentifier ("Identifier"); // It is all hack. We just reuse CSharpIdentifier here.
			identifier.AstConfig.NodeCreator = delegate (AstContext ctx, ParseTreeNode node) {
				node.AstNode = node.Token.ValueString;
			};

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
			var enum_member_initializer = DefaultNonTerminal ("enum_member_initializer");
			var terminate_decl_or_body = DefaultNonTerminal ("terminate_decl_or_body");
			var assignments = DefaultNonTerminal ("assignments");
			var assign_expr = DefaultNonTerminal ("assign_expr");
			var annotations = DefaultNonTerminal ("annotations");
			var annotation = DefaultNonTerminal ("annotation");
			var annot_assign_expr = DefaultNonTerminal ("annot_assign_expr");
			var modifiers = DefaultNonTerminal ("modifiers");
			var modifier = DefaultNonTerminal ("modifier");
			var argument_decls = DefaultNonTerminal ("argument_decls");
			var argument_decl = DefaultNonTerminal ("argument_decl");
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
			var value_literal = TerminalFactory.CreateCSharpString ("string_value_literal") | numeric_literal | keyword_null;

			// <construction_rules>

			compile_unit.Rule = opt_package_decl + imports + type_decls;
			imports.Rule = MakeStarRule (imports, null, import);
			opt_package_decl.Rule = package_decl | Empty;
			package_decl.Rule = keyword_package + type_name + ";";
			imports.Rule = MakeStarRule (imports, null, import);
			import.Rule = keyword_import + type_name + ";";
			type_decls.Rule = MakeStarRule (type_decls, type_decl);

			type_decl.Rule = class_decl | interface_decl | enum_decl;
			enum_decl.Rule = annotations + modifiers + keyword_enum + identifier + opt_implements_decl + "{" + (Empty | enum_member_initializer) + type_members + "}";
			class_decl.Rule = annotations + modifiers + keyword_class + identifier + opt_generic_arg_decl + opt_extends_decl + opt_implements_decl + type_body;
			interface_decl.Rule = annotations + modifiers + (keyword_interface | keyword_at_interface) + identifier + opt_generic_arg_decl + opt_extends_decl + opt_implements_decl + type_body;
			interface_decl.Rule = annotations + modifiers + (keyword_interface | keyword_at_interface) + identifier + opt_generic_arg_decl + opt_extends_decl + opt_implements_decl + type_body;
			opt_generic_arg_decl.Rule = Empty | ("<" + generic_instance_arguments + ">");
			opt_extends_decl.Rule = Empty | keyword_extends + MakePlusRule (implements_decl, ToTerm (","), type_name); // when it is used with an interface, it can be more than one...
			opt_implements_decl.Rule = Empty | keyword_implements + MakePlusRule (implements_decl, ToTerm (","), type_name);
			type_body.Rule = "{" + type_members + "}";
			annotations.Rule = MakeStarRule (DefaultNonTerminal ("annotation_list"), annotation);
			annotation.Rule = "@" + type_name + (Empty | "(" + MakeStarRule (DefaultNonTerminal ("annotinitlist"), ToTerm (","), annot_assign_expr) + ")");
			annot_assign_expr.Rule = assign_expr | identifier + "=" + "{" + MakeStarRule (DefaultNonTerminal ("inner_annot_list"), ToTerm (","), annotations) + "}";

			modifiers.Rule = MakeStarRule (modifiers, modifier);
			// HACK: I believe this is an Irony bug that adding opt_generic_arg_decl here results in shift-reduce conflict, but it's too complicated to investigate the actual issue.
			// As a workaround I add generic arguments as part of this "modifier" so that it can be safely added to a generic method declaration.
			modifier.Rule = keyword_public | keyword_protected | keyword_final | keyword_abstract | keyword_synchronized | keyword_default | keyword_native | keyword_volatile | keyword_transient | keyword_static | ("<" + generic_instance_arguments + ">");

			type_members.Rule = MakeStarRule (DefaultNonTerminal ("type_member_list"), type_member);
			type_member.Rule = type_decl | ctor_decl | method_decl | field_decl | static_ctor_decl;
			enum_member_initializer.Rule = MakeStarRule (DefaultNonTerminal ("enum_body"), ToTerm (","), identifier + "(" + ")") + ";";

			static_ctor_decl.Rule = annotations + keyword_static + "{" + assignments + "}";
			assignments.Rule = MakeStarRule (assignments, assign_expr + ";");
			assign_expr.Rule = identifier + "=" + (value_literal | type_name | "{" + MakeStarRule (DefaultNonTerminal ("values"), ToTerm (","), value_literal) + "}");

			field_decl.Rule = annotations + modifiers + type_name + identifier + (Empty | "=" + value_literal) + ";";
			terminate_decl_or_body.Rule = ";" | ("{" + expressions + "}") | (keyword_default + default_value_literal + ";");

			ctor_decl.Rule = annotations + modifiers + identifier + "(" + argument_decls + ")" + opt_throws_decl + terminate_decl_or_body;

			method_decl.Rule = annotations + modifiers /*+ opt_generic_arg_decl*/ + type_name + identifier + "(" + argument_decls + ")" + opt_throws_decl + terminate_decl_or_body;

			expressions.Rule = MakeStarRule (expressions, call_super | runtime_exception);
			call_super.Rule = keyword_super + "(" + MakeStarRule (DefaultNonTerminal ("super_args"), ToTerm (","), default_value_expr) + ")" + ";";
			default_value_expr.Rule = keyword_null | ("(" + type_name + ")" + keyword_null) | default_value_literal;
			default_value_literal.Rule = numeric_terminal | "\"\"" | ("{" + "}") | keyword_true | keyword_false;
			runtime_exception.Rule = ToTerm ("throw") + "new" + "RuntimeException" + "(\"Stub!\"" + ")" + ";";

			argument_decls.Rule = annotations | MakeStarRule (argument_decls, ToTerm (","), argument_decl);

			argument_decl.Rule = annotations + type_name + identifier;

			opt_throws_decl.Rule = Empty | keyword_throws + MakeStarRule (opt_throws_decl, ToTerm (","), type_name);

			type_name.Rule = dotted_identifier | array_type | vararg_type | generic_type;

			vararg_type.Rule = type_name + "...";
			array_type.Rule = type_name + "[" + "]";

			generic_type.Rule = dotted_identifier + generic_instance_arguments_spec;
			generic_instance_arguments_spec.Rule = "<" + generic_instance_arguments + ">";
			generic_instance_arguments.Rule = MakePlusRule (generic_instance_arguments, ToTerm (","), (type_name | "?") + (Empty | keyword_extends + MakePlusRule (DefaultNonTerminal ("types"), ToTerm ("&"), type_name) | keyword_super + type_name));

			dotted_identifier.Rule = MakePlusRule (dotted_identifier, ToTerm ("."), identifier);

			this.Root = compile_unit;
		}
	}
}

