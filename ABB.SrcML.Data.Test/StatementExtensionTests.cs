/******************************************************************************
 * Copyright (c) 2013 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Vinay Augustine (ABB Group) - initial API, implementation, & documentation
 *****************************************************************************/

using ABB.SrcML.Test.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ABB.SrcML.Data.Test {

    [TestFixture]
    [Category("Build")]
    public class StatementExtensionTests {
        private Dictionary<Language, AbstractCodeParser> CodeParser;
        private Dictionary<Language, SrcMLFileUnitSetup> FileUnitSetup;

        [TestFixtureSetUp]
        public void ClassSetup() {
            FileUnitSetup = new Dictionary<Language, SrcMLFileUnitSetup>() {
                { Language.CPlusPlus, new SrcMLFileUnitSetup(Language.CPlusPlus) },
                { Language.CSharp, new SrcMLFileUnitSetup(Language.CSharp) },
                { Language.Java, new SrcMLFileUnitSetup(Language.Java) },
            };
            CodeParser = new Dictionary<Language, AbstractCodeParser>() {
                { Language.CPlusPlus, new CPlusPlusCodeParser() },
                { Language.CSharp, new CSharpCodeParser() },
                { Language.Java, new JavaCodeParser() },
            };
        }

        [Test]
        public void TestGetCallsTo_Simple() {
            //void foo() {
            //  printf("Hello");
            //}
            //
            //int main() {
            //  foo();
            //  return 0;
            //}
            /*string xml = @"<function><type><name pos:line=""1"" pos:column=""1"">void</name></type> <name pos:line=""1"" pos:column=""6"">foo</name><parameter_list pos:line=""1"" pos:column=""9"">()</parameter_list> <block pos:line=""1"" pos:column=""12"">{
  <expr_stmt><expr><call><name pos:line=""2"" pos:column=""3"">printf</name><argument_list pos:line=""2"" pos:column=""9"">(<argument><expr><literal type=""string"" pos:line=""2"" pos:column=""10"">""Hello""</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt>
}</block></function>

<function><type><name pos:line=""5"" pos:column=""1"">int</name></type> <name pos:line=""5"" pos:column=""5"">main</name><parameter_list pos:line=""5"" pos:column=""9"">()</parameter_list> <block pos:line=""5"" pos:column=""12"">{
  <expr_stmt><expr><call><name pos:line=""6"" pos:column=""3"">foo</name><argument_list pos:line=""6"" pos:column=""6"">()</argument_list></call></expr>;</expr_stmt>
  <return pos:line=""7"" pos:column=""3"">return <expr><literal type=""number"" pos:line=""7"" pos:column=""10"">0</literal></expr>;</return>
}</block></function>";*/
            string xml = @"<function pos:start=""1:1"" pos:end=""3:1""><type pos:start=""1:1"" pos:end=""1:4""><name pos:start=""1:1"" pos:end=""1:4"">void</name></type> <name pos:start=""1:6"" pos:end=""1:8"">foo</name><parameter_list pos:start=""1:9"" pos:end=""1:10"">()</parameter_list> <block pos:start=""1:12"" pos:end=""3:1"">{<block_content pos:start=""2:3"" pos:end=""2:18"">
  <expr_stmt pos:start=""2:3"" pos:end=""2:18""><expr pos:start=""2:3"" pos:end=""2:17""><call pos:start=""2:3"" pos:end=""2:17""><name pos:start=""2:3"" pos:end=""2:8"">printf</name><argument_list pos:start=""2:9"" pos:end=""2:17"">(<argument pos:start=""2:10"" pos:end=""2:16""><expr pos:start=""2:10"" pos:end=""2:16""><literal type=""string"" pos:start=""2:10"" pos:end=""2:16"">""Hello""</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt>
</block_content>}</block></function>

<function pos:start=""5:1"" pos:end=""8:1""><type pos:start=""5:1"" pos:end=""5:3""><name pos:start=""5:1"" pos:end=""5:3"">int</name></type> <name pos:start=""5:5"" pos:end=""5:8"">main</name><parameter_list pos:start=""5:9"" pos:end=""5:10"">()</parameter_list> <block pos:start=""5:12"" pos:end=""8:1"">{<block_content pos:start=""6:3"" pos:end=""7:11"">
  <expr_stmt pos:start=""6:3"" pos:end=""6:8""><expr pos:start=""6:3"" pos:end=""6:7""><call pos:start=""6:3"" pos:end=""6:7""><name pos:start=""6:3"" pos:end=""6:5"">foo</name><argument_list pos:start=""6:6"" pos:end=""6:7"">()</argument_list></call></expr>;</expr_stmt>
  <return pos:start=""7:3"" pos:end=""7:11"">return <expr pos:start=""7:10"" pos:end=""7:10""><literal type=""number"" pos:start=""7:10"" pos:end=""7:10"">0</literal></expr>;</return>
</block_content>}</block></function>";

            var xmlElement = FileUnitSetup[Language.CPlusPlus].GetFileUnitForXmlSnippet(xml, "foo.cpp");

            var globalScope = CodeParser[Language.CPlusPlus].ParseFileUnit(xmlElement);
            var fooMethod = globalScope.GetNamedChildren<MethodDefinition>("foo").First();
            var mainMethod = globalScope.GetNamedChildren<MethodDefinition>("main").First();

            Assert.That(mainMethod.ContainsCallTo(fooMethod));
            var fooCalls = mainMethod.GetCallsTo(fooMethod, true).ToList();
            Assert.AreEqual(1, fooCalls.Count);
            var expectedFooCall = mainMethod.FindExpressions<MethodCall>(true).First(mc => mc.Name == "foo");
            Assert.AreSame(expectedFooCall, fooCalls[0]);

            var callsToFoo = fooMethod.GetCallsToSelf().ToList();
            Assert.AreEqual(1, callsToFoo.Count);
            Assert.AreSame(expectedFooCall, callsToFoo[0]);
        }

        [Test]
        public void TestGetCallsTo_Multiple() {
            //void star() { }
            //
            //void bar() { star(); }
            //
            //void foo() {
            //    bar();
            //    if(0) bar();
            //}
            /*string xml = @"<function><type><name pos:line=""1"" pos:column=""1"">void</name></type> <name pos:line=""1"" pos:column=""6"">star</name><parameter_list pos:line=""1"" pos:column=""10"">()</parameter_list> <block pos:line=""1"" pos:column=""13"">{ }</block></function>

<function><type><name pos:line=""3"" pos:column=""1"">void</name></type> <name pos:line=""3"" pos:column=""6"">bar</name><parameter_list pos:line=""3"" pos:column=""9"">()</parameter_list> <block pos:line=""3"" pos:column=""12"">{ <expr_stmt><expr><call><name pos:line=""3"" pos:column=""14"">star</name><argument_list pos:line=""3"" pos:column=""18"">()</argument_list></call></expr>;</expr_stmt> }</block></function>

<function><type><name pos:line=""5"" pos:column=""1"">void</name></type> <name pos:line=""5"" pos:column=""6"">foo</name><parameter_list pos:line=""5"" pos:column=""9"">()</parameter_list> <block pos:line=""5"" pos:column=""12"">{
    <expr_stmt><expr><call><name pos:line=""6"" pos:column=""5"">bar</name><argument_list pos:line=""6"" pos:column=""8"">()</argument_list></call></expr>;</expr_stmt>
    <if_stmt pos:line=""7"" pos:column=""5""><if pos:line=""7"" pos:column=""5"">if<condition pos:line=""7"" pos:column=""7"">(<expr><literal type=""number"" pos:line=""7"" pos:column=""8"">0</literal></expr>)</condition><then pos:line=""7"" pos:column=""10""> <expr_stmt><expr><call><name pos:line=""7"" pos:column=""11"">bar</name><argument_list pos:line=""7"" pos:column=""14"">()</argument_list></call></expr>;</expr_stmt></then></if></if_stmt>
}</block></function>";*/
            string xml = @"<function pos:start=""1:1"" pos:end=""1:15""><type pos:start=""1:1"" pos:end=""1:4""><name pos:start=""1:1"" pos:end=""1:4"">void</name></type> <name pos:start=""1:6"" pos:end=""1:9"">star</name><parameter_list pos:start=""1:10"" pos:end=""1:11"">()</parameter_list> <block pos:start=""1:13"" pos:end=""1:15"">{<block_content> </block_content>}</block></function>

<function pos:start=""3:1"" pos:end=""3:22""><type pos:start=""3:1"" pos:end=""3:4""><name pos:start=""3:1"" pos:end=""3:4"">void</name></type> <name pos:start=""3:6"" pos:end=""3:8"">bar</name><parameter_list pos:start=""3:9"" pos:end=""3:10"">()</parameter_list> <block pos:start=""3:12"" pos:end=""3:22"">{<block_content pos:start=""3:14"" pos:end=""3:20""> <expr_stmt pos:start=""3:14"" pos:end=""3:20""><expr pos:start=""3:14"" pos:end=""3:19""><call pos:start=""3:14"" pos:end=""3:19""><name pos:start=""3:14"" pos:end=""3:17"">star</name><argument_list pos:start=""3:18"" pos:end=""3:19"">()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>

<function pos:start=""5:1"" pos:end=""8:1""><type pos:start=""5:1"" pos:end=""5:4""><name pos:start=""5:1"" pos:end=""5:4"">void</name></type> <name pos:start=""5:6"" pos:end=""5:8"">foo</name><parameter_list pos:start=""5:9"" pos:end=""5:10"">()</parameter_list> <block pos:start=""5:12"" pos:end=""8:1"">{<block_content pos:start=""6:5"" pos:end=""7:16"">
    <expr_stmt pos:start=""6:5"" pos:end=""6:10""><expr pos:start=""6:5"" pos:end=""6:9""><call pos:start=""6:5"" pos:end=""6:9""><name pos:start=""6:5"" pos:end=""6:7"">bar</name><argument_list pos:start=""6:8"" pos:end=""6:9"">()</argument_list></call></expr>;</expr_stmt>
    <if_stmt pos:start=""7:5"" pos:end=""7:16""><if pos:start=""7:5"" pos:end=""7:16"">if<condition pos:start=""7:7"" pos:end=""7:9"">(<expr pos:start=""7:8"" pos:end=""7:8""><literal type=""number"" pos:start=""7:8"" pos:end=""7:8"">0</literal></expr>)</condition><block type=""pseudo"" pos:start=""7:11"" pos:end=""7:16""><block_content pos:start=""7:11"" pos:end=""7:16""> <expr_stmt pos:start=""7:11"" pos:end=""7:16""><expr pos:start=""7:11"" pos:end=""7:15""><call pos:start=""7:11"" pos:end=""7:15""><name pos:start=""7:11"" pos:end=""7:13"">bar</name><argument_list pos:start=""7:14"" pos:end=""7:15"">()</argument_list></call></expr>;</expr_stmt></block_content></block></if></if_stmt>
</block_content>}</block></function>";


            var unit = FileUnitSetup[Language.CPlusPlus].GetFileUnitForXmlSnippet(xml, "test.cpp");

            var globalScope = CodeParser[Language.CPlusPlus].ParseFileUnit(unit);

            var methodFoo = globalScope.GetDescendants<MethodDefinition>().First(md => md.Name == "foo");
            var methodBar = globalScope.GetDescendants<MethodDefinition>().First(md => md.Name == "bar");
            var methodStar = globalScope.GetDescendants<MethodDefinition>().First(md => md.Name == "star");

            Assert.That(methodFoo.ContainsCallTo(methodBar));
            Assert.AreEqual(2, methodFoo.GetCallsTo(methodBar, true).Count());

            Assert.That(methodBar.ContainsCallTo(methodStar));
            Assert.AreEqual(1, methodBar.GetCallsTo(methodStar, true).Count());

            Assert.IsFalse(methodFoo.ContainsCallTo(methodStar));
            Assert.IsFalse(methodBar.ContainsCallTo(methodFoo));
            Assert.IsFalse(methodStar.ContainsCallTo(methodFoo));
            Assert.IsFalse(methodStar.ContainsCallTo(methodBar));
        }

        [Test]
        public void TestGetCallsTo_Masking() {
            //void foo() { printf("Global foo"); }
            //
            //class Bar {
            //public:
            //  void foo() { printf("Bar::foo"); }
            //  void baz() { foo(); }
            //};
            /*var xml = @"<function><type><name pos:line=""1"" pos:column=""1"">void</name></type> <name pos:line=""1"" pos:column=""6"">foo</name><parameter_list pos:line=""1"" pos:column=""9"">()</parameter_list> <block pos:line=""1"" pos:column=""12"">{ <expr_stmt><expr><call><name pos:line=""1"" pos:column=""14"">printf</name><argument_list pos:line=""1"" pos:column=""20"">(<argument><expr><literal type=""string"" pos:line=""1"" pos:column=""21"">""Global foo""</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt> }</block></function>

<class pos:line=""3"" pos:column=""1"">class <name pos:line=""3"" pos:column=""7"">Bar</name> <block pos:line=""3"" pos:column=""11"">{<private type=""default"" pos:line=""3"" pos:column=""12"">
</private><public pos:line=""4"" pos:column=""1"">public:
  <function><type><name pos:line=""5"" pos:column=""3"">void</name></type> <name pos:line=""5"" pos:column=""8"">foo</name><parameter_list pos:line=""5"" pos:column=""11"">()</parameter_list> <block pos:line=""5"" pos:column=""14"">{ <expr_stmt><expr><call><name pos:line=""5"" pos:column=""16"">printf</name><argument_list pos:line=""5"" pos:column=""22"">(<argument><expr><literal type=""string"" pos:line=""5"" pos:column=""23"">""Bar::foo""</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt> }</block></function>
  <function><type><name pos:line=""6"" pos:column=""3"">void</name></type> <name pos:line=""6"" pos:column=""8"">baz</name><parameter_list pos:line=""6"" pos:column=""11"">()</parameter_list> <block pos:line=""6"" pos:column=""14"">{ <expr_stmt><expr><call><name pos:line=""6"" pos:column=""16"">foo</name><argument_list pos:line=""6"" pos:column=""19"">()</argument_list></call></expr>;</expr_stmt> }</block></function>
</public>}</block>;</class>";*/
            var xml = @"<function pos:start=""1:1"" pos:end=""1:36""><type pos:start=""1:1"" pos:end=""1:4""><name pos:start=""1:1"" pos:end=""1:4"">void</name></type> <name pos:start=""1:6"" pos:end=""1:8"">foo</name><parameter_list pos:start=""1:9"" pos:end=""1:10"">()</parameter_list> <block pos:start=""1:12"" pos:end=""1:36"">{<block_content pos:start=""1:14"" pos:end=""1:34""> <expr_stmt pos:start=""1:14"" pos:end=""1:34""><expr pos:start=""1:14"" pos:end=""1:33""><call pos:start=""1:14"" pos:end=""1:33""><name pos:start=""1:14"" pos:end=""1:19"">printf</name><argument_list pos:start=""1:20"" pos:end=""1:33"">(<argument pos:start=""1:21"" pos:end=""1:32""><expr pos:start=""1:21"" pos:end=""1:32""><literal type=""string"" pos:start=""1:21"" pos:end=""1:32"">""Global foo""</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>

<class pos:start=""3:1"" pos:end=""7:2"">class <name pos:start=""3:7"" pos:end=""3:9"">Bar</name> <block pos:start=""3:11"" pos:end=""7:1"">{<private type=""default"" pos:start=""4:1"" pos:end=""4:6"">
</private><public pos:start=""4:1"" pos:end=""6:23"">public:
  <function pos:start=""5:3"" pos:end=""5:36""><type pos:start=""5:3"" pos:end=""5:6""><name pos:start=""5:3"" pos:end=""5:6"">void</name></type> <name pos:start=""5:8"" pos:end=""5:10"">foo</name><parameter_list pos:start=""5:11"" pos:end=""5:12"">()</parameter_list> <block pos:start=""5:14"" pos:end=""5:36"">{<block_content pos:start=""5:16"" pos:end=""5:34""> <expr_stmt pos:start=""5:16"" pos:end=""5:34""><expr pos:start=""5:16"" pos:end=""5:33""><call pos:start=""5:16"" pos:end=""5:33""><name pos:start=""5:16"" pos:end=""5:21"">printf</name><argument_list pos:start=""5:22"" pos:end=""5:33"">(<argument pos:start=""5:23"" pos:end=""5:32""><expr pos:start=""5:23"" pos:end=""5:32""><literal type=""string"" pos:start=""5:23"" pos:end=""5:32"">""Bar::foo""</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>
  <function pos:start=""6:3"" pos:end=""6:23""><type pos:start=""6:3"" pos:end=""6:6""><name pos:start=""6:3"" pos:end=""6:6"">void</name></type> <name pos:start=""6:8"" pos:end=""6:10"">baz</name><parameter_list pos:start=""6:11"" pos:end=""6:12"">()</parameter_list> <block pos:start=""6:14"" pos:end=""6:23"">{<block_content pos:start=""6:16"" pos:end=""6:21""> <expr_stmt pos:start=""6:16"" pos:end=""6:21""><expr pos:start=""6:16"" pos:end=""6:20""><call pos:start=""6:16"" pos:end=""6:20""><name pos:start=""6:16"" pos:end=""6:18"">foo</name><argument_list pos:start=""6:19"" pos:end=""6:20"">()</argument_list></call></expr>;</expr_stmt> </block_content>}</block></function>
</public>}</block>;</class>";

            var xmlElement = FileUnitSetup[Language.CPlusPlus].GetFileUnitForXmlSnippet(xml, "Bar.cpp");

            var globalScope = CodeParser[Language.CPlusPlus].ParseFileUnit(xmlElement);
            var globalFooMethod = globalScope.GetNamedChildren<MethodDefinition>("foo").First();
            var bar = globalScope.GetNamedChildren<TypeDefinition>("Bar").First();
            var barFooMethod = bar.GetNamedChildren<MethodDefinition>("foo").First();
            var bazMethod = bar.GetNamedChildren<MethodDefinition>("baz").First();

            Assert.That(bazMethod.ContainsCallTo(barFooMethod));
            Assert.IsFalse(bazMethod.ContainsCallTo(globalFooMethod));
            var fooCalls = bazMethod.GetCallsTo(barFooMethod, true).ToList();
            Assert.AreEqual(1, fooCalls.Count);
            var expectedFooCall = bazMethod.FindExpressions<MethodCall>(true).First(mc => mc.Name == "foo");
            Assert.AreSame(expectedFooCall, fooCalls[0]);

            Assert.IsEmpty(globalFooMethod.GetCallsToSelf());
            Assert.AreEqual(1, barFooMethod.GetCallsToSelf().Count());
        }

        [Test]
        public void TestGetCallsTo_NonRecursive() {
            //int Qux() { return 42; }
            //int Xyzzy() { return 17; }
            //
            //void foo() {
            //  if(Qux()) {
            //    print(Xyzzy());
            //  }
            //}
            /*string xml = @"<function><type><name pos:line=""1"" pos:column=""1"">int</name></type> <name pos:line=""1"" pos:column=""5"">Qux</name><parameter_list pos:line=""1"" pos:column=""8"">()</parameter_list> <block pos:line=""1"" pos:column=""11"">{ <return pos:line=""1"" pos:column=""13"">return <expr><literal type=""number"" pos:line=""1"" pos:column=""20"">42</literal></expr>;</return> }</block></function>
<function><type><name pos:line=""2"" pos:column=""1"">int</name></type> <name pos:line=""2"" pos:column=""5"">Xyzzy</name><parameter_list pos:line=""2"" pos:column=""10"">()</parameter_list> <block pos:line=""2"" pos:column=""13"">{ <return pos:line=""2"" pos:column=""15"">return <expr><literal type=""number"" pos:line=""2"" pos:column=""22"">17</literal></expr>;</return> }</block></function>

<function><type><name pos:line=""4"" pos:column=""1"">void</name></type> <name pos:line=""4"" pos:column=""6"">foo</name><parameter_list pos:line=""4"" pos:column=""9"">()</parameter_list> <block pos:line=""4"" pos:column=""12"">{
  <if_stmt><if pos:line=""5"" pos:column=""3"">if<condition pos:line=""5"" pos:column=""5"">(<expr><call><name pos:line=""5"" pos:column=""6"">Qux</name><argument_list pos:line=""5"" pos:column=""9"">()</argument_list></call></expr>)</condition><then pos:line=""5"" pos:column=""12""> <block pos:line=""5"" pos:column=""13"">{
    <expr_stmt><expr><call><name pos:line=""6"" pos:column=""5"">print</name><argument_list pos:line=""6"" pos:column=""10"">(<argument><expr><call><name pos:line=""6"" pos:column=""11"">Xyzzy</name><argument_list pos:line=""6"" pos:column=""16"">()</argument_list></call></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  }</block></then></if></if_stmt>
}</block></function>";*/
            string xml = @"<function pos:start=""1:1"" pos:end=""1:24""><type pos:start=""1:1"" pos:end=""1:3""><name pos:start=""1:1"" pos:end=""1:3"">int</name></type> <name pos:start=""1:5"" pos:end=""1:7"">Qux</name><parameter_list pos:start=""1:8"" pos:end=""1:9"">()</parameter_list> <block pos:start=""1:11"" pos:end=""1:24"">{<block_content pos:start=""1:13"" pos:end=""1:22""> <return pos:start=""1:13"" pos:end=""1:22"">return <expr pos:start=""1:20"" pos:end=""1:21""><literal type=""number"" pos:start=""1:20"" pos:end=""1:21"">42</literal></expr>;</return> </block_content>}</block></function>
<function pos:start=""2:1"" pos:end=""2:26""><type pos:start=""2:1"" pos:end=""2:3""><name pos:start=""2:1"" pos:end=""2:3"">int</name></type> <name pos:start=""2:5"" pos:end=""2:9"">Xyzzy</name><parameter_list pos:start=""2:10"" pos:end=""2:11"">()</parameter_list> <block pos:start=""2:13"" pos:end=""2:26"">{<block_content pos:start=""2:15"" pos:end=""2:24""> <return pos:start=""2:15"" pos:end=""2:24"">return <expr pos:start=""2:22"" pos:end=""2:23""><literal type=""number"" pos:start=""2:22"" pos:end=""2:23"">17</literal></expr>;</return> </block_content>}</block></function>

<function pos:start=""4:1"" pos:end=""8:1""><type pos:start=""4:1"" pos:end=""4:4""><name pos:start=""4:1"" pos:end=""4:4"">void</name></type> <name pos:start=""4:6"" pos:end=""4:8"">foo</name><parameter_list pos:start=""4:9"" pos:end=""4:10"">()</parameter_list> <block pos:start=""4:12"" pos:end=""8:1"">{<block_content pos:start=""5:3"" pos:end=""7:3"">
  <if_stmt pos:start=""5:3"" pos:end=""7:3""><if pos:start=""5:3"" pos:end=""7:3"">if<condition pos:start=""5:5"" pos:end=""5:11"">(<expr pos:start=""5:6"" pos:end=""5:10""><call pos:start=""5:6"" pos:end=""5:10""><name pos:start=""5:6"" pos:end=""5:8"">Qux</name><argument_list pos:start=""5:9"" pos:end=""5:10"">()</argument_list></call></expr>)</condition> <block pos:start=""5:13"" pos:end=""7:3"">{<block_content pos:start=""6:5"" pos:end=""6:19"">
    <expr_stmt pos:start=""6:5"" pos:end=""6:19""><expr pos:start=""6:5"" pos:end=""6:18""><call pos:start=""6:5"" pos:end=""6:18""><name pos:start=""6:5"" pos:end=""6:9"">print</name><argument_list pos:start=""6:10"" pos:end=""6:18"">(<argument pos:start=""6:11"" pos:end=""6:17""><expr pos:start=""6:11"" pos:end=""6:17""><call pos:start=""6:11"" pos:end=""6:17""><name pos:start=""6:11"" pos:end=""6:15"">Xyzzy</name><argument_list pos:start=""6:16"" pos:end=""6:17"">()</argument_list></call></expr></argument>)</argument_list></call></expr>;</expr_stmt>
  </block_content>}</block></if></if_stmt>
</block_content>}</block></function>";

            var xmlElement = FileUnitSetup[Language.CPlusPlus].GetFileUnitForXmlSnippet(xml, "foo.cpp");

            var globalScope = CodeParser[Language.CPlusPlus].ParseFileUnit(xmlElement);
            var quxMethod = globalScope.GetNamedChildren<MethodDefinition>("Qux").First();
            var xyzzyMethod = globalScope.GetNamedChildren<MethodDefinition>("Xyzzy").First();
            var ifStmt = globalScope.GetDescendants<IfStatement>().First();

            Assert.That(ifStmt.ContainsCallTo(quxMethod));
            Assert.That(ifStmt.ContainsCallTo(xyzzyMethod));

            Assert.AreEqual(1, ifStmt.GetCallsTo(quxMethod, false).Count());
            Assert.AreEqual(0, ifStmt.GetCallsTo(xyzzyMethod, false).Count());
        }
    }
}