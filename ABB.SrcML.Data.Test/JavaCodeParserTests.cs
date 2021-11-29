/******************************************************************************
 * Copyright (c) 2013 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Vinay Augustine (ABB Group) - initial API, implementation, & documentation
 *    Patrick Francis (ABB Group) - API, implementation, & documentation
 *****************************************************************************/

using ABB.SrcML.Test.Utilities;
using NUnit.Framework;
using System.Linq;
using System.Xml.Linq;

namespace ABB.SrcML.Data.Test {

    [TestFixture]
    [Category("Build")]
    internal class JavaCodeParserTests {
        private AbstractCodeParser codeParser;
        private SrcMLFileUnitSetup fileSetup;

        [TestFixtureSetUp]
        public void ClassSetup() {
            codeParser = new JavaCodeParser();
            fileSetup = new SrcMLFileUnitSetup(Language.Java);
        }

        [Test]
        public void TestCreateAliasesForFiles_ImportClass() {
            //import x.y.z;
            /*string xml = @"<import>import <name><name>x</name><operator>.</operator><name>y</name><operator>.</operator><name>z</name></name>;</import>";*/
            string xml = @"<import>import <name><name>x</name><operator>.</operator><name>y</name><operator>.</operator><name>z</name></name>;</import>";
            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(1, globalScope.ChildStatements.Count);

            var actual = globalScope.ChildStatements[0] as AliasStatement;
            Assert.IsNotNull(actual);
            Assert.AreEqual("z", actual.AliasName);
            Assert.AreEqual("x . y . z", actual.Target.ToString());
        }

        [Test]
        public void TestCreateAliasesForFiles_ImportNamespace() {
            // import x . /*test */ y  /*test */ . z .* /*test*/;
            //string xml = @"<import>import <name><name>x</name> <operator>.</operator> <comment type=""block"">/*test */</comment> <name>y</name>  <comment type=""block"">/*test */</comment> <operator>.</operator> <name>z</name></name> .* <comment type=""block"">/*test*/</comment>;</import>";*/
            string xml = @"<import>import <name><name>x</name> <operator>.</operator> <comment type=""block"">/*test */</comment> <name>y</name>  <comment type=""block"">/*test */</comment> <operator>.</operator> <name>z</name> <operator>.</operator><name>*</name></name> <comment type=""block"">/*test*/</comment>;</import>";
            
            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(1, globalScope.ChildStatements.Count);

            var actual = globalScope.ChildStatements[0] as ImportStatement;
            Assert.IsNotNull(actual);
            Assert.AreEqual("x . y . z", actual.ImportedNamespace.ToString());
        }

        [Test]
        public void TestCreateTypeDefinition_ClassInPackage() {
            //package A.B.C;
            //public class D { }
            /*string xml = @"<package>package <name><name>A</name><operator>.</operator><name>B</name><operator>.</operator><name>C</name></name>;</package>
<class><specifier>public</specifier> class <name>D</name> <block>{ }</block></class>";*/
            string xml = @"<package>package <name><name>A</name><operator>.</operator><name>B</name><operator>.</operator><name>C</name></name>;</package>
<class><specifier>public</specifier> class <name>D</name> <block>{ }</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "D.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.IsTrue(globalScope.IsGlobal);
            Assert.AreEqual(1, globalScope.ChildStatements.Count());

            var packageA = globalScope.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(packageA);
            Assert.AreEqual("A", packageA.Name);
            Assert.IsFalse(packageA.IsGlobal);
            Assert.AreEqual(1, packageA.ChildStatements.Count());

            var packageAB = packageA.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(packageAB);
            Assert.AreEqual("B", packageAB.Name);
            Assert.IsFalse(packageAB.IsGlobal);
            Assert.AreEqual(1, packageAB.ChildStatements.Count());

            var packageABC = packageAB.ChildStatements.First() as NamespaceDefinition;
            Assert.IsNotNull(packageABC);
            Assert.AreEqual("C", packageABC.Name);
            Assert.IsFalse(packageABC.IsGlobal);
            Assert.AreEqual(1, packageABC.ChildStatements.Count());

            var typeD = packageABC.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(typeD, "Type D is not a type definition");
            Assert.AreEqual("D", typeD.Name);
        }

        [Test]
        public void TestCreateTypeDefinitions_Class() {
            // class A { }
            /*string xml = @"<class>class <name>A</name> <block>{
}</block></class>";*/
            string xml = @"<class>class <name>A</name> <block>{ }</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.IsTrue(globalScope.IsGlobal);

            var actual = globalScope.ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(actual);
            Assert.AreEqual("A", actual.Name);
            Assert.AreEqual(0, actual.ChildStatements.Count);
        }

        [Test]
        public void TestCreateTypeDefinitions_ClassInFunction() {
            // class A { int foo() { class B { } } }
            /*string xml = @"<class>class <name>A</name> <block>{
	<function><type><name>int</name></type> <name>foo</name><parameter_list>()</parameter_list> <block>{
		<class>class <name>B</name> <block>{
		}</block></class>
}</block></function>
}</block></class>";*/
            string xml = @"<class>class <name>A</name> <block>{ <function><type><name>int</name></type> <name>foo</name><parameter_list>()</parameter_list> <block>{<block_content> <class>class <name>B</name> <block>{ }</block></class> </block_content>}</block></function> }</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);

            var typeA = globalScope.ChildStatements.First() as TypeDefinition;
            var fooMethod = typeA.ChildStatements.First() as MethodDefinition;
            var typeB = fooMethod.ChildStatements.First() as TypeDefinition;

            Assert.AreEqual("A", typeA.GetFullName());
            Assert.AreSame(typeA, fooMethod.ParentStatement);
            Assert.AreEqual("A.foo", fooMethod.GetFullName());
            Assert.AreSame(fooMethod, typeB.ParentStatement);
            Assert.AreEqual("A.foo.B", typeB.GetFullName());
        }

        [Test]
        public void TestCreateTypeDefinitions_ClassWithExtendsAndImplements() {
            //Foo.java
            //public class Foo extends xyzzy implements A, B, C {
            //    public int bar;
            //}
            /*string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <super><extends>extends <name>xyzzy</name></extends> <implements>implements <name>A</name>, <name>B</name>, <name>C</name></implements></super> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";*/
            string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <super_list><extends>extends <super><name>xyzzy</name></super></extends> <implements>implements <super><name>A</name></super>, <super><name>B</name></super>, <super><name>C</name></super></implements></super_list> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.java");

            var actual = codeParser.ParseFileUnit(xmlElement).ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(actual);
            var globalNamespace = actual.ParentStatement as NamespaceDefinition;
            Assert.IsNotNull(globalNamespace);
            Assert.AreEqual("Foo", actual.Name);
            Assert.AreEqual(4, actual.ParentTypeNames.Count);
            Assert.That(globalNamespace.IsGlobal);

            var parentNames = from parent in actual.ParentTypeNames
                              select parent.Name;

            var tests = Enumerable.Zip<string, string, bool>(new[] { "xyzzy", "A", "B", "C" }, parentNames, (e, a) => e == a);
            foreach(var test in tests) {
                Assert.That(test);
            }
        }

        [Test]
        public void TestCreateTypeDefinitions_ClassWithInnerClass() {
            // class A { class B { } }
            /*string xml = @"<class>class <name>A</name> <block>{
	<class>class <name>B</name> <block>{
	}</block></class>
}</block></class>";*/
            string xml = @"<class>class <name>A</name> <block>{ <class>class <name>B</name> <block>{ }</block></class> }</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);

            var typeA = globalScope.ChildStatements.First() as TypeDefinition;
            var typeB = typeA.ChildStatements.First() as TypeDefinition;

            Assert.AreSame(typeA, typeB.ParentStatement);
            Assert.AreEqual("A", typeA.GetFullName());
            Assert.AreEqual("A.B", typeB.GetFullName());
        }

        [Test]
        public void TestCreateTypeDefinitions_ClassWithParents() {
            // class A implements B,C,D { }
            /*string xml = @"<class>class <name>A</name> <super><implements>implements <name>B</name>,<name>C</name>,<name>D</name></implements></super> <block>{
}</block></class>";*/
            string xml = @"<class>class <name>A</name> <super_list><implements>implements <super><name>B</name></super>,<super><name>C</name></super>,<super><name>D</name></super></implements></super_list> <block>{ }</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var actual = codeParser.ParseFileUnit(xmlElement).ChildStatements.First() as TypeDefinition;
            var globalNamespace = actual.ParentStatement as NamespaceDefinition;
            Assert.AreEqual("A", actual.Name);
            Assert.AreEqual(3, actual.ParentTypeNames.Count);
            Assert.That(globalNamespace.IsGlobal);

            var parentNames = from parent in actual.ParentTypeNames
                              select parent.Name;

            var tests = Enumerable.Zip<string, string, bool>(new[] { "B", "C", "D" }, parentNames, (e, a) => e == a);
            foreach(var test in tests) {
                Assert.That(test);
            }
        }

        [Test]
        public void TestCreateTypeDefinitions_ClassWithQualifiedParent() {
            // class D implements A.B.C { }
            /*string xml = @"<class>class <name>D</name> <super><implements>implements <name><name>A</name><operator>.</operator><name>B</name><operator>.</operator><name>C</name></name></implements></super> <block>{ }</block></class>";*/
            string xml = @"<class>class <name>D</name> <super_list><implements>implements <super><name><name>A</name><operator>.</operator><name>B</name><operator>.</operator><name>C</name></name></super></implements></super_list> <block>{ }</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "D.java");

            var actual = codeParser.ParseFileUnit(xmlElement).ChildStatements.First() as TypeDefinition;
            var globalNamespace = actual.ParentStatement as NamespaceDefinition;

            Assert.AreEqual("D", actual.Name);
            Assert.AreEqual(1, actual.ParentTypeNames.Count);
            Assert.That(globalNamespace.IsGlobal);

            var parent = actual.ParentTypeNames.First();

            Assert.AreEqual("C", parent.Name);
            
            var prefixNames = parent.Prefix.Names.ToList();
            Assert.AreEqual(2, prefixNames.Count);
            Assert.AreEqual("A", prefixNames[0].Name);
            Assert.AreEqual("B", prefixNames[1].Name);
        }

        [Test]
        public void TestCreateTypeDefinitions_ClassWithSuperClass() {
            //Foo.java
            //public class Foo extends xyzzy {
            //    public int bar;
            //}
            /*string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <super><extends>extends <name>xyzzy</name></extends></super> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";*/
            string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <super_list><extends>extends <super><name>xyzzy</name></super></extends></super_list> <block>{
    <decl_stmt><decl><type><specifier>public</specifier> <name>int</name></type> <name>bar</name></decl>;</decl_stmt>
}</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.java");

            var actual = codeParser.ParseFileUnit(xmlElement).ChildStatements.First() as TypeDefinition;
            Assert.IsNotNull(actual);
            Assert.AreEqual("Foo", actual.Name);
            Assert.AreEqual(1, actual.ParentTypeNames.Count);
            Assert.AreEqual("xyzzy", actual.ParentTypeNames.First().Name);
            var globalNamespace = actual.ParentStatement as NamespaceDefinition;
            Assert.IsNotNull(globalNamespace);
            Assert.That(globalNamespace.IsGlobal);
        }

        [Test]
        public void TestCreateTypeDefinitions_InnerClassWithNamespace() {
            // package A; class B { class C { } }
            /*string xml = @"<package>package <name>A</name>;</package>
<class>class <name>B</name> <block>{
	<class>class <name>C</name> <block>{
	}</block></class>
}</block></class>";*/
            string xml = @"<package>package <name>A</name>;</package> <class>class <name>B</name> <block>{ <class>class <name>C</name> <block>{ }</block></class> }</block></class>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "B.java");
            var globalScope = codeParser.ParseFileUnit(xmlElement);

            var typeDefinitions = globalScope.GetDescendants<TypeDefinition>().ToList();
            Assert.AreEqual(2, typeDefinitions.Count());

            var outer = typeDefinitions.First();
            var inner = typeDefinitions.Last();

            Assert.AreEqual("B", outer.Name);
            Assert.AreEqual("A", outer.GetAncestors<NamespaceDefinition>().First().GetFullName());
            Assert.AreEqual("A.B", outer.GetFullName());

            Assert.AreEqual("C", inner.Name);
            Assert.AreEqual("A", inner.GetAncestors<NamespaceDefinition>().First().GetFullName());
            Assert.AreEqual("A.B.C", inner.GetFullName());
        }

        [Test]
        public void TestCreateTypeDefinitions_Interface() {
            // interface A { }
            /*string xml = @"<class type=""interface"">interface <name>A</name> <block>{
}</block></class>";*/
            string xml = @"<interface>interface <name>A</name> <block>{ }</block></interface>";

            XElement xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var actual = codeParser.ParseFileUnit(xmlElement).ChildStatements.First() as TypeDefinition;
            var globalNamespace = actual.ParentStatement as NamespaceDefinition;

            Assert.AreEqual("A", actual.Name);
            Assert.AreEqual(TypeKind.Interface, actual.Kind);
            Assert.That(globalNamespace.IsGlobal);
        }

        [Test]
        public void TestFieldCreation() {
            // # A.java class A { int B; }
            /*string xml = @"<class>class <name>A</name> <block>{
    <decl_stmt><decl><type><name>int</name></type> <name>B</name></decl>;</decl_stmt>
}</block></class>";*/
            string xml = @"<class>class <name>A</name> <block>{ <decl_stmt><decl><type><name>int</name></type> <name>B</name></decl>;</decl_stmt> }</block></class>";

            var xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);

            var typeA = globalScope.ChildStatements.First();
            Assert.AreEqual(1, typeA.ChildStatements.Count);

            var fieldB = typeA.ChildStatements.First().Content as VariableDeclaration;
            Assert.IsNotNull(fieldB);
            Assert.AreEqual("B", fieldB.Name);
            Assert.AreEqual("int", fieldB.VariableType.Name);
            Assert.AreEqual(AccessModifier.None, fieldB.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForMethod_None() {
            //public class Foo {
            //    bool Bar() { return true; }
            //}
            /*string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{
    <function><type><name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><name>true</name></expr>;</return> }</block></function>
}</block></class>";*/
            string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{
    <function><type><name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
}</block></class>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.java");

            var globalScope = codeParser.ParseFileUnit(unit);
            var method = globalScope.GetDescendants<MethodDefinition>().First();

            Assert.AreEqual(AccessModifier.None, method.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForMethod_Normal() {
            //public class Foo {
            //    public bool Bar() { return true; }
            //}
            /*string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{
    <function><type><specifier>public</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><name>true</name></expr>;</return> }</block></function>
}</block></class>";*/
            string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{
    <function><type><specifier>public</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
}</block></class>";

            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.java");

            var globalScope = codeParser.ParseFileUnit(unit);
            var method = globalScope.GetDescendants<MethodDefinition>().First();

            Assert.AreEqual(AccessModifier.Public, method.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForMethod_Static() {
            //public class Foo {
            //    static public bool Bar() { return true; }
            //}
            /*string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{
    <function><type><specifier>static</specifier> <specifier>public</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><name>true</name></expr>;</return> }</block></function>
}</block></class>";*/
            string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{
    <function><type><specifier>static</specifier> <specifier>public</specifier> <name>bool</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
}</block></class>";
            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.java");

            var globalScope = codeParser.ParseFileUnit(unit);
            var method = globalScope.GetDescendants<MethodDefinition>().First();

            Assert.AreEqual(AccessModifier.Public, method.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForType_None() {
            //class Foo {}
            /*string xml = @"<class>class <name>Foo</name> <block>{}</block></class>";*/
            string xml = @"<class>class <name>Foo</name> <block>{}</block></class>";
            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.java");

            var globalScope = codeParser.ParseFileUnit(unit);
            var type = globalScope.GetDescendants<TypeDefinition>().First();

            Assert.AreEqual(AccessModifier.None, type.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForType_Normal() {
            //public class Foo {}
            /*string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{}</block></class>";*/
            string xml = @"<class><specifier>public</specifier> class <name>Foo</name> <block>{}</block></class>";
            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.java");

            var globalScope = codeParser.ParseFileUnit(unit);
            var type = globalScope.GetDescendants<TypeDefinition>().First();

            Assert.AreEqual(AccessModifier.Public, type.Accessibility);
        }

        [Test]
        public void TestGetAccessModifierForType_Static() {
            //static public class Foo {}
            /*string xml = @"<class><specifier>static</specifier> <specifier>public</specifier> class <name>Foo</name> <block>{}</block></class>";*/
            string xml = @"<class><specifier>static</specifier> <specifier>public</specifier> class <name>Foo</name> <block>{}</block></class>";
            var unit = fileSetup.GetFileUnitForXmlSnippet(xml, "Foo.java");

            var globalScope = codeParser.ParseFileUnit(unit);
            var type = globalScope.GetDescendants<TypeDefinition>().First();

            Assert.AreEqual(AccessModifier.Public, type.Accessibility);
        }



        [Test]
        public void TestMethodCallCreation_WithConflictingMethodNames() {
            //# A.java
            //class A {
            //    B b;
            //    boolean Contains() { b.Contains(); }
            //}
            /*string a_xml = @"<class>class <name>A</name> <block>{
    <decl_stmt><decl><type><name>B</name></type> <name>b</name></decl>;</decl_stmt>
    <function><type><name>boolean</name></type> <name>Contains</name><parameter_list>()</parameter_list> <block>{ <expr_stmt><expr><call><name><name>b</name><operator>.</operator><name>Contains</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> }</block></function>
}</block></class>";*/
            string a_xml = @"<class>class <name>A</name> <block>{
    <decl_stmt><decl><type><name>B</name></type> <name>b</name></decl>;</decl_stmt>
    <function><type><name>boolean</name></type> <name>Contains</name><parameter_list>()</parameter_list> <block>{<block_content> <expr_stmt><expr><call><name><name>b</name><operator>.</operator><name>Contains</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>
}</block></class>";

            //class B {
            //    boolean Contains() { return true; }
            //}
            /*string b_xml = @"<class>class <name>B</name> <block>{
    <function><type><name>boolean</name></type> <name>Contains</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> }</block></function>
}</block></class>";*/
            string b_xml = @"<class>class <name>B</name> <block>{
    <function><type><name>boolean</name></type> <name>Contains</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><literal type=""boolean"">true</literal></expr>;</return> </block_content>}</block></function>
}</block></class>";

            var fileUnitA = fileSetup.GetFileUnitForXmlSnippet(a_xml, "A.java");
            var fileUnitB = fileSetup.GetFileUnitForXmlSnippet(b_xml, "B.java");

            var scopeForA = codeParser.ParseFileUnit(fileUnitA);
            var scopeForB = codeParser.ParseFileUnit(fileUnitB);
            var globalScope = scopeForA.Merge(scopeForB);

            var containsMethods = globalScope.GetDescendants<MethodDefinition>().Where(m => m.Name == "Contains").ToList();

            var aDotContains = containsMethods.FirstOrDefault(m => m.GetAncestors<TypeDefinition>().FirstOrDefault().Name == "A");
            Assert.IsNotNull(aDotContains);
            var bDotContains = containsMethods.FirstOrDefault(m => m.GetAncestors<TypeDefinition>().FirstOrDefault().Name == "B");
            Assert.IsNotNull(bDotContains);
            Assert.AreEqual("A.Contains", aDotContains.GetFullName());
            Assert.AreEqual("B.Contains", bDotContains.GetFullName());

            Assert.AreEqual(1, aDotContains.ChildStatements.Count);
            var containsCall = aDotContains.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(containsCall);
            Assert.AreSame(bDotContains, containsCall.FindMatches().First());
            Assert.AreNotSame(aDotContains, containsCall.FindMatches().First());
        }

        [Test]
        public void TestMethodCallCreation_WithThisKeyword() {
            //class A {
            //    void Bar() { }
            //    class B {
            //        int a;
            //        void Foo() { this.Bar(); }
            //        void Bar() { return this.a; }
            //    }
            //}
            /*string a_xml = @"<class>class <name>A</name> <block>{
    <function><type><name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ }</block></function>
    <class>class <name>B</name> <block>{
        <decl_stmt><decl><type><name>int</name></type> <name>a</name></decl>;</decl_stmt>
        <function><type><name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ <expr_stmt><expr><call><name><name>this</name><operator>.</operator><name>Bar</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> }</block></function>
        <function><type><name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ <return>return <expr><name>this</name><operator>.</operator><name>a</name></expr>;</return> }</block></function>
    }</block></class>
}</block></class>";*/
            string a_xml = @"<class>class <name>A</name> <block>{
    <function><type><name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function>
    <class>class <name>B</name> <block>{
        <decl_stmt><decl><type><name>int</name></type> <name>a</name></decl>;</decl_stmt>
        <function><type><name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> <expr_stmt><expr><call><name><name>this</name><operator>.</operator><name>Bar</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>
        <function><type><name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> <return>return <expr><name><name>this</name><operator>.</operator><name>a</name></name></expr>;</return> </block_content>}</block></function>
    }</block></class>
}</block></class>";

            var fileUnit = fileSetup.GetFileUnitForXmlSnippet(a_xml, "A.java");
            var globalScope = codeParser.ParseFileUnit(fileUnit);

            var aDotBar = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Bar" && ((TypeDefinition)m.ParentStatement).Name == "A");
            Assert.IsNotNull(aDotBar);
            var aDotBDotFoo = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Foo");
            Assert.IsNotNull(aDotBDotFoo);
            var aDotBDotBar = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "Bar" && ((TypeDefinition)m.ParentStatement).Name == "B");
            Assert.IsNotNull(aDotBDotBar);

            Assert.AreEqual("A.Bar", aDotBar.GetFullName());
            Assert.AreEqual("A.B.Foo", aDotBDotFoo.GetFullName());
            Assert.AreEqual("A.B.Bar", aDotBDotBar.GetFullName());

            Assert.AreEqual(1, aDotBDotFoo.ChildStatements.Count);
            var barCall = aDotBDotFoo.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(barCall);
            var matches = barCall.FindMatches().ToList();
            Assert.AreEqual(1, matches.Count);
            Assert.AreSame(aDotBDotBar, matches.First());
        }

        [Test]
        public void TestMethodCallCreation_WithSuperKeyword() {
            //class B {
            //  public void Foo() { } 
            //}
            //class C extends B { 
            //  public void Foo() { }
            //  public void Bar() { 
            //    super.Foo(); 
            //  }
            //}
            /*string xml = @"<class>class <name>B</name> <block>{
  <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ }</block></function> 
}</block></class>
<class>class <name>C</name> <super><extends>extends <name>B</name></extends></super> <block>{ 
  <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{ }</block></function>
  <function><type><specifier>public</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{ 
    <expr_stmt><expr><call><name><name>super</name><operator>.</operator><name>Foo</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> 
  }</block></function>
}</block></class>";*/
            string xml = @"<class>class <name>B</name> <block>{
  <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function> 
}</block></class>
<class>class <name>C</name> <super_list><extends>extends <super><name>B</name></super></extends></super_list> <block>{ 
  <function><type><specifier>public</specifier> <name>void</name></type> <name>Foo</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></function>
  <function><type><specifier>public</specifier> <name>void</name></type> <name>Bar</name><parameter_list>()</parameter_list> <block>{<block_content> 
    <expr_stmt><expr><call><name><name>super</name><operator>.</operator><name>Foo</name></name><argument_list>()</argument_list></call></expr>;</expr_stmt> 
  </block_content>}</block></function>
}</block></class>";

            var xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(2, globalScope.ChildStatements.Count);

            var fooMethods = globalScope.GetDescendants<MethodDefinition>().ToList();

            var bDotFoo = fooMethods.FirstOrDefault(m => m.GetAncestors<TypeDefinition>().FirstOrDefault().Name == "B");
            Assert.IsNotNull(bDotFoo);
            var cDotFoo = fooMethods.FirstOrDefault(m => m.GetAncestors<TypeDefinition>().FirstOrDefault().Name == "C");
            Assert.IsNotNull(cDotFoo);

            var bar = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(md => md.Name == "Bar");
            Assert.IsNotNull(bar);
            Assert.AreEqual(1, bar.ChildStatements.Count);
            var methodCall = bar.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(methodCall);
            Assert.AreSame(bDotFoo, methodCall.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestMethodCallCreation_SuperConstructor() {
            //class B {
            //  public B(int num) { }
            //}
            //class C extends B { 
            //  public C() {
            //    super(17);
            //  }
            //}
            /*string xml = @"<class>class <name>B</name> <block>{
  <constructor><specifier>public</specifier> <name>B</name><parameter_list>(<parameter><decl><type><name>int</name></type> <name>num</name></decl></parameter>)</parameter_list> <block>{ }</block></constructor>
}</block></class>
<class>class <name>C</name> <super><extends>extends <name>B</name></extends></super> <block>{ 
  <constructor><specifier>public</specifier> <name>C</name><parameter_list>()</parameter_list> <block>{
    <expr_stmt><expr><call><name>super</name><argument_list>(<argument><expr><literal type=""number"">17</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  }</block></constructor>
}</block></class>";*/
            string xml = @"<class>class <name>B</name> <block>{
  <constructor><specifier>public</specifier> <name>B</name><parameter_list>(<parameter><decl><type><name>int</name></type> <name>num</name></decl></parameter>)</parameter_list> <block>{<block_content> </block_content>}</block></constructor>
}</block></class>
<class>class <name>C</name> <super_list><extends>extends <super><name>B</name></super></extends></super_list> <block>{ 
  <constructor><specifier>public</specifier> <name>C</name><parameter_list>()</parameter_list> <block>{<block_content>
    <expr_stmt><expr><call><name>super</name><argument_list>(<argument><expr><literal type=""number"">17</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  </block_content>}</block></constructor>
}</block></class>";

            var xmlElement = fileSetup.GetFileUnitForXmlSnippet(xml, "A.java");

            var globalScope = codeParser.ParseFileUnit(xmlElement);
            Assert.AreEqual(2, globalScope.ChildStatements.Count);

            var bConstructor = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "B");
            Assert.IsNotNull(bConstructor);
            var cConstructor = globalScope.GetDescendants<MethodDefinition>().FirstOrDefault(m => m.Name == "C");
            Assert.IsNotNull(cConstructor);
            
            Assert.AreEqual(1, cConstructor.ChildStatements.Count);
            var superCall = cConstructor.ChildStatements[0].Content.GetDescendantsAndSelf<MethodCall>().FirstOrDefault();
            Assert.IsNotNull(superCall);
            Assert.IsTrue(superCall.IsConstructor);
            Assert.AreSame(bConstructor, superCall.FindMatches().FirstOrDefault());
        }

        [Test]
        public void TestMethodCallCreation_ConstructorFromOtherNamespace() {
            //package A.B;
            //class C {
            //	public C() { }
            //}
            /*string c_xml = @"<package>package <name>A</name>.<name>B</name>;</package>
<class>class <name>C</name> <block>{
	<constructor><specifier>public</specifier> <name>C</name><parameter_list>()</parameter_list> <block>{ }</block></constructor>
}</block></class>";*/
            string c_xml = @"<package>package <name><name>A</name><operator>.</operator><name>B</name></name>;</package>
<class>class <name>C</name> <block>{
	<constructor><specifier>public</specifier> <name>C</name><parameter_list>()</parameter_list> <block>{<block_content> </block_content>}</block></constructor>
}</block></class>";

            //package A.D;
            //import A.B.*;
            //class E {
            //	public void main() {
            //		C c = new C();
            //	}
            //}
            /*string e_xml = @"<package>package <name>A</name>.<name>D</name>;</package>
<import>import <name>A</name>.<name>B</name>.*;</import>
<class>class <name>E</name> <block>{
	<function><type><specifier>public</specifier> <name>void</name></type> <name>main</name><parameter_list>()</parameter_list> <block>{
		<decl_stmt><decl><type><name>C</name></type> <name>c</name> =<init> <expr><operator>new</operator> <call><name>C</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt>
	}</block></function>
}</block></class>";*/
            string e_xml = @"<package>package <name><name>A</name><operator>.</operator><name>D</name></name>;</package>
<import>import <name><name>A</name><operator>.</operator><name>B</name><operator>.</operator><name>*</name></name>;</import>
<class>class <name>E</name> <block>{
	<function><type><specifier>public</specifier> <name>void</name></type> <name>main</name><parameter_list>()</parameter_list> <block>{<block_content>
		<decl_stmt><decl><type><name>C</name></type> <name>c</name> <init>= <expr><operator>new</operator> <call><name>C</name><argument_list>()</argument_list></call></expr></init></decl>;</decl_stmt>
	</block_content>}</block></function>
}</block></class>";

            var cUnit = fileSetup.GetFileUnitForXmlSnippet(c_xml, "C.java");
            var eUnit = fileSetup.GetFileUnitForXmlSnippet(e_xml, "E.java");

            var cScope = codeParser.ParseFileUnit(cUnit);
            var eScope = codeParser.ParseFileUnit(eUnit);

            var globalScope = cScope.Merge(eScope);

            var typeC = (from typeDefinition in globalScope.GetDescendantsAndSelf<TypeDefinition>()
                         where typeDefinition.Name == "C"
                         select typeDefinition).First();

            var typeE = (from typeDefinition in globalScope.GetDescendantsAndSelf<TypeDefinition>()
                         where typeDefinition.Name == "E"
                         select typeDefinition).First();

            Assert.IsNotNull(typeC, "Could not find class C");
            Assert.IsNotNull(typeE, "Could not find class E");

            var constructorForC = typeC.ChildStatements.OfType<MethodDefinition>().FirstOrDefault();

            Assert.IsNotNull(constructorForC, "C has no methods");
            Assert.AreEqual("C", constructorForC.Name);
            Assert.That(constructorForC.IsConstructor);

            var eDotMain = typeE.GetNamedChildren<MethodDefinition>("main").FirstOrDefault();

            Assert.IsNotNull(eDotMain, "could not find main method in E");
            Assert.AreEqual("main", eDotMain.Name);

            var callToC = (from stmt in eDotMain.GetDescendants()
                           from expr in stmt.GetExpressions()
                           from call in expr.GetDescendantsAndSelf<MethodCall>()
                           select call).FirstOrDefault();
            Assert.IsNotNull(callToC, "main contains no calls");
            Assert.AreEqual("C", callToC.Name);
            Assert.That(callToC.IsConstructor);

            Assert.AreEqual(constructorForC, callToC.FindMatches().FirstOrDefault());
        }
        
        [Test]
        public void TestVariablesWithSpecifiers() {
            //public static int A;
            //public final int B;
            //private static final Foo C;
            /*string testXml = @"<decl_stmt><decl><type><specifier>public</specifier> <specifier>static</specifier> <name>int</name></type> <name>A</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>public</specifier> <specifier>final</specifier> <name>int</name></type> <name>B</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>private</specifier> <specifier>static</specifier> <specifier>final</specifier> <name>Foo</name></type> <name>C</name></decl>;</decl_stmt>";*/
            string testXml = @"<decl_stmt><decl><type><specifier>public</specifier> <specifier>static</specifier> <name>int</name></type> <name>A</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>public</specifier> <specifier>final</specifier> <name>int</name></type> <name>B</name></decl>;</decl_stmt>
<decl_stmt><decl><type><specifier>private</specifier> <specifier>static</specifier> <specifier>final</specifier> <name>Foo</name></type> <name>C</name></decl>;</decl_stmt>";

            var testUnit = fileSetup.GetFileUnitForXmlSnippet(testXml, "test.java");

            var globalScope = codeParser.ParseFileUnit(testUnit);
            Assert.AreEqual(3, globalScope.ChildStatements.Count);

            var declA = globalScope.ChildStatements[0].Content as VariableDeclaration;
            Assert.IsNotNull(declA);
            Assert.AreEqual("A", declA.Name);
            Assert.AreEqual("int", declA.VariableType.Name);
            Assert.AreEqual(AccessModifier.Public, declA.Accessibility);

            var declB = globalScope.ChildStatements[1].Content as VariableDeclaration;
            Assert.IsNotNull(declB);
            Assert.AreEqual("B", declB.Name);
            Assert.AreEqual("int", declB.VariableType.Name);
            Assert.AreEqual(AccessModifier.Public, declB.Accessibility);

            var declC = globalScope.ChildStatements[2].Content as VariableDeclaration;
            Assert.IsNotNull(declC);
            Assert.AreEqual("C", declC.Name);
            Assert.AreEqual("Foo", declC.VariableType.Name);
            Assert.AreEqual(AccessModifier.Private, declC.Accessibility);
        }

        [Test]
        public void TestImport_NameResolution() {
            //A.java
            //import Foo.Bar.*;
            //package A;
            //public class Robot {
            //  public Baz GetThingy() { return new Baz(); }
            //}
            /*string xmlA = @"<import pos:line=""1"" pos:column=""1"">import <name><name pos:line=""1"" pos:column=""8"">Foo</name><operator pos:line=""1"" pos:column=""11"">.</operator><name pos:line=""1"" pos:column=""12"">Bar</name></name>.*;</import>
<package pos:line=""2"" pos:column=""1"">package <name pos:line=""2"" pos:column=""9"">A</name>;</package>
<class><specifier pos:line=""3"" pos:column=""1"">public</specifier> class <name pos:line=""3"" pos:column=""14"">Robot</name> <block pos:line=""3"" pos:column=""20"">{
  <function><type><specifier pos:line=""4"" pos:column=""3"">public</specifier> <name pos:line=""4"" pos:column=""10"">Baz</name></type> <name pos:line=""4"" pos:column=""14"">GetThingy</name><parameter_list pos:line=""4"" pos:column=""23"">()</parameter_list> <block pos:line=""4"" pos:column=""26"">{ <return pos:line=""4"" pos:column=""28"">return <expr><operator pos:line=""4"" pos:column=""35"">new</operator> <call><name pos:line=""4"" pos:column=""39"">Baz</name><argument_list pos:line=""4"" pos:column=""42"">()</argument_list></call></expr>;</return> }</block></function>
}</block></class>";*/
            string xmlA = @"<import pos:start=""1:1"" pos:end=""1:17"">import <name pos:start=""1:8"" pos:end=""1:16""><name pos:start=""1:8"" pos:end=""1:10"">Foo</name><operator pos:start=""1:11"" pos:end=""1:11"">.</operator><name pos:start=""1:12"" pos:end=""1:14"">Bar</name><operator pos:start=""1:15"" pos:end=""1:15"">.</operator><name pos:start=""1:16"" pos:end=""1:16"">*</name></name>;</import>
<package pos:start=""2:1"" pos:end=""2:10"">package <name pos:start=""2:9"" pos:end=""2:9"">A</name>;</package>
<class pos:start=""3:1"" pos:end=""5:1""><specifier pos:start=""3:1"" pos:end=""3:6"">public</specifier> class <name pos:start=""3:14"" pos:end=""3:18"">Robot</name> <block pos:start=""3:20"" pos:end=""5:1"">{
  <function pos:start=""4:3"" pos:end=""4:46""><type pos:start=""4:3"" pos:end=""4:12""><specifier pos:start=""4:3"" pos:end=""4:8"">public</specifier> <name pos:start=""4:10"" pos:end=""4:12"">Baz</name></type> <name pos:start=""4:14"" pos:end=""4:22"">GetThingy</name><parameter_list pos:start=""4:23"" pos:end=""4:24"">()</parameter_list> <block pos:start=""4:26"" pos:end=""4:46"">{<block_content pos:start=""4:28"" pos:end=""4:44""> <return pos:start=""4:28"" pos:end=""4:44"">return <expr pos:start=""4:35"" pos:end=""4:43""><operator pos:start=""4:35"" pos:end=""4:37"">new</operator> <call pos:start=""4:39"" pos:end=""4:43""><name pos:start=""4:39"" pos:end=""4:41"">Baz</name><argument_list pos:start=""4:42"" pos:end=""4:43"">()</argument_list></call></expr>;</return> </block_content>}</block></function>
}</block></class>";

            XElement xmlElementA = fileSetup.GetFileUnitForXmlSnippet(xmlA, "A.java");
            //B.java
            //package Foo.Bar;
            //public class Baz {
            //  public Baz() { }
            //}
            /*string xmlB = @"<package pos:line=""1"" pos:column=""1"">package <name><name pos:line=""1"" pos:column=""9"">Foo</name><operator pos:line=""1"" pos:column=""12"">.</operator><name pos:line=""1"" pos:column=""13"">Bar</name></name>;</package>
<class><specifier pos:line=""2"" pos:column=""1"">public</specifier> class <name pos:line=""2"" pos:column=""14"">Baz</name> <block pos:line=""2"" pos:column=""18"">{
  <constructor><specifier pos:line=""3"" pos:column=""3"">public</specifier> <name pos:line=""3"" pos:column=""10"">Baz</name><parameter_list pos:line=""3"" pos:column=""13"">()</parameter_list> <block pos:line=""3"" pos:column=""16"">{ }</block></constructor>
}</block></class>";*/
            string xmlB = @"<package pos:start=""1:1"" pos:end=""1:16"">package <name pos:start=""1:9"" pos:end=""1:15""><name pos:start=""1:9"" pos:end=""1:11"">Foo</name><operator pos:start=""1:12"" pos:end=""1:12"">.</operator><name pos:start=""1:13"" pos:end=""1:15"">Bar</name></name>;</package>
<class pos:start=""2:1"" pos:end=""4:1""><specifier pos:start=""2:1"" pos:end=""2:6"">public</specifier> class <name pos:start=""2:14"" pos:end=""2:16"">Baz</name> <block pos:start=""2:18"" pos:end=""4:1"">{
  <constructor pos:start=""3:3"" pos:end=""3:18""><specifier pos:start=""3:3"" pos:end=""3:8"">public</specifier> <name pos:start=""3:10"" pos:end=""3:12"">Baz</name><parameter_list pos:start=""3:13"" pos:end=""3:14"">()</parameter_list> <block pos:start=""3:16"" pos:end=""3:18"">{<block_content> </block_content>}</block></constructor>
}</block></class>";

            XElement xmlElementB = fileSetup.GetFileUnitForXmlSnippet(xmlB, "B.java");
            
            var scopeA = codeParser.ParseFileUnit(xmlElementA);
            var scopeB = codeParser.ParseFileUnit(xmlElementB);
            var globalScope = scopeA.Merge(scopeB);
            Assert.AreEqual(2, globalScope.ChildStatements.Count);

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
            //A.java
            //package Foo.Bar;
            //public class Baz {
            //  public static void DoTheThing() { };
            //}
            /*string xmlA = @"<package pos:line=""1"" pos:column=""1"">package <name><name pos:line=""1"" pos:column=""9"">Foo</name><operator pos:line=""1"" pos:column=""12"">.</operator><name pos:line=""1"" pos:column=""13"">Bar</name></name>;</package>
<class><specifier pos:line=""2"" pos:column=""1"">public</specifier> class <name pos:line=""2"" pos:column=""14"">Baz</name> <block pos:line=""2"" pos:column=""18"">{
  <function><type><specifier pos:line=""3"" pos:column=""3"">public</specifier> <specifier pos:line=""3"" pos:column=""10"">static</specifier> <name pos:line=""3"" pos:column=""17"">void</name></type> <name pos:line=""3"" pos:column=""22"">DoTheThing</name><parameter_list pos:line=""3"" pos:column=""32"">()</parameter_list> <block pos:line=""3"" pos:column=""35"">{ }</block></function><empty_stmt pos:line=""3"" pos:column=""38"">;</empty_stmt>
}</block></class>";*/
            string xmlA = @"<package pos:start=""1:1"" pos:end=""1:16"">package <name pos:start=""1:9"" pos:end=""1:15""><name pos:start=""1:9"" pos:end=""1:11"">Foo</name><operator pos:start=""1:12"" pos:end=""1:12"">.</operator><name pos:start=""1:13"" pos:end=""1:15"">Bar</name></name>;</package>
<class pos:start=""2:1"" pos:end=""4:1""><specifier pos:start=""2:1"" pos:end=""2:6"">public</specifier> class <name pos:start=""2:14"" pos:end=""2:16"">Baz</name> <block pos:start=""2:18"" pos:end=""4:1"">{
  <function pos:start=""3:3"" pos:end=""3:37""><type pos:start=""3:3"" pos:end=""3:20""><specifier pos:start=""3:3"" pos:end=""3:8"">public</specifier> <specifier pos:start=""3:10"" pos:end=""3:15"">static</specifier> <name pos:start=""3:17"" pos:end=""3:20"">void</name></type> <name pos:start=""3:22"" pos:end=""3:31"">DoTheThing</name><parameter_list pos:start=""3:32"" pos:end=""3:33"">()</parameter_list> <block pos:start=""3:35"" pos:end=""3:37"">{<block_content> </block_content>}</block></function><empty_stmt pos:start=""3:38"" pos:end=""3:38"">;</empty_stmt>
}</block></class>";

            XElement xmlElementA = fileSetup.GetFileUnitForXmlSnippet(xmlA, "A.java");
            //B.java
            //import Foo.Bar.Baz;
            //package A;
            //public class B {
            //  public B() {
            //    Baz.DoTheThing();
            //  }
            //}
            /*string xmlB = @"<import pos:line=""1"" pos:column=""1"">import <name><name pos:line=""1"" pos:column=""8"">Foo</name><operator pos:line=""1"" pos:column=""11"">.</operator><name pos:line=""1"" pos:column=""12"">Bar</name><operator pos:line=""1"" pos:column=""15"">.</operator><name pos:line=""1"" pos:column=""16"">Baz</name></name>;</import>
<package pos:line=""2"" pos:column=""1"">package <name pos:line=""2"" pos:column=""9"">A</name>;</package>
<class><specifier pos:line=""3"" pos:column=""1"">public</specifier> class <name pos:line=""3"" pos:column=""14"">B</name> <block pos:line=""3"" pos:column=""16"">{
  <constructor><specifier pos:line=""4"" pos:column=""3"">public</specifier> <name pos:line=""4"" pos:column=""10"">B</name><parameter_list pos:line=""4"" pos:column=""11"">()</parameter_list> <block pos:line=""4"" pos:column=""14"">{
    <expr_stmt><expr><call><name><name pos:line=""5"" pos:column=""5"">Baz</name><operator pos:line=""5"" pos:column=""8"">.</operator><name pos:line=""5"" pos:column=""9"">DoTheThing</name></name><argument_list pos:line=""5"" pos:column=""19"">()</argument_list></call></expr>;</expr_stmt>
  }</block></constructor>
}</block></class>";*/
            string xmlB = @"<import pos:start=""1:1"" pos:end=""1:19"">import <name pos:start=""1:8"" pos:end=""1:18""><name pos:start=""1:8"" pos:end=""1:10"">Foo</name><operator pos:start=""1:11"" pos:end=""1:11"">.</operator><name pos:start=""1:12"" pos:end=""1:14"">Bar</name><operator pos:start=""1:15"" pos:end=""1:15"">.</operator><name pos:start=""1:16"" pos:end=""1:18"">Baz</name></name>;</import>
<package pos:start=""2:1"" pos:end=""2:10"">package <name pos:start=""2:9"" pos:end=""2:9"">A</name>;</package>
<class pos:start=""3:1"" pos:end=""7:1""><specifier pos:start=""3:1"" pos:end=""3:6"">public</specifier> class <name pos:start=""3:14"" pos:end=""3:14"">B</name> <block pos:start=""3:16"" pos:end=""7:1"">{
  <constructor pos:start=""4:3"" pos:end=""6:3""><specifier pos:start=""4:3"" pos:end=""4:8"">public</specifier> <name pos:start=""4:10"" pos:end=""4:10"">B</name><parameter_list pos:start=""4:11"" pos:end=""4:12"">()</parameter_list> <block pos:start=""4:14"" pos:end=""6:3"">{<block_content pos:start=""5:5"" pos:end=""5:21"">
    <expr_stmt pos:start=""5:5"" pos:end=""5:21""><expr pos:start=""5:5"" pos:end=""5:20""><call pos:start=""5:5"" pos:end=""5:20""><name pos:start=""5:5"" pos:end=""5:18""><name pos:start=""5:5"" pos:end=""5:7"">Baz</name><operator pos:start=""5:8"" pos:end=""5:8"">.</operator><name pos:start=""5:9"" pos:end=""5:18"">DoTheThing</name></name><argument_list pos:start=""5:19"" pos:end=""5:20"">()</argument_list></call></expr>;</expr_stmt>
  </block_content>}</block></constructor>
}</block></class>";

            XElement xmlElementB = fileSetup.GetFileUnitForXmlSnippet(xmlB, "B.java");
            
            var scopeA = codeParser.ParseFileUnit(xmlElementA);
            var scopeB = codeParser.ParseFileUnit(xmlElementB);
            var globalScope = scopeA.Merge(scopeB);
            Assert.AreEqual(2, globalScope.ChildStatements.Count);

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

        [Test]
        public void BasicParentTest_Java() {
            // # A.java class A implements B { }
            /*string a_xml = @"<class>class <name>A</name> <super><implements>implements <name>B</name></implements></super> <block>{
}</block></class>";*/
            string a_xml = @"<class>class <name>A</name> <super_list><implements>implements <super><name>B</name></super></implements></super_list> <block>{ }</block></class>";

            // # B.java class B { }
            /*string b_xml = @"<class>class <name>B</name> <block>{
}</block></class>";*/
            string b_xml = @"<class>class <name>B</name> <block>{ }</block></class>";

            // # C.java class C { A a; }
            /*string c_xml = @"<class>class <name>C</name> <block>{
	<decl_stmt><decl><type><name>A</name></type> <name>a</name></decl>;</decl_stmt>
}</block></class>";*/
            string c_xml = @"<class>class <name>C</name> <block>{ <decl_stmt><decl><type><name>A</name></type> <name>a</name></decl>;</decl_stmt> }</block></class>";

            var fileUnitA = fileSetup.GetFileUnitForXmlSnippet(a_xml, "A.java");
            var fileUnitB = fileSetup.GetFileUnitForXmlSnippet(b_xml, "B.java");
            var fileUnitC = fileSetup.GetFileUnitForXmlSnippet(c_xml, "C.java");

            var globalScope = codeParser.ParseFileUnit(fileUnitA);
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(fileUnitB));
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(fileUnitC));

            Assert.AreEqual(3, globalScope.ChildStatements.Count());

            var typeDefinitions = globalScope.GetDescendants<TypeDefinition>().OrderBy(t => t.Name).ToList();

            var typeA = typeDefinitions[0];
            var typeB = typeDefinitions[1];
            var typeC = typeDefinitions[2];

            Assert.AreEqual("B", typeB.Name);

            var cDotA = (from stmt in typeC.GetDescendants()
                         from decl in stmt.GetExpressions().OfType<VariableDeclaration>()
                         select decl).FirstOrDefault();
            var parentOfA = typeA.ParentTypeNames.FirstOrDefault();
            Assert.That(cDotA.VariableType.FindMatches().FirstOrDefault(), Is.SameAs(typeA));
            Assert.That(parentOfA.FindMatches().FirstOrDefault(), Is.SameAs(typeB));
        }

        [Test]
        public void TestTypeUseForOtherNamespace() {
            //package A.B;
            //class C {
            //    int Foo();
            //}
            /*string c_xml = @"<package pos:line=""1"" pos:column=""1"">package <name><name pos:line=""1"" pos:column=""9"">A</name><operator pos:line=""1"" pos:column=""10"">.</operator><name pos:line=""1"" pos:column=""11"">B</name></name>;</package>
<class pos:line=""2"" pos:column=""1"">class <name pos:line=""2"" pos:column=""7"">C</name> <block pos:line=""2"" pos:column=""9"">{
    <function_decl><type><name pos:line=""3"" pos:column=""5"">int</name></type> <name pos:line=""3"" pos:column=""9"">Foo</name><parameter_list pos:line=""3"" pos:column=""12"">()</parameter_list>;</function_decl>
}</block></class>";*/
            string c_xml = @"<package pos:start=""1:1"" pos:end=""1:12"">package <name pos:start=""1:9"" pos:end=""1:11""><name pos:start=""1:9"" pos:end=""1:9"">A</name><operator pos:start=""1:10"" pos:end=""1:10"">.</operator><name pos:start=""1:11"" pos:end=""1:11"">B</name></name>;</package>
<class pos:start=""2:1"" pos:end=""4:1"">class <name pos:start=""2:7"" pos:end=""2:7"">C</name> <block pos:start=""2:9"" pos:end=""4:1"">{
    <function_decl pos:start=""3:5"" pos:end=""3:14""><type pos:start=""3:5"" pos:end=""3:7""><name pos:start=""3:5"" pos:end=""3:7"">int</name></type> <name pos:start=""3:9"" pos:end=""3:11"">Foo</name><parameter_list pos:start=""3:12"" pos:end=""3:13"">()</parameter_list>;</function_decl>
}</block></class>";

            //package D;
            //import A.B.*;
            //class E {
            //    public static void main() {
            //        C c = new C();
            //        c.Foo();
            //    }
            //}
            /*string e_xml = @"<package pos:line=""1"" pos:column=""1"">package <name pos:line=""1"" pos:column=""9"">D</name>;</package>
<import pos:line=""2"" pos:column=""1"">import <name><name pos:line=""2"" pos:column=""8"">A</name><operator pos:line=""2"" pos:column=""9"">.</operator><name pos:line=""2"" pos:column=""10"">B</name></name>.*;</import>
<class pos:line=""3"" pos:column=""1"">class <name pos:line=""3"" pos:column=""7"">E</name> <block pos:line=""3"" pos:column=""9"">{
    <function><type><specifier pos:line=""4"" pos:column=""5"">public</specifier> <specifier pos:line=""4"" pos:column=""12"">static</specifier> <name pos:line=""4"" pos:column=""19"">void</name></type> <name pos:line=""4"" pos:column=""24"">main</name><parameter_list pos:line=""4"" pos:column=""28"">()</parameter_list> <block pos:line=""4"" pos:column=""31"">{
        <decl_stmt><decl><type><name pos:line=""5"" pos:column=""9"">C</name></type> <name pos:line=""5"" pos:column=""11"">c</name> <init pos:line=""5"" pos:column=""13"">= <expr><operator pos:line=""5"" pos:column=""15"">new</operator> <call><name pos:line=""5"" pos:column=""19"">C</name><argument_list pos:line=""5"" pos:column=""20"">()</argument_list></call></expr></init></decl>;</decl_stmt>
        <expr_stmt><expr><call><name><name pos:line=""6"" pos:column=""9"">c</name><operator pos:line=""6"" pos:column=""10"">.</operator><name pos:line=""6"" pos:column=""11"">Foo</name></name><argument_list pos:line=""6"" pos:column=""14"">()</argument_list></call></expr>;</expr_stmt>
    }</block></function>
}</block></class>";*/
            string e_xml = @"<package pos:start=""1:1"" pos:end=""1:10"">package <name pos:start=""1:9"" pos:end=""1:9"">D</name>;</package>
<import pos:start=""2:1"" pos:end=""2:13"">import <name pos:start=""2:8"" pos:end=""2:12""><name pos:start=""2:8"" pos:end=""2:8"">A</name><operator pos:start=""2:9"" pos:end=""2:9"">.</operator><name pos:start=""2:10"" pos:end=""2:10"">B</name><operator pos:start=""2:11"" pos:end=""2:11"">.</operator><name pos:start=""2:12"" pos:end=""2:12"">*</name></name>;</import>
<class pos:start=""3:1"" pos:end=""8:1"">class <name pos:start=""3:7"" pos:end=""3:7"">E</name> <block pos:start=""3:9"" pos:end=""8:1"">{
    <function pos:start=""4:5"" pos:end=""7:5""><type pos:start=""4:5"" pos:end=""4:22""><specifier pos:start=""4:5"" pos:end=""4:10"">public</specifier> <specifier pos:start=""4:12"" pos:end=""4:17"">static</specifier> <name pos:start=""4:19"" pos:end=""4:22"">void</name></type> <name pos:start=""4:24"" pos:end=""4:27"">main</name><parameter_list pos:start=""4:28"" pos:end=""4:29"">()</parameter_list> <block pos:start=""4:31"" pos:end=""7:5"">{<block_content pos:start=""5:9"" pos:end=""6:16"">
        <decl_stmt pos:start=""5:9"" pos:end=""5:22""><decl pos:start=""5:9"" pos:end=""5:21""><type pos:start=""5:9"" pos:end=""5:9""><name pos:start=""5:9"" pos:end=""5:9"">C</name></type> <name pos:start=""5:11"" pos:end=""5:11"">c</name> <init pos:start=""5:13"" pos:end=""5:21"">= <expr pos:start=""5:15"" pos:end=""5:21""><operator pos:start=""5:15"" pos:end=""5:17"">new</operator> <call pos:start=""5:19"" pos:end=""5:21""><name pos:start=""5:19"" pos:end=""5:19"">C</name><argument_list pos:start=""5:20"" pos:end=""5:21"">()</argument_list></call></expr></init></decl>;</decl_stmt>
        <expr_stmt pos:start=""6:9"" pos:end=""6:16""><expr pos:start=""6:9"" pos:end=""6:15""><call pos:start=""6:9"" pos:end=""6:15""><name pos:start=""6:9"" pos:end=""6:13""><name pos:start=""6:9"" pos:end=""6:9"">c</name><operator pos:start=""6:10"" pos:end=""6:10"">.</operator><name pos:start=""6:11"" pos:end=""6:13"">Foo</name></name><argument_list pos:start=""6:14"" pos:end=""6:15"">()</argument_list></call></expr>;</expr_stmt>
    </block_content>}</block></function>
}</block></class>";

            var cUnit = fileSetup.GetFileUnitForXmlSnippet(c_xml, "C.java");
            var eUnit = fileSetup.GetFileUnitForXmlSnippet(e_xml, "E.java");

            var globalScope = codeParser.ParseFileUnit(cUnit);
            globalScope = globalScope.Merge(codeParser.ParseFileUnit(eUnit));

            var typeC = globalScope.GetDescendants<TypeDefinition>().Where(t => t.Name == "C").FirstOrDefault();
            var typeE = globalScope.GetDescendants<TypeDefinition>().Where(t => t.Name == "E").FirstOrDefault();

            var mainMethod = typeE.GetNamedChildren<MethodDefinition>("main").FirstOrDefault();
            Assert.AreEqual("main", mainMethod.Name);

            var fooMethod = typeC.ChildStatements.OfType<MethodDefinition>().FirstOrDefault();
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
    }
}