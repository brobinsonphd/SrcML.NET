﻿/******************************************************************************
 * Copyright (c) 2010 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Vinay Augustine (ABB Group) - initial API, implementation, & documentation
 *****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.IO;
using ABB.SrcML;
using System.Xml.Linq;

namespace ABB.SrcML.Test
{
    [TestFixture]
    [Category("Build")]
    public class SRCTest
    {
        [TestFixtureSetUp]
        public static void SRCTestInitialize()
        {
            Directory.CreateDirectory("srctest");
            Directory.CreateDirectory("srctest_xml");

            File.WriteAllText(@"srctest\foo.c", String.Format(@"int foo() {{{0}    printf(""hello world!""); bool x = 5 < 3; printd(""what?"");}}{0}{0}void bar() {{{0}    while(1){0}    {{{0}        printg(""bar"");{0}    }}{0}}}", Environment.NewLine));

            File.WriteAllText("srctest\\bar.c", String.Format(@"int bar() {{{0}printf(""good bye, world"");{0}}}", Environment.NewLine));
        }

        [TestFixtureTearDown]
        public static void SRCTestCleanup()
        {
            foreach (var file in Directory.GetFiles("srctest"))
            {
                File.Delete(file);
            }
            foreach (var file in Directory.GetFiles("srctest_xml"))
            {
                File.Delete(file);
            }
            Directory.Delete("srctest");
            Directory.Delete("srctest_xml");
        }

        [Test]
        public void CheckPositionNumberWithSingleUnit()
        {
            var srcmlObject = new ABB.SrcML.SrcML(Environment.GetEnvironmentVariable("SRCMLBINDIR"));

            var doc = srcmlObject.GenerateSrcMLFromFile(@"srctest\foo.c", @"srctest_xml\singleunit_position.xml");

            var firstUnit = doc.FileUnits.First();

            Assert.AreEqual(0, firstUnit.GetSrcStartLinePosition());
            Assert.AreEqual(1, firstUnit.Descendants(SRC.Type).First().Element(SRC.Name).GetSrcStartLinePosition());
            Assert.AreEqual(1, firstUnit.Element(SRC.Function).GetSrcStartLinePosition());
            Assert.AreEqual(5, firstUnit.Descendants(SRC.Name).First(n => n.Value == "foo").GetSrcStartLinePosition());
            Assert.AreEqual(5, firstUnit.Descendants(SRC.Name).First(n => n.Value == "printf").GetSrcStartLinePosition());
            Assert.AreEqual(45, firstUnit.Descendants(SRC.Name).First(n => n.Value == "printd").GetSrcStartLinePosition());

            Assert.AreEqual(6, firstUnit.Descendants(SRC.Name).First(n => n.Value == "bar").GetSrcStartLinePosition());
            Assert.AreEqual(10, firstUnit.Descendants(SRC.Condition).First().GetSrcStartLinePosition());
            Assert.AreEqual(9, firstUnit.Descendants(SRC.Name).First(n => n.Value == "printg").GetSrcStartLinePosition());
        }

        [Test]
        public void GetSrcLineNumberWithSingleUnit()
        {
            File.WriteAllText("srctest\\singleunitlinenum.c", @"int foo() { printf(""hello world!""); }");

            var namespaceParamters = new[] { "--position" };

            var srcmlObject = new Src2SrcMLRunner(Environment.GetEnvironmentVariable("SRCMLBINDIR"), namespaceParamters);

            var doc = srcmlObject.GenerateSrcMLFromFile("srctest\\singleunitlinenum.c", @"srctest_xml\singleunit_linenumber.xml");

            var unit = doc.FileUnits.First();

            Assert.AreEqual(1, unit.GetSrcStartLineNumber());
            Assert.AreEqual(1, unit.Element(SRC.Function).GetSrcStartLineNumber());
            Assert.AreEqual(1, unit.Descendants(SRC.Call).First().GetSrcStartLineNumber());
        }

        [Test]
        public void GetLineInfoWithString()
        {
            var source = @"int foo() { printf(a); }";
            var namespaceParamters = new[] { "--position" };
            var srcmlObject = new Src2SrcMLRunner(Environment.GetEnvironmentVariable("SRCMLBINDIR"), namespaceParamters);

            var xml = srcmlObject.GenerateSrcMLFromString(source);

            var element = XElement.Parse(xml).Elements().First();

            Assert.AreEqual(1, element.GetSrcStartLineNumber());
            Assert.AreEqual(1, element.GetSrcStartLinePosition());
        }

        [Test]
        public void GetSrcLineNumberWithMultipleUnit()
        {
            var srcmlObject = new Src2SrcMLRunner(Environment.GetEnvironmentVariable("SRCMLBINDIR"));

            var doc = srcmlObject.GenerateSrcMLFromDirectory("srctest", "srctest_xml\\multipleunit_linenumber.xml");
            
            var firstUnit = doc.FileUnits.First();

            Assert.AreEqual(1, firstUnit.Element(SRC.Function).GetSrcStartLineNumber());
            Assert.AreEqual(2, firstUnit.Descendants(SRC.Call).First().GetSrcStartLineNumber());
        }

        [Test]
        public void ToSourceTest()
        {
            var text = File.ReadAllText("srctest\\foo.c");

            var srcmlObject = new Src2SrcMLRunner(Environment.GetEnvironmentVariable("SRCMLBINDIR"));

            var doc = srcmlObject.GenerateSrcMLFromFile("srctest\\foo.c", "srctest_xml\\srctest_tosource.xml");

            var firstUnit = doc.FileUnits.First();

            string contentsFromXml = firstUnit.ToSource();

            Assert.AreEqual(text, contentsFromXml);
        }

        [Test]
        public void ParentStatementTest()
        {
            var srcmlObject = new Src2SrcMLRunner(Environment.GetEnvironmentVariable("SRCMLBINDIR"));

            var doc = srcmlObject.GenerateSrcMLFromFile("srctest\\foo.c", "srctest_xml\\srctest_parentstatement.xml");
            var firstUnit = doc.FileUnits.First();
            var function = firstUnit.Element(SRC.Function);
            
            var expression = function.Element(SRC.Block).Element(SRC.BlockContent).Element(SRC.ExpressionStatement);
            var call = expression.Descendants(SRC.Call).First();

            var declaration = function.Element(SRC.Block).Element(SRC.BlockContent).Element(SRC.DeclarationStatement);
            var variable = declaration.Element(SRC.Declaration).Element(SRC.Name);

            var unitParent = firstUnit.ParentStatement();
            var functionParent = function.ParentStatement();
            var callParent = call.ParentStatement();
            var variableParent = variable.ParentStatement();

            Assert.IsNull(unitParent);
            Assert.IsNull(functionParent);
            Assert.AreEqual(callParent, expression);
            Assert.AreEqual(declaration, variableParent);            
        }

        [Test]
        public void ContainsCallToTest()
        {
            string source = @"int foo() { printf(a); int x = 5; }";
            var srcmlObject = new Src2SrcMLRunner(Environment.GetEnvironmentVariable("SRCMLBINDIR"));
            var xml = srcmlObject.GenerateSrcMLFromString(source);

            var element = XElement.Parse(xml);
            var expression = element.Descendants(SRC.DeclarationStatement).First();

            Assert.IsTrue(element.ContainsCallTo("printf"));
            Assert.IsFalse(expression.ContainsCallTo("printf"));
        }
    }
}
