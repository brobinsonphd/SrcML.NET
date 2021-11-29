/******************************************************************************
 * Copyright (c) 2014 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Patrick Francis (ABB Group) - initial API, implementation, & documentation
 *    Vinay Augustine (ABB Group) - initial API, implementation, & documentation
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.SrcML.Test.Utilities;
using NUnit.Framework;

namespace ABB.SrcML.Data.Test {
    [TestFixture]
    [Category("Build")]
    public class ProgamElementTests {
        private Dictionary<Language, AbstractCodeParser> codeParsers;
        private Dictionary<Language, SrcMLFileUnitSetup> fileSetup;

        [TestFixtureSetUp]
        public void ClassSetup() {
            codeParsers = new Dictionary<Language, AbstractCodeParser>() {
                {Language.CPlusPlus, new CPlusPlusCodeParser()},
                {Language.CSharp, new CSharpCodeParser()},
                {Language.Java, new JavaCodeParser()}
            };
            fileSetup = new Dictionary<Language, SrcMLFileUnitSetup>() {
                {Language.CPlusPlus, new SrcMLFileUnitSetup(Language.CPlusPlus)},
                {Language.CSharp, new SrcMLFileUnitSetup(Language.CSharp)},
                {Language.Java, new SrcMLFileUnitSetup(Language.Java)}
            };
        }

        [Test]
        public void TestSiblingsBeforeSelf() {
            var a = new VariableUse() {Name = "a"};
            var plus = new OperatorUse() {Text = "+"};
            var foo = new VariableUse() {Name = "foo"};
            var times = new OperatorUse() {Text = "*"};
            var b = new VariableUse() {Name = "b"};
            var exp = new Expression();
            exp.AddComponents(new Expression[] {a, plus, foo, times, b});

            var fooSiblings = foo.GetSiblingsBeforeSelf().ToList();
            Assert.AreEqual(2, fooSiblings.Count());
            Assert.AreSame(a, fooSiblings[0]);
            Assert.AreSame(plus, fooSiblings[1]);

            var aSiblings = a.GetSiblingsBeforeSelf().ToList();
            Assert.AreEqual(0, aSiblings.Count());
        }

        [Test]
        public void TestSiblingsBeforeSelf_MissingChild() {
            var a = new VariableUse() {Name = "a"};
            var plus = new OperatorUse() {Text = "+"};
            var foo = new VariableUse() {Name = "foo"};
            var times = new OperatorUse() {Text = "*"};
            var b = new VariableUse() {Name = "b"};
            var exp = new Expression();
            exp.AddComponents(new Expression[] {a, plus, foo, times, b});

            var dot = new OperatorUse {
                Text = ".",
                ParentExpression = exp
            };

            Assert.Throws<InvalidOperationException>(() => dot.GetSiblingsBeforeSelf());
        }

        [Test]
        public void TestSiblingsAfterSelf() {
            var a = new VariableUse() {Name = "a"};
            var plus = new OperatorUse() {Text = "+"};
            var foo = new VariableUse() {Name = "foo"};
            var times = new OperatorUse() {Text = "*"};
            var b = new VariableUse() {Name = "b"};
            var exp = new Expression();
            exp.AddComponents(new Expression[] {a, plus, foo, times, b});

            var plusSiblings = plus.GetSiblingsAfterSelf().ToList();
            Assert.AreEqual(3, plusSiblings.Count());
            Assert.AreSame(foo, plusSiblings[0]);
            Assert.AreSame(times, plusSiblings[1]);
            Assert.AreSame(b, plusSiblings[2]);

            var bSiblings = b.GetSiblingsAfterSelf().ToList();
            Assert.AreEqual(0, bSiblings.Count());
        }

        [Test]
        public void TestSiblingsAfterSelf_MissingChild() {
            var a = new VariableUse() {Name = "a"};
            var plus = new OperatorUse() {Text = "+"};
            var foo = new VariableUse() {Name = "foo"};
            var times = new OperatorUse() {Text = "*"};
            var b = new VariableUse() {Name = "b"};
            var exp = new Expression();
            exp.AddComponents(new Expression[] {a, plus, foo, times, b});

            var dot = new OperatorUse {
                Text = ".",
                ParentExpression = exp
            };

            Assert.Throws<InvalidOperationException>(() => dot.GetSiblingsAfterSelf());
        }

        [TestCase(Language.CPlusPlus)]
        [TestCase(Language.CSharp)]
        [TestCase(Language.Java)]
        public void TestGetNamedChildren_Statement(Language lang) {
            //int foo = 17;
            //while(bar) {
            //  MethodCall(foo);
            //  int foo = 42;
            //}
            /*string xml = @"<decl_stmt><decl><type><name pos:line=""1"" pos:column=""1"">int</name></type> <name pos:line=""1"" pos:column=""5"">foo</name> <init pos:line=""1"" pos:column=""9"">= <expr><literal type=""number"" pos:line=""1"" pos:column=""11"">17</literal></expr></init></decl>;</decl_stmt>
<while pos:line=""2"" pos:column=""1"">while<condition pos:line=""2"" pos:column=""6"">(<expr><name pos:line=""2"" pos:column=""7"">bar</name></expr>)</condition> <block pos:line=""2"" pos:column=""12"">{
  <expr_stmt><expr><call><name pos:line=""3"" pos:column=""3"">MethodCall</name><argument_list pos:line=""3"" pos:column=""13"">(<argument><expr><name pos:line=""3"" pos:column=""14"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  <decl_stmt><decl><type><name pos:line=""4"" pos:column=""3"">int</name></type> <name pos:line=""4"" pos:column=""7"">foo</name> <init pos:line=""4"" pos:column=""11"">= <expr><literal type=""number"" pos:line=""4"" pos:column=""13"">42</literal></expr></init></decl>;</decl_stmt>
}</block></while>";*/
            string xml = @"<decl_stmt pos:start=""1:1"" pos:end=""1:13""><decl pos:start=""1:1"" pos:end=""1:12""><type pos:start=""1:1"" pos:end=""1:3""><name pos:start=""1:1"" pos:end=""1:3"">int</name></type> <name pos:start=""1:5"" pos:end=""1:7"">foo</name> <init pos:start=""1:9"" pos:end=""1:12"">= <expr pos:start=""1:11"" pos:end=""1:12""><literal type=""number"" pos:start=""1:11"" pos:end=""1:12"">17</literal></expr></init></decl>;</decl_stmt>
<while pos:start=""2:1"" pos:end=""5:1"">while<condition pos:start=""2:6"" pos:end=""2:10"">(<expr pos:start=""2:7"" pos:end=""2:9""><name pos:start=""2:7"" pos:end=""2:9"">bar</name></expr>)</condition> <block pos:start=""2:12"" pos:end=""5:1"">{<block_content pos:start=""3:3"" pos:end=""4:15"">
  <expr_stmt pos:start=""3:3"" pos:end=""3:18""><expr pos:start=""3:3"" pos:end=""3:17""><call pos:start=""3:3"" pos:end=""3:17""><name pos:start=""3:3"" pos:end=""3:12"">MethodCall</name><argument_list pos:start=""3:13"" pos:end=""3:17"">(<argument pos:start=""3:14"" pos:end=""3:16""><expr pos:start=""3:14"" pos:end=""3:16""><name pos:start=""3:14"" pos:end=""3:16"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  <decl_stmt pos:start=""4:3"" pos:end=""4:15""><decl pos:start=""4:3"" pos:end=""4:14""><type pos:start=""4:3"" pos:end=""4:5""><name pos:start=""4:3"" pos:end=""4:5"">int</name></type> <name pos:start=""4:7"" pos:end=""4:9"">foo</name> <init pos:start=""4:11"" pos:end=""4:14"">= <expr pos:start=""4:13"" pos:end=""4:14""><literal type=""number"" pos:start=""4:13"" pos:end=""4:14"">42</literal></expr></init></decl>;</decl_stmt>
</block_content>}</block></while>";

            var xmlElement = fileSetup[lang].GetFileUnitForXmlSnippet(xml, "test.code");

            var globalScope = codeParsers[lang].ParseFileUnit(xmlElement);
            Assert.AreEqual(2, globalScope.ChildStatements.Count);

            var whileStmt = globalScope.GetDescendants<WhileStatement>().First();
            Assert.AreEqual(2, whileStmt.ChildStatements.Count);
            var fooUse = whileStmt.ChildStatements[0].Content.GetDescendantsAndSelf<NameUse>().First(n => n.Name == "foo");
            var localFoo = whileStmt.ChildStatements[1].Content.GetDescendantsAndSelf<VariableDeclaration>().First(v => v.Name == "foo");

            var allChildren = whileStmt.GetNamedChildren("foo").ToList();
            Assert.AreEqual(1, allChildren.Count);
            Assert.AreSame(localFoo, allChildren[0]);

            Assert.IsEmpty(whileStmt.GetNamedChildren(fooUse).ToList());
        }

        [TestCase(Language.CPlusPlus)]
        [TestCase(Language.CSharp)]
        [TestCase(Language.Java)]
        public void TestGetNamedChildren_IfStatement(Language lang) {
            //int foo = 17;
            //if(bar) {
            //  int foo = 42;
            //  MethodCall(foo);
            //} else {
            //  MethodCall2(foo);
            //  int foo = 101;
            //}
            /*string xml = @"<decl_stmt><decl><type><name pos:line=""1"" pos:column=""1"">int</name></type> <name pos:line=""1"" pos:column=""5"">foo</name> <init pos:line=""1"" pos:column=""9"">= <expr><literal type=""number"" pos:line=""1"" pos:column=""11"">17</literal></expr></init></decl>;</decl_stmt>
<if_stmt pos:line=""2"" pos:column=""1""><if pos:line=""2"" pos:column=""1"">if<condition pos:line=""2"" pos:column=""3"">(<expr><name pos:line=""2"" pos:column=""4"">bar</name></expr>)</condition><then pos:line=""2"" pos:column=""8""> <block pos:line=""2"" pos:column=""9"">{
  <decl_stmt><decl><type><name pos:line=""3"" pos:column=""3"">int</name></type> <name pos:line=""3"" pos:column=""7"">foo</name> <init pos:line=""3"" pos:column=""11"">= <expr><literal type=""number"" pos:line=""3"" pos:column=""13"">42</literal></expr></init></decl>;</decl_stmt>
  <expr_stmt><expr><call><name pos:line=""4"" pos:column=""3"">MethodCall</name><argument_list pos:line=""4"" pos:column=""13"">(<argument><expr><name pos:line=""4"" pos:column=""14"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
}</block></then> <else pos:line=""5"" pos:column=""3"">else <block pos:line=""5"" pos:column=""8"">{
  <expr_stmt><expr><call><name pos:line=""6"" pos:column=""3"">MethodCall2</name><argument_list pos:line=""6"" pos:column=""14"">(<argument><expr><name pos:line=""6"" pos:column=""15"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  <decl_stmt><decl><type><name pos:line=""7"" pos:column=""3"">int</name></type> <name pos:line=""7"" pos:column=""7"">foo</name> <init pos:line=""7"" pos:column=""11"">= <expr><literal type=""number"" pos:line=""7"" pos:column=""13"">101</literal></expr></init></decl>;</decl_stmt>
}</block></else></if></if_stmt>";*/
            string xml = @"<decl_stmt pos:start=""1:1"" pos:end=""1:13""><decl pos:start=""1:1"" pos:end=""1:12""><type pos:start=""1:1"" pos:end=""1:3""><name pos:start=""1:1"" pos:end=""1:3"">int</name></type> <name pos:start=""1:5"" pos:end=""1:7"">foo</name> <init pos:start=""1:9"" pos:end=""1:12"">= <expr pos:start=""1:11"" pos:end=""1:12""><literal type=""number"" pos:start=""1:11"" pos:end=""1:12"">17</literal></expr></init></decl>;</decl_stmt>
<if_stmt pos:start=""2:1"" pos:end=""8:1""><if pos:start=""2:1"" pos:end=""5:1"">if<condition pos:start=""2:3"" pos:end=""2:7"">(<expr pos:start=""2:4"" pos:end=""2:6""><name pos:start=""2:4"" pos:end=""2:6"">bar</name></expr>)</condition> <block pos:start=""2:9"" pos:end=""5:1"">{<block_content pos:start=""3:3"" pos:end=""4:18"">
  <decl_stmt pos:start=""3:3"" pos:end=""3:15""><decl pos:start=""3:3"" pos:end=""3:14""><type pos:start=""3:3"" pos:end=""3:5""><name pos:start=""3:3"" pos:end=""3:5"">int</name></type> <name pos:start=""3:7"" pos:end=""3:9"">foo</name> <init pos:start=""3:11"" pos:end=""3:14"">= <expr pos:start=""3:13"" pos:end=""3:14""><literal type=""number"" pos:start=""3:13"" pos:end=""3:14"">42</literal></expr></init></decl>;</decl_stmt>
  <expr_stmt pos:start=""4:3"" pos:end=""4:18""><expr pos:start=""4:3"" pos:end=""4:17""><call pos:start=""4:3"" pos:end=""4:17""><name pos:start=""4:3"" pos:end=""4:12"">MethodCall</name><argument_list pos:start=""4:13"" pos:end=""4:17"">(<argument pos:start=""4:14"" pos:end=""4:16""><expr pos:start=""4:14"" pos:end=""4:16""><name pos:start=""4:14"" pos:end=""4:16"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
</block_content>}</block></if> <else pos:start=""5:3"" pos:end=""8:1"">else <block pos:start=""5:8"" pos:end=""8:1"">{<block_content pos:start=""6:3"" pos:end=""7:16"">
  <expr_stmt pos:start=""6:3"" pos:end=""6:19""><expr pos:start=""6:3"" pos:end=""6:18""><call pos:start=""6:3"" pos:end=""6:18""><name pos:start=""6:3"" pos:end=""6:13"">MethodCall2</name><argument_list pos:start=""6:14"" pos:end=""6:18"">(<argument pos:start=""6:15"" pos:end=""6:17""><expr pos:start=""6:15"" pos:end=""6:17""><name pos:start=""6:15"" pos:end=""6:17"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  <decl_stmt pos:start=""7:3"" pos:end=""7:16""><decl pos:start=""7:3"" pos:end=""7:15""><type pos:start=""7:3"" pos:end=""7:5""><name pos:start=""7:3"" pos:end=""7:5"">int</name></type> <name pos:start=""7:7"" pos:end=""7:9"">foo</name> <init pos:start=""7:11"" pos:end=""7:15"">= <expr pos:start=""7:13"" pos:end=""7:15""><literal type=""number"" pos:start=""7:13"" pos:end=""7:15"">101</literal></expr></init></decl>;</decl_stmt>
</block_content>}</block></else></if_stmt>";

            var xmlElement = fileSetup[lang].GetFileUnitForXmlSnippet(xml, "test.code");

            var globalScope = codeParsers[lang].ParseFileUnit(xmlElement);
            Assert.AreEqual(2, globalScope.ChildStatements.Count);

            var ifStatement = globalScope.GetDescendants<IfStatement>().First();
            Assert.AreEqual(2, ifStatement.ChildStatements.Count);
            var thenFoo = ifStatement.ChildStatements[0].Content.GetDescendantsAndSelf<VariableDeclaration>().First(v => v.Name == "foo");
            var thenFooUse = ifStatement.ChildStatements[1].Content.GetDescendantsAndSelf<NameUse>().First(n => n.Name == "foo");

            Assert.AreEqual(2, ifStatement.ElseStatements.Count);
            var elseFooUse = ifStatement.ElseStatements[0].Content.GetDescendantsAndSelf<NameUse>().First(n => n.Name == "foo");
            var elseFoo = ifStatement.ElseStatements[1].Content.GetDescendantsAndSelf<VariableDeclaration>().First(v => v.Name == "foo");

            var allChildren = ifStatement.GetNamedChildren("foo").ToList();
            Assert.AreEqual(2, allChildren.Count);
            Assert.AreSame(thenFoo, allChildren[0]);
            Assert.AreSame(elseFoo, allChildren[1]);

            var thenMatches = ifStatement.GetNamedChildren(thenFooUse).ToList();
            Assert.AreEqual(1, thenMatches.Count);
            Assert.AreSame(thenFoo, thenMatches[0]);

            Assert.IsEmpty(ifStatement.GetNamedChildren(elseFooUse).ToList());
        }

        [TestCase(Language.CPlusPlus)]
        [TestCase(Language.CSharp)]
        [TestCase(Language.Java)]
        public void TestGetNamedChildren_TryStatement(Language lang) {
            //int foo = 17;
            //try {
            //  int foo = 42;
            //  MethodCall(foo);
            //} finally {
            //  MethodCall2(foo);
            //  int foo = 101;
            //}
            /*string xml = @"<decl_stmt><decl><type><name pos:line=""1"" pos:column=""1"">int</name></type> <name pos:line=""1"" pos:column=""5"">foo</name> <init pos:line=""1"" pos:column=""9"">= <expr><literal type=""number"" pos:line=""1"" pos:column=""11"">17</literal></expr></init></decl>;</decl_stmt>
<try pos:line=""2"" pos:column=""1"">try <block pos:line=""2"" pos:column=""5"">{
  <decl_stmt><decl><type><name pos:line=""3"" pos:column=""3"">int</name></type> <name pos:line=""3"" pos:column=""7"">foo</name> <init pos:line=""3"" pos:column=""11"">= <expr><literal type=""number"" pos:line=""3"" pos:column=""13"">42</literal></expr></init></decl>;</decl_stmt>
  <expr_stmt><expr><call><name pos:line=""4"" pos:column=""3"">MethodCall</name><argument_list pos:line=""4"" pos:column=""13"">(<argument><expr><name pos:line=""4"" pos:column=""14"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
}</block> <finally pos:line=""5"" pos:column=""3"">finally <block pos:line=""5"" pos:column=""11"">{
  <expr_stmt><expr><call><name pos:line=""6"" pos:column=""3"">MethodCall2</name><argument_list pos:line=""6"" pos:column=""14"">(<argument><expr><name pos:line=""6"" pos:column=""15"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  <decl_stmt><decl><type><name pos:line=""7"" pos:column=""3"">int</name></type> <name pos:line=""7"" pos:column=""7"">foo</name> <init pos:line=""7"" pos:column=""11"">= <expr><literal type=""number"" pos:line=""7"" pos:column=""13"">101</literal></expr></init></decl>;</decl_stmt>
}</block></finally></try>";*/
            string xml = @"<decl_stmt pos:start=""1:1"" pos:end=""1:13""><decl pos:start=""1:1"" pos:end=""1:12""><type pos:start=""1:1"" pos:end=""1:3""><name pos:start=""1:1"" pos:end=""1:3"">int</name></type> <name pos:start=""1:5"" pos:end=""1:7"">foo</name> <init pos:start=""1:9"" pos:end=""1:12"">= <expr pos:start=""1:11"" pos:end=""1:12""><literal type=""number"" pos:start=""1:11"" pos:end=""1:12"">17</literal></expr></init></decl>;</decl_stmt>
<try pos:start=""2:1"" pos:end=""8:1"">try <block pos:start=""2:5"" pos:end=""5:1"">{<block_content pos:start=""3:3"" pos:end=""4:18"">
  <decl_stmt pos:start=""3:3"" pos:end=""3:15""><decl pos:start=""3:3"" pos:end=""3:14""><type pos:start=""3:3"" pos:end=""3:5""><name pos:start=""3:3"" pos:end=""3:5"">int</name></type> <name pos:start=""3:7"" pos:end=""3:9"">foo</name> <init pos:start=""3:11"" pos:end=""3:14"">= <expr pos:start=""3:13"" pos:end=""3:14""><literal type=""number"" pos:start=""3:13"" pos:end=""3:14"">42</literal></expr></init></decl>;</decl_stmt>
  <expr_stmt pos:start=""4:3"" pos:end=""4:18""><expr pos:start=""4:3"" pos:end=""4:17""><call pos:start=""4:3"" pos:end=""4:17""><name pos:start=""4:3"" pos:end=""4:12"">MethodCall</name><argument_list pos:start=""4:13"" pos:end=""4:17"">(<argument pos:start=""4:14"" pos:end=""4:16""><expr pos:start=""4:14"" pos:end=""4:16""><name pos:start=""4:14"" pos:end=""4:16"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
</block_content>}</block> <finally pos:start=""5:3"" pos:end=""8:1"">finally <block pos:start=""5:11"" pos:end=""8:1"">{<block_content pos:start=""6:3"" pos:end=""7:16"">
  <expr_stmt pos:start=""6:3"" pos:end=""6:19""><expr pos:start=""6:3"" pos:end=""6:18""><call pos:start=""6:3"" pos:end=""6:18""><name pos:start=""6:3"" pos:end=""6:13"">MethodCall2</name><argument_list pos:start=""6:14"" pos:end=""6:18"">(<argument pos:start=""6:15"" pos:end=""6:17""><expr pos:start=""6:15"" pos:end=""6:17""><name pos:start=""6:15"" pos:end=""6:17"">foo</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  <decl_stmt pos:start=""7:3"" pos:end=""7:16""><decl pos:start=""7:3"" pos:end=""7:15""><type pos:start=""7:3"" pos:end=""7:5""><name pos:start=""7:3"" pos:end=""7:5"">int</name></type> <name pos:start=""7:7"" pos:end=""7:9"">foo</name> <init pos:start=""7:11"" pos:end=""7:15"">= <expr pos:start=""7:13"" pos:end=""7:15""><literal type=""number"" pos:start=""7:13"" pos:end=""7:15"">101</literal></expr></init></decl>;</decl_stmt>
</block_content>}</block></finally></try>";

            var xmlElement = fileSetup[lang].GetFileUnitForXmlSnippet(xml, "test.code");

            var globalScope = codeParsers[lang].ParseFileUnit(xmlElement);
            Assert.AreEqual(2, globalScope.ChildStatements.Count);

            var tryStatement = globalScope.GetDescendants<TryStatement>().First();
            Assert.AreEqual(2, tryStatement.ChildStatements.Count);
            var tryFoo = tryStatement.ChildStatements[0].Content.GetDescendantsAndSelf<VariableDeclaration>().First(v => v.Name == "foo");
            var tryFooUse = tryStatement.ChildStatements[1].Content.GetDescendantsAndSelf<NameUse>().First(n => n.Name == "foo");

            Assert.AreEqual(2, tryStatement.FinallyStatements.Count);
            var finallyFooUse = tryStatement.FinallyStatements[0].Content.GetDescendantsAndSelf<NameUse>().First(n => n.Name == "foo");
            var finallyFoo = tryStatement.FinallyStatements[1].Content.GetDescendantsAndSelf<VariableDeclaration>().First(v => v.Name == "foo");

            var allChildren = tryStatement.GetNamedChildren("foo").ToList();
            Assert.AreEqual(2, allChildren.Count);
            Assert.AreSame(tryFoo, allChildren[0]);
            Assert.AreSame(finallyFoo, allChildren[1]);

            var tryMatches = tryStatement.GetNamedChildren(tryFooUse).ToList();
            Assert.AreEqual(1, tryMatches.Count);
            Assert.AreSame(tryFoo, tryMatches[0]);

            Assert.IsEmpty(tryStatement.GetNamedChildren(finallyFooUse).ToList());
        }
    }
}
