/******************************************************************************
 * Copyright (c) 2013 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Patrick Francis (ABB Group) - initial API, implementation, & documentation
 *****************************************************************************/

using ABB.SrcML.Test.Utilities;
using NUnit.Framework;
using System.Linq;
using System.Xml.Linq;

namespace ABB.SrcML.Data.Test {

    [TestFixture]
    [Category("Build")]
    public class CSharpCodeParserTests {
        private CSharpCodeParser codeParser;
        private SrcMLFileUnitSetup fileSetup;

        [TestFixtureSetUp]
        public void ClassSetup() {
            codeParser = new CSharpCodeParser();
            fileSetup = new SrcMLFileUnitSetup(Language.CSharp);
        }

        [Test]
        public void TestNamespace() {
            //namespace A { 
            //	public class foo { }
            //}
            /*var xml = @"<namespace>namespace <name>A</name> <block>{ 
	<class><specifier>public</specifier> class <name>foo</name> <block>{ }</block></class>
}</block></namespace>";*/

            var xml = @"<namespace>namespace <name>A</name> <block>{
	<class><specifier>public</specifier> class <name>foo</name> <block>{ }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            Assert.IsTrue(globalScope.IsGlobal);

            var actual = globalScope.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(actual);
            Assert.AreEqual("A", actual.Name);
            Assert.AreEqual(1, actual.ChildStatements.Count);
        }

        [Test]
        public void TestCallToGenericMethod() {
            //namespace A {
            //    public class B {
            //        void Foo<T>(T t) { }
            //        void Bar() { Foo(this); }
            //    }
            //}
            /*var xml = @"<namespace>namespace <name>A</name> <block>{
    <class><specifier>public</specifier> class <name>B</name> <block>{
        <function><type><name>void</name></type> <name><name>Foo</name><argument_list>&lt;<argument><name>T</name></argument>&gt;</argument_list></name><parameter_list>(<parameter><decl><type><name>T</name></type> <name>t</name></decl></parameter>)</parameter_list> <block>{ }</block></function>
        <function><type><name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <expr_stmt><expr><call><name>Foo</name><argument_list>(<argument><expr><name>this</name></expr></argument>)</argument_list></call></expr>;</expr_stmt> }</block></function>
    }</block></class>
}</block></namespace>";*/

            var xml = @"<namespace>namespace <name>A</name>
<block>{
    <class><specifier>public</specifier> class <name>B</name> <block>{
        <function><type><name>void</name></type> <name><name>Foo</name><argument_list type=""generic"">&lt;<argument><name>T</name></argument>&gt;</argument_list></name><parameter_list>(<parameter><decl><type><name>T</name></type> <name>t</name></decl></parameter>)</parameter_list> <block>{<block_content> </block_content>}</block></function>
        <function><type><name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <expr_stmt><expr><call><name>Foo</name><argument_list>(<argument><expr><name>this</name></expr></argument>)</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");
            var globalScope = codeParser.ParseFileUnit(unit);

            var foo = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(md => md.Name == "Foo");
            var bar = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(md => md.Name == "Bar");
            Assert.IsNotNull(foo);
            Assert.IsNotNull(bar);

            Assert.AreEqual(1, bar.ChildStatements.Count);
            var callToFoo = bar.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(callToFoo);

            Assert.AreSame(foo, callToFoo.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestCallToGrandparent() {
            //namespace A {
            //    public class B { public void Foo() { } }
            //    public class C : B { }
            //    public class D : C { public void Bar() { Foo() } }
            //}
            /*var xml = @"<namespace>namespace <name>A</name> <block>{
    <class><specifier>public</specifier> class <name>B</name> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ }</block></function> }</block></class>
    <class><specifier>public</specifier> class <name>C</name> <super>: <name>B</name></super> <block>{ }</block></class>
    <class><specifier>public</specifier> class <name>D</name> <super>: <name>C</name></super> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <expr_stmt><expr><call><name>Foo</name><argument_list>()</argument_list></call></expr></expr_stmt> }</block></function> }</block></class>
}</block></namespace>";*/

            var xml = @"<namespace>namespace <name>A</name> <block>{
    <class><specifier>public</specifier> class <name>B</name> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function> }</block></class>
    <class><specifier>public</specifier> class <name>C</name> <super_list>: <super><name>B</name></super></super_list> <block>{ }</block></class>
    <class><specifier>public</specifier> class <name>D</name> <super_list>: <super><name>C</name></super></super_list> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <expr_stmt><expr><call><name>Foo</name><argument_list>()</argument_list></call></expr></expr_stmt> </block_content>}</block></function> }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");
            var scope = codeParser.ParseFileUnit(unit);

            var bDotFoo = scope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            var dDotBar = scope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Bar");
            Assert.IsNotNull(bDotFoo);
            Assert.IsNotNull(dDotBar);

            Assert.AreEqual(1, dDotBar.ChildStatements.Count);
            var callToFoo = dDotBar.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(callToFoo);

            Assert.AreSame(bDotFoo, callToFoo.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestMethodCallToParentOfCallingObject() {
            //class A { void Foo() { } }
            /*string a_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">A</name> <block pos:line=""1"" pos:column=""9"">{ <function><type><name pos:line=""1"" pos:column=""11"">void</name></type> <name pos:line=""1"" pos:column=""16"">Foo</name><parameter_list pos:line=""1"" pos:column=""19"">()</parameter_list> <block pos:line=""1"" pos:column=""22"">{ }</block></function> }</block></class>";*/
            string a_xml = @"<class pos:start=""1:1"" pos:end=""1:26"">class <name pos:start=""1:7"" pos:end=""1:7"">A</name> <block pos:start=""1:9"" pos:end=""1:26"">{ <function pos:start=""1:11"" pos:end=""1:24""><type pos:start=""1:11"" pos:end=""1:14""><name pos:start=""1:11"" pos:end=""1:14"">void</name></type> <name pos:start=""1:16"" pos:end=""1:18"">Foo</name><parameter_list pos:start=""1:19"" pos:end=""1:20"">()</parameter_list> <block pos:start=""1:22"" pos:end=""1:24"">{<block_content> </block_content>}</block></function> }</block></class>";

            //class B : A { void Bar() { } }
            /*string b_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">B</name> <super pos:line=""1"" pos:column=""9"">: <name pos:line=""1"" pos:column=""11"">A</name></super> <block pos:line=""1"" pos:column=""13"">{ <function><type><name pos:line=""1"" pos:column=""15"">void</name></type> <name pos:line=""1"" pos:column=""20"">Bar</name><parameter_list pos:line=""1"" pos:column=""23"">()</parameter_list> <block pos:line=""1"" pos:column=""26"">{ }</block></function> }</block></class>";*/
            string b_xml = @"<class pos:start=""1:1"" pos:end=""1:30"">class <name pos:start=""1:7"" pos:end=""1:7"">B</name> <super_list pos:start=""1:9"" pos:end=""1:11"">: <super pos:start=""1:11"" pos:end=""1:11""><name pos:start=""1:11"" pos:end=""1:11"">A</name></super></super_list> <block pos:start=""1:13"" pos:end=""1:30"">{ <function pos:start=""1:15"" pos:end=""1:28""><type pos:start=""1:15"" pos:end=""1:18""><name pos:start=""1:15"" pos:end=""1:18"">void</name></type> <name pos:start=""1:20"" pos:end=""1:22"">Bar</name><parameter_list pos:start=""1:23"" pos:end=""1:24"">()</parameter_list> <block pos:start=""1:26"" pos:end=""1:28"">{<block_content> </block_content>}</block></function> }</block></class>";


            //class C {
            //	private B b;
            //	void main() {
            //		b.Foo();
            //	}
            //}
            /*string c_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">C</name> <block pos:line=""1"" pos:column=""9"">{
    <decl_stmt><decl><type><specifier pos:line=""2"" pos:column=""5"">private</specifier> <name pos:line=""2"" pos:column=""13"">B</name></type> <name pos:line=""2"" pos:column=""15"">b</name></decl>;</decl_stmt>
    <function><type><name pos:line=""3"" pos:column=""5"">void</name></type> <name pos:line=""3"" pos:column=""10"">main</name><parameter_list pos:line=""3"" pos:column=""14"">()</parameter_list> <block pos:line=""3"" pos:column=""17"">{
        <expr_stmt><expr><call><name><name pos:line=""4"" pos:column=""9"">b</name><operator pos:line=""4"" pos:column=""10"">.</operator><name pos:line=""4"" pos:column=""11"">Foo</name></name><argument_list pos:line=""4"" pos:column=""14"">()</argument_list></call></expr>;</expr_stmt>
    }</block></function>
}</block></class>";*/

            string c_xml = @"<class pos:start=""1:1"" pos:end=""7:1"">class <name pos:start=""1:7"" pos:end=""1:7"">C</name>
<block pos:start=""2:1"" pos:end=""7:1"">{
	<decl_stmt pos:start=""3:9"" pos:end=""3:20""><decl pos:start=""3:9"" pos:end=""3:19""><type pos:start=""3:9"" pos:end=""3:17""><specifier pos:start=""3:9"" pos:end=""3:15"">private</specifier> <name pos:start=""3:17"" pos:end=""3:17"">B</name></type> <name pos:start=""3:19"" pos:end=""3:19"">b</name></decl>;</decl_stmt>
	<function pos:start=""4:9"" pos:end=""6:9""><type pos:start=""4:9"" pos:end=""4:12""><name pos:start=""4:9"" pos:end=""4:12"">void</name></type> <name pos:start=""4:14"" pos:end=""4:17"">main</name><parameter_list pos:start=""4:18"" pos:end=""4:19"">()</parameter_list> <block pos:start=""4:21"" pos:end=""6:9"">{<block_content pos:start=""5:17"" pos:end=""5:24"">
		<expr_stmt pos:start=""5:17"" pos:end=""5:24""><expr pos:start=""5:17"" pos:end=""5:23""><call pos:start=""5:17"" pos:end=""5:23""><name pos:start=""5:17"" pos:end=""5:21""><name pos:start=""5:17"" pos:end=""5:17"">b</name><operator pos:start=""5:18"" pos:end=""5:18"">.</operator><name pos:start=""5:19"" pos:end=""5:21"">Foo</name></name><argument_list pos:start=""5:22"" pos:end=""5:23"">()</argument_list></call></expr>;</expr_stmt>
	</block_content>}</block></function>
}</block></class>";

            var aUnit = fileSetup.GetFileUnitForXmlSnippet(a_xml, "A.cs");
            var bUnit = fileSetup.GetFileUnitForXmlSnippet(b_xml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(c_xml, "C.cs");

            NamespaceDefinition globalScope = codeParser.ParseFileUnit(aUnit);
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(bUnit));
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(cUnit));

            var typeA = globalScope.GetNamedChildren<TypeDefinition>("A").FirstOrDefault();
            var typeB = globalScope.GetNamedChildren<TypeDefinition>("B").FirstOrDefault();
            var typeC = globalScope.GetNamedChildren<TypeDefinition>("C").FirstOrDefault();

            Assert.IsNotNull(typeA, "could not find class A");
            Assert.IsNotNull(typeB, "could not find class B");
            Assert.IsNotNull(typeC, "could not find class C");

            var aDotFoo = typeA.GetNamedChildren<MethodDefinition>("Foo").FirstOrDefault();
            var cDotMain = typeC.GetNamedChildren<MethodDefinition>("main").FirstOrDefault();

            Assert.IsNotNull(aDotFoo, "could not find method A.Foo()");
            Assert.IsNotNull(cDotMain, "could not find method C.main()");

            var callToFooFromC = cDotMain.FindExpressions<MethodCall>(true).FirstOrDefault();

            Assert.IsNotNull(callToFooFromC, "could not find any calls in C.main()");
            Assert.AreEqual("Foo", callToFooFromC.Name);
            var callingObject = callToFooFromC.GetSiblingsBeforeSelf<NameUse>().Last();
            Assert.AreEqual("b", callingObject.Name);

            Assert.AreEqual(typeB, callingObject.ResolveType().FirstOrDefault());
            Assert.AreEqual(aDotFoo, callToFooFromC.FindMatches().FirstOrDefault());
        }

        [Test]
        [Category("Todo")]
        public void TestCallWithTypeParameters() {
            //namespace A {
            //    public interface IOdb { 
            //        int Query();
            //        int Query<T>();
            //    }
            //    public class Test {
            //        public IOdb Open() { }
            //        void Test1() {
            //            IOdb odb = Open();
            //            var query = odb.Query<Foo>();
            //        }
            //    }
            //}
            /*var xml = @"<namespace>namespace <name>A</name> <block>{
    <class type=""interface""><specifier>public</specifier> interface <name>IOdb</name> <block>{ 
        <function_decl><type><name>int</name></type> <name>Query</name><parameter_list>()</parameter_list>;</function_decl>
        <function_decl><type><name>int</name></type> <name><name>Query</name><argument_list>&lt;<argument><name>T</name></argument>&gt;</argument_list></name><parameter_list>()</parameter_list>;</function_decl>
    }</block></class>
    <class><specifier>public</specifier> class <name>Test</name> <block>{
        <function><type><specifier>public</specifier> <name>IOdb</name></type> <name>Open</name><parameter_list>()</parameter_list> <block>{ }</block></function>
        <function><type><name>void</name></type> <name>Test1</name><parameter_list>()</parameter_list> <block>{
            <decl_stmt><decl><type><name>IOdb</name></type> <name>odb</name> <init>= <expr><call><name>Open</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt>
            <decl_stmt><decl><type><name>var</name></type> <name>query</name> <init>= <expr><call><name><name>odb</name><operator>.</operator><name><name>Query</name><argument_list>&lt;<argument><name>Foo</name></argument>&gt;</argument_list></name></name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt>
        }</block></function>
    }</block></class>
}</block></namespace>";*/            

            var xml = @"<namespace>namespace <name>A</name> <block>{
    <interface><specifier>public</specifier> interface <name>IOdb</name> <block>{ 
        <function_decl><type><name>int</name></type> <name>Query</name><parameter_list>()</parameter_list>;</function_decl>
        <function_decl><type><name>int</name></type> <name><name>Query</name><argument_list type=""generic"">&lt;<argument><name>T</name></argument>&gt;</argument_list></name><parameter_list>()</parameter_list>;</function_decl>
    }</block></interface>
    <class><specifier>public</specifier> class <name>Test</name> <block>{
        <function><type><specifier>public</specifier> <name>IOdb</name></type> <name>Open</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function>
        <function><type><name>void</name></type> <name>Test1</name><parameter_list>()</parameter_list> <block>{<block_content>
            <decl_stmt><decl><type><name>IOdb</name></type> <name>odb</name> <init>= <expr><call><name>Open</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt>
            <decl_stmt><decl><type><name>var</name></type> <name>query</name> <init>= <expr><call><name><name>odb</name><operator>.</operator><name><name>Query</name><argument_list type=""generic"">&lt;<argument><name>Foo</name></argument>&gt;</argument_list></name></name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt>
        </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");
            var scope = codeParser.ParseFileUnit(unit);

            //TODO: update to search for method with type params, not just LastOrDefault
            var queryTMethod = scope.GetDescendants<MethodDefinition>().LastOrDefault(m => m.Name == "Query");
            Assert.IsNotNull(queryTMethod);
            var test1Method = scope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Test1");
            Assert.IsNotNull(test1Method);

            Assert.AreEqual(2, test1Method.ChildStatements.Count);
            var callToQuery = test1Method.ChildStatements[1].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(callToQuery);         
        }

        [Test]
        public void TestCallConstructor() {
            //class Foo {
            //  public Foo() { }
            //}
            //class Bar {
            //  Foo myFoo = new Foo();
            //}
            /*string xml = @"<class>class <name>Foo</name> <block>{
  <constructor><specifier>public</specifier> <name>Foo</name><parameter_list>()</parameter_list> <block>{ }</block></constructor>
}</block></class>
<class>class <name>Bar</name> <block>{
  <decl_stmt><decl><type><name>Foo</name></type> <name>myFoo</name> <init>= <expr><operator>new</operator> <call><name>Foo</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt>
}</block></class>";*/

            string xml = @"<class>class <name>Foo</name> <block>{
  <constructor><specifier>public</specifier> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></constructor>
}</block></class>
<class>class <name>Bar</name> <block>{
  <decl_stmt><decl><type><name>Foo</name></type> <name>myFoo</name> <init>= <expr><operator>new</operator> <call><name>Foo</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt>
}</block></class>";

            var xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(2, globalScope.ChildStatements.Count);

            var fooConstructor = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            Assert.IsNotNull(fooConstructor);
            var fooCall = globalScope.ChildStatements[1].ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault(mc => mc.Name == "Foo");
            Assert.IsNotNull(fooCall);
            Assert.AreSame(fooConstructor, fooCall.FindMatches().First());
        }

        [Test]
        public void TestConstructorWithBaseKeyword() {
            // B.cs namespace A { class B { public B() { } } }
            /*string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <block>{ <constructor><specifier>public</specifier> <name>B</name><parameter_list>()</parameter_list> <block>{ }</block></constructor> }</block></class> }</block></namespace>";*/

            string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <block>{ <constructor><specifier>public</specifier> <name>B</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></constructor> }</block></class> }</block></namespace>";

            // C.cs namespace A { class C : B { public C() : base() { } } }
            /*string cXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>C</name> <super>: <name>B</name></super> <block>{ <constructor><specifier>public</specifier> <name>C</name><parameter_list>()</parameter_list> <member_list>: <call><name>base</name><argument_list>()</argument_list></call> </member_list><block>{ }</block></constructor> }</block></class> }</block></namespace>";*/

            string cXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>C</name> <super_list>: <super><name>B</name></super></super_list> <block>{ <constructor><specifier>public</specifier> <name>C</name><parameter_list>()</parameter_list> <member_init_list>: <call><name>base</name><argument_list>()</argument_list></call> </member_init_list><block>{<block_content> </block_content>}</block></constructor> }</block></class> }</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(cXml, "C.cs");

            var bScope = codeParser.ParseFileUnit(bUnit);
            var cScope = codeParser.ParseFileUnit(cUnit);
            var globalScope = bScope.Merge(cScope);

            var bConstructor = globalScope.GetDescendants<MethodDefinition>().First(m => m.Name == "B" && m.IsConstructor);
            var cConstructor = globalScope.GetDescendants<MethodDefinition>().First(m => m.Name == "C" && m.IsConstructor);
            Assert.AreEqual(1, cConstructor.ConstructorInitializers.Count);

            var methodCall = cConstructor.ConstructorInitializers[0];
            Assert.IsNotNull(methodCall);
            Assert.That(methodCall.IsConstructor);
            Assert.That(methodCall.IsConstructorInitializer);
            Assert.AreEqual("base", methodCall.Name);
            Assert.AreSame(bConstructor, methodCall.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestConstructorWithThisKeyword() {
            // B.cs
            //namespace A {
            //    class B {
            //        public B() : this(0) { }
            //        public B(int i) { }
            //    }
            //}

            /*string bXml = @"<namespace>namespace <name>A</name> <block>{
    <class>class <name>B</name> <block>{
        <constructor><specifier>public</specifier> <name>B</name><parameter_list>()</parameter_list> <member_list>: <call><name>this</name><argument_list>(<argument><expr><literal type=""number"">0</literal></expr></argument>)</argument_list></call> </member_list><block>{ }</block></constructor>
        <constructor><specifier>public</specifier> <name>B</name><parameter_list>(<parameter><decl><type><name>int</name></type> <name>i</name></decl></parameter>)</parameter_list> <block>{ }</block></constructor>
    }</block></class>
}</block></namespace>";*/

            string bXml = @"<namespace>namespace <name>A</name>
<block>{
    <class>class <name>B</name> <block>{
        <constructor><specifier>public</specifier> <name>B</name><parameter_list>()</parameter_list> <member_init_list>: <call><name>this</name><argument_list>(<argument><expr><literal type=""number"">0</literal></expr></argument>)</argument_list></call> </member_init_list><block>{<block_content> </block_content>}</block></constructor>
        <constructor><specifier>public</specifier> <name>B</name><parameter_list>(<parameter><decl><type><name>int</name></type> <name>i</name></decl></parameter>)</parameter_list> <block>{<block_content> </block_content>}</block></constructor>
    }</block></class>
}</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");

            var globalScope = codeParser.ParseFileUnit(bUnit);

            var oneArgumentConstructor = globalScope.GetDescendants<MethodDefinition>().First(m => m.Name == "B" && m.Parameters.Count == 1);
            var defaultConstructor = globalScope.GetDescendants<MethodDefinition>().First(m => m.Name == "B" && m.Parameters.Count == 0);
            Assert.AreEqual(1, defaultConstructor.ConstructorInitializers.Count);

            var methodCall = defaultConstructor.ConstructorInitializers[0];
            Assert.IsNotNull(methodCall);
            Assert.That(methodCall.IsConstructor);
            Assert.That(methodCall.IsConstructorInitializer);
            Assert.AreEqual("this", methodCall.Name);
            Assert.AreSame(oneArgumentConstructor, methodCall.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestCreateAliasesForFiles_UsingNamespace() {
            // using x.y.z;
            /*string xml = @"<using>using <name><name>x</name><operator>.</operator><name>y</name><operator>.</operator><name>z</name></name>;</using>";*/

            string xml = @"<using>using <name><name>x</name><operator>.</operator><name>y</name><operator>.</operator><name>z</name></name>;</using>";
            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(1, globalScope.ChildStatements.Count);

            var actual = globalScope.ChildStatements[0] as ImportStatement;
            Assert.IsNotNull(actual);
            Assert.AreEqual("x . y . z", actual.ImportedNamespace.ToString());
        }

        [Test]
        public void TestCreateAliasesForFiles_UsingAlias() {
            // using x = Foo.Bar.Baz;
            /*string xml = @"<using>using <name>x</name> <init>= <expr><name><name>Foo</name><operator>.</operator><name>Bar</name><operator>.</operator><name>Baz</name></name></expr></init>;</using>";*/

            string xml = @"<using>using <name>x</name> <init>= <expr><name><name>Foo</name><operator>.</operator><name>Bar</name><operator>.</operator><name>Baz</name></name></expr></init>;</using>";
            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(1, globalScope.ChildStatements.Count);

            var actual = globalScope.ChildStatements[0] as AliasStatement;
            Assert.IsNotNull(actual);
            Assert.AreEqual("x", actual.AliasName);
            Assert.AreEqual("Foo . Bar . Baz", actual.Target.ToString());
        }

        [Test]
        public void TestGetImports() {
            //B.cs
            //namespace x.y.z {}
            /*string xmlB = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name><name pos:line=""1"" pos:column=""11"">x</name><operator pos:line=""1"" pos:column=""12"">.</operator><name pos:line=""1"" pos:column=""13"">y</name><operator pos:line=""1"" pos:column=""14"">.</operator><name pos:line=""1"" pos:column=""15"">z</name></name> <block pos:line=""1"" pos:column=""17"">{}</block></namespace>";*/

            string xmlB = @"<namespace pos:start=""1:1"" pos:end=""1:19"">namespace <name pos:start=""1:11"" pos:end=""1:15""><name pos:start=""1:11"" pos:end=""1:11"">x</name><operator pos:start=""1:12"" pos:end=""1:12"">.</operator><name pos:start=""1:13"" pos:end=""1:13"">y</name><operator pos:start=""1:14"" pos:end=""1:14"">.</operator><name pos:start=""1:15"" pos:end=""1:15"">z</name></name> <block pos:start=""1:17"" pos:end=""1:19"">{ }</block></namespace>";
            
            XElement xmlElementB = fileSetup.GetFileUnitForXmlSnippet(xmlB, "B.cs");
            //A.cs
            //using x.y.z;
            //foo = 17;
            /*string xmlA = @"<using pos:line=""1"" pos:column=""1"">using <name><name pos:line=""1"" pos:column=""7"">x</name><operator pos:line=""1"" pos:column=""8"">.</operator><name pos:line=""1"" pos:column=""9"">y</name><operator pos:line=""1"" pos:column=""10"">.</operator><name pos:line=""1"" pos:column=""11"">z</name></name>;</using>
<expr_stmt><expr><name pos:line=""2"" pos:column=""1"">foo</name> <operator pos:line=""2"" pos:column=""5"">=</operator> <literal type=""number"" pos:line=""2"" pos:column=""7"">17</literal></expr>;</expr_stmt>";*/

            string xmlA = @"<using pos:start=""1:1"" pos:end=""1:12"">using <name pos:start=""1:7"" pos:end=""1:11""><name pos:start=""1:7"" pos:end=""1:7"">x</name><operator pos:start=""1:8"" pos:end=""1:8"">.</operator><name pos:start=""1:9"" pos:end=""1:9"">y</name><operator pos:start=""1:10"" pos:end=""1:10"">.</operator><name pos:start=""1:11"" pos:end=""1:11"">z</name></name>;</using>
<expr_stmt pos:start=""2:1"" pos:end=""2:9""><expr pos:start=""2:1"" pos:end=""2:8""><name pos:start=""2:1"" pos:end=""2:3"">foo</name> <operator pos:start=""2:5"" pos:end=""2:5"">=</operator> <literal type=""number"" pos:start=""2:7"" pos:end=""2:8"">17</literal></expr>;</expr_stmt>";

            XElement xmlElementA = fileSetup.GetFileUnitForXmlSnippet(xmlA, "A.cs");
            
            var scopeA = codeParser.ParseFileUnit(xmlElementA);
            var scopeB = codeParser.ParseFileUnit(xmlElementB);
            var globalScope = scopeA.Merge(scopeB);
            Assert.AreEqual(3, globalScope.ChildStatements.Count);
            var foo = globalScope.ChildStatements[1].Content.GetDescendantsAndSelf<NameUse>().FirstOrDefault(n => n.Name == "foo");
            Assert.IsNotNull(foo);
            var imports = foo.GetImports().ToList();
            Assert.AreEqual(1, imports.Count);
            Assert.AreEqual("x . y . z", imports[0].ImportedNamespace.ToString());

            var nsd = globalScope.GetDescendants<NamespaceDefinition>().FirstOrDefault(ns => ns.Name == "z");
            Assert.IsNotNull(nsd);
            var zUse = imports[0].ImportedNamespace.GetDescendantsAndSelf<NameUse>().LastOrDefault();
            Assert.IsNotNull(zUse);
            Assert.AreEqual("z", zUse.Name);
            Assert.AreSame(nsd, zUse.FindMatches().First());
        }

        [Test]
        public void TestGetImports_NestedImportNamespace() {
            //A.cs
            //namespace bar.baz {}
            /*string xmlA = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name><name pos:line=""1"" pos:column=""11"">bar</name><operator pos:line=""1"" pos:column=""14"">.</operator><name pos:line=""1"" pos:column=""15"">baz</name></name> <block pos:line=""1"" pos:column=""19"">{}</block></namespace>";*/

            string xmlA = @"<namespace pos:start=""1:1"" pos:end=""1:21"">namespace <name pos:start=""1:11"" pos:end=""1:17""><name pos:start=""1:11"" pos:end=""1:13"">bar</name><operator pos:start=""1:14"" pos:end=""1:14"">.</operator><name pos:start=""1:15"" pos:end=""1:17"">baz</name></name> <block pos:start=""1:19"" pos:end=""1:21"">{ }</block></namespace>";
            
            XElement xmlElementA = fileSetup.GetFileUnitForXmlSnippet(xmlA, "A.cs");
            //B.cs
            //using x.y.z;
            //if(bar) {
            //  using bar.baz;
            //  foo = 17;
            //}
            /*string xmlB = @"<using pos:line=""1"" pos:column=""1"">using <name><name pos:line=""1"" pos:column=""7"">x</name><operator pos:line=""1"" pos:column=""8"">.</operator><name pos:line=""1"" pos:column=""9"">y</name><operator pos:line=""1"" pos:column=""10"">.</operator><name pos:line=""1"" pos:column=""11"">z</name></name>;</using>
<if_stmt pos:line=""2"" pos:column=""1""><if pos:line=""2"" pos:column=""1"">if<condition pos:line=""2"" pos:column=""3"">(<expr><name pos:line=""2"" pos:column=""4"">bar</name></expr>)</condition><then pos:line=""2"" pos:column=""8""> <block pos:line=""2"" pos:column=""9"">{
  <using pos:line=""3"" pos:column=""3"">using <name><name pos:line=""3"" pos:column=""9"">bar</name><operator pos:line=""3"" pos:column=""12"">.</operator><name pos:line=""3"" pos:column=""13"">baz</name></name>;</using>
  <expr_stmt><expr><name pos:line=""4"" pos:column=""3"">foo</name> <operator pos:line=""4"" pos:column=""7"">=</operator> <literal type=""number"" pos:line=""4"" pos:column=""9"">17</literal></expr>;</expr_stmt>
}</block></then></if></if_stmt>";*/

            string xmlB = @"<using pos:start=""1:1"" pos:end=""1:12"">using <name pos:start=""1:7"" pos:end=""1:11""><name pos:start=""1:7"" pos:end=""1:7"">x</name><operator pos:start=""1:8"" pos:end=""1:8"">.</operator><name pos:start=""1:9"" pos:end=""1:9"">y</name><operator pos:start=""1:10"" pos:end=""1:10"">.</operator><name pos:start=""1:11"" pos:end=""1:11"">z</name></name>;</using>
<if_stmt pos:start=""2:1"" pos:end=""5:1""><if pos:start=""2:1"" pos:end=""5:1"">if<condition pos:start=""2:3"" pos:end=""2:7"">(<expr pos:start=""2:4"" pos:end=""2:6""><name pos:start=""2:4"" pos:end=""2:6"">bar</name></expr>)</condition> <block pos:start=""2:9"" pos:end=""5:1"">{<block_content pos:start=""3:3"" pos:end=""4:11"">
  <using pos:start=""3:3"" pos:end=""3:16"">using <name pos:start=""3:9"" pos:end=""3:15""><name pos:start=""3:9"" pos:end=""3:11"">bar</name><operator pos:start=""3:12"" pos:end=""3:12"">.</operator><name pos:start=""3:13"" pos:end=""3:15"">baz</name></name>;</using>
  <expr_stmt pos:start=""4:3"" pos:end=""4:11""><expr pos:start=""4:3"" pos:end=""4:10""><name pos:start=""4:3"" pos:end=""4:5"">foo</name> <operator pos:start=""4:7"" pos:end=""4:7"">=</operator> <literal type=""number"" pos:start=""4:9"" pos:end=""4:10"">17</literal></expr>;</expr_stmt>
</block_content>}</block></if></if_stmt>";

            XElement xmlElementB = fileSetup.GetFileUnitForXmlSnippet(xmlB, "B.cs");

            var scopeA = codeParser.ParseFileUnit(xmlElementA);
            var scopeB = codeParser.ParseFileUnit(xmlElementB);
            var globalScope = scopeA.Merge(scopeB);
            var foo = globalScope.ChildStatements[2].ChildStatements[1].Content.GetDescendantsAndSelf<NameUse>().FirstOrDefault(n => n.Name == "foo");
            Assert.IsNotNull(foo);
            var imports = foo.GetImports().ToList();
            Assert.AreEqual(2, imports.Count);
            Assert.AreEqual("bar . baz", imports[0].ImportedNamespace.ToString());
            Assert.AreEqual("x . y . z", imports[1].ImportedNamespace.ToString());

            var baz = globalScope.GetDescendants<NamespaceDefinition>().FirstOrDefault(ns => ns.Name == "baz");
            Assert.IsNotNull(baz);
            var bazUse = imports[0].ImportedNamespace.GetDescendantsAndSelf<NameUse>().LastOrDefault();
            Assert.IsNotNull(bazUse);
            Assert.AreEqual("baz", bazUse.Name);
            Assert.AreSame(baz, bazUse.FindMatches().First());
        }

        [Test]
        public void TestGetImports_SeparateFiles() {
            //A.cs
            //using x.y.z;
            //Foo = 17;
            /*string xmlA = @"<using pos:line=""1"" pos:column=""1"">using <name><name pos:line=""1"" pos:column=""7"">x</name><operator pos:line=""1"" pos:column=""8"">.</operator><name pos:line=""1"" pos:column=""9"">y</name><operator pos:line=""1"" pos:column=""10"">.</operator><name pos:line=""1"" pos:column=""11"">z</name></name>;</using>
<expr_stmt><expr><name pos:line=""2"" pos:column=""1"">Foo</name> <operator pos:line=""2"" pos:column=""5"">=</operator> <literal type=""number"" pos:line=""2"" pos:column=""7"">17</literal></expr>;</expr_stmt>";*/

            string xmlA = @"<using pos:start=""1:1"" pos:end=""1:12"">using <name pos:start=""1:7"" pos:end=""1:11""><name pos:start=""1:7"" pos:end=""1:7"">x</name><operator pos:start=""1:8"" pos:end=""1:8"">.</operator><name pos:start=""1:9"" pos:end=""1:9"">y</name><operator pos:start=""1:10"" pos:end=""1:10"">.</operator><name pos:start=""1:11"" pos:end=""1:11"">z</name></name>;</using>
<expr_stmt pos:start=""2:1"" pos:end=""2:9""><expr pos:start=""2:1"" pos:end=""2:8""><name pos:start=""2:1"" pos:end=""2:3"">Foo</name> <operator pos:start=""2:5"" pos:end=""2:5"">=</operator> <literal type=""number"" pos:start=""2:7"" pos:end=""2:8"">17</literal></expr>;</expr_stmt>";
            
            XElement xmlElementA = fileSetup.GetFileUnitForXmlSnippet(xmlA, "A.cs");
            //B.cs
            //using a.b.howdy;
            //Bar();
            /*string xmlB = @"<using pos:line=""1"" pos:column=""1"">using <name><name pos:line=""1"" pos:column=""7"">a</name><operator pos:line=""1"" pos:column=""8"">.</operator><name pos:line=""1"" pos:column=""9"">b</name><operator pos:line=""1"" pos:column=""10"">.</operator><name pos:line=""1"" pos:column=""11"">howdy</name></name>;</using>
<expr_stmt><expr><call><name pos:line=""2"" pos:column=""1"">Bar</name><argument_list pos:line=""2"" pos:column=""4"">()</argument_list></call></expr>;</expr_stmt>";*/

            string xmlB = @"<using pos:start=""1:1"" pos:end=""1:16"">using <name pos:start=""1:7"" pos:end=""1:15""><name pos:start=""1:7"" pos:end=""1:7"">a</name><operator pos:start=""1:8"" pos:end=""1:8"">.</operator><name pos:start=""1:9"" pos:end=""1:9"">b</name><operator pos:start=""1:10"" pos:end=""1:10"">.</operator><name pos:start=""1:11"" pos:end=""1:15"">howdy</name></name>;</using>
<expr_stmt pos:start=""2:1"" pos:end=""2:6""><expr pos:start=""2:1"" pos:end=""2:5""><call pos:start=""2:1"" pos:end=""2:5""><name pos:start=""2:1"" pos:end=""2:3"">Bar</name><argument_list pos:start=""2:4"" pos:end=""2:5"">()</argument_list></call></expr>;</expr_stmt>";
            
            XElement xmlElementB = fileSetup.GetFileUnitForXmlSnippet(xmlB, "B.cs");

            var scopeA = codeParser.ParseFileUnit(xmlElementA);
            var scopeB = codeParser.ParseFileUnit(xmlElementB);
            var globalScope = scopeA.Merge(scopeB);
            Assert.AreEqual(4, globalScope.ChildStatements.Count);

            var foo = globalScope.ChildStatements[1].Content.GetDescendantsAndSelf<NameUse>().FirstOrDefault(nu => nu.Name == "Foo");
            Assert.IsNotNull(foo);
            var fooImports = foo.GetImports().ToList();
            Assert.AreEqual(1, fooImports.Count);
            Assert.AreEqual("x . y . z", fooImports[0].ImportedNamespace.ToString());

            var bar = globalScope.ChildStatements[3].Content.GetDescendantsAndSelf<NameUse>().FirstOrDefault(nu => nu.Name == "Bar");
            Assert.IsNotNull(bar);
            var barImports = bar.GetImports().ToList();
            Assert.AreEqual(1, barImports.Count);
            Assert.AreEqual("a . b . howdy", barImports[0].ImportedNamespace.ToString());
        }

        [Test]
        public void TestGetAliases_NestedUsingAlias() {
            //A.cs
            //namespace bar {
            //  class baz {}
            //}
            /*string xmlA = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name pos:line=""1"" pos:column=""11"">bar</name> <block pos:line=""1"" pos:column=""15"">{
  <class pos:line=""2"" pos:column=""3"">class <name pos:line=""2"" pos:column=""9"">baz</name> <block pos:line=""2"" pos:column=""13"">{}</block></class>
}</block></namespace>";*/

            string xmlA = @"<namespace pos:start=""1:1"" pos:end=""4:1"">namespace <name pos:start=""1:11"" pos:end=""1:13"">bar</name> <block pos:start=""2:1"" pos:end=""4:1"">{
  <class pos:start=""3:3"" pos:end=""3:14"">class <name pos:start=""3:9"" pos:end=""3:11"">baz</name> <block pos:start=""3:13"" pos:end=""3:14"">{}</block></class>
}</block></namespace>";

            XElement xmlElementA = fileSetup.GetFileUnitForXmlSnippet(xmlA, "A.cs");
            //B.cs
            //using x.y.z;
            //if(bar) {
            //  using x = bar.baz;
            //  foo = 17;
            //}
            /*string xmlB = @"<using pos:line=""1"" pos:column=""1"">using <name><name pos:line=""1"" pos:column=""7"">x</name><operator pos:line=""1"" pos:column=""8"">.</operator><name pos:line=""1"" pos:column=""9"">y</name><operator pos:line=""1"" pos:column=""10"">.</operator><name pos:line=""1"" pos:column=""11"">z</name></name>;</using>
<if_stmt pos:line=""2"" pos:column=""1""><if pos:line=""2"" pos:column=""1"">if<condition pos:line=""2"" pos:column=""3"">(<expr><name pos:line=""2"" pos:column=""4"">bar</name></expr>)</condition><then pos:line=""2"" pos:column=""8""> <block pos:line=""2"" pos:column=""9"">{
  <using pos:line=""3"" pos:column=""3"">using <name pos:line=""3"" pos:column=""9"">x</name> <init pos:line=""3"" pos:column=""11"">= <expr><name><name pos:line=""3"" pos:column=""13"">bar</name><operator pos:line=""3"" pos:column=""16"">.</operator><name pos:line=""3"" pos:column=""17"">baz</name></name></expr></init>;</using>
  <expr_stmt><expr><name pos:line=""4"" pos:column=""3"">foo</name> <operator pos:line=""4"" pos:column=""7"">=</operator> <literal type=""number"" pos:line=""4"" pos:column=""9"">17</literal></expr>;</expr_stmt>
}</block></then></if></if_stmt>";*/

            string xmlB = @"<using pos:start=""1:1"" pos:end=""1:12"">using <name pos:start=""1:7"" pos:end=""1:11""><name pos:start=""1:7"" pos:end=""1:7"">x</name><operator pos:start=""1:8"" pos:end=""1:8"">.</operator><name pos:start=""1:9"" pos:end=""1:9"">y</name><operator pos:start=""1:10"" pos:end=""1:10"">.</operator><name pos:start=""1:11"" pos:end=""1:11"">z</name></name>;</using>
<if_stmt pos:start=""2:1"" pos:end=""5:1""><if pos:start=""2:1"" pos:end=""5:1"">if<condition pos:start=""2:3"" pos:end=""2:7"">(<expr pos:start=""2:4"" pos:end=""2:6""><name pos:start=""2:4"" pos:end=""2:6"">bar</name></expr>)</condition> <block pos:start=""2:9"" pos:end=""5:1"">{<block_content pos:start=""3:3"" pos:end=""4:11"">
  <using pos:start=""3:3"" pos:end=""3:20"">using <name pos:start=""3:9"" pos:end=""3:9"">x</name> <init pos:start=""3:11"" pos:end=""3:19"">= <expr pos:start=""3:13"" pos:end=""3:19""><name pos:start=""3:13"" pos:end=""3:19""><name pos:start=""3:13"" pos:end=""3:15"">bar</name><operator pos:start=""3:16"" pos:end=""3:16"">.</operator><name pos:start=""3:17"" pos:end=""3:19"">baz</name></name></expr></init>;</using>
  <expr_stmt pos:start=""4:3"" pos:end=""4:11""><expr pos:start=""4:3"" pos:end=""4:10""><name pos:start=""4:3"" pos:end=""4:5"">foo</name> <operator pos:start=""4:7"" pos:end=""4:7"">=</operator> <literal type=""number"" pos:start=""4:9"" pos:end=""4:10"">17</literal></expr>;</expr_stmt>
</block_content>}</block></if></if_stmt>";

            XElement xmlElementB = fileSetup.GetFileUnitForXmlSnippet(xmlB, "A.cs");
            
            var scopeA = codeParser.ParseFileUnit(xmlElementA);
            var scopeB = codeParser.ParseFileUnit(xmlElementB);
            var globalScope = scopeA.Merge(scopeB);
            var foo = globalScope.ChildStatements[2].ChildStatements[1].Content.GetDescendantsAndSelf<NameUse>().FirstOrDefault(n => n.Name == "foo");
            Assert.IsNotNull(foo);
            var aliases = foo.GetAliases().ToList();
            Assert.AreEqual(1, aliases.Count);
            Assert.AreEqual("bar . baz", aliases[0].Target.ToString());
            Assert.AreEqual("x", aliases[0].AliasName);
            var imports = foo.GetImports().ToList();
            Assert.AreEqual(1, imports.Count);
            Assert.AreEqual("x . y . z", imports[0].ImportedNamespace.ToString());

            var baz = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(ns => ns.Name == "baz");
            Assert.IsNotNull(baz);
            var bazUse = aliases[0].Target.GetDescendantsAndSelf<NameUse>().LastOrDefault(nu => nu.Name == "baz");
            Assert.IsNotNull(bazUse);
            Assert.AreSame(baz, bazUse.FindMatches().First());
        }

        [Test]
        public void TestImport_NameResolution() {
            //A.cs
            //using Foo.Bar;
            //
            //namespace A {
            //  public class Robot {
            //    public Baz GetThingy() { return new Baz(); }
            //  }
            //}
            /*string xmlA = @"<using pos:line=""1"" pos:column=""1"">using <name><name pos:line=""1"" pos:column=""7"">Foo</name><operator pos:line=""1"" pos:column=""10"">.</operator><name pos:line=""1"" pos:column=""11"">Bar</name></name>;</using>

<namespace pos:line=""3"" pos:column=""1"">namespace <name pos:line=""3"" pos:column=""11"">A</name> <block pos:line=""3"" pos:column=""13"">{
  <class><specifier pos:line=""4"" pos:column=""3"">public</specifier> class <name pos:line=""4"" pos:column=""16"">Robot</name> <block pos:line=""4"" pos:column=""22"">{
    <function><type><specifier pos:line=""5"" pos:column=""5"">public</specifier> <name pos:line=""5"" pos:column=""12"">Baz</name></type> <name pos:line=""5"" pos:column=""16"">GetThingy</name><parameter_list pos:line=""5"" pos:column=""25"">()</parameter_list> <block pos:line=""5"" pos:column=""28"">{ <return pos:line=""5"" pos:column=""30"">return <expr><operator pos:line=""5"" pos:column=""37"">new</operator> <call><name pos:line=""5"" pos:column=""41"">Baz</name><argument_list pos:line=""5"" pos:column=""44"">()</argument_list></call></expr>;</return> }</block></function>
  }</block></class>
}</block></namespace>";*/

            string xmlA = @"<using pos:start=""1:1"" pos:end=""1:14"">using <name pos:start=""1:7"" pos:end=""1:13""><name pos:start=""1:7"" pos:end=""1:9"">Foo</name><operator pos:start=""1:10"" pos:end=""1:10"">.</operator><name pos:start=""1:11"" pos:end=""1:13"">Bar</name></name>;</using>

<namespace pos:start=""3:1"" pos:end=""7:1"">namespace <name pos:start=""3:11"" pos:end=""3:11"">A</name> <block pos:start=""3:13"" pos:end=""7:1"">{
  <class pos:start=""4:3"" pos:end=""6:3""><specifier pos:start=""4:3"" pos:end=""4:8"">public</specifier> class <name pos:start=""4:16"" pos:end=""4:20"">Robot</name> <block pos:start=""4:22"" pos:end=""6:3"">{
    <function pos:start=""5:5"" pos:end=""5:48""><type pos:start=""5:5"" pos:end=""5:14""><specifier pos:start=""5:5"" pos:end=""5:10"">public</specifier> <name pos:start=""5:12"" pos:end=""5:14"">Baz</name></type> <name pos:start=""5:16"" pos:end=""5:24"">GetThingy</name><parameter_list pos:start=""5:25"" pos:end=""5:26"">()</parameter_list> <block pos:start=""5:28"" pos:end=""5:48"">{<block_content pos:start=""5:30"" pos:end=""5:46""> <return pos:start=""5:30"" pos:end=""5:46"">return <expr pos:start=""5:37"" pos:end=""5:45""><operator pos:start=""5:37"" pos:end=""5:39"">new</operator> <call pos:start=""5:41"" pos:end=""5:45""><name pos:start=""5:41"" pos:end=""5:43"">Baz</name><argument_list pos:start=""5:44"" pos:end=""5:45"">()</argument_list></call></expr>;</return> </block_content>}</block></function>
  }</block></class>
}</block></namespace>";

            XElement xmlElementA = fileSetup.GetFileUnitForXmlSnippet(xmlA, "A.cs");
            //B.cs
            //namespace Foo.Bar {
            //  public class Baz {
            //    public Baz() { }
            //  }
            //}
            /*string xmlB = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name><name pos:line=""1"" pos:column=""11"">Foo</name><operator pos:line=""1"" pos:column=""14"">.</operator><name pos:line=""1"" pos:column=""15"">Bar</name></name> <block pos:line=""1"" pos:column=""19"">{
  <class><specifier pos:line=""2"" pos:column=""3"">public</specifier> class <name pos:line=""2"" pos:column=""16"">Baz</name> <block pos:line=""2"" pos:column=""20"">{
    <constructor><specifier pos:line=""3"" pos:column=""5"">public</specifier> <name pos:line=""3"" pos:column=""12"">Baz</name><parameter_list pos:line=""3"" pos:column=""15"">()</parameter_list> <block pos:line=""3"" pos:column=""18"">{ }</block></constructor>
  }</block></class>
}</block></namespace>";*/

            string xmlB = @"<namespace pos:start=""1:1"" pos:end=""6:1"">namespace <name pos:start=""1:11"" pos:end=""1:17""><name pos:start=""1:11"" pos:end=""1:13"">Foo</name><operator pos:start=""1:14"" pos:end=""1:14"">.</operator><name pos:start=""1:15"" pos:end=""1:17"">Bar</name></name>
<block pos:start=""2:1"" pos:end=""6:1"">{
  <class pos:start=""3:3"" pos:end=""5:3""><specifier pos:start=""3:3"" pos:end=""3:8"">public</specifier> class <name pos:start=""3:16"" pos:end=""3:18"">Baz</name> <block pos:start=""3:20"" pos:end=""5:3"">{
    <constructor pos:start=""4:5"" pos:end=""4:20""><specifier pos:start=""4:5"" pos:end=""4:10"">public</specifier> <name pos:start=""4:12"" pos:end=""4:14"">Baz</name><parameter_list pos:start=""4:15"" pos:end=""4:16"">()</parameter_list> <block pos:start=""4:18"" pos:end=""4:20"">{<block_content> </block_content>}</block></constructor>
  }</block></class>
}</block></namespace>";

            XElement xmlElementB = fileSetup.GetFileUnitForXmlSnippet(xmlB, "B.cs");
            
            var scopeA = codeParser.ParseFileUnit(xmlElementA);
            var scopeB = codeParser.ParseFileUnit(xmlElementB);
            var globalScope = scopeA.Merge(scopeB);
            Assert.AreEqual(3, globalScope.ChildStatements.Count);

            var baz = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "Baz");
            Assert.IsNotNull(baz);

            var thingy = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "GetThingy");
            Assert.IsNotNull(thingy);
            var thingyTypes = thingy.ReturnType.FindMatches().ToList();
            Assert.AreEqual(1, thingyTypes.Count);
            Assert.AreSame(baz, thingyTypes[0]);

            var bazDef = baz.GetNamedChildren<MethodDefinition>("Baz").First();
            var bazCall = thingy.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault(mc => mc.Name == "Baz");
            Assert.IsNotNull(bazCall);
            Assert.AreSame(bazDef, bazCall.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestAlias_NameResolution() {
            //A.cs
            //namespace Foo.Bar {
            //  public class Baz {
            //    public static void DoTheThing() { };
            //  }
            //}
            /*string xmlA = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name><name pos:line=""1"" pos:column=""11"">Foo</name><operator pos:line=""1"" pos:column=""14"">.</operator><name pos:line=""1"" pos:column=""15"">Bar</name></name> <block pos:line=""1"" pos:column=""19"">{
  <class><specifier pos:line=""2"" pos:column=""3"">public</specifier> class <name pos:line=""2"" pos:column=""16"">Baz</name> <block pos:line=""2"" pos:column=""20"">{
    <function><type><specifier pos:line=""3"" pos:column=""5"">public</specifier> <specifier pos:line=""3"" pos:column=""12"">static</specifier> <name pos:line=""3"" pos:column=""19"">void</name></type> <name pos:line=""3"" pos:column=""24"">DoTheThing</name><parameter_list pos:line=""3"" pos:column=""34"">()</parameter_list> <block pos:line=""3"" pos:column=""37"">{ }</block></function><empty_stmt pos:line=""3"" pos:column=""40"">;</empty_stmt>
  }</block></class>
}</block></namespace>";*/

            string xmlA = @"<namespace pos:start=""1:1"" pos:end=""6:1"">namespace <name pos:start=""1:11"" pos:end=""1:17""><name pos:start=""1:11"" pos:end=""1:13"">Foo</name><operator pos:start=""1:14"" pos:end=""1:14"">.</operator><name pos:start=""1:15"" pos:end=""1:17"">Bar</name></name>
<block pos:start=""2:1"" pos:end=""6:1"">{
  <class pos:start=""3:3"" pos:end=""5:3""><specifier pos:start=""3:3"" pos:end=""3:8"">public</specifier> class <name pos:start=""3:16"" pos:end=""3:18"">Baz</name> <block pos:start=""3:20"" pos:end=""5:3"">{
    <function pos:start=""4:5"" pos:end=""4:39""><type pos:start=""4:5"" pos:end=""4:22""><specifier pos:start=""4:5"" pos:end=""4:10"">public</specifier> <specifier pos:start=""4:12"" pos:end=""4:17"">static</specifier> <name pos:start=""4:19"" pos:end=""4:22"">void</name></type> <name pos:start=""4:24"" pos:end=""4:33"">DoTheThing</name><parameter_list pos:start=""4:34"" pos:end=""4:35"">()</parameter_list> <block pos:start=""4:37"" pos:end=""4:39"">{<block_content> </block_content>}</block></function><empty_stmt pos:start=""4:40"" pos:end=""4:40"">;</empty_stmt>
  }</block></class>
}</block></namespace>";

            XElement xmlElementA = fileSetup.GetFileUnitForXmlSnippet(xmlA, "A.cs");
            //B.cs
            //using Baz = Foo.Bar.Baz;
            //namespace A {
            //  public class B {
            //    public B() {
            //      Baz.DoTheThing();
            //    }
            //  }
            //}
            /*string xmlB = @"<using pos:line=""1"" pos:column=""1"">using <name pos:line=""1"" pos:column=""7"">Baz</name> <init pos:line=""1"" pos:column=""11"">= <expr><name><name pos:line=""1"" pos:column=""13"">Foo</name><operator pos:line=""1"" pos:column=""16"">.</operator><name pos:line=""1"" pos:column=""17"">Bar</name><operator pos:line=""1"" pos:column=""20"">.</operator><name pos:line=""1"" pos:column=""21"">Baz</name></name></expr></init>;</using>
<namespace pos:line=""2"" pos:column=""1"">namespace <name pos:line=""2"" pos:column=""11"">A</name> <block pos:line=""2"" pos:column=""13"">{
  <class><specifier pos:line=""3"" pos:column=""3"">public</specifier> class <name pos:line=""3"" pos:column=""16"">B</name> <block pos:line=""3"" pos:column=""18"">{
    <constructor><specifier pos:line=""4"" pos:column=""5"">public</specifier> <name pos:line=""4"" pos:column=""12"">B</name><parameter_list pos:line=""4"" pos:column=""13"">()</parameter_list> <block pos:line=""4"" pos:column=""16"">{
      <expr_stmt><expr><call><name><name pos:line=""5"" pos:column=""7"">Baz</name><operator pos:line=""5"" pos:column=""10"">.</operator><name pos:line=""5"" pos:column=""11"">DoTheThing</name></name><argument_list pos:line=""5"" pos:column=""21"">()</argument_list></call></expr>;</expr_stmt>
    }</block></constructor>
  }</block></class>
}</block></namespace>";*/

            string xmlB = @"<using pos:start=""1:1"" pos:end=""1:24"">using <name pos:start=""1:7"" pos:end=""1:9"">Baz</name> <init pos:start=""1:11"" pos:end=""1:23"">= <expr pos:start=""1:13"" pos:end=""1:23""><name pos:start=""1:13"" pos:end=""1:23""><name pos:start=""1:13"" pos:end=""1:15"">Foo</name><operator pos:start=""1:16"" pos:end=""1:16"">.</operator><name pos:start=""1:17"" pos:end=""1:19"">Bar</name><operator pos:start=""1:20"" pos:end=""1:20"">.</operator><name pos:start=""1:21"" pos:end=""1:23"">Baz</name></name></expr></init>;</using>
<namespace pos:start=""2:1"" pos:end=""8:1"">namespace <name pos:start=""2:11"" pos:end=""2:11"">A</name> <block pos:start=""2:13"" pos:end=""8:1"">{
  <class pos:start=""3:3"" pos:end=""7:3""><specifier pos:start=""3:3"" pos:end=""3:8"">public</specifier> class <name pos:start=""3:16"" pos:end=""3:16"">B</name> <block pos:start=""3:18"" pos:end=""7:3"">{
    <constructor pos:start=""4:5"" pos:end=""6:5""><specifier pos:start=""4:5"" pos:end=""4:10"">public</specifier> <name pos:start=""4:12"" pos:end=""4:12"">B</name><parameter_list pos:start=""4:13"" pos:end=""4:14"">()</parameter_list> <block pos:start=""4:16"" pos:end=""6:5"">{<block_content pos:start=""5:7"" pos:end=""5:23"">
      <expr_stmt pos:start=""5:7"" pos:end=""5:23""><expr pos:start=""5:7"" pos:end=""5:22""><call pos:start=""5:7"" pos:end=""5:22""><name pos:start=""5:7"" pos:end=""5:20""><name pos:start=""5:7"" pos:end=""5:9"">Baz</name><operator pos:start=""5:10"" pos:end=""5:10"">.</operator><name pos:start=""5:11"" pos:end=""5:20"">DoTheThing</name></name><argument_list pos:start=""5:21"" pos:end=""5:22"">()</argument_list></call></expr>;</expr_stmt>
    </block_content>}</block></constructor>
  }</block></class>
}</block></namespace>";

            XElement xmlElementB = fileSetup.GetFileUnitForXmlSnippet(xmlB, "B.cs");
            
            var scopeA = codeParser.ParseFileUnit(xmlElementA);
            var scopeB = codeParser.ParseFileUnit(xmlElementB);
            var globalScope = scopeA.Merge(scopeB);
            Assert.AreEqual(3, globalScope.ChildStatements.Count);

            var thingDef = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(md => md.Name == "DoTheThing");
            Assert.IsNotNull(thingDef);
            Assert.AreEqual("Baz", ((TypeDefinition)thingDef.ParentStatement).Name);

            var bDef = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(md => md.Name == "B");
            Assert.IsNotNull(bDef);
            Assert.AreEqual(1, bDef.ChildStatements.Count);
            var thingCall = bDef.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(thingCall);
            Assert.AreSame(thingDef, thingCall.FindMatches().First());
        }

        /*[Test]
        [Category("SrcMLUpdate")]
        public void TestUsingBlock_SingleDecl() {
            //using(var f = File.Open("out.txt")) {
            //  ;
            //}
            string xml = @"<using>using(<decl><type><name>var</name></type> <name>f</name> <init>= <expr><call><name><name>File</name><operator>.</operator><name>Open</name></name><argument_list>(<argument><expr><literal type=""string"">""out.txt""</literal></expr></argument>)</argument_list></call></expr></init></decl>) <block>{
  <empty_stmt>;</empty_stmt>
}</block></using>";
            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(1, globalScope.ChildStatements.Count);

            var actual = globalScope.ChildStatements[0] as UsingBlockStatement;
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.ChildStatements.Count);
            Assert.IsNotNull(actual.Initializer);
            var decls = actual.Initializer.GetDescendantsAndSelf<VariableDeclaration>().ToList();
            Assert.AreEqual(1, decls.Count);
            Assert.AreEqual("f", decls[0].Name);
            Assert.AreEqual("var", decls[0].VariableType.Name);
            Assert.IsNotNull(decls[0].Initializer);
        }

        [Test]
        [Category("SrcMLUpdate")]
        public void TestUsingBlock_MultipleDecl() {
            // using(Foo a = new Foo(1), b = new Foo(2)) { ; }
            string xml = @"<using>using(<decl><type><name>Foo</name></type> <name>a</name> <init>= <expr><operator>new</operator> <call><name>Foo</name><argument_list>(<argument><expr><literal type=""number"">1</literal></expr></argument>)</argument_list></call></expr></init><operator>,</operator> <name>b</name> <init>= <expr><operator>new</operator> <call><name>Foo</name><argument_list>(<argument><expr><literal type=""number"">2</literal></expr></argument>)</argument_list></call></expr></init></decl>) <block>{ <empty_stmt>;</empty_stmt> }</block></using>";
            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(1, globalScope.ChildStatements.Count);

            var actual = globalScope.ChildStatements[0] as UsingBlockStatement;
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.ChildStatements.Count);
            Assert.IsNotNull(actual.Initializer);
            var decls = actual.Initializer.GetDescendantsAndSelf<VariableDeclaration>().ToList();
            Assert.AreEqual(2, decls.Count);
            Assert.AreEqual("a", decls[0].Name);
            Assert.AreEqual("Foo", decls[0].VariableType.Name);
            Assert.IsNotNull(decls[0].Initializer);
            Assert.AreEqual("b", decls[1].Name);
            Assert.AreEqual("Foo", decls[1].VariableType.Name);
            Assert.IsNotNull(decls[1].Initializer);
        }

        [Test]
        [Category("SrcMLUpdate")]
        public void TestUsingBlock_Expression() {
            //using(bar = new Foo()) { ; }
            string xml = @"<using_stmt>using(<expr><name>bar</name> <operator>=</operator> <operator>new</operator> <call><name>Foo</name><argument_list>()</argument_list></call></expr>) <block>{ <empty_stmt>;</empty_stmt> }</block></using_stmt>";
            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(1, globalScope.ChildStatements.Count);

            var actual = globalScope.ChildStatements[0] as UsingBlockStatement;
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.ChildStatements.Count);
            var init = actual.Initializer;
            Assert.IsNotNull(actual.Initializer);
            Assert.AreEqual(4, init.Components.Count);
            var bar = init.Components[0] as NameUse;
            Assert.IsNotNull(bar);
            Assert.AreEqual("bar", bar.Name);
            var equals = init.Components[1] as OperatorUse;
            Assert.IsNotNull(equals);
            Assert.AreEqual("=", equals.Text);
            var newOp = init.Components[2] as OperatorUse;
            Assert.IsNotNull(newOp);
            Assert.AreEqual("new", newOp.Text);
            var foo = init.Components[3] as MethodCall;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(0, foo.Arguments.Count);
        }
        */

        [Test]
        public void TestCreateTypeDefinition_Class() {
            ////Foo.cs
            //public class Foo {
            //    public int bar;
            //}
            /*string fooXml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";*/

            string fooXml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";
            var fooFileUnit = fileSetup.GetFileUnitForXmlSnippet(fooXml, "Foo.cs");
            var globalScope = codeParser.ParseFileUnit(fooFileUnit);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var foo = globalScope.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(TypeKind.Class, foo.Kind);
            Assert.AreEqual(1, foo.ChildStatements.Count());

            var bar = foo.ChildStatements[0].Content as VariableDeclaration;
            Assert.IsNotNull(bar);
            Assert.AreEqual("bar", bar.Name);
            Assert.AreEqual("int", bar.VariableType.Name);
            Assert.AreEqual(AccessModifier.Public, bar.Accessibility);
        }

        [Test]
        public void TestCreateTypeDefinition_ClassWithParent() {
            ////Foo.cs
            //public class Foo : Baz {
            //    public int bar;
            //}
            /*string fooXml = @"<class><specifier>public</specifier> class <name>Foo</name> <super>: <name>Baz</name></super> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";*/

            string fooXml = @"<class><specifier>public</specifier> class <name>Foo</name> <super_list>: <super><name>Baz</name></super></super_list> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";
            var fooFileUnit = fileSetup.GetFileUnitForXmlSnippet(fooXml, "Foo.cs");
            var globalScope = codeParser.ParseFileUnit(fooFileUnit);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var foo = globalScope.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(TypeKind.Class, foo.Kind);
            Assert.AreEqual(1, foo.ChildStatements.Count());
            Assert.AreEqual(1, foo.ParentTypeNames.Count);
            Assert.AreEqual("Baz", foo.ParentTypeNames.First().Name);
        }

        [Test]
        public void TestCreateTypeDefinition_ClassWithQualifiedParent() {
            ////Foo.cs
            //public class Foo : Baz, System.IDisposable {
            //    public int bar;
            //}
            /*string fooXml = @"<class><specifier>public</specifier> class <name>Foo</name> <super>: <name>Baz</name>, <name><name>System</name><operator>.</operator><name>IDisposable</name></name></super> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";*/

            string fooXml = @"<class><specifier>public</specifier> class <name>Foo</name> <super_list>: <super><name>Baz</name></super>, <super><name><name>System</name><operator>.</operator><name>IDisposable</name></name></super></super_list> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";

            var fooFileUnit = fileSetup.GetFileUnitForXmlSnippet(fooXml, "Foo.cs");
            var globalScope = codeParser.ParseFileUnit(fooFileUnit);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var foo = globalScope.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(TypeKind.Class, foo.Kind);
            Assert.AreEqual(1, foo.ChildStatements.Count());
            Assert.AreEqual(2, foo.ParentTypeNames.Count);
            Assert.AreEqual("Baz", foo.ParentTypeNames[0].Name);
            Assert.AreEqual("IDisposable", foo.ParentTypeNames[1].Name);
            Assert.AreEqual("System", foo.ParentTypeNames[1].Prefix.Names.First().Name);
        }

        [Test]
        public void TestCreateTypeDefinition_CompoundNamespace() {
            ////Foo.cs
            //namespace Example.Level2.Level3 {
            //    public class Foo {
            //        public int bar;
            //    }
            //}
            /*string fooXml = @"<namespace>namespace <name><name>Example</name><operator>.</operator><name>Level2</name><operator>.</operator><name>Level3</name></name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
    }</block></class>
}</block></namespace>";*/

            string fooXml = @"<namespace>namespace <name><name>Example</name><operator>.</operator><name>Level2</name><operator>.</operator><name>Level3</name></name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
    }</block></class>
}</block></namespace>";

            var fooFileUnit = fileSetup.GetFileUnitForXmlSnippet(fooXml, "Foo.cs");
            var globalScope = codeParser.ParseFileUnit(fooFileUnit);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var example = globalScope.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(example);
            Assert.AreEqual("Example", example.Name);
            Assert.AreEqual(1, example.ChildStatements.Count());
            var level2 = example.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(level2);
            Assert.AreEqual("Level2", level2.Name);
            Assert.AreEqual(1, level2.ChildStatements.Count());
            var level3 = level2.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(level3);
            Assert.AreEqual("Level3", level3.Name);
            Assert.AreEqual(1, level3.ChildStatements.Count());
            var foo = level3.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(1, foo.ChildStatements.Count());
        }

        [Test]
        public void TestCreateTypeDefinition_Interface() {
            ////Foo.cs
            //public interface Foo {
            //    public int GetBar();
            //}
            /*string fooXml = @"<class type=""interface""><specifier>public</specifier> interface <name>Foo</name> <block>{
    <function_decl><type><specifier>public</specifier> <name>int</name></type> <name>GetBar</name><parameter_list>()</parameter_list>;</function_decl>
}</block></class>";*/

            string fooXml = @"<interface><specifier>public</specifier> interface <name>Foo</name> <block>{
    <function_decl><type><specifier>public</specifier> <name>int</name></type> <name>GetBar</name><parameter_list>()</parameter_list>;</function_decl>
}</block></interface>";

            var fooFileUnit = fileSetup.GetFileUnitForXmlSnippet(fooXml, "Foo.cs");
            var globalScope = codeParser.ParseFileUnit(fooFileUnit);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var foo = globalScope.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(TypeKind.Interface, foo.Kind);
            Assert.AreEqual(1, foo.ChildStatements.Count());
        }

        [Test]
        public void TestCreateTypeDefinition_Namespace() {
            ////Foo.cs
            //namespace Example {
            //    public class Foo {
            //        public int bar;
            //    }
            //}
            /*string fooXml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
    }</block></class>
}</block></namespace>";*/

            string fooXml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
    }</block></class>
}</block></namespace>";

            var fooFileUnit = fileSetup.GetFileUnitForXmlSnippet(fooXml, "Foo.cs");
            var globalScope = codeParser.ParseFileUnit(fooFileUnit);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var example = globalScope.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(example);
            Assert.AreEqual("Example", example.Name);
            Assert.AreEqual(1, example.ChildStatements.Count());
            var foo = example.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(1, foo.ChildStatements.Count());
        }

        [Test]
        public void TestCreateTypeDefinition_NestedCompoundNamespace() {
            ////Foo.cs
            //namespace Watermelon {
            //    namespace Example.Level2.Level3 {
            //        public class Foo {
            //            public int bar;
            //        }
            //    }
            //}
            /*string fooXml = @"<namespace>namespace <name>Watermelon</name> <block>{
    <namespace>namespace <name><name>Example</name><operator>.</operator><name>Level2</name><operator>.</operator><name>Level3</name></name> <block>{
        <class><specifier>public</specifier> class <name>Foo</name> <block>{
            <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
        }</block></class>
    }</block></namespace>
}</block></namespace>";*/

            string fooXml = @"<namespace>namespace <name>Watermelon</name> <block>{
    <namespace>namespace <name><name>Example</name><operator>.</operator><name>Level2</name><operator>.</operator><name>Level3</name></name> <block>{
        <class><specifier>public</specifier> class <name>Foo</name> <block>{
            <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
        }</block></class>
    }</block></namespace>
}</block></namespace>";

            var fooFileUnit = fileSetup.GetFileUnitForXmlSnippet(fooXml, "Foo.cs");
            var globalScope = codeParser.ParseFileUnit(fooFileUnit);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var watermelon = globalScope.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(watermelon);
            Assert.AreEqual("Watermelon", watermelon.Name);
            Assert.AreEqual(1, watermelon.ChildStatements.Count());
            var example = watermelon.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(example);
            Assert.AreEqual("Example", example.Name);
            Assert.AreEqual(1, example.ChildStatements.Count());
            var level2 = example.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(level2);
            Assert.AreEqual("Level2", level2.Name);
            Assert.AreEqual(1, level2.ChildStatements.Count());
            var level3 = level2.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(level3);
            Assert.AreEqual("Level3", level3.Name);
            Assert.AreEqual(1, level3.ChildStatements.Count());
            var foo = level3.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(1, foo.ChildStatements.Count());
        }

        [Test]
        public void TestCreateTypeDefinition_Struct() {
            ////Foo.cs
            //public struct Foo {
            //    public int bar;
            //}
            /*string fooXml = @"<struct><specifier>public</specifier> struct <name>Foo</name> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></struct>";*/

            string fooXml = @"<struct><specifier>public</specifier> struct <name>Foo</name> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></struct>";

            var fooFileUnit = fileSetup.GetFileUnitForXmlSnippet(fooXml, "Foo.cs");
            var globalScope = codeParser.ParseFileUnit(fooFileUnit);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var foo = globalScope.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual(TypeKind.Struct, foo.Kind);
            Assert.AreEqual(1, foo.ChildStatements.Count());
        }

        [Test]
        public void TestCreateTypeDefinitions_ClassWithInnerClass() {
            ////A.cs
            //class A {
            //    class B {}
            //}
            /*string xml = @"<class>class <name>A</name> <block>{
    <class>class <name>B</name> <block>{}</block></class>
}</block></class>";*/

            string xml = @"<class>class <name>A</name> <block>{
    <class>class <name>B</name> <block>{}</block></class>
}</block></class>";

            var xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");
            var globalScope = codeParser.ParseFileUnit(xmlElement);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var typeA = globalScope.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(typeA);
            Assert.AreEqual(1, typeA.ChildStatements.Count());
            var typeB = typeA.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(typeB);

            Assert.AreSame(typeA, typeB.ParentStatement);
            Assert.AreEqual("A", typeA.GetFullName());
            Assert.AreEqual("A.B", typeB.GetFullName());
        }

        [Test]
        public void TestCreateTypeDefinitions_InnerClassWithNamespace() {
            ////A.cs
            //namespace Foo {
            //    class A {
            //        class B {}
            //    }
            //}
            /*string xml = @"<namespace>namespace <name>Foo</name> <block>{
    <class>class <name>A</name> <block>{
        <class>class <name>B</name> <block>{}</block></class>
    }</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Foo</name> <block>{
    <class>class <name>A</name> <block>{
        <class>class <name>B</name> <block>{}</block></class>
    }</block></class>
}</block></namespace>";

            var xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");
            var globalScope = codeParser.ParseFileUnit(xmlElement);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var foo = globalScope.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(foo);
            Assert.AreEqual(1, foo.ChildStatements.Count());
            var typeA = foo.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(typeA);
            Assert.AreEqual(1, typeA.ChildStatements.Count());
            var typeB = typeA.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(typeB);

            Assert.AreEqual("A", typeA.Name);
            Assert.AreEqual("Foo", typeA.GetAncestors<NamespaceDefinition>().First().GetFullName());
            Assert.AreEqual("Foo.A", typeA.GetFullName());

            Assert.AreEqual("B", typeB.Name);
            Assert.AreEqual("Foo", typeB.GetAncestors<NamespaceDefinition>().First().GetFullName());
            Assert.AreEqual("Foo.A.B", typeB.GetFullName());
        }

        [Test]
        public void TestDeclarationWithTypeVarFromConstructor() {
            // B.cs namespace A { class B { public B() { }; } }
            /*string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <block>{ <constructor><specifier>public</specifier> <name>B</name><parameter_list>()</parameter_list> <block>{ }</block></constructor><empty_stmt>;</empty_stmt> }</block></class> }</block></namespace>";*/
            string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <block>{ <constructor><specifier>public</specifier> <name>B</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></constructor><empty_stmt>;</empty_stmt> }</block></class> }</block></namespace>";
            
            // C.cs namespace A { class C { void main() { var b = new B(); } } }
            /*string cXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>C</name> <block>{ <function><type><name>void</name></type> <name>main</name><parameter_list>()</parameter_list> <block>{ <decl_stmt><decl><type><name>var</name></type> <name>b</name> =<init> <expr><operator>new</operator> <call><name>B</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt> }</block></function> }</block></class> }</block></namespace>";*/
            string cXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>C</name> <block>{ <function><type><name>void</name></type> <name>main</name><parameter_list>()</parameter_list> <block>{<block_content> <decl_stmt><decl><type><name>var</name></type> <name>b</name> <init>= <expr><operator>new</operator> <call><name>B</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt> </block_content>}</block></function> }</block></class> }</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(cXml, "C.cs");
            var bScope = codeParser.ParseFileUnit(bUnit);
            var cScope = codeParser.ParseFileUnit(cUnit);
            var globalScope = bScope.Merge(cScope);

            var typeB = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "B");
            Assert.IsNotNull(typeB);
            var main = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "main");
            Assert.IsNotNull(main);

            Assert.AreEqual(1, main.ChildStatements.Count);
            var varDecl = main.ChildStatements[0].Content.GetDescendantsAndSelf<VariableDeclaration>().FirstOrDefault();
            Assert.IsNotNull(varDecl);

            Assert.AreSame(typeB, varDecl.ResolveType().FirstOrDefault());
        }

        [Test]
        public void TestDeclarationWithTypeVarFromImplicitConstructor() {
            // B.cs namespace A { class B { } }
            /*string bXml = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name pos:line=""1"" pos:column=""11"">A</name> <block pos:line=""1"" pos:column=""13"">{ <class pos:line=""1"" pos:column=""15"">class <name pos:line=""1"" pos:column=""21"">B</name> <block pos:line=""1"" pos:column=""23"">{ }</block></class> }</block></namespace>";*/
            string bXml = @"<namespace pos:start=""1:1"" pos:end=""1:27"">namespace <name pos:start=""1:11"" pos:end=""1:11"">A</name> <block pos:start=""1:13"" pos:end=""1:27"">{ <class pos:start=""1:15"" pos:end=""1:25"">class <name pos:start=""1:21"" pos:end=""1:21"">B</name> <block pos:start=""1:23"" pos:end=""1:25"">{ }</block></class> }</block></namespace>";
            
            // C.cs namespace A { class C { void main() { var b = new B(); } } }
            /*string cXml = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name pos:line=""1"" pos:column=""11"">A</name> <block pos:line=""1"" pos:column=""13"">{ <class pos:line=""1"" pos:column=""15"">class <name pos:line=""1"" pos:column=""21"">C</name> <block pos:line=""1"" pos:column=""23"">{ <function><type><name pos:line=""1"" pos:column=""25"">void</name></type> <name pos:line=""1"" pos:column=""30"">main</name><parameter_list pos:line=""1"" pos:column=""34"">()</parameter_list> <block pos:line=""1"" pos:column=""37"">{ <decl_stmt><decl><type><name pos:line=""1"" pos:column=""39"">var</name></type> <name pos:line=""1"" pos:column=""43"">b</name> <init pos:line=""1"" pos:column=""45"">= <expr><operator pos:line=""1"" pos:column=""47"">new</operator> <call><name pos:line=""1"" pos:column=""51"">B</name><argument_list pos:line=""1"" pos:column=""52"">()</argument_list></call></expr></init></decl>;</decl_stmt> }</block></function> }</block></class> }</block></namespace>";*/
            string cXml = @"<namespace pos:start=""1:1"" pos:end=""1:60"">namespace <name pos:start=""1:11"" pos:end=""1:11"">A</name> <block pos:start=""1:13"" pos:end=""1:60"">{ <class pos:start=""1:15"" pos:end=""1:58"">class <name pos:start=""1:21"" pos:end=""1:21"">C</name> <block pos:start=""1:23"" pos:end=""1:58"">{ <function pos:start=""1:25"" pos:end=""1:56""><type pos:start=""1:25"" pos:end=""1:28""><name pos:start=""1:25"" pos:end=""1:28"">void</name></type> <name pos:start=""1:30"" pos:end=""1:33"">main</name><parameter_list pos:start=""1:34"" pos:end=""1:35"">()</parameter_list> <block pos:start=""1:37"" pos:end=""1:56"">{<block_content pos:start=""1:39"" pos:end=""1:54""> <decl_stmt pos:start=""1:39"" pos:end=""1:54""><decl pos:start=""1:39"" pos:end=""1:53""><type pos:start=""1:39"" pos:end=""1:41""><name pos:start=""1:39"" pos:end=""1:41"">var</name></type> <name pos:start=""1:43"" pos:end=""1:43"">b</name> <init pos:start=""1:45"" pos:end=""1:53"">= <expr pos:start=""1:47"" pos:end=""1:53""><operator pos:start=""1:47"" pos:end=""1:49"">new</operator> <call pos:start=""1:51"" pos:end=""1:53""><name pos:start=""1:51"" pos:end=""1:51"">B</name><argument_list pos:start=""1:52"" pos:end=""1:53"">()</argument_list></call></expr></init></decl>;</decl_stmt> </block_content>}</block></function> }</block></class> }</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(cXml, "C.cs");
            var bScope = codeParser.ParseFileUnit(bUnit);
            var cScope = codeParser.ParseFileUnit(cUnit);
            var globalScope = bScope.Merge(cScope);

            var typeB = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "B");
            Assert.IsNotNull(typeB);
            var main = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "main");
            Assert.IsNotNull(main);

            Assert.AreEqual(1, main.ChildStatements.Count);
            var varDecl = main.ChildStatements[0].Content.GetDescendantsAndSelf<VariableDeclaration>().FirstOrDefault();
            Assert.IsNotNull(varDecl);

            Assert.AreSame(typeB, varDecl.ResolveType().FirstOrDefault());
        }

        [Test]
        public void TestDeclarationWithTypeVarFromMethod() {
            //namespace A {
            //    class B {
            //        public static void main() { var b = getB(); }
            //        public static B getB() { return new B(); }
            //    }
            //}
            /*string xml = @"<namespace>namespace <name>A</name> <block>{
    <class>class <name>B</name> <block>{
        <function><type><specifier>public</specifier> <specifier>static</specifier> <name>void</name></type> <name>main</name><parameter_list>()</parameter_list> <block>{ <decl_stmt><decl><type><name>var</name></type> <name>b</name> =<init> <expr><call><name>getB</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt> }</block></function>
        <function><type><specifier>public</specifier> <specifier>static</specifier> <name>B</name></type> <name>getB</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><operator>new</operator> <call><name>B</name><argument_list>()</argument_list></call></expr>;</return> }</block></function>
    }</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>A</name> <block>{
    <class>class <name>B</name> <block>{
        <function><type><specifier>public</specifier> <specifier>static</specifier> <name>void</name></type> <name>main</name><parameter_list>()</parameter_list> <block>{<block_content> <decl_stmt><decl><type><name>var</name></type> <name>b</name> <init>= <expr><call><name>getB</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt> </block_content>}</block></function>
        <function><type><specifier>public</specifier> <specifier>static</specifier> <name>B</name></type> <name>getB</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><operator>new</operator> <call><name>B</name><argument_list>()</argument_list></call></expr>;</return> </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "B.cs");
            var globalScope = codeParser.ParseFileUnit(unit);

            var typeB = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "B");
            Assert.IsNotNull(typeB);
            var mainMethod = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "main");
            Assert.IsNotNull(mainMethod);

            Assert.AreEqual(1, mainMethod.ChildStatements.Count);
            var varDecl = mainMethod.ChildStatements[0].Content.GetDescendantsAndSelf<VariableDeclaration>().FirstOrDefault();
            Assert.IsNotNull(varDecl);

            Assert.AreSame(typeB, varDecl.ResolveType().FirstOrDefault());
        }

        [Test]
        public void TestDeclarationWithTypeVarInForeach() {
            //class Foo {
            //    int[] GetInts() {
            //        return new[] {1, 2, 3, 4};
            //    }
            //    int main() {
            //        foreach(var num in GetInts()) {
            //            print(num);
            //        }
            //    }
            //}
            /*string xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">Foo</name> <block pos:line=""1"" pos:column=""11"">{
    <function><type><name pos:line=""2"" pos:column=""5"">int</name><index pos:line=""2"" pos:column=""8"">[]</index></type> <name pos:line=""2"" pos:column=""11"">GetInts</name><parameter_list pos:line=""2"" pos:column=""18"">()</parameter_list> <block pos:line=""2"" pos:column=""21"">{
        <return pos:line=""3"" pos:column=""9"">return <expr><operator pos:line=""3"" pos:column=""16"">new</operator><index pos:line=""3"" pos:column=""19"">[]</index> <block pos:line=""3"" pos:column=""22"">{<expr><literal type=""number"" pos:line=""3"" pos:column=""23"">1</literal></expr><operator pos:line=""3"" pos:column=""24"">,</operator> <expr><literal type=""number"" pos:line=""3"" pos:column=""26"">2</literal></expr><operator pos:line=""3"" pos:column=""27"">,</operator> <expr><literal type=""number"" pos:line=""3"" pos:column=""29"">3</literal></expr><operator pos:line=""3"" pos:column=""30"">,</operator> <expr><literal type=""number"" pos:line=""3"" pos:column=""32"">4</literal></expr>}</block></expr>;</return>
    }</block></function>
    <function><type><name pos:line=""5"" pos:column=""5"">int</name></type> <name pos:line=""5"" pos:column=""9"">main</name><parameter_list pos:line=""5"" pos:column=""13"">()</parameter_list> <block pos:line=""5"" pos:column=""16"">{
        <foreach pos:line=""6"" pos:column=""9"">foreach(<init><decl><type><name pos:line=""6"" pos:column=""17"">var</name></type> <name pos:line=""6"" pos:column=""21"">num</name> <range pos:line=""6"" pos:column=""25"">in <expr><call><name pos:line=""6"" pos:column=""28"">GetInts</name><argument_list pos:line=""6"" pos:column=""35"">()</argument_list></call></expr></range></decl></init>) <block pos:line=""6"" pos:column=""39"">{
            <expr_stmt><expr><call><name pos:line=""7"" pos:column=""13"">print</name><argument_list pos:line=""7"" pos:column=""18"">(<argument><expr><name pos:line=""7"" pos:column=""19"">num</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
        }</block></foreach>
    }</block></function>
}</block></class>";*/

            string xml = @"<class pos:start=""1:1"" pos:end=""11:1"">class <name pos:start=""1:7"" pos:end=""1:9"">Foo</name>
<block pos:start=""2:1"" pos:end=""11:1"">{
    <function pos:start=""3:5"" pos:end=""5:5""><type pos:start=""3:5"" pos:end=""3:9""><name pos:start=""3:5"" pos:end=""3:9""><name pos:start=""3:5"" pos:end=""3:7"">int</name><index pos:start=""3:8"" pos:end=""3:9"">[]</index></name></type> <name pos:start=""3:11"" pos:end=""3:17"">GetInts</name><parameter_list pos:start=""3:18"" pos:end=""3:19"">()</parameter_list> <block pos:start=""3:21"" pos:end=""5:5"">{<block_content pos:start=""4:9"" pos:end=""4:34"">
        <return pos:start=""4:9"" pos:end=""4:34"">return <expr pos:start=""4:16"" pos:end=""4:33""><operator pos:start=""4:16"" pos:end=""4:18"">new</operator><index pos:start=""4:19"" pos:end=""4:20"">[]</index> <block pos:start=""4:22"" pos:end=""4:33"">{<expr pos:start=""4:23"" pos:end=""4:23""><literal type=""number"" pos:start=""4:23"" pos:end=""4:23"">1</literal></expr>, <expr pos:start=""4:26"" pos:end=""4:26""><literal type=""number"" pos:start=""4:26"" pos:end=""4:26"">2</literal></expr>, <expr pos:start=""4:29"" pos:end=""4:29""><literal type=""number"" pos:start=""4:29"" pos:end=""4:29"">3</literal></expr>, <expr pos:start=""4:32"" pos:end=""4:32""><literal type=""number"" pos:start=""4:32"" pos:end=""4:32"">4</literal></expr>}</block></expr>;</return>
    </block_content>}</block></function>
    <function pos:start=""6:5"" pos:end=""10:5""><type pos:start=""6:5"" pos:end=""6:7""><name pos:start=""6:5"" pos:end=""6:7"">int</name></type> <name pos:start=""6:9"" pos:end=""6:12"">main</name><parameter_list pos:start=""6:13"" pos:end=""6:14"">()</parameter_list> <block pos:start=""6:16"" pos:end=""10:5"">{<block_content pos:start=""7:9"" pos:end=""9:9"">
        <foreach pos:start=""7:9"" pos:end=""9:9"">foreach<control pos:start=""7:16"" pos:end=""7:37"">(<init pos:start=""7:17"" pos:end=""7:36""><decl pos:start=""7:17"" pos:end=""7:36""><type pos:start=""7:17"" pos:end=""7:19""><name pos:start=""7:17"" pos:end=""7:19"">var</name></type> <name pos:start=""7:21"" pos:end=""7:23"">num</name> <range pos:start=""7:25"" pos:end=""7:36"">in <expr pos:start=""7:28"" pos:end=""7:36""><call pos:start=""7:28"" pos:end=""7:36""><name pos:start=""7:28"" pos:end=""7:34"">GetInts</name><argument_list pos:start=""7:35"" pos:end=""7:36"">()</argument_list></call></expr></range></decl></init>)</control> <block pos:start=""7:39"" pos:end=""9:9"">{<block_content pos:start=""8:13"" pos:end=""8:23"">
            <expr_stmt pos:start=""8:13"" pos:end=""8:23""><expr pos:start=""8:13"" pos:end=""8:22""><call pos:start=""8:13"" pos:end=""8:22""><name pos:start=""8:13"" pos:end=""8:17"">print</name><argument_list pos:start=""8:18"" pos:end=""8:22"">(<argument pos:start=""8:19"" pos:end=""8:21""><expr pos:start=""8:19"" pos:end=""8:21""><name pos:start=""8:19"" pos:end=""8:21"">num</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
        </block_content>}</block></foreach>
    </block_content>}</block></function>
}</block></class>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "B.cs");
            var globalScope = codeParser.ParseFileUnit(unit);

            var loop = globalScope.GetDescendants<ForeachStatement>().First();
            var varDecl = loop.Condition.GetDescendantsAndSelf<VariableDeclaration>().FirstOrDefault();
            Assert.IsNotNull(varDecl);

            Assert.AreSame(BuiltInTypeFactory.GetBuiltIn(new TypeUse() {Name = "int"}), varDecl.ResolveType().FirstOrDefault());
        }

        [Test]
        public void TestFieldCreation() {
            //// A.cs
            //class A {
            //    public int Foo;
            //}
            /*string xml = @"<class>class <name>A</name> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>Foo</name></decl>;</decl_stmt>
}</block></class>";*/

            string xml = @"<class>class <name>A</name> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>Foo</name></decl>;</decl_stmt>
}</block></class>";

            var xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");
            var globalScope = codeParser.ParseFileUnit(xmlElement);

            Assert.AreEqual(1, globalScope.ChildStatements.Count());
            var typeA = globalScope.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(typeA);
            Assert.AreEqual(1, typeA.ChildStatements.Count());
            var foo = typeA.ChildStatements.First().Content as VariableDeclaration;
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.Name);
            Assert.AreEqual("int", foo.VariableType.Name);
            Assert.AreEqual(AccessModifier.Public, foo.Accessibility);
        }

        [Test]
        public void TestFindParentType() {
            // namespace A { class B : C { } }
            /*string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <super>: <name>C</name></super> <block>{<private type=""default""> </private>}</block> <decl/></class>}</block></namespace>";*/

            string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <super_list>: <super><name>C</name></super></super_list> <block>{ }</block></class> }</block></namespace>";

            // namespace A { class C { } }
            /*string cXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>C</name> <block>{<private type=""default""> </private>}</block> <decl/></class>}</block></namespace>";*/

            string cXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>C</name> <block>{ }</block></class> }</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(cXml, "D.cs");

            var bScope = codeParser.ParseFileUnit(bUnit);
            var cScope = codeParser.ParseFileUnit(cUnit);

            var globalScope = bScope.Merge(cScope);

            var typeB = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "B");
            Assert.IsNotNull(typeB);
            var typeC = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "C");
            Assert.IsNotNull(typeC);

            Assert.AreEqual(1, typeB.ParentTypeNames.Count);
            Assert.AreSame(typeC, typeB.ParentTypeNames[0].ResolveType().FirstOrDefault());
        }

        [Test]
        public void TestFindQualifiedParentType() {
            // namespace A { class B : C.D { } }
            /*string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <super>: <name><name>C</name><operator>.</operator><name>D</name></name></super> <block>{<private type=""default""> </private>}</block> <decl/></class>}</block></namespace>";*/

            string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <super_list>: <super><name><name>C</name><operator>.</operator><name>D</name></name></super></super_list> <block>{ }</block></class> }</block></namespace>";

            // namespace C { class D { } }
            /*string dXml = @"<namespace>namespace <name>C</name> <block>{ <class>class <name>D</name> <block>{<private type=""default""> </private>}</block> <decl/></class>}</block></namespace>";*/

            string dXml = @"<namespace>namespace <name>C</name> <block>{ <class>class <name>D</name> <block>{ }</block></class> }</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var dUnit = fileSetup.GetFileUnitForXmlSnippet(dXml, "D.cs");

            var bScope = codeParser.ParseFileUnit(bUnit);
            var dScope = codeParser.ParseFileUnit(dUnit);

            var globalScope = bScope.Merge(dScope);

            var typeB = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "B");
            Assert.IsNotNull(typeB);
            var typeD = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "D");
            Assert.IsNotNull(typeD);

            Assert.AreEqual(1, typeB.ParentTypeNames.Count);
            Assert.AreSame(typeD, typeB.ParentTypeNames[0].ResolveType().FirstOrDefault());
        }

        [Test]
        public void TestGenericType() {
            //public class B<T> { }
            /*var xml = @"<class><specifier>public</specifier> class <name><name>B</name><argument_list>&lt;<argument><name>T</name></argument>&gt;</argument_list></name> <block>{ }</block></class>";*/

            var xml = @"<class><specifier>public</specifier> class <name><name>B</name><argument_list type=""generic"">&lt;<argument><name>T</name></argument>&gt;</argument_list></name> <block>{ }</block></class>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "B.cs");
            var scope = codeParser.ParseFileUnit(unit);

            var typeB = scope.GetDescendants<TypeDefinition>().FirstOrDefault();
            Assert.IsNotNull(typeB);
            Assert.AreEqual("B", typeB.Name);
        }

        [Test]
        public void TestGenericVariableDeclaration() {
            //Dictionary<string,int> map;
            /*string xml = @"<decl_stmt><decl><type><name><name>Dictionary</name><argument_list>&lt;<argument><name>string</name></argument>,<argument><name>int</name></argument>&gt;</argument_list></name></type> <name>map</name></decl>;</decl_stmt>";*/

            string xml = @"<decl_stmt><decl><type><name><name>Dictionary</name><argument_list type=""generic"">&lt;<argument><name>string</name></argument>, <argument><name>int</name></argument>&gt;</argument_list></name></type> <name>map</name></decl>;</decl_stmt>";

            var testUnit = fileSetup.GetFileUnitForXmlSnippet(xml, "test.cs");

            var testScope = codeParser.ParseFileUnit(testUnit);

            var testDeclaration = testScope.ChildStatements.First().Content as VariableDeclaration;
            Assert.IsNotNull(testDeclaration, "could not find the test declaration");
            Assert.AreEqual("map", testDeclaration.Name);
            Assert.AreEqual("Dictionary", testDeclaration.VariableType.Name);
            Assert.That(testDeclaration.VariableType.IsGeneric);
            Assert.AreEqual(2, testDeclaration.VariableType.TypeParameters.Count);
            Assert.AreEqual("string", testDeclaration.VariableType.TypeParameters.First().Name);
            Assert.AreEqual("int", testDeclaration.VariableType.TypeParameters.Last().Name);
        }

        [Test]
        public void TestGenericVariableDeclarationWithPrefix() {
            //System.Collection.Dictionary<string,int> map;
            /*string xml = @"<decl_stmt><decl><type><name><name>System</name><operator>.</operator><name>Collection</name><operator>.</operator><name><name>Dictionary</name><argument_list>&lt;<argument><name>string</name></argument>,<argument><name>int</name></argument>&gt;</argument_list></name></name></type> <name>map</name></decl>;</decl_stmt>";*/

            string xml = @"<decl_stmt><decl><type><name><name>System</name><operator>.</operator><name>Collection</name><operator>.</operator><name><name>Dictionary</name><argument_list type=""generic"">&lt;<argument><name>string</name></argument>, <argument><name>int</name></argument>&gt;</argument_list></name></name></type> <name>map</name></decl>;</decl_stmt>";

            var testUnit = fileSetup.GetFileUnitForXmlSnippet(xml, "test.cs");

            var testScope = codeParser.ParseFileUnit(testUnit);

            var testDeclaration = testScope.ChildStatements.First().Content as VariableDeclaration;
            Assert.IsNotNull(testDeclaration, "could not find the test declaration");
            Assert.AreEqual("map", testDeclaration.Name);
            Assert.AreEqual("Dictionary", testDeclaration.VariableType.Name);
            var prefixNames = testDeclaration.VariableType.Prefix.Names.ToList();
            Assert.AreEqual(2, prefixNames.Count);
            Assert.AreEqual("System", prefixNames[0].Name);
            Assert.AreEqual("Collection", prefixNames[1].Name);
            Assert.That(testDeclaration.VariableType.IsGeneric);
            Assert.AreEqual(2, testDeclaration.VariableType.TypeParameters.Count);
            Assert.AreEqual("string", testDeclaration.VariableType.TypeParameters.First().Name);
            Assert.AreEqual("int", testDeclaration.VariableType.TypeParameters.Last().Name);
        }

        [Test]
        public void TestGetAccessModifierForMethod_InternalProtected() {
            //namespace Example {
            //    public class Foo {
            //        internal protected bool Bar() { return true; }
            //    }
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><specifier>internal</specifier> <specifier>protected</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> }</block></function>
    }</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><specifier>internal</specifier> <specifier>protected</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");
            
            var globalScope = codeParser.ParseFileUnit(unit);
            var method = globalScope.GetDescendants<MethodDefinition>().First();

            Assert.AreEqual(AccessModifier.ProtectedInternal, method.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForMethod_None() {
            //namespace Example {
            //    public class Foo {
            //        bool Bar() { return true; }
            //    }
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> }</block></function>
    }</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var method = globalScope.GetDescendants<MethodDefinition>().First();

            Assert.AreEqual(AccessModifier.None, method.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForMethod_Normal() {
            //namespace Example {
            //    public class Foo {
            //        public bool Bar() { return true; }
            //    }
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><specifier>public</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> }</block></function>
    }</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><specifier>public</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var method = globalScope.GetDescendants<MethodDefinition>().First();

            Assert.AreEqual(AccessModifier.Public, method.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForMethod_ProtectedInternal() {
            //namespace Example {
            //    public class Foo {
            //        protected internal bool Bar() { return true; }
            //    }
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><specifier>protected</specifier> <specifier>internal</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> }</block></function>
    }</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><specifier>protected</specifier> <specifier>internal</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var method = globalScope.GetDescendants<MethodDefinition>().First();

            Assert.AreEqual(AccessModifier.ProtectedInternal, method.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForMethod_ProtectedInternalStatic() {
            //namespace Example {
            //    public class Foo {
            //        protected static internal bool Bar() { return true; }
            //    }
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><specifier>protected</specifier> <specifier>static</specifier> <specifier>internal</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> }</block></function>
    }</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{
        <function><type><specifier>protected</specifier> <specifier>static</specifier> <specifier>internal</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var method = globalScope.GetDescendants<MethodDefinition>().First();

            Assert.AreEqual(AccessModifier.ProtectedInternal, method.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForType_InternalProtected() {
            //namespace Example {
            //    internal protected class Foo {}
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>internal</specifier> <specifier>protected</specifier> class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>internal</specifier> <specifier>protected</specifier> class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var type = globalScope.GetDescendants<TypeDefinition>().First();

            Assert.AreEqual(AccessModifier.ProtectedInternal, type.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForType_None() {
            //namespace Example {
            //    class Foo {}
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class>class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class>class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var type = globalScope.GetDescendants<TypeDefinition>().First();

            Assert.AreEqual(AccessModifier.None, type.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForType_Normal() {
            //namespace Example {
            //    public class Foo {}
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>public</specifier> class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var type = globalScope.GetDescendants<TypeDefinition>().First();

            Assert.AreEqual(AccessModifier.Public, type.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForType_ProtectedInternal() {
            //namespace Example {
            //    protected internal class Foo {}
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>protected</specifier> <specifier>internal</specifier> class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>protected</specifier> <specifier>internal</specifier> class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var type = globalScope.GetDescendants<TypeDefinition>().First();

            Assert.AreEqual(AccessModifier.ProtectedInternal, type.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForType_ProtectedInternalStatic() {
            //namespace Example {
            //    protected static internal class Foo {}
            //}
            /*string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>protected</specifier> <specifier>static</specifier> <specifier>internal</specifier> class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";*/

            string xml = @"<namespace>namespace <name>Example</name> <block>{
    <class><specifier>protected</specifier> <specifier>static</specifier> <specifier>internal</specifier> class <name>Foo</name> <block>{}</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs");

            var globalScope = codeParser.ParseFileUnit(unit);
            var type = globalScope.GetDescendants<TypeDefinition>().First();

            Assert.AreEqual(AccessModifier.ProtectedInternal, type.Accessibility);
        }

        [Test]
        public void TestMethodCallWithBaseKeyword() {
            // B.cs namespace A { class B { public virtual void Foo() { } } }
            /*string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <block>{ <function><type><specifier>public</specifier> <specifier>virtual</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ }</block></function> }</block></class> }</block></namespace>";*/
            string bXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <block>{ <function><type><specifier>public</specifier> <specifier>virtual</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function> }</block></class> }</block></namespace>";
            
            // C.cs namespace A { class C : B { public override void Foo() { base.Foo(); } } }
            /*string cXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>C</name> <super>: <name>B</name></super> <block>{ <function><type><specifier>public</specifier> <specifier>override</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ <expr_stmt><expr><call><name><name>base</name><operator>.</operator><name>Foo</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> }</block></function> }</block></class> }</block></namespace>";*/
            string cXml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>C</name> <super_list>: <super><name>B</name></super></super_list> <block>{ <function><type><specifier>public</specifier> <specifier>override</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> <expr_stmt><expr><call><name><name>base</name><operator>.</operator><name>Foo</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function> }</block></class> }</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(cXml, "C.cs");

            var bScope = codeParser.ParseFileUnit(bUnit);
            var cScope = codeParser.ParseFileUnit(cUnit);
            var globalScope = bScope.Merge(cScope);

            var fooMethods = globalScope.GetDescendants<MethodDefinition>().ToList();

            var bDotFoo = fooMethods.FirstOrDefault(m => m.GetAncestors<TypeDefinition>().FirstOrDefault().Name == "B");
            Assert.IsNotNull(bDotFoo);
            var cDotFoo = fooMethods.FirstOrDefault(m => m.GetAncestors<TypeDefinition>().FirstOrDefault().Name == "C");
            Assert.IsNotNull(cDotFoo);

            Assert.AreEqual(1, cDotFoo.ChildStatements.Count);
            var methodCall = cDotFoo.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(methodCall);
            Assert.AreSame(bDotFoo, methodCall.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestMethodDefinitionWithReturnType() {
            //int Foo() { }
            /*string xml = @"<function><type><name>int</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ }</block></function>";*/

            string xml = @"<function><type><name>int</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function>";

            var testUnit = fileSetup.GetFileUnitForXmlSnippet(xml, "test.cpp");

            var testScope = codeParser.ParseFileUnit(testUnit);

            var method = testScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            Assert.IsNotNull(method, "could not find the test method");

            Assert.AreEqual("int", method.ReturnType.Name);
        }

        [Test]
        public void TestMethodDefinitionWithReturnTypeAndWithSpecifier() {
            //static int Foo() { }
            /*string xml = @"<function><type><specifier>static</specifier> <name>int</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ }</block></function>";*/

            string xml = @"<function><type><specifier>static</specifier> <name>int</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function>";

            var testUnit = fileSetup.GetFileUnitForXmlSnippet(xml, "test.cpp");

            var testScope = codeParser.ParseFileUnit(testUnit);

            var method = testScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            Assert.IsNotNull(method, "could not find the test method");

            Assert.AreEqual("int", method.ReturnType.Name);
        }

        [Test]
        public void TestMethodDefinitionWithVoidReturn() {
            //void Foo() { }
            /*string xml = @"<function><type><name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ }</block></function>";*/

            string xml = @"<function><type><name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function>";

            var testUnit = fileSetup.GetFileUnitForXmlSnippet(xml, "test.cpp");

            var testScope = codeParser.ParseFileUnit(testUnit);

            var method = testScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            Assert.IsNotNull(method, "could not find the test method");

            Assert.IsNull(method.ReturnType, "return type should be null");
        }


        [Test]
        public void TestProperty() {
            // namespace A { class B { int Foo { get; set; } } }
            /*string xml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <block>{ <decl_stmt><decl><type><name>int</name></type> <name>Foo</name> <block>{ <function_decl><name>get</name>;</function_decl> <function_decl><name>set</name>;</function_decl> }</block></decl></decl_stmt> }</block></class> }</block></namespace>";*/

            string xml = @"<namespace>namespace <name>A</name> <block>{ <class>class <name>B</name> <block>{ <property><type><name>int</name></type> <name>Foo</name> <block>{ <function_decl><name>get</name>;</function_decl> <function_decl><name>set</name>;</function_decl> }</block></property> }</block></class> }</block></namespace>";

            var testUnit = fileSetup.GetFileUnitForXmlSnippet(xml, "B.cs");
            var testScope = codeParser.ParseFileUnit(testUnit);

            var classB = testScope.GetDescendants<TypeDefinition>().FirstOrDefault();

            Assert.IsNotNull(classB);
            Assert.AreEqual(1, classB.ChildStatements.Count());

            var fooProperty = classB.ChildStatements.First() as PropertyDefinition;
            Assert.IsNotNull(fooProperty);
            Assert.AreEqual("Foo", fooProperty.Name);
            Assert.AreEqual("int", fooProperty.ReturnType.Name);
            Assert.AreEqual(AccessModifier.None, fooProperty.Accessibility);
            Assert.IsNotNull(fooProperty.Getter);
            Assert.IsNotNull(fooProperty.Setter);
        }

        [Test]
        public void TestPropertyAsCallingObject() {
            // B.cs
            //namespace A {
            //  class B {
            //    C Foo { get; set; }
            //  }
            //}
            /*string bXml = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name pos:line=""1"" pos:column=""11"">A</name> <block pos:line=""1"" pos:column=""13"">{
  <class pos:line=""2"" pos:column=""3"">class <name pos:line=""2"" pos:column=""9"">B</name> <block pos:line=""2"" pos:column=""11"">{
    <decl_stmt><decl><type><name pos:line=""3"" pos:column=""5"">C</name></type> <name pos:line=""3"" pos:column=""7"">Foo</name> <block pos:line=""3"" pos:column=""11"">{ <function_decl><name pos:line=""3"" pos:column=""13"">get</name>;</function_decl> <function_decl><name pos:line=""3"" pos:column=""18"">set</name>;</function_decl> }</block></decl></decl_stmt>
  }</block></class>
}</block></namespace>";*/

            string bXml = @"<namespace pos:start=""1:1"" pos:end=""6:1"">namespace <name pos:start=""1:11"" pos:end=""1:11"">A</name> <block pos:start=""2:1"" pos:end=""6:1"">{
  <class pos:start=""3:3"" pos:end=""5:3"">class <name pos:start=""3:9"" pos:end=""3:9"">B</name> <block pos:start=""3:11"" pos:end=""5:3"">{
    <property pos:start=""4:5"" pos:end=""4:23""><type pos:start=""4:5"" pos:end=""4:5""><name pos:start=""4:5"" pos:end=""4:5"">C</name></type> <name pos:start=""4:7"" pos:end=""4:9"">Foo</name> <block pos:start=""4:11"" pos:end=""4:23"">{ <function_decl pos:start=""4:13"" pos:end=""4:16""><name pos:start=""4:13"" pos:end=""4:15"">get</name>;</function_decl> <function_decl pos:start=""4:18"" pos:end=""4:21""><name pos:start=""4:18"" pos:end=""4:20"">set</name>;</function_decl> }</block></property>
  }</block></class>
}</block></namespace>";

            // C.cs
            //namespace A {
            //	class C {
            //		static void main() {
            //			B b = new B();
            //			b.Foo.Bar();
            //		}
            //		void Bar() { }
            //	}
            //}
            /*string cXml = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name pos:line=""1"" pos:column=""11"">A</name> <block pos:line=""1"" pos:column=""13"">{
    <class pos:line=""2"" pos:column=""5"">class <name pos:line=""2"" pos:column=""11"">C</name> <block pos:line=""2"" pos:column=""13"">{
        <function><type><specifier pos:line=""3"" pos:column=""9"">static</specifier> <name pos:line=""3"" pos:column=""16"">void</name></type> <name pos:line=""3"" pos:column=""21"">main</name><parameter_list pos:line=""3"" pos:column=""25"">()</parameter_list> <block pos:line=""3"" pos:column=""28"">{
            <decl_stmt><decl><type><name pos:line=""4"" pos:column=""13"">B</name></type> <name pos:line=""4"" pos:column=""15"">b</name> <init pos:line=""4"" pos:column=""17"">= <expr><operator pos:line=""4"" pos:column=""19"">new</operator> <call><name pos:line=""4"" pos:column=""23"">B</name><argument_list pos:line=""4"" pos:column=""24"">()</argument_list></call></expr></init></decl>;</decl_stmt>
            <expr_stmt><expr><call><name><name pos:line=""5"" pos:column=""13"">b</name><operator pos:line=""5"" pos:column=""14"">.</operator><name pos:line=""5"" pos:column=""15"">Foo</name><operator pos:line=""5"" pos:column=""18"">.</operator><name pos:line=""5"" pos:column=""19"">Bar</name></name><argument_list pos:line=""5"" pos:column=""22"">()</argument_list></call></expr>;</expr_stmt>
        }</block></function>
        <function><type><name pos:line=""7"" pos:column=""9"">void</name></type> <name pos:line=""7"" pos:column=""14"">Bar</name><parameter_list pos:line=""7"" pos:column=""17"">()</parameter_list> <block pos:line=""7"" pos:column=""20"">{ }</block></function>
    }</block></class>
}</block></namespace>";*/

            string cXml = @"<namespace pos:start=""1:1"" pos:end=""10:1"">namespace <name pos:start=""1:11"" pos:end=""1:11"">A</name> <block pos:start=""2:1"" pos:end=""10:1"">{
	<class pos:start=""3:9"" pos:end=""9:9"">class <name pos:start=""3:15"" pos:end=""3:15"">C</name> <block pos:start=""3:17"" pos:end=""9:9"">{
		<function pos:start=""4:17"" pos:end=""7:17""><type pos:start=""4:17"" pos:end=""4:27""><specifier pos:start=""4:17"" pos:end=""4:22"">static</specifier> <name pos:start=""4:24"" pos:end=""4:27"">void</name></type> <name pos:start=""4:29"" pos:end=""4:32"">main</name><parameter_list pos:start=""4:33"" pos:end=""4:34"">()</parameter_list> <block pos:start=""4:36"" pos:end=""7:17"">{<block_content pos:start=""5:25"" pos:end=""6:36"">
			<decl_stmt pos:start=""5:25"" pos:end=""5:38""><decl pos:start=""5:25"" pos:end=""5:37""><type pos:start=""5:25"" pos:end=""5:25""><name pos:start=""5:25"" pos:end=""5:25"">B</name></type> <name pos:start=""5:27"" pos:end=""5:27"">b</name> <init pos:start=""5:29"" pos:end=""5:37"">= <expr pos:start=""5:31"" pos:end=""5:37""><operator pos:start=""5:31"" pos:end=""5:33"">new</operator> <call pos:start=""5:35"" pos:end=""5:37""><name pos:start=""5:35"" pos:end=""5:35"">B</name><argument_list pos:start=""5:36"" pos:end=""5:37"">()</argument_list></call></expr></init></decl>;</decl_stmt>
			<expr_stmt pos:start=""6:25"" pos:end=""6:36""><expr pos:start=""6:25"" pos:end=""6:35""><call pos:start=""6:25"" pos:end=""6:35""><name pos:start=""6:25"" pos:end=""6:33""><name pos:start=""6:25"" pos:end=""6:25"">b</name><operator pos:start=""6:26"" pos:end=""6:26"">.</operator><name pos:start=""6:27"" pos:end=""6:29"">Foo</name><operator pos:start=""6:30"" pos:end=""6:30"">.</operator><name pos:start=""6:31"" pos:end=""6:33"">Bar</name></name><argument_list pos:start=""6:34"" pos:end=""6:35"">()</argument_list></call></expr>;</expr_stmt>
		</block_content>}</block></function>
		<function pos:start=""8:17"" pos:end=""8:30""><type pos:start=""8:17"" pos:end=""8:20""><name pos:start=""8:17"" pos:end=""8:20"">void</name></type> <name pos:start=""8:22"" pos:end=""8:24"">Bar</name><parameter_list pos:start=""8:25"" pos:end=""8:26"">()</parameter_list> <block pos:start=""8:28"" pos:end=""8:30"">{<block_content> </block_content>}</block></function>
	}</block></class>
}</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(cXml, "C.cs");
            var bScope = codeParser.ParseFileUnit(bUnit);
            var cScope = codeParser.ParseFileUnit(cUnit);

            var globalScope = bScope.Merge(cScope);

            var classB = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "B");
            Assert.IsNotNull(classB);
            var classC = globalScope.GetDescendants<TypeDefinition>().FirstOrDefault(t => t.Name == "C");
            Assert.IsNotNull(classC);

            var mainMethod = classC.GetNamedChildren<MethodDefinition>("main").FirstOrDefault();
            var barMethod = classC.GetNamedChildren<MethodDefinition>("Bar").FirstOrDefault();
            Assert.IsNotNull(mainMethod);
            Assert.IsNotNull(barMethod);

            Assert.AreEqual(2, mainMethod.ChildStatements.Count);
            var callToBar = mainMethod.ChildStatements[1].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(callToBar);
            Assert.AreSame(barMethod, callToBar.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestStaticMethodCall() {
            //namespace A { public class B { public static void Bar() { } } }
            /*var bXml = @"<namespace>namespace <name>A</name> <block>{ <class><specifier>public</specifier> class <name>B</name> <block>{ <function><type><specifier>public</specifier> <specifier>static</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ }</block></function> }</block></class> }</block></namespace>";*/
            var bXml = @"<namespace>namespace <name>A</name> <block>{ <class><specifier>public</specifier> class <name>B</name> <block>{ <function><type><specifier>public</specifier> <specifier>static</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function> }</block></class> }</block></namespace>";
            
            //namespace A { public class C { public void Foo() { B.Bar(); } } }
            /*var cXml = @"<namespace>namespace <name>A</name> <block>{ <class><specifier>public</specifier> class <name>C</name> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ <expr_stmt><expr><call><name><name>B</name><operator>.</operator><name>Bar</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> }</block></function> }</block></class> }</block></namespace>";*/
            var cXml = @"<namespace>namespace <name>A</name> <block>{ <class><specifier>public</specifier> class <name>C</name> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> <expr_stmt><expr><call><name><name>B</name><operator>.</operator><name>Bar</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function> }</block></class> }</block></namespace>";

            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(cXml, "C.cs");

            var bScope = codeParser.ParseFileUnit(bUnit);
            var cScope = codeParser.ParseFileUnit(cUnit);

            var globalScope = bScope.Merge(cScope);

            var fooMethod = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            var barMethod = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Bar");
            Assert.IsNotNull(fooMethod);
            Assert.IsNotNull(barMethod);

            Assert.AreEqual(1, fooMethod.ChildStatements.Count);
            var callToBar = fooMethod.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(callToBar);
            Assert.AreSame(barMethod, callToBar.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestStaticMethodCallInDifferentNamespace() {
            //namespace A { public class B { public static void Bar() { } } }
            /*var bXml = @"<namespace>namespace <name>A</name> <block>{ <class><specifier>public</specifier> class <name>B</name> <block>{ <function><type><specifier>public</specifier> <specifier>static</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ }</block></function> }</block></class> }</block></namespace>";*/
            var bXml = @"<namespace>namespace <name>A</name> <block>{ <class><specifier>public</specifier> class <name>B</name> <block>{ <function><type><specifier>public</specifier> <specifier>static</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function> }</block></class> }</block></namespace>";
            
            //namespace C { public class D { public void Foo() { A.B.Bar(); } } }
            /*var dXml = @"<namespace>namespace <name>C</name> <block>{ <class><specifier>public</specifier> class <name>D</name> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ <expr_stmt><expr><call><name><name>A</name><operator>.</operator><name>B</name><operator>.</operator><name>Bar</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> }</block></function> }</block></class> }</block></namespace>";*/
            var dXml = @"<namespace>namespace <name>C</name> <block>{ <class><specifier>public</specifier> class <name>D</name> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> <expr_stmt><expr><call><name><name>A</name><operator>.</operator><name>B</name><operator>.</operator><name>Bar</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function> }</block></class> }</block></namespace>";


            var bUnit = fileSetup.GetFileUnitForXmlSnippet(bXml, "B.cs");
            var dUnit = fileSetup.GetFileUnitForXmlSnippet(dXml, "C.cs");

            var bScope = codeParser.ParseFileUnit(bUnit);
            var dScope = codeParser.ParseFileUnit(dUnit);

            var globalScope = bScope.Merge(dScope);

            var fooMethod = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            var barMethod = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Bar");
            Assert.IsNotNull(fooMethod);
            Assert.IsNotNull(barMethod);

            Assert.AreEqual(1, fooMethod.ChildStatements.Count);
            var callToBar = fooMethod.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(callToBar);
            Assert.AreSame(barMethod, callToBar.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestVariablesWithSpecifiers() {
            //static int A;
            //public const int B;
            //public static readonly Foo C;
            //volatile  int D;
            /*string testXml = @"<decl_stmt><decl><type><specifier>static</specifier> <name>int</name></type> <name>A</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>public</specifier> <specifier>const</specifier> <name>int</name></type> <name>B</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>public</specifier> <specifier>static</specifier> <specifier>readonly</specifier> <name>Foo</name></type> <name>C</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>volatile</specifier>  <name>int</name></type> <name>D</name></decl>;</decl_stmt>";*/

            string testXml = @"<decl_stmt><decl><type><specifier>static</specifier> <name>int</name></type> <name>A</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>public</specifier> <specifier>const</specifier> <name>int</name></type> <name>B</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>public</specifier> <specifier>static</specifier> <specifier>readonly</specifier> <name>Foo</name></type> <name>C</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>volatile</specifier>  <name>int</name></type> <name>D</name></decl>;</decl_stmt>";

            var testUnit = fileSetup.GetFileUnitForXmlSnippet(testXml, "test.cs");

            var globalScope = codeParser.ParseFileUnit(testUnit);
            Assert.AreEqual(4, globalScope.ChildStatements.Count);

            var declA = globalScope.ChildStatements[0].Content as VariableDeclaration;
            Assert.IsNotNull(declA);
            Assert.AreEqual("A", declA.Name);
            Assert.AreEqual("int", declA.VariableType.Name);
            Assert.AreEqual(AccessModifier.None, declA.Accessibility);

            var declB = globalScope.ChildStatements[1].Content as VariableDeclaration;
            Assert.IsNotNull(declB);
            Assert.AreEqual("B", declB.Name);
            Assert.AreEqual("int", declB.VariableType.Name);
            Assert.AreEqual(AccessModifier.Public, declB.Accessibility);

            var declC = globalScope.ChildStatements[2].Content as VariableDeclaration;
            Assert.IsNotNull(declC);
            Assert.AreEqual("C", declC.Name);
            Assert.AreEqual("Foo", declC.VariableType.Name);
            Assert.AreEqual(AccessModifier.Public, declC.Accessibility);

            var declD = globalScope.ChildStatements[3].Content as VariableDeclaration;
            Assert.IsNotNull(declD);
            Assert.AreEqual("D", declD.Name);
            Assert.AreEqual("int", declD.VariableType.Name);
            Assert.AreEqual(AccessModifier.None, declD.Accessibility);
        }

        [Test]
        public void TestStaticInstanceVariable() {
            //namespace A {
            //	class B {
            //		public static B Instance { get; set; }
            //		public void Bar() { }
            //	}
            //	
            //	class C { public void Foo() { B.Instance.Bar(); } }
            //}
            /*var xml = @"<namespace>namespace <name>A</name> <block>{
	<class>class <name>B</name> <block>{
		<decl_stmt><decl><type><specifier>public</specifier> <specifier>static</specifier> <name>B</name></type> <name>Instance</name> <block>{ <function_decl><name>get</name>;</function_decl> <function_decl><name>set</name>;</function_decl> }</block></decl></decl_stmt>
		<function><type><specifier>public</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ }</block></function>
	}</block></class>
	
	<class>class <name>C</name> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ <expr_stmt><expr><call><name><name>B</name><operator>.</operator><name>Instance</name><operator>.</operator><name>Bar</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> }</block></function> }</block></class>
}</block></namespace>";*/

            var xml = @"<namespace>namespace <name>A</name> <block>{
	<class>class <name>B</name> <block>{
		<property><type><specifier>public</specifier> <specifier>static</specifier> <name>B</name></type> <name>Instance</name> <block>{ <function_decl><name>get</name>;</function_decl> <function_decl><name>set</name>;</function_decl> }</block></property>
		<function><type><specifier>public</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function>
	}</block></class>
	
	<class>class <name>C</name> <block>{ <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> <expr_stmt><expr><call><name><name>B</name><operator>.</operator><name>Instance</name><operator>.</operator><name>Bar</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function> }</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(unit);

            var methodBar = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Bar");
            Assert.IsNotNull(methodBar);
            var methodFoo = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            Assert.IsNotNull(methodFoo);

            Assert.AreEqual(1, methodFoo.ChildStatements.Count);
            var callToBar = methodFoo.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(callToBar);
            Assert.AreSame(methodBar, callToBar.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestStaticInstanceVariableInDifferentNamespace() {
            //namespace A {
            //	class B {
            //		public static B Instance { get; set; }
            //		public void Bar() { }
            //	}
            //}
            /*var aXml = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name pos:line=""1"" pos:column=""11"">A</name> <block pos:line=""1"" pos:column=""13"">{
	<class pos:line=""2"" pos:column=""9"">class <name pos:line=""2"" pos:column=""15"">B</name> <block pos:line=""2"" pos:column=""17"">{
		<decl_stmt><decl><type><specifier pos:line=""3"" pos:column=""17"">public</specifier> <specifier pos:line=""3"" pos:column=""24"">static</specifier> <name pos:line=""3"" pos:column=""31"">B</name></type> <name pos:line=""3"" pos:column=""33"">Instance</name> <block pos:line=""3"" pos:column=""42"">{ <function_decl><name pos:line=""3"" pos:column=""44"">get</name>;</function_decl> <function_decl><name pos:line=""3"" pos:column=""49"">set</name>;</function_decl> }</block></decl></decl_stmt>
		<function><type><specifier pos:line=""4"" pos:column=""17"">public</specifier> <name pos:line=""4"" pos:column=""24"">void</name></type> <name pos:line=""4"" pos:column=""29"">Bar</name><parameter_list pos:line=""4"" pos:column=""32"">()</parameter_list> <block pos:line=""4"" pos:column=""35"">{ }</block></function>
	}</block></class>
}</block></namespace>";*/

            var aXml = @"<namespace pos:start=""1:1"" pos:end=""7:1"">namespace <name pos:start=""1:11"" pos:end=""1:11"">A</name> <block pos:start=""2:1"" pos:end=""7:1"">{
	<class pos:start=""3:9"" pos:end=""6:9"">class <name pos:start=""3:15"" pos:end=""3:15"">B</name> <block pos:start=""3:17"" pos:end=""6:9"">{
		<property pos:start=""4:17"" pos:end=""4:54""><type pos:start=""4:17"" pos:end=""4:31""><specifier pos:start=""4:17"" pos:end=""4:22"">public</specifier> <specifier pos:start=""4:24"" pos:end=""4:29"">static</specifier> <name pos:start=""4:31"" pos:end=""4:31"">B</name></type> <name pos:start=""4:33"" pos:end=""4:40"">Instance</name> <block pos:start=""4:42"" pos:end=""4:54"">{ <function_decl pos:start=""4:44"" pos:end=""4:47""><name pos:start=""4:44"" pos:end=""4:46"">get</name>;</function_decl> <function_decl pos:start=""4:49"" pos:end=""4:52""><name pos:start=""4:49"" pos:end=""4:51"">set</name>;</function_decl> }</block></property>
		<function pos:start=""5:17"" pos:end=""5:37""><type pos:start=""5:17"" pos:end=""5:27""><specifier pos:start=""5:17"" pos:end=""5:22"">public</specifier> <name pos:start=""5:24"" pos:end=""5:27"">void</name></type> <name pos:start=""5:29"" pos:end=""5:31"">Bar</name><parameter_list pos:start=""5:32"" pos:end=""5:33"">()</parameter_list> <block pos:start=""5:35"" pos:end=""5:37"">{<block_content> </block_content>}</block></function>
	}</block></class>
}</block></namespace>";

            //using A;
            //
            //namespace C {
            //	class D {
            //		public void Foo() { B.Instance.Bar(); }
            //	}
            //}
            /*var cXml = @"<using pos:line=""1"" pos:column=""1"">using <name pos:line=""1"" pos:column=""7"">A</name>;</using>

<namespace pos:line=""3"" pos:column=""1"">namespace <name pos:line=""3"" pos:column=""11"">C</name> <block pos:line=""3"" pos:column=""13"">{
    <class pos:line=""4"" pos:column=""5"">class <name pos:line=""4"" pos:column=""11"">D</name> <block pos:line=""4"" pos:column=""13"">{
        <function><type><specifier pos:line=""5"" pos:column=""9"">public</specifier> <name pos:line=""5"" pos:column=""16"">void</name></type> <name pos:line=""5"" pos:column=""21"">Foo</name><parameter_list pos:line=""5"" pos:column=""24"">()</parameter_list> <block pos:line=""5"" pos:column=""27"">{ <expr_stmt><expr><call><name><name pos:line=""5"" pos:column=""29"">B</name><operator pos:line=""5"" pos:column=""30"">.</operator><name pos:line=""5"" pos:column=""31"">Instance</name><operator pos:line=""5"" pos:column=""39"">.</operator><name pos:line=""5"" pos:column=""40"">Bar</name></name><argument_list pos:line=""5"" pos:column=""43"">()</argument_list></call></expr>;</expr_stmt> }</block></function>
    }</block></class>
}</block></namespace>";*/

            var cXml = @"<using pos:start=""1:1"" pos:end=""1:8"">using <name pos:start=""1:7"" pos:end=""1:7"">A</name>;</using>

<namespace pos:start=""3:1"" pos:end=""8:1"">namespace <name pos:start=""3:11"" pos:end=""3:11"">C</name> <block pos:start=""4:1"" pos:end=""8:1"">{
	<class pos:start=""5:9"" pos:end=""7:9"">class <name pos:start=""5:15"" pos:end=""5:15"">D</name> <block pos:start=""5:17"" pos:end=""7:9"">{
		<function pos:start=""6:17"" pos:end=""6:55""><type pos:start=""6:17"" pos:end=""6:27""><specifier pos:start=""6:17"" pos:end=""6:22"">public</specifier> <name pos:start=""6:24"" pos:end=""6:27"">void</name></type> <name pos:start=""6:29"" pos:end=""6:31"">Foo</name><parameter_list pos:start=""6:32"" pos:end=""6:33"">()</parameter_list> <block pos:start=""6:35"" pos:end=""6:55"">{<block_content pos:start=""6:37"" pos:end=""6:53""> <expr_stmt pos:start=""6:37"" pos:end=""6:53""><expr pos:start=""6:37"" pos:end=""6:52""><call pos:start=""6:37"" pos:end=""6:52""><name pos:start=""6:37"" pos:end=""6:50""><name pos:start=""6:37"" pos:end=""6:37"">B</name><operator pos:start=""6:38"" pos:end=""6:38"">.</operator><name pos:start=""6:39"" pos:end=""6:46"">Instance</name><operator pos:start=""6:47"" pos:end=""6:47"">.</operator><name pos:start=""6:48"" pos:end=""6:50"">Bar</name></name><argument_list pos:start=""6:51"" pos:end=""6:52"">()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>
	}</block></class>
}</block></namespace>";

            var aUnit = fileSetup.GetFileUnitForXmlSnippet(aXml, "A.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(cXml, "C.cs");
            var aScope = codeParser.ParseFileUnit(aUnit);
            var cScope = codeParser.ParseFileUnit(cUnit);
            var globalScope = aScope.Merge(cScope);

            var methodBar = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Bar");
            Assert.IsNotNull(methodBar);
            var methodFoo = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            Assert.IsNotNull(methodFoo);

            Assert.AreEqual(1, methodFoo.ChildStatements.Count);
            var callToBar = methodFoo.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(callToBar);
            Assert.AreSame(methodBar, callToBar.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestCallAsCallingObject() {
            //namespace A {
            //	public class B {
            //		void main() {
            //			Foo().Bar();
            //		}
            //
            //		C Foo() { return new C(); }
            //	}
            //
            //	public class C {
            //		void Bar() { }
            //	}
            //}
            /*var xml = @"<namespace>namespace <name>A</name> <block>{
	<class><specifier>public</specifier> class <name>B</name> <block>{
		<function><type><name>void</name></type> <name>main</name><parameter_list>()</parameter_list> <block>{
			<expr_stmt><expr><call><name>Foo</name><argument_list>()</argument_list></call><operator>.</operator><call><name>Bar</name><argument_list>()</argument_list></call></expr>;</expr_stmt>
		}</block></function>

		<function><type><name>C</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><operator>new</operator> <call><name>C</name><argument_list>()</argument_list></call></expr>;</return> }</block></function>
	}</block></class>

	<class><specifier>public</specifier> class <name>C</name> <block>{
		<function><type><name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ }</block></function>
	}</block></class>
}</block></namespace>";*/

            var xml = @"<namespace pos:start=""1:1"" pos:end=""14:1"">namespace <name pos:start=""1:11"" pos:end=""1:11"">A</name>
<block pos:start=""2:1"" pos:end=""14:1"">{
	<class pos:start=""3:9"" pos:end=""9:9""><specifier pos:start=""3:9"" pos:end=""3:14"">public</specifier> class <name pos:start=""3:22"" pos:end=""3:22"">B</name> <block pos:start=""3:24"" pos:end=""9:9"">{
		<function pos:start=""4:17"" pos:end=""6:17""><type pos:start=""4:17"" pos:end=""4:20""><name pos:start=""4:17"" pos:end=""4:20"">void</name></type> <name pos:start=""4:22"" pos:end=""4:25"">main</name><parameter_list pos:start=""4:26"" pos:end=""4:27"">()</parameter_list> <block pos:start=""4:29"" pos:end=""6:17"">{<block_content pos:start=""5:25"" pos:end=""5:36"">
			<expr_stmt pos:start=""5:25"" pos:end=""5:36""><expr pos:start=""5:25"" pos:end=""5:35""><call pos:start=""5:25"" pos:end=""5:29""><name pos:start=""5:25"" pos:end=""5:27"">Foo</name><argument_list pos:start=""5:28"" pos:end=""5:29"">()</argument_list></call><operator pos:start=""5:30"" pos:end=""5:30"">.</operator><call pos:start=""5:31"" pos:end=""5:35""><name pos:start=""5:31"" pos:end=""5:33"">Bar</name><argument_list pos:start=""5:34"" pos:end=""5:35"">()</argument_list></call></expr>;</expr_stmt>
		</block_content>}</block></function>

		<function pos:start=""8:17"" pos:end=""8:43""><type pos:start=""8:17"" pos:end=""8:17""><name pos:start=""8:17"" pos:end=""8:17"">C</name></type> <name pos:start=""8:19"" pos:end=""8:21"">Foo</name><parameter_list pos:start=""8:22"" pos:end=""8:23"">()</parameter_list> <block pos:start=""8:25"" pos:end=""8:43"">{<block_content pos:start=""8:27"" pos:end=""8:41""> <return pos:start=""8:27"" pos:end=""8:41"">return <expr pos:start=""8:34"" pos:end=""8:40""><operator pos:start=""8:34"" pos:end=""8:36"">new</operator> <call pos:start=""8:38"" pos:end=""8:40""><name pos:start=""8:38"" pos:end=""8:38"">C</name><argument_list pos:start=""8:39"" pos:end=""8:40"">()</argument_list></call></expr>;</return> </block_content>}</block></function>
	}</block></class>

	<class pos:start=""11:9"" pos:end=""13:9""><specifier pos:start=""11:9"" pos:end=""11:14"">public</specifier> class <name pos:start=""11:22"" pos:end=""11:22"">C</name> <block pos:start=""11:24"" pos:end=""13:9"">{
		<function pos:start=""12:17"" pos:end=""12:30""><type pos:start=""12:17"" pos:end=""12:20""><name pos:start=""12:17"" pos:end=""12:20"">void</name></type> <name pos:start=""12:22"" pos:end=""12:24"">Bar</name><parameter_list pos:start=""12:25"" pos:end=""12:26"">()</parameter_list> <block pos:start=""12:28"" pos:end=""12:30"">{<block_content> </block_content>}</block></function>
	}</block></class>
}</block></namespace>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "B.cs");
            var globalScope = codeParser.ParseFileUnit(unit);

            var mainMethod = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "main");
            var fooMethod = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            var barMethod = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Bar");
            Assert.IsNotNull(mainMethod);
            Assert.IsNotNull(fooMethod);
            Assert.IsNotNull(barMethod);

            Assert.AreEqual(1, mainMethod.ChildStatements.Count);
            var callToFoo = mainMethod.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault(mc => mc.Name == "Foo");
            var callToBar = mainMethod.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault(mc => mc.Name == "Bar");
            Assert.IsNotNull(callToFoo);
            Assert.IsNotNull(callToBar);

            Assert.AreSame(fooMethod, callToFoo.FindMatches().FirstOrDefault());
            Assert.AreSame(barMethod, callToBar.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestResolveVariable_Field() {
            //class A {
            //  public int Foo;
            //  public A() {
            //    Foo = 42;
            //  }
            //}
            /*string xml = @"<class>class <name>A</name> <block>{
  <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>Foo</name></decl>;</decl_stmt>
  <constructor><specifier>public</specifier> <name>A</name><parameter_list>()</parameter_list> <block>{
    <expr_stmt><expr><name>Foo</name> <operator>=</operator> <literal type=""number"">42</literal></expr>;</expr_stmt>
  }</block></constructor>
}</block></class>";*/

            string xml = @"<class pos:start=""1:1"" pos:end=""7:1"">class <name pos:start=""1:7"" pos:end=""1:7"">A</name> <block pos:start=""2:1"" pos:end=""7:1"">{
  <decl_stmt pos:start=""3:3"" pos:end=""3:17""><decl pos:start=""3:3"" pos:end=""3:16""><type pos:start=""3:3"" pos:end=""3:12""><specifier pos:start=""3:3"" pos:end=""3:8"">public</specifier> <name pos:start=""3:10"" pos:end=""3:12"">int</name></type> <name pos:start=""3:14"" pos:end=""3:16"">Foo</name></decl>;</decl_stmt>
  <constructor pos:start=""4:3"" pos:end=""6:3""><specifier pos:start=""4:3"" pos:end=""4:8"">public</specifier> <name pos:start=""4:10"" pos:end=""4:10"">A</name><parameter_list pos:start=""4:11"" pos:end=""4:12"">()</parameter_list> <block pos:start=""4:14"" pos:end=""6:3"">{<block_content pos:start=""5:5"" pos:end=""5:13"">
    <expr_stmt pos:start=""5:5"" pos:end=""5:13""><expr pos:start=""5:5"" pos:end=""5:12""><name pos:start=""5:5"" pos:end=""5:7"">Foo</name> <operator pos:start=""5:9"" pos:end=""5:9"">=</operator> <literal type=""number"" pos:start=""5:11"" pos:end=""5:12"">42</literal></expr>;</expr_stmt>
  </block_content>}</block></constructor>
}</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            var fooDecl = globalScope.GetNamedChildren<TypeDefinition>("A").First().GetNamedChildren<VariableDeclaration>("Foo").First();
            var aConstructor = globalScope.GetDescendants<MethodDefinition>().First(m => m.Name == "A");
            Assert.AreEqual(1, aConstructor.ChildStatements.Count);
            var fooUse = aConstructor.ChildStatements[0].Content.GetDescendants<NameUse>().FirstOrDefault(n => n.Name == "Foo");
            Assert.IsNotNull(fooUse);
            Assert.AreSame(fooDecl, fooUse.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestResolveVariable_FieldInParent() {
            //class B {
            //  public int Foo;
            //}
            //class A : B {
            //  public A() {
            //    Foo = 42;
            //  }
            //}
            /*var xml = @"<class>class <name>B</name> <block>{
  <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>Foo</name></decl>;</decl_stmt>
}</block></class>
<class>class <name>A</name> <super>: <name>B</name></super> <block>{
  <constructor><specifier>public</specifier> <name>A</name><parameter_list>()</parameter_list> <block>{
    <expr_stmt><expr><name>Foo</name> <operator>=</operator> <literal type=""number"">42</literal></expr>;</expr_stmt>
  }</block></constructor>
}</block></class>";*/

            var xml = @"<class>class <name>B</name> <block>{
  <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>Foo</name></decl>;</decl_stmt>
}</block></class>
<class>class <name>A</name> <super_list>: <super><name>B</name></super></super_list> <block>{
  <constructor><specifier>public</specifier> <name>A</name><parameter_list>()</parameter_list> <block>{<block_content>
    <expr_stmt><expr><name>Foo</name> <operator>=</operator> <literal type=""number"">42</literal></expr>;</expr_stmt>
  </block_content>}</block></constructor>
}</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            var fooDecl = globalScope.GetNamedChildren<TypeDefinition>("B").First().GetNamedChildren<VariableDeclaration>("Foo").First();
            var aConstructor = globalScope.GetDescendants<MethodDefinition>().First(m => m.Name == "A");
            Assert.AreEqual(1, aConstructor.ChildStatements.Count);
            var fooUse = aConstructor.ChildStatements[0].Content.GetDescendants<NameUse>().FirstOrDefault(n => n.Name == "Foo");
            Assert.IsNotNull(fooUse);
            Assert.AreSame(fooDecl, fooUse.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestCallingVariableDeclaredInParentClass() {
            //class A { void Foo() { } }
            /*string a_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">A</name> <block pos:line=""1"" pos:column=""9"">{ <function><type><name pos:line=""1"" pos:column=""11"">void</name></type> <name pos:line=""1"" pos:column=""16"">Foo</name><parameter_list pos:line=""1"" pos:column=""19"">()</parameter_list> <block pos:line=""1"" pos:column=""22"">{ }</block></function> }</block></class>";*/
            string a_xml = @"<class pos:start=""1:1"" pos:end=""1:26"">class <name pos:start=""1:7"" pos:end=""1:7"">A</name> <block pos:start=""1:9"" pos:end=""1:26"">{ <function pos:start=""1:11"" pos:end=""1:24""><type pos:start=""1:11"" pos:end=""1:14""><name pos:start=""1:11"" pos:end=""1:14"">void</name></type> <name pos:start=""1:16"" pos:end=""1:18"">Foo</name><parameter_list pos:start=""1:19"" pos:end=""1:20"">()</parameter_list> <block pos:start=""1:22"" pos:end=""1:24"">{<block_content> </block_content>}</block></function> }</block></class>";

            //class B { protected A a; }
            /*string b_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">B</name> <block pos:line=""1"" pos:column=""9"">{ <decl_stmt><decl><type><specifier pos:line=""1"" pos:column=""11"">protected</specifier> <name pos:line=""1"" pos:column=""21"">A</name></type> <name pos:line=""1"" pos:column=""23"">a</name></decl>;</decl_stmt> }</block></class>";*/
            string b_xml = @"<class pos:start=""1:1"" pos:end=""1:26"">class <name pos:start=""1:7"" pos:end=""1:7"">B</name> <block pos:start=""1:9"" pos:end=""1:26"">{ <decl_stmt pos:start=""1:11"" pos:end=""1:24""><decl pos:start=""1:11"" pos:end=""1:23""><type pos:start=""1:11"" pos:end=""1:21""><specifier pos:start=""1:11"" pos:end=""1:19"">protected</specifier> <name pos:start=""1:21"" pos:end=""1:21"">A</name></type> <name pos:start=""1:23"" pos:end=""1:23"">a</name></decl>;</decl_stmt> }</block></class>";

            //class C : B { void Bar() { a.Foo(); } }
            /*string c_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">C</name> <super pos:line=""1"" pos:column=""9"">: <name pos:line=""1"" pos:column=""11"">B</name></super> <block pos:line=""1"" pos:column=""13"">{ <function><type><name pos:line=""1"" pos:column=""15"">void</name></type> <name pos:line=""1"" pos:column=""20"">Bar</name><parameter_list pos:line=""1"" pos:column=""23"">()</parameter_list> <block pos:line=""1"" pos:column=""26"">{ <expr_stmt><expr><call><name><name pos:line=""1"" pos:column=""28"">a</name><operator pos:line=""1"" pos:column=""29"">.</operator><name pos:line=""1"" pos:column=""30"">Foo</name></name><argument_list pos:line=""1"" pos:column=""33"">()</argument_list></call></expr>;</expr_stmt> }</block></function> }</block></class>";*/
            string c_xml = @"<class pos:start=""1:1"" pos:end=""1:39"">class <name pos:start=""1:7"" pos:end=""1:7"">C</name> <super_list pos:start=""1:9"" pos:end=""1:11"">: <super pos:start=""1:11"" pos:end=""1:11""><name pos:start=""1:11"" pos:end=""1:11"">B</name></super></super_list> <block pos:start=""1:13"" pos:end=""1:39"">{ <function pos:start=""1:15"" pos:end=""1:37""><type pos:start=""1:15"" pos:end=""1:18""><name pos:start=""1:15"" pos:end=""1:18"">void</name></type> <name pos:start=""1:20"" pos:end=""1:22"">Bar</name><parameter_list pos:start=""1:23"" pos:end=""1:24"">()</parameter_list> <block pos:start=""1:26"" pos:end=""1:37"">{<block_content pos:start=""1:28"" pos:end=""1:35""> <expr_stmt pos:start=""1:28"" pos:end=""1:35""><expr pos:start=""1:28"" pos:end=""1:34""><call pos:start=""1:28"" pos:end=""1:34""><name pos:start=""1:28"" pos:end=""1:32""><name pos:start=""1:28"" pos:end=""1:28"">a</name><operator pos:start=""1:29"" pos:end=""1:29"">.</operator><name pos:start=""1:30"" pos:end=""1:32"">Foo</name></name><argument_list pos:start=""1:33"" pos:end=""1:34"">()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function> }</block></class>";

            var aUnit = fileSetup.GetFileUnitForXmlSnippet(a_xml, "A.cs");
            var bUnit = fileSetup.GetFileUnitForXmlSnippet(b_xml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(c_xml, "C.cs");

            var globalScope = codeParser.ParseFileUnit(aUnit);
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(bUnit));
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(cUnit));

            var typeA = globalScope.GetNamedChildren<TypeDefinition>("A").FirstOrDefault();
            var typeB = globalScope.GetNamedChildren<TypeDefinition>("B").FirstOrDefault();
            var typeC = globalScope.GetNamedChildren<TypeDefinition>("C").FirstOrDefault();

            Assert.IsNotNull(typeA, "could not find class A");
            Assert.IsNotNull(typeB, "could not find class B");
            Assert.IsNotNull(typeC, "could not find class C");

            var aDotFoo = typeA.GetNamedChildren<MethodDefinition>("Foo").FirstOrDefault();
            Assert.IsNotNull(aDotFoo, "could not find method A.Foo()");

            var cDotBar = typeC.GetNamedChildren<MethodDefinition>("Bar").FirstOrDefault();
            Assert.IsNotNull(cDotBar, "could not find method C.Bar()");

            var callToFoo = cDotBar.FindExpressions<MethodCall>(true).FirstOrDefault();
            Assert.IsNotNull(callToFoo, "could not find any method calls in C.Bar()");
            Assert.AreEqual("Foo", callToFoo.Name);

            Assert.AreEqual(aDotFoo, callToFoo.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestVariableDeclaredInCallingObjectWithParentClass() {
            //class A { B b; }
            /*string a_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">A</name> <block pos:line=""1"" pos:column=""9"">{ <decl_stmt><decl><type><name pos:line=""1"" pos:column=""11"">B</name></type> <name pos:line=""1"" pos:column=""13"">b</name></decl>;</decl_stmt> }</block></class>";*/
            string a_xml = @"<class pos:start=""1:1"" pos:end=""1:16"">class <name pos:start=""1:7"" pos:end=""1:7"">A</name> <block pos:start=""1:9"" pos:end=""1:16"">{ <decl_stmt pos:start=""1:11"" pos:end=""1:14""><decl pos:start=""1:11"" pos:end=""1:13""><type pos:start=""1:11"" pos:end=""1:11""><name pos:start=""1:11"" pos:end=""1:11"">B</name></type> <name pos:start=""1:13"" pos:end=""1:13"">b</name></decl>;</decl_stmt> }</block></class>";

            //class B { void Foo() { } }
            /*string b_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">B</name> <block pos:line=""1"" pos:column=""9"">{ <function><type><name pos:line=""1"" pos:column=""11"">void</name></type> <name pos:line=""1"" pos:column=""16"">Foo</name><parameter_list pos:line=""1"" pos:column=""19"">()</parameter_list> <block pos:line=""1"" pos:column=""22"">{ }</block></function> }</block></class>";*/
            string b_xml = @"<class pos:start=""1:1"" pos:end=""1:26"">class <name pos:start=""1:7"" pos:end=""1:7"">B</name> <block pos:start=""1:9"" pos:end=""1:26"">{ <function pos:start=""1:11"" pos:end=""1:24""><type pos:start=""1:11"" pos:end=""1:14""><name pos:start=""1:11"" pos:end=""1:14"">void</name></type> <name pos:start=""1:16"" pos:end=""1:18"">Foo</name><parameter_list pos:start=""1:19"" pos:end=""1:20"">()</parameter_list> <block pos:start=""1:22"" pos:end=""1:24"">{<block_content> </block_content>}</block></function> }</block></class>";

            //class C : A { }
            /*string c_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">C</name> <super pos:line=""1"" pos:column=""9"">: <name pos:line=""1"" pos:column=""11"">A</name></super> <block pos:line=""1"" pos:column=""13"">{ }</block></class>";*/
            string c_xml = @"<class pos:start=""1:1"" pos:end=""1:15"">class <name pos:start=""1:7"" pos:end=""1:7"">C</name> <super_list pos:start=""1:9"" pos:end=""1:11"">: <super pos:start=""1:11"" pos:end=""1:11""><name pos:start=""1:11"" pos:end=""1:11"">A</name></super></super_list> <block pos:start=""1:13"" pos:end=""1:15"">{ }</block></class>";


            //class D {
            //	C c;
            //	void Bar() { c.b.Foo(); }
            //}
            /*string d_xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">D</name> <block pos:line=""1"" pos:column=""9"">{
    <decl_stmt><decl><type><name pos:line=""2"" pos:column=""5"">C</name></type> <name pos:line=""2"" pos:column=""7"">c</name></decl>;</decl_stmt>
    <function><type><name pos:line=""3"" pos:column=""5"">void</name></type> <name pos:line=""3"" pos:column=""10"">Bar</name><parameter_list pos:line=""3"" pos:column=""13"">()</parameter_list> <block pos:line=""3"" pos:column=""16"">{ <expr_stmt><expr><call><name><name pos:line=""3"" pos:column=""18"">c</name><operator pos:line=""3"" pos:column=""19"">.</operator><name pos:line=""3"" pos:column=""20"">b</name><operator pos:line=""3"" pos:column=""21"">.</operator><name pos:line=""3"" pos:column=""22"">Foo</name></name><argument_list pos:line=""3"" pos:column=""25"">()</argument_list></call></expr>;</expr_stmt> }</block></function>
}</block></class>";*/

            string d_xml = @"<class pos:start=""1:1"" pos:end=""5:1"">class <name pos:start=""1:7"" pos:end=""1:7"">D</name> <block pos:start=""2:1"" pos:end=""5:1"">{
	<decl_stmt pos:start=""3:9"" pos:end=""3:12""><decl pos:start=""3:9"" pos:end=""3:11""><type pos:start=""3:9"" pos:end=""3:9""><name pos:start=""3:9"" pos:end=""3:9"">C</name></type> <name pos:start=""3:11"" pos:end=""3:11"">c</name></decl>;</decl_stmt>
	<function pos:start=""4:9"" pos:end=""4:33""><type pos:start=""4:9"" pos:end=""4:12""><name pos:start=""4:9"" pos:end=""4:12"">void</name></type> <name pos:start=""4:14"" pos:end=""4:16"">Bar</name><parameter_list pos:start=""4:17"" pos:end=""4:18"">()</parameter_list> <block pos:start=""4:20"" pos:end=""4:33"">{<block_content pos:start=""4:22"" pos:end=""4:31""> <expr_stmt pos:start=""4:22"" pos:end=""4:31""><expr pos:start=""4:22"" pos:end=""4:30""><call pos:start=""4:22"" pos:end=""4:30""><name pos:start=""4:22"" pos:end=""4:28""><name pos:start=""4:22"" pos:end=""4:22"">c</name><operator pos:start=""4:23"" pos:end=""4:23"">.</operator><name pos:start=""4:24"" pos:end=""4:24"">b</name><operator pos:start=""4:25"" pos:end=""4:25"">.</operator><name pos:start=""4:26"" pos:end=""4:28"">Foo</name></name><argument_list pos:start=""4:29"" pos:end=""4:30"">()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>
}</block></class>";

            var aUnit = fileSetup.GetFileUnitForXmlSnippet(a_xml, "A.cs");
            var bUnit = fileSetup.GetFileUnitForXmlSnippet(b_xml, "B.cs");
            var cUnit = fileSetup.GetFileUnitForXmlSnippet(c_xml, "C.cs");
            var dUnit = fileSetup.GetFileUnitForXmlSnippet(d_xml, "D.cs");

            var globalScope = codeParser.ParseFileUnit(aUnit);
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(bUnit));
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(cUnit));
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(dUnit));

            var typeA = globalScope.GetNamedChildren<TypeDefinition>("A").FirstOrDefault();
            var typeB = globalScope.GetNamedChildren<TypeDefinition>("B").FirstOrDefault();
            var typeC = globalScope.GetNamedChildren<TypeDefinition>("C").FirstOrDefault();
            var typeD = globalScope.GetNamedChildren<TypeDefinition>("D").FirstOrDefault();

            Assert.IsNotNull(typeA, "could not find class A");
            Assert.IsNotNull(typeB, "could not find class B");
            Assert.IsNotNull(typeC, "could not find class C");
            Assert.IsNotNull(typeD, "could not find class D");

            var adotB = typeA.GetNamedChildren<VariableDeclaration>("b").FirstOrDefault();
            Assert.IsNotNull(adotB, "could not find variable A.b");
            Assert.AreEqual("b", adotB.Name);

            var bDotFoo = typeB.GetNamedChildren<MethodDefinition>("Foo").FirstOrDefault();
            Assert.IsNotNull(bDotFoo, "could not method B.Foo()");

            var dDotBar = typeD.GetNamedChildren<MethodDefinition>("Bar").FirstOrDefault();
            Assert.IsNotNull(dDotBar, "could not find method D.Bar()");

            var callToFoo = dDotBar.FindExpressions<MethodCall>(true).FirstOrDefault();
            Assert.IsNotNull(callToFoo, "could not find any method calls in D.Bar()");

            Assert.AreEqual(bDotFoo, callToFoo.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestResolveArrayVariable_Property() {
            //class Foo {
            //  Collection<int> Parameters { get; set; }
            //  void DoWork() {
            //    printf(Parameters[0]);
            //  }
            //}
            /*string xml = @"<class>class <name>Foo</name> <block>{
  <decl_stmt><decl><type><name><name>Collection</name><argument_list>&lt;<argument><name>int</name></argument>&gt;</argument_list></name></type> <name>Parameters</name> <block>{ <function_decl><name>get</name>;</function_decl> <function_decl><name>set</name>;</function_decl> }</block></decl></decl_stmt>
  <function><type><name>void</name></type> <name>DoWork</name><parameter_list>()</parameter_list> <block>{
    <expr_stmt><expr><call><name>printf</name><argument_list>(<argument><expr><name><name>Parameters</name><index>[<expr><literal type=""number"">0</literal></expr>]</index></name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  }</block></function>
}</block></class>";*/

            string xml = @"<class pos:start=""1:1"" pos:end=""7:1"">class <name pos:start=""1:7"" pos:end=""1:9"">Foo</name> <block pos:start=""2:1"" pos:end=""7:1"">{
  <property pos:start=""3:3"" pos:end=""3:42""><type pos:start=""3:3"" pos:end=""3:17""><name pos:start=""3:3"" pos:end=""3:17""><name pos:start=""3:3"" pos:end=""3:12"">Collection</name><argument_list type=""generic"" pos:start=""3:13"" pos:end=""3:17"">&lt;<argument pos:start=""3:14"" pos:end=""3:16""><name pos:start=""3:14"" pos:end=""3:16"">int</name></argument>&gt;</argument_list></name></type> <name pos:start=""3:19"" pos:end=""3:28"">Parameters</name> <block pos:start=""3:30"" pos:end=""3:42"">{ <function_decl pos:start=""3:32"" pos:end=""3:35""><name pos:start=""3:32"" pos:end=""3:34"">get</name>;</function_decl> <function_decl pos:start=""3:37"" pos:end=""3:40""><name pos:start=""3:37"" pos:end=""3:39"">set</name>;</function_decl> }</block></property>
  <function pos:start=""4:3"" pos:end=""6:3""><type pos:start=""4:3"" pos:end=""4:6""><name pos:start=""4:3"" pos:end=""4:6"">void</name></type> <name pos:start=""4:8"" pos:end=""4:13"">DoWork</name><parameter_list pos:start=""4:14"" pos:end=""4:15"">()</parameter_list> <block pos:start=""4:17"" pos:end=""6:3"">{<block_content pos:start=""5:5"" pos:end=""5:26"">
    <expr_stmt pos:start=""5:5"" pos:end=""5:26""><expr pos:start=""5:5"" pos:end=""5:25""><call pos:start=""5:5"" pos:end=""5:25""><name pos:start=""5:5"" pos:end=""5:10"">printf</name><argument_list pos:start=""5:11"" pos:end=""5:25"">(<argument pos:start=""5:12"" pos:end=""5:24""><expr pos:start=""5:12"" pos:end=""5:24""><name pos:start=""5:12"" pos:end=""5:24""><name pos:start=""5:12"" pos:end=""5:21"">Parameters</name><index pos:start=""5:22"" pos:end=""5:24"">[<expr pos:start=""5:23"" pos:end=""5:23""><literal type=""number"" pos:start=""5:23"" pos:end=""5:23"">0</literal></expr>]</index></name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  </block_content>}</block></function>
}</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            var paramDecl = globalScope.GetDescendants<PropertyDefinition>().First(p => p.Name == "Parameters");
            Assert.IsNotNull(paramDecl);
            var doWork = globalScope.GetDescendants<MethodDefinition>().First(m => m.Name == "DoWork");
            Assert.AreEqual(1, doWork.ChildStatements.Count);
            var paramUse = doWork.ChildStatements[0].Content.GetDescendantsAndSelf<NameUse>().FirstOrDefault(n => n.Name == "Parameters");
            Assert.IsNotNull(paramUse);
            Assert.AreSame(paramDecl, paramUse.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestTypeUseForOtherNamespace() {
            //namespace A.B {
            //    class C {
            //        int Foo() { }
            //    }
            //}
            /*string c_xml = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name><name pos:line=""1"" pos:column=""11"">A</name><operator pos:line=""1"" pos:column=""12"">.</operator><name pos:line=""1"" pos:column=""13"">B</name></name> <block pos:line=""1"" pos:column=""15"">{
    <class pos:line=""2"" pos:column=""5"">class <name pos:line=""2"" pos:column=""11"">C</name> <block pos:line=""2"" pos:column=""13"">{
        <function><type><name pos:line=""3"" pos:column=""9"">int</name></type> <name pos:line=""3"" pos:column=""13"">Foo</name><parameter_list pos:line=""3"" pos:column=""16"">()</parameter_list> <block pos:line=""3"" pos:column=""19"">{ }</block></function>
    }</block></class>
}</block></namespace>";*/

            string c_xml = @"<namespace pos:start=""1:1"" pos:end=""6:1"">namespace <name pos:start=""1:11"" pos:end=""1:13""><name pos:start=""1:11"" pos:end=""1:11"">A</name><operator pos:start=""1:12"" pos:end=""1:12"">.</operator><name pos:start=""1:13"" pos:end=""1:13"">B</name></name> <block pos:start=""2:1"" pos:end=""6:1"">{
    <class pos:start=""3:5"" pos:end=""5:5"">class <name pos:start=""3:11"" pos:end=""3:11"">C</name> <block pos:start=""3:13"" pos:end=""5:5"">{
        <function pos:start=""4:9"" pos:end=""4:21""><type pos:start=""4:9"" pos:end=""4:11""><name pos:start=""4:9"" pos:end=""4:11"">int</name></type> <name pos:start=""4:13"" pos:end=""4:15"">Foo</name><parameter_list pos:start=""4:16"" pos:end=""4:17"">()</parameter_list> <block pos:start=""4:19"" pos:end=""4:21"">{<block_content> </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            //using A.B;
            //namespace D {
            //    class E {
            //        void main() {
            //            C c = new C();
            //            c.Foo();
            //        }
            //    }
            //}
            /*string e_xml = @"<using pos:line=""1"" pos:column=""1"">using <name><name pos:line=""1"" pos:column=""7"">A</name><operator pos:line=""1"" pos:column=""8"">.</operator><name pos:line=""1"" pos:column=""9"">B</name></name>;</using>
<namespace pos:line=""2"" pos:column=""1"">namespace <name pos:line=""2"" pos:column=""11"">D</name> <block pos:line=""2"" pos:column=""13"">{
    <class pos:line=""3"" pos:column=""5"">class <name pos:line=""3"" pos:column=""11"">E</name> <block pos:line=""3"" pos:column=""13"">{
        <function><type><name pos:line=""4"" pos:column=""9"">void</name></type> <name pos:line=""4"" pos:column=""14"">main</name><parameter_list pos:line=""4"" pos:column=""18"">()</parameter_list> <block pos:line=""4"" pos:column=""21"">{
            <decl_stmt><decl><type><name pos:line=""5"" pos:column=""13"">C</name></type> <name pos:line=""5"" pos:column=""15"">c</name> <init pos:line=""5"" pos:column=""17"">= <expr><operator pos:line=""5"" pos:column=""19"">new</operator> <call><name pos:line=""5"" pos:column=""23"">C</name><argument_list pos:line=""5"" pos:column=""24"">()</argument_list></call></expr></init></decl>;</decl_stmt>
            <expr_stmt><expr><call><name><name pos:line=""6"" pos:column=""13"">c</name><operator pos:line=""6"" pos:column=""14"">.</operator><name pos:line=""6"" pos:column=""15"">Foo</name></name><argument_list pos:line=""6"" pos:column=""18"">()</argument_list></call></expr>;</expr_stmt>
        }</block></function>
    }</block></class>
}</block></namespace>";*/

            string e_xml = @"<using pos:start=""1:1"" pos:end=""1:10"">using <name pos:start=""1:7"" pos:end=""1:9""><name pos:start=""1:7"" pos:end=""1:7"">A</name><operator pos:start=""1:8"" pos:end=""1:8"">.</operator><name pos:start=""1:9"" pos:end=""1:9"">B</name></name>;</using>
<namespace pos:start=""2:1"" pos:end=""9:1"">namespace <name pos:start=""2:11"" pos:end=""2:11"">D</name> <block pos:start=""2:13"" pos:end=""9:1"">{
    <class pos:start=""3:5"" pos:end=""8:5"">class <name pos:start=""3:11"" pos:end=""3:11"">E</name> <block pos:start=""3:13"" pos:end=""8:5"">{
        <function pos:start=""4:9"" pos:end=""7:9""><type pos:start=""4:9"" pos:end=""4:12""><name pos:start=""4:9"" pos:end=""4:12"">void</name></type> <name pos:start=""4:14"" pos:end=""4:17"">main</name><parameter_list pos:start=""4:18"" pos:end=""4:19"">()</parameter_list> <block pos:start=""4:21"" pos:end=""7:9"">{<block_content pos:start=""5:13"" pos:end=""6:20"">
            <decl_stmt pos:start=""5:13"" pos:end=""5:26""><decl pos:start=""5:13"" pos:end=""5:25""><type pos:start=""5:13"" pos:end=""5:13""><name pos:start=""5:13"" pos:end=""5:13"">C</name></type> <name pos:start=""5:15"" pos:end=""5:15"">c</name> <init pos:start=""5:17"" pos:end=""5:25"">= <expr pos:start=""5:19"" pos:end=""5:25""><operator pos:start=""5:19"" pos:end=""5:21"">new</operator> <call pos:start=""5:23"" pos:end=""5:25""><name pos:start=""5:23"" pos:end=""5:23"">C</name><argument_list pos:start=""5:24"" pos:end=""5:25"">()</argument_list></call></expr></init></decl>;</decl_stmt>
            <expr_stmt pos:start=""6:13"" pos:end=""6:20""><expr pos:start=""6:13"" pos:end=""6:19""><call pos:start=""6:13"" pos:end=""6:19""><name pos:start=""6:13"" pos:end=""6:17""><name pos:start=""6:13"" pos:end=""6:13"">c</name><operator pos:start=""6:14"" pos:end=""6:14"">.</operator><name pos:start=""6:15"" pos:end=""6:17"">Foo</name></name><argument_list pos:start=""6:18"" pos:end=""6:19"">()</argument_list></call></expr>;</expr_stmt>
        </block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var cUnit = fileSetup.GetFileUnitForXmlSnippet(c_xml, "C.cpp");
            var eUnit = fileSetup.GetFileUnitForXmlSnippet(e_xml, "E.cpp");

            NamespaceDefinition globalScope = codeParser.ParseFileUnit(cUnit);
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(eUnit)) as NamespaceDefinition;

            var typeC = globalScope.GetDescendants<TypeDefinition>().Where(t => t.Name == "C").FirstOrDefault();
            var typeE = globalScope.GetDescendants<TypeDefinition>().Where(t => t.Name == "E").FirstOrDefault();

            var mainMethod = typeE.ChildStatements.First() as MethodDefinition;
            Assert.IsNotNull(mainMethod, "is not a method definition");
            Assert.AreEqual("main", mainMethod.Name);

            var fooMethod = typeC.GetNamedChildren<MethodDefinition>("Foo").FirstOrDefault();
            Assert.IsNotNull(fooMethod, "no method foo found");
            Assert.AreEqual("Foo", fooMethod.Name);

            var cDeclaration = mainMethod.FindExpressions<VariableDeclaration>(true).FirstOrDefault();
            Assert.IsNotNull(cDeclaration, "No declaration found");
            Assert.AreSame(typeC, cDeclaration.VariableType.ResolveType().FirstOrDefault());

            var callToCConstructor = mainMethod.FindExpressions<MethodCall>(true).First();
            var callToFoo = mainMethod.FindExpressions<MethodCall>(true).Last();

            Assert.AreEqual("C", callToCConstructor.Name);
            Assert.That(callToCConstructor.IsConstructor);
            Assert.IsNull(callToCConstructor.FindMatches().FirstOrDefault());

            Assert.AreEqual("Foo", callToFoo.Name);
            Assert.AreSame(fooMethod, callToFoo.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestLockStatement() {
            //lock(myVar) {
            //    myVar.DoFoo();
            //}
            /*string xml = @"<lock pos:line=""1"" pos:column=""1"">lock(<expr><name pos:line=""1"" pos:column=""6"">myVar</name></expr>) <block pos:line=""1"" pos:column=""13"">{
    <expr_stmt><expr><call><name><name pos:line=""2"" pos:column=""5"">myVar</name><operator pos:line=""2"" pos:column=""10"">.</operator><name pos:line=""2"" pos:column=""11"">DoFoo</name></name><argument_list pos:line=""2"" pos:column=""16"">()</argument_list></call></expr>;</expr_stmt>
}</block></lock>";*/

            string xml = @"<lock pos:start=""1:1"" pos:end=""4:1"">lock <init pos:start=""1:6"" pos:end=""1:12"">(<expr pos:start=""1:7"" pos:end=""1:11""><name pos:start=""1:7"" pos:end=""1:11"">myVar</name></expr>)</init>
<block pos:start=""2:1"" pos:end=""4:1"">{<block_content pos:start=""3:5"" pos:end=""3:18"">
    <expr_stmt pos:start=""3:5"" pos:end=""3:18""><expr pos:start=""3:5"" pos:end=""3:17""><call pos:start=""3:5"" pos:end=""3:17""><name pos:start=""3:5"" pos:end=""3:15""><name pos:start=""3:5"" pos:end=""3:9"">myVar</name><operator pos:start=""3:10"" pos:end=""3:10"">.</operator><name pos:start=""3:11"" pos:end=""3:15"">DoFoo</name></name><argument_list pos:start=""3:16"" pos:end=""3:17"">()</argument_list></call></expr>;</expr_stmt>
</block_content>}</block></lock>";

            var xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.cs");
            var globalScope = codeParser.ParseFileUnit(xmlElement);

            var lockStmt = globalScope.ChildStatements.First() as LockStatement;
            Assert.IsNotNull(lockStmt);
            Assert.AreEqual(1, lockStmt.ChildStatements.Count);

            var lockVar = lockStmt.LockExpression as NameUse;
            Assert.IsNotNull(lockVar);
            Assert.AreEqual("myVar", lockVar.Name);
        }
    }
}