using ABB.SrcML.Test.Utilities;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace ABB.SrcML.Data.Test {

    [TestFixture]
    [Category("Build")]
    internal class SrcMLLocationTests {
        private SrcMLFileUnitSetup fileUnitSetup;

        [TestFixtureSetUp]
        public void SetUpFixture() {
            fileUnitSetup = new SrcMLFileUnitSetup(Language.CSharp);
        }

        [Test]
        public void TestContains_NoSibling() {
            ////Foo.cs
            //class Foo {
            //    int Bar(){return 0;}
            //}
            /*var xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">Foo</name> <block pos:line=""1"" pos:column=""11"">{
    <function><type><name pos:line=""2"" pos:column=""5"">int</name></type> <name pos:line=""2"" pos:column=""9"">Bar</name><parameter_list pos:line=""2"" pos:column=""12"">()</parameter_list><block pos:line=""2"" pos:column=""14"">{<return pos:line=""2"" pos:column=""15"">return <expr><literal type=""number"" pos:line=""2"" pos:column=""22"">0</literal></expr>;</return>}</block></function>
}</block></class>";*/
            var xml = @"<class pos:start=""1:1"" pos:end=""4:1"">class <name pos:start=""1:7"" pos:end=""1:9"">Foo</name> <block pos:start=""2:1"" pos:end=""4:1"">{
    <function pos:start=""3:5"" pos:end=""3:24""><type pos:start=""3:5"" pos:end=""3:7""><name pos:start=""3:5"" pos:end=""3:7"">int</name></type> <name pos:start=""3:9"" pos:end=""3:11"">Bar</name><parameter_list pos:start=""3:12"" pos:end=""3:13"">()</parameter_list><block pos:start=""3:14"" pos:end=""3:24"">{<block_content pos:start=""3:15"" pos:end=""3:23""><return pos:start=""3:15"" pos:end=""3:23"">return <expr pos:start=""3:22"" pos:end=""3:22""><literal type=""number"" pos:start=""3:22"" pos:end=""3:22"">0</literal></expr>;</return></block_content>}</block></function>
}</block></class>";

            var classElement = fileUnitSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs").Descendants(SRC.Class).First();
            var methodElement = classElement.Descendants(SRC.Function).First();
            var classLoc = new SrcMLLocation(classElement, "Foo.cs");
            var methodLoc = new SrcMLLocation(methodElement, "Foo.cs");
            Assert.IsTrue(classLoc.Contains(methodLoc));
        }

        [Test]
        public void TestContains_Reflexive() {
            ////Foo.cs
            //class Foo {
            //    int Bar(){return 0;}
            //}
            /*var xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">Foo</name> <block pos:line=""1"" pos:column=""11"">{
    <function><type><name pos:line=""2"" pos:column=""5"">int</name></type> <name pos:line=""2"" pos:column=""9"">Bar</name><parameter_list pos:line=""2"" pos:column=""12"">()</parameter_list><block pos:line=""2"" pos:column=""14"">{<return pos:line=""2"" pos:column=""15"">return <expr><literal type=""number"" pos:line=""2"" pos:column=""22"">0</literal></expr>;</return>}</block></function>
}</block></class>";*/
            var xml = @"<class pos:start=""1:1"" pos:end=""3:1"">class <name pos:start=""1:7"" pos:end=""1:9"">Foo</name> <block pos:start=""1:11"" pos:end=""3:1"">{
    <function pos:start=""2:5"" pos:end=""2:24""><type pos:start=""2:5"" pos:end=""2:7""><name pos:start=""2:5"" pos:end=""2:7"">int</name></type> <name pos:start=""2:9"" pos:end=""2:11"">Bar</name><parameter_list pos:start=""2:12"" pos:end=""2:13"">()</parameter_list><block pos:start=""2:14"" pos:end=""2:24"">{<block_content pos:start=""2:15"" pos:end=""2:23""><return pos:start=""2:15"" pos:end=""2:23"">return <expr pos:start=""2:22"" pos:end=""2:22""><literal type=""number"" pos:start=""2:22"" pos:end=""2:22"">0</literal></expr>;</return></block_content>}</block></function>
}</block></class>";
            var classElement = fileUnitSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs").Descendants(SRC.Class).First();
            var classLoc = new SrcMLLocation(classElement, "Foo.cs");
            Assert.IsTrue(classLoc.Contains(classLoc));
        }

        [Test]
        public void TestContains_Sibling() {
            ////Foo.cs
            //class Foo {
            //    string Bar(){
            //        string a = "Hello, world!";
            //        return a;
            //    }
            //    int Baz(){ return 0; }
            //}
            /*var xml = @"<class pos:line=""1"" pos:column=""1"">class <name pos:line=""1"" pos:column=""7"">Foo</name> <block pos:line=""1"" pos:column=""11"">{
    <function><type><name pos:line=""2"" pos:column=""5"">string</name></type> <name pos:line=""2"" pos:column=""12"">Bar</name><parameter_list pos:line=""2"" pos:column=""15"">()</parameter_list><block pos:line=""2"" pos:column=""17"">{
        <decl_stmt><decl><type><name pos:line=""3"" pos:column=""9"">string</name></type> <name pos:line=""3"" pos:column=""16"">a</name> =<init pos:line=""3"" pos:column=""19""> <expr><literal type=""string"" pos:line=""3"" pos:column=""20"">""Hello, world!""</literal></expr></init></decl>;</decl_stmt>
        <return pos:line=""4"" pos:column=""9"">return <expr><name pos:line=""4"" pos:column=""16"">a</name></expr>;</return>
    }</block></function>
    <function><type><name pos:line=""6"" pos:column=""5"">int</name></type> <name pos:line=""6"" pos:column=""9"">Baz</name><parameter_list pos:line=""6"" pos:column=""12"">()</parameter_list><block pos:line=""6"" pos:column=""14"">{ <return pos:line=""6"" pos:column=""16"">return <expr><literal type=""number"" pos:line=""6"" pos:column=""23"">0</literal></expr>;</return> }</block></function>
}</block></class>";*/
            var xml = @"<class pos:start=""1:1"" pos:end=""7:1"">class <name pos:start=""1:7"" pos:end=""1:9"">Foo</name> <block pos:start=""1:11"" pos:end=""7:1"">{
    <function pos:start=""2:5"" pos:end=""5:5""><type pos:start=""2:5"" pos:end=""2:10""><name pos:start=""2:5"" pos:end=""2:10"">string</name></type> <name pos:start=""2:12"" pos:end=""2:14"">Bar</name><parameter_list pos:start=""2:15"" pos:end=""2:16"">()</parameter_list><block pos:start=""2:17"" pos:end=""5:5"">{<block_content pos:start=""3:9"" pos:end=""4:17"">
        <decl_stmt pos:start=""3:9"" pos:end=""3:35""><decl pos:start=""3:9"" pos:end=""3:34""><type pos:start=""3:9"" pos:end=""3:14""><name pos:start=""3:9"" pos:end=""3:14"">string</name></type> <name pos:start=""3:16"" pos:end=""3:16"">a</name> <init pos:start=""3:18"" pos:end=""3:34"">= <expr pos:start=""3:20"" pos:end=""3:34""><literal type=""string"" pos:start=""3:20"" pos:end=""3:34"">""Hello, world!""</literal></expr></init></decl>;</decl_stmt>
        <return pos:start=""4:9"" pos:end=""4:17"">return <expr pos:start=""4:16"" pos:end=""4:16""><name pos:start=""4:16"" pos:end=""4:16"">a</name></expr>;</return>
    </block_content>}</block></function>
    <function pos:start=""6:5"" pos:end=""6:26""><type pos:start=""6:5"" pos:end=""6:7""><name pos:start=""6:5"" pos:end=""6:7"">int</name></type> <name pos:start=""6:9"" pos:end=""6:11"">Baz</name><parameter_list pos:start=""6:12"" pos:end=""6:13"">()</parameter_list><block pos:start=""6:14"" pos:end=""6:26"">{<block_content pos:start=""6:16"" pos:end=""6:24""> <return pos:start=""6:16"" pos:end=""6:24"">return <expr pos:start=""6:23"" pos:end=""6:23""><literal type=""number"" pos:start=""6:23"" pos:end=""6:23"">0</literal></expr>;</return> </block_content>}</block></function>
}</block></class>";

            var methodElement = fileUnitSetup.GetFileUnitForXmlSnippet(xml, "Foo.cs").Descendants(SRC.Function).First();
            var declElement = methodElement.Descendants(SRC.DeclarationStatement).First();
            var methodLoc = new SrcMLLocation(methodElement, "Foo.cs");
            var declLoc = new SrcMLLocation(declElement, "Foo.cs");
            Assert.IsTrue(methodLoc.Contains(declLoc));
        }

        [Test]
        public void TestContains_TwoLevel() {
            ////Example.cs
            //namespace Example {
            //    class Foo {
            //        int Bar(){return 0;}
            //    }
            //}
            /*var xml = @"<namespace pos:line=""1"" pos:column=""1"">namespace <name pos:line=""1"" pos:column=""11"">Example</name> <block pos:line=""1"" pos:column=""19"">{
    <class pos:line=""2"" pos:column=""5"">class <name pos:line=""2"" pos:column=""11"">Foo</name> <block pos:line=""2"" pos:column=""15"">{
        <function><type><name pos:line=""3"" pos:column=""9"">int</name></type> <name pos:line=""3"" pos:column=""13"">Bar</name><parameter_list pos:line=""3"" pos:column=""16"">()</parameter_list><block pos:line=""3"" pos:column=""18"">{<return pos:line=""3"" pos:column=""19"">return <expr><literal type=""number"" pos:line=""3"" pos:column=""26"">0</literal></expr>;</return>}</block></function>
    }</block></class>
}</block></namespace>";*/
            var xml = @"<namespace pos:start=""1:1"" pos:end=""5:1"">namespace <name pos:start=""1:11"" pos:end=""1:17"">Example</name> <block pos:start=""1:19"" pos:end=""5:1"">{
    <class pos:start=""2:5"" pos:end=""4:5"">class <name pos:start=""2:11"" pos:end=""2:13"">Foo</name> <block pos:start=""2:15"" pos:end=""4:5"">{
        <function pos:start=""3:9"" pos:end=""3:28""><type pos:start=""3:9"" pos:end=""3:11""><name pos:start=""3:9"" pos:end=""3:11"">int</name></type> <name pos:start=""3:13"" pos:end=""3:15"">Bar</name><parameter_list pos:start=""3:16"" pos:end=""3:17"">()</parameter_list><block pos:start=""3:18"" pos:end=""3:28"">{<block_content pos:start=""3:19"" pos:end=""3:27""><return pos:start=""3:19"" pos:end=""3:27"">return <expr pos:start=""3:26"" pos:end=""3:26""><literal type=""number"" pos:start=""3:26"" pos:end=""3:26"">0</literal></expr>;</return></block_content>}</block></function>
    }</block></class>
}</block></namespace>";

            var namespaceElement = fileUnitSetup.GetFileUnitForXmlSnippet(xml, "Example.cs").Element(SRC.Namespace);
            var methodElement = namespaceElement.Descendants(SRC.Function).First();
            var namespaceLoc = new SrcMLLocation(namespaceElement, "Example.cs");
            var methodLoc = new SrcMLLocation(methodElement, "Example.cs");
            Assert.IsTrue(namespaceLoc.Contains(methodLoc));
        }

        [Test]
        public void TestGetXElement() {
            var archive = new SrcMLArchive("SrcMLLocationTest", false, new SrcMLGenerator("SrcML"));
            var sourcePath = Path.GetFullPath(@"c:\SrcML\TestInputs\class_test.h");
            archive.AddOrUpdateFile(sourcePath);

            var unit = archive.GetXElementForSourceFile(sourcePath);
            Assert.IsNotNull(unit);
            var classElement = unit.Descendants(SRC.Class).FirstOrDefault();
            Assert.IsNotNull(classElement);
            
            var parser = new CPlusPlusCodeParser();
            var globalScope = parser.ParseFileUnit(unit);
            var typeDefinition = globalScope.ChildStatements.OfType<TypeDefinition>().FirstOrDefault();
            Assert.IsNotNull(typeDefinition);

            var element = typeDefinition.PrimaryLocation.GetXElement(archive);
            Assert.IsNotNull(element);
            Assert.AreEqual(classElement.GetSrcStartLineNumber(), element.GetSrcStartLineNumber());
            Assert.AreEqual(classElement.GetSrcStartLinePosition(), element.GetSrcStartLinePosition());
            Assert.AreEqual(classElement.GetXPath(), element.GetXPath());
        }
    }
}