using ABB.SrcML.Test.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ABB.SrcML.Data.Test {

    [TestFixture]
    [Category("Build")]
    public class GetStatementTests {
        private Dictionary<Language, SrcMLFileUnitSetup> fileSetup;
        private Dictionary<Language, AbstractCodeParser> parser;

        [TestFixtureSetUp]
        public void ClassSetup() {
            parser = new Dictionary<Language, AbstractCodeParser>
                      {
                          {Language.CSharp, new CSharpCodeParser()},
                          {Language.CPlusPlus, new CPlusPlusCodeParser()}
                      };
            fileSetup = new Dictionary<Language, SrcMLFileUnitSetup>
                        {
                            {Language.CSharp, new SrcMLFileUnitSetup(Language.CSharp)},
                            {Language.CPlusPlus, new SrcMLFileUnitSetup(Language.CPlusPlus)}
                        };
        }

        [Test]
        public void TestGlobalStatement() {
            ////Example.cpp
            //char* bar = "Hello, world!";
            //int foo = 42;
            /*var xml = @"<decl_stmt><decl><type><name pos:line=""1"" pos:column=""1"">char</name><type:modifier pos:line=""1"" pos:column=""5"">*</type:modifier></type> <name pos:line=""1"" pos:column=""7"">bar</name> =<init pos:line=""1"" pos:column=""12""> <expr><literal type=""string"" pos:line=""1"" pos:column=""13"">""Hello, world!""</literal></expr></init></decl>;</decl_stmt>
<decl_stmt><decl><type><name pos:line=""2"" pos:column=""1"">int</name></type> <name pos:line=""2"" pos:column=""5"">foo</name> =<init pos:line=""2"" pos:column=""10""> <expr><literal type=""number"" pos:line=""2"" pos:column=""11"">42</literal></expr></init></decl>;</decl_stmt>";*/
            var xml = @"<decl_stmt pos:start=""1:1"" pos:end=""1:28""><decl pos:start=""1:1"" pos:end=""1:27""><type pos:start=""1:1"" pos:end=""1:5""><name pos:start=""1:1"" pos:end=""1:4"">char</name><modifier pos:start=""1:5"" pos:end=""1:5"">*</modifier></type> <name pos:start=""1:7"" pos:end=""1:9"">bar</name> <init pos:start=""1:11"" pos:end=""1:27"">= <expr pos:start=""1:13"" pos:end=""1:27""><literal type=""string"" pos:start=""1:13"" pos:end=""1:27"">""Hello, world!""</literal></expr></init></decl>;</decl_stmt>
<decl_stmt pos:start=""2:1"" pos:end=""2:13""><decl pos:start=""2:1"" pos:end=""2:12""><type pos:start=""2:1"" pos:end=""2:3""><name pos:start=""2:1"" pos:end=""2:3"">int</name></type> <name pos:start=""2:5"" pos:end=""2:7"">foo</name> <init pos:start=""2:9"" pos:end=""2:12"">= <expr pos:start=""2:11"" pos:end=""2:12""><literal type=""number"" pos:start=""2:11"" pos:end=""2:12"">42</literal></expr></init></decl>;</decl_stmt>";
            
            var unit = fileSetup[Language.CPlusPlus].GetFileUnitForXmlSnippet(xml, "Example.cpp");
            var globalScope = parser[Language.CPlusPlus].ParseFileUnit(unit);
            var actual = globalScope.GetStatementForLocation(new SourceLocation("Example.cpp", 2, 5));
            Assert.AreSame(globalScope.ChildStatements[1], actual);
        }

        [Test]
        public void TestLocationInClass_CSharp() {
            ////Foo.cs
            //namespace Example {
            //    class Foo {
            //        int bar = 42;
            //    }
            //}
            /*var xml = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name pos:line=""1"" pos:column=""11"">Example</name> <block pos:line=""1"" pos:column=""19"">{
    <class pos:line=""2"" pos:column=""5"">class <name pos:line=""2"" pos:column=""11"">Foo</name> <block pos:line=""2"" pos:column=""15"">{
        <decl_stmt><decl><type><name pos:line=""3"" pos:column=""9"">int</name></type> <name pos:line=""3"" pos:column=""13"">bar</name> =<init pos:line=""3"" pos:column=""18""> <expr><literal type=""number"" pos:line=""3"" pos:column=""19"">42</literal></expr></init></decl>;</decl_stmt>
    }</block></class>
}</block></namespace>";*/
            var xml = @"<namespace pos:start=""1:1"" pos:end=""5:1"">namespace <name pos:start=""1:11"" pos:end=""1:17"">Example</name> <block pos:start=""1:19"" pos:end=""5:1"">{
    <class pos:start=""2:5"" pos:end=""4:5"">class <name pos:start=""2:11"" pos:end=""2:13"">Foo</name> <block pos:start=""2:15"" pos:end=""4:5"">{
        <decl_stmt pos:start=""3:9"" pos:end=""3:21""><decl pos:start=""3:9"" pos:end=""3:20""><type pos:start=""3:9"" pos:end=""3:11""><name pos:start=""3:9"" pos:end=""3:11"">int</name></type> <name pos:start=""3:13"" pos:end=""3:15"">bar</name> <init pos:start=""3:17"" pos:end=""3:20"">= <expr pos:start=""3:19"" pos:end=""3:20""><literal type=""number"" pos:start=""3:19"" pos:end=""3:20"">42</literal></expr></init></decl>;</decl_stmt>
    }</block></class>
}</block></namespace>";

            var unit = fileSetup[Language.CSharp].GetFileUnitForXmlSnippet(xml, "Foo.cs");
            var globalScope = parser[Language.CSharp].ParseFileUnit(unit);

            var foo = globalScope.GetDescendants<TypeDefinition>().First(t => t.Name == "Foo");
            var stmt = foo.ChildStatements[0];
            var stmtActual = globalScope.GetStatementForLocation(new SourceLocation("Foo.cs", 3, 14));
            Assert.AreSame(stmt, stmtActual);

            var fooActual = globalScope.GetStatementForLocation(new SourceLocation("Foo.cs", 2, 6));
            Assert.AreSame(foo, fooActual);
        }

        [Test]
        public void TestLocationInMain_Cpp() {
            //#include <iostream>
            //
            //char* MyFunction(int foo) {
            //    if(foo > 0) {
            //        return "Hello world!";
            //    } else {
            //        return "Goodbye cruel world!";
            //    }
            //}
            //
            //int main(int argc, char* argv[]) {
            //    std::cout<<MyFunction(42);
            //    return 0;
            //}
            /*var xml = @"<cpp:include pos:line=""1"" pos:column=""1"">#<cpp:directive pos:line=""1"" pos:column=""2"">include</cpp:directive> <cpp:file pos:line=""1"" pos:column=""10"">&lt;iostream&gt;</cpp:file></cpp:include>

<function><type><name pos:line=""3"" pos:column=""1"">char</name><type:modifier pos:line=""3"" pos:column=""5"">*</type:modifier></type> <name pos:line=""3"" pos:column=""7"">MyFunction</name><parameter_list pos:line=""3"" pos:column=""17"">(<parameter><decl><type><name pos:line=""3"" pos:column=""18"">int</name></type> <name pos:line=""3"" pos:column=""22"">foo</name></decl></parameter>)</parameter_list> <block pos:line=""3"" pos:column=""27"">{
    <if pos:line=""4"" pos:column=""5"">if<condition pos:line=""4"" pos:column=""7"">(<expr><name pos:line=""4"" pos:column=""8"">foo</name> <operator pos:line=""4"" pos:column=""12"">&gt;</operator> <literal type=""number"" pos:line=""4"" pos:column=""14"">0</literal></expr>)</condition><then pos:line=""4"" pos:column=""16""> <block pos:line=""4"" pos:column=""17"">{
        <return pos:line=""5"" pos:column=""9"">return <expr><literal type=""string"" pos:line=""5"" pos:column=""16"">""Hello world!""</literal></expr>;</return>
    }</block></then> <else pos:line=""6"" pos:column=""7"">else <block pos:line=""6"" pos:column=""12"">{
        <return pos:line=""7"" pos:column=""9"">return <expr><literal type=""string"" pos:line=""7"" pos:column=""16"">""Goodbye cruel world!""</literal></expr>;</return>
    }</block></else></if>
}</block></function>

<function><type><name pos:line=""11"" pos:column=""1"">int</name></type> <name pos:line=""11"" pos:column=""5"">main</name><parameter_list pos:line=""11"" pos:column=""9"">(<parameter><decl><type><name pos:line=""11"" pos:column=""10"">int</name></type> <name pos:line=""11"" pos:column=""14"">argc</name></decl></parameter>, <parameter><decl><type><name pos:line=""11"" pos:column=""20"">char</name><type:modifier pos:line=""11"" pos:column=""24"">*</type:modifier></type> <name><name pos:line=""11"" pos:column=""26"">argv</name><index pos:line=""11"" pos:column=""30"">[]</index></name></decl></parameter>)</parameter_list> <block pos:line=""11"" pos:column=""34"">{
    <expr_stmt><expr><name><name pos:line=""12"" pos:column=""5"">std</name><operator pos:line=""12"" pos:column=""8"">::</operator><name pos:line=""12"" pos:column=""10"">cout</name></name><operator pos:line=""12"" pos:column=""14"">&lt;&lt;</operator><call><name pos:line=""12"" pos:column=""16"">MyFunction</name><argument_list pos:line=""12"" pos:column=""26"">(<argument><expr><literal type=""number"" pos:line=""12"" pos:column=""27"">42</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt>
    <return pos:line=""13"" pos:column=""5"">return <expr><literal type=""number"" pos:line=""13"" pos:column=""12"">0</literal></expr>;</return>
}</block></function>";*/

            var xml = @"<cpp:include pos:start=""1:1"" pos:end=""1:19"">#<cpp:directive pos:start=""1:2"" pos:end=""1:8"">include</cpp:directive> <cpp:file pos:start=""1:10"" pos:end=""1:19"">&lt;iostream&gt;</cpp:file></cpp:include>

<function pos:start=""3:1"" pos:end=""9:1""><type pos:start=""3:1"" pos:end=""3:5""><name pos:start=""3:1"" pos:end=""3:4"">char</name><modifier pos:start=""3:5"" pos:end=""3:5"">*</modifier></type> <name pos:start=""3:7"" pos:end=""3:16"">MyFunction</name><parameter_list pos:start=""3:17"" pos:end=""3:25"">(<parameter pos:start=""3:18"" pos:end=""3:24""><decl pos:start=""3:18"" pos:end=""3:24""><type pos:start=""3:18"" pos:end=""3:20""><name pos:start=""3:18"" pos:end=""3:20"">int</name></type> <name pos:start=""3:22"" pos:end=""3:24"">foo</name></decl></parameter>)</parameter_list> <block pos:start=""3:27"" pos:end=""9:1"">{<block_content pos:start=""4:5"" pos:end=""8:5"">
    <if_stmt pos:start=""4:5"" pos:end=""8:5""><if pos:start=""4:5"" pos:end=""6:5"">if<condition pos:start=""4:7"" pos:end=""4:15"">(<expr pos:start=""4:8"" pos:end=""4:14""><name pos:start=""4:8"" pos:end=""4:10"">foo</name> <operator pos:start=""4:12"" pos:end=""4:12"">&gt;</operator> <literal type=""number"" pos:start=""4:14"" pos:end=""4:14"">0</literal></expr>)</condition> <block pos:start=""4:17"" pos:end=""6:5"">{<block_content pos:start=""5:9"" pos:end=""5:30"">
        <return pos:start=""5:9"" pos:end=""5:30"">return <expr pos:start=""5:16"" pos:end=""5:29""><literal type=""string"" pos:start=""5:16"" pos:end=""5:29"">""Hello world!""</literal></expr>;</return>
    </block_content>}</block></if> <else pos:start=""6:7"" pos:end=""8:5"">else <block pos:start=""6:12"" pos:end=""8:5"">{<block_content pos:start=""7:9"" pos:end=""7:38"">
        <return pos:start=""7:9"" pos:end=""7:38"">return <expr pos:start=""7:16"" pos:end=""7:37""><literal type=""string"" pos:start=""7:16"" pos:end=""7:37"">""Goodbye cruel world!""</literal></expr>;</return>
    </block_content>}</block></else></if_stmt>
</block_content>}</block></function>

<function pos:start=""11:1"" pos:end=""14:1""><type pos:start=""11:1"" pos:end=""11:3""><name pos:start=""11:1"" pos:end=""11:3"">int</name></type> <name pos:start=""11:5"" pos:end=""11:8"">main</name><parameter_list pos:start=""11:9"" pos:end=""11:32"">(<parameter pos:start=""11:10"" pos:end=""11:17""><decl pos:start=""11:10"" pos:end=""11:17""><type pos:start=""11:10"" pos:end=""11:17""><name pos:start=""11:10"" pos:end=""11:12"">int</name></type> <name pos:start=""11:14"" pos:end=""11:17"">argc</name></decl></parameter>, <parameter pos:start=""11:20"" pos:end=""11:31""><decl pos:start=""11:20"" pos:end=""11:31""><type pos:start=""11:20"" pos:end=""11:29""><name pos:start=""11:20"" pos:end=""11:23"">char</name><modifier pos:start=""11:24"" pos:end=""11:24"">*</modifier></type> <name pos:start=""11:26"" pos:end=""11:31""><name pos:start=""11:26"" pos:end=""11:29"">argv</name><index pos:start=""11:30"" pos:end=""11:31"">[]</index></name></decl></parameter>)</parameter_list> <block pos:start=""11:34"" pos:end=""14:1"">{<block_content pos:start=""12:5"" pos:end=""13:13"">
    <expr_stmt pos:start=""12:5"" pos:end=""12:30""><expr pos:start=""12:5"" pos:end=""12:29""><name pos:start=""12:5"" pos:end=""12:13""><name pos:start=""12:5"" pos:end=""12:7"">std</name><operator pos:start=""12:8"" pos:end=""12:9"">::</operator><name pos:start=""12:10"" pos:end=""12:13"">cout</name></name><operator pos:start=""12:14"" pos:end=""12:15"">&lt;&lt;</operator><call pos:start=""12:16"" pos:end=""12:29""><name pos:start=""12:16"" pos:end=""12:25"">MyFunction</name><argument_list pos:start=""12:26"" pos:end=""12:29"">(<argument pos:start=""12:27"" pos:end=""12:28""><expr pos:start=""12:27"" pos:end=""12:28""><literal type=""number"" pos:start=""12:27"" pos:end=""12:28"">42</literal></expr></argument>)</argument_list></call></expr>;</expr_stmt>
    <return pos:start=""13:5"" pos:end=""13:13"">return <expr pos:start=""13:12"" pos:end=""13:12""><literal type=""number"" pos:start=""13:12"" pos:end=""13:12"">0</literal></expr>;</return>
</block_content>}</block></function>";

            var fileUnit = fileSetup[Language.CPlusPlus].GetFileUnitForXmlSnippet(xml, "function_def.cpp");
            var globalScope = parser[Language.CPlusPlus].ParseFileUnit(fileUnit);

            var main = globalScope.GetDescendants<MethodDefinition>().First(md => md.Name == "main");
            Assert.AreSame(main.ChildStatements[0], globalScope.GetStatementForLocation(new SourceLocation("function_def.cpp", 12, 20)));
        }

        [Test]
        public void TestLocationInMethodDefinition_Cpp() {
            ////Foo.h
            //class Foo {
            //public:
            //    int bar(int);
            //}
            /*var hXml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">Foo</name> <block pos:line=""1"" pos:column=""11"">{<private type=""default"" pos:line=""1"" pos:column=""12"">
</private><public pos:line=""2"" pos:column=""1"">public:
    <function_decl><type><name pos:line=""3"" pos:column=""5"">int</name></type> <name pos:line=""3"" pos:column=""9"">bar</name><parameter_list pos:line=""3"" pos:column=""12"">(<parameter><decl><type><name pos:line=""3"" pos:column=""13"">int</name></type></decl></parameter>)</parameter_list>;</function_decl>
</public>}</block><decl/></class>";*/

            var hXml = @"<class pos:start=""1:1"" pos:end=""4:1"">class <name pos:start=""1:7"" pos:end=""1:9"">Foo</name> <block pos:start=""1:11"" pos:end=""4:1"">{<private type=""default"" pos:start=""2:1"" pos:end=""2:6"">
</private><public pos:start=""2:1"" pos:end=""3:17"">public:
    <function_decl pos:start=""3:5"" pos:end=""3:17""><type pos:start=""3:5"" pos:end=""3:7""><name pos:start=""3:5"" pos:end=""3:7"">int</name></type> <name pos:start=""3:9"" pos:end=""3:11"">bar</name><parameter_list pos:start=""3:12"" pos:end=""3:16"">(<parameter pos:start=""3:13"" pos:end=""3:15""><decl pos:start=""3:13"" pos:end=""3:15""><type pos:start=""3:13"" pos:end=""3:15""><name pos:start=""3:13"" pos:end=""3:15"">int</name></type></decl></parameter>)</parameter_list>;</function_decl>
</public>}</block><decl/></class>";

            var hUnit = fileSetup[Language.CPlusPlus].GetFileUnitForXmlSnippet(hXml, "Foo.h");
            var globalScope = parser[Language.CPlusPlus].ParseFileUnit(hUnit);
            ////Foo.cpp
            //#include "Foo.h"
            //int Foo::bar(int baz) {
            //    return baz + 1;
            //}
            /*var cppXml = @"<cpp:include pos:line=""1"" pos:column=""1"">#<cpp:directive pos:line=""1"" pos:column=""2"">include</cpp:directive> <cpp:file><literal type=""string"" pos:line=""1"" pos:column=""10"">""Foo.h""</literal></cpp:file></cpp:include>
<function><type><name pos:line=""2"" pos:column=""1"">int</name></type> <name><name pos:line=""2"" pos:column=""5"">Foo</name><operator pos:line=""2"" pos:column=""8"">::</operator><name pos:line=""2"" pos:column=""10"">bar</name></name><parameter_list pos:line=""2"" pos:column=""13"">(<parameter><decl><type><name pos:line=""2"" pos:column=""14"">int</name></type> <name pos:line=""2"" pos:column=""18"">baz</name></decl></parameter>)</parameter_list> <block pos:line=""2"" pos:column=""23"">{
    <return pos:line=""3"" pos:column=""5"">return <expr><name pos:line=""3"" pos:column=""12"">baz</name> <operator pos:line=""3"" pos:column=""16"">+</operator> <literal type=""number"" pos:line=""3"" pos:column=""18"">1</literal></expr>;</return>
}</block></function>";*/
            var cppXml = @"<cpp:include pos:start=""1:1"" pos:end=""1:16"">#<cpp:directive pos:start=""1:2"" pos:end=""1:8"">include</cpp:directive> <cpp:file pos:start=""1:10"" pos:end=""1:16"">""Foo.h""</cpp:file></cpp:include>
<function pos:start=""2:1"" pos:end=""4:1""><type pos:start=""2:1"" pos:end=""2:3""><name pos:start=""2:1"" pos:end=""2:3"">int</name></type> <name pos:start=""2:5"" pos:end=""2:12""><name pos:start=""2:5"" pos:end=""2:7"">Foo</name><operator pos:start=""2:8"" pos:end=""2:9"">::</operator><name pos:start=""2:10"" pos:end=""2:12"">bar</name></name><parameter_list pos:start=""2:13"" pos:end=""2:21"">(<parameter pos:start=""2:14"" pos:end=""2:20""><decl pos:start=""2:14"" pos:end=""2:20""><type pos:start=""2:14"" pos:end=""2:16""><name pos:start=""2:14"" pos:end=""2:16"">int</name></type> <name pos:start=""2:18"" pos:end=""2:20"">baz</name></decl></parameter>)</parameter_list> <block pos:start=""2:23"" pos:end=""4:1"">{<block_content pos:start=""3:5"" pos:end=""3:19"">
    <return pos:start=""3:5"" pos:end=""3:19"">return <expr pos:start=""3:12"" pos:end=""3:18""><name pos:start=""3:12"" pos:end=""3:14"">baz</name> <operator pos:start=""3:16"" pos:end=""3:16"">+</operator> <literal type=""number"" pos:start=""3:18"" pos:end=""3:18"">1</literal></expr>;</return>
</block_content>}</block></function>";

            var cppUnit = fileSetup[Language.CPlusPlus].GetFileUnitForXmlSnippet(cppXml, "Foo.cpp");
            globalScope = globalScope.Merge(parser[Language.CPlusPlus].ParseFileUnit(cppUnit));

            var bar = globalScope.GetDescendants<MethodDefinition>().First(md => md.Name == "bar");
            Assert.AreEqual(1, bar.ChildStatements.Count);
            Assert.AreEqual(bar.ChildStatements[0], globalScope.GetStatementForLocation(new SourceLocation("Foo.cpp", 3, 8)));
        }

        [Test]
        public void TestLocationInForLoop() {
            //for(int i = 0; i < foo.Count; i++) {
            //    Bar(i);
            //}
            /*var xml = @"<for pos:line=""1"" pos:column=""1"">for(<init><decl><type><name pos:line=""1"" pos:column=""5"">int</name></type> <name pos:line=""1"" pos:column=""9"">i</name> <init pos:line=""1"" pos:column=""11"">= <expr><literal type=""number"" pos:line=""1"" pos:column=""13"">0</literal></expr></init></decl>;</init> <condition><expr><name pos:line=""1"" pos:column=""16"">i</name> <operator pos:line=""1"" pos:column=""18"">&lt;</operator> <name><name pos:line=""1"" pos:column=""20"">foo</name><operator pos:line=""1"" pos:column=""23"">.</operator><name pos:line=""1"" pos:column=""24"">Count</name></name></expr>;</condition> <incr><expr><name pos:line=""1"" pos:column=""31"">i</name><operator pos:line=""1"" pos:column=""32"">++</operator></expr></incr>) <block pos:line=""1"" pos:column=""36"">{
    <expr_stmt><expr><call><name pos:line=""2"" pos:column=""5"">Bar</name><argument_list pos:line=""2"" pos:column=""8"">(<argument><expr><name pos:line=""2"" pos:column=""9"">i</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
}</block></for>";*/
            var xml = @"<for pos:start=""1:1"" pos:end=""3:1"">for<control pos:start=""1:4"" pos:end=""1:34"">(<init pos:start=""1:5"" pos:end=""1:14""><decl pos:start=""1:5"" pos:end=""1:13""><type pos:start=""1:5"" pos:end=""1:7""><name pos:start=""1:5"" pos:end=""1:7"">int</name></type> <name pos:start=""1:9"" pos:end=""1:9"">i</name> <init pos:start=""1:11"" pos:end=""1:13"">= <expr pos:start=""1:13"" pos:end=""1:13""><literal type=""number"" pos:start=""1:13"" pos:end=""1:13"">0</literal></expr></init></decl>;</init> <condition pos:start=""1:16"" pos:end=""1:29""><expr pos:start=""1:16"" pos:end=""1:28""><name pos:start=""1:16"" pos:end=""1:16"">i</name> <operator pos:start=""1:18"" pos:end=""1:18"">&lt;</operator> <name pos:start=""1:20"" pos:end=""1:28""><name pos:start=""1:20"" pos:end=""1:22"">foo</name><operator pos:start=""1:23"" pos:end=""1:23"">.</operator><name pos:start=""1:24"" pos:end=""1:28"">Count</name></name></expr>;</condition> <incr pos:start=""1:31"" pos:end=""1:33""><expr pos:start=""1:31"" pos:end=""1:33""><name pos:start=""1:31"" pos:end=""1:31"">i</name><operator pos:start=""1:32"" pos:end=""1:33"">++</operator></expr></incr>)</control> <block pos:start=""1:36"" pos:end=""3:1"">{<block_content pos:start=""2:5"" pos:end=""2:11"">
    <expr_stmt pos:start=""2:5"" pos:end=""2:11""><expr pos:start=""2:5"" pos:end=""2:10""><call pos:start=""2:5"" pos:end=""2:10""><name pos:start=""2:5"" pos:end=""2:7"">Bar</name><argument_list pos:start=""2:8"" pos:end=""2:10"">(<argument pos:start=""2:9"" pos:end=""2:9""><expr pos:start=""2:9"" pos:end=""2:9""><name pos:start=""2:9"" pos:end=""2:9"">i</name></expr></argument>)</argument_list></call></expr>;</expr_stmt>
</block_content>}</block></for>";

            var xmlElement = fileSetup[Language.CSharp].GetFileUnitForXmlSnippet(xml, "Foo.cs");
            var globalScope = parser[Language.CSharp].ParseFileUnit(xmlElement);

            var forLoop = globalScope.GetDescendants<ForStatement>().First();
            Assert.AreSame(forLoop, globalScope.GetStatementForLocation(new SourceLocation("Foo.cs", 1, 12)));
        }
    }
}