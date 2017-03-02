﻿using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Test
{
    [TestFixture]
    class TestSaveState
    {
        [OneTimeSetUp]
        public void setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        [Test]
        public void save_and_load_grouped_components()
        {
            //reading in test excel file, process raw components before testing neucode pairs.
            Lollipop.correctionFactors = null;
            Lollipop.raw_experimental_components.Clear();
            Func<InputFile, IEnumerable<Component>> componentReader = c => new ComponentReader().read_components_from_xlsx(c, Lollipop.correctionFactors);
            InputFile noisy = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "noisy.xlsx"), Labeling.NeuCode, Purpose.Identification);
            Lollipop.input_files.Add(noisy);

            string inFileId = noisy.UniqueId.ToString();

            Lollipop.neucode_labeled = true;
            Lollipop.process_raw_components();
            Assert.AreEqual(223, Lollipop.raw_experimental_components.Count);
            int charge_count = Lollipop.raw_experimental_components.SelectMany(c => c.charge_states).Count();

            string file_path = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_saveAll.xml");
            SaveState.save_all(file_path);
            Lollipop.raw_experimental_components.Clear();
            SaveState.open_all(file_path);
            Assert.AreEqual(223, Lollipop.raw_experimental_components.Count);
            Assert.AreEqual(charge_count, Lollipop.raw_experimental_components.SelectMany(c => c.charge_states).Count());
        }

        [Test]
        public void restore_lollipop_settings()
        {
            Lollipop defaults = new Lollipop();
            StringBuilder builder = SaveState.save_method();
            foreach (PropertyInfo property in typeof(Lollipop).GetProperties())
            {
                if (property.PropertyType == typeof(int))
                {
                    property.SetValue(null, Convert.ToInt32(property.GetValue(null)) + 1);
                    Assert.AreEqual(Convert.ToInt32(property.GetValue(defaults)) + 1, Convert.ToInt32(property.GetValue(null))); //the int values were changed in the current program
                }
                else if (property.PropertyType == typeof(double))
                {
                    property.SetValue(null, Convert.ToDouble(property.GetValue(null)) + 1);
                    Assert.AreEqual(Convert.ToDouble(property.GetValue(defaults)) + 1, Convert.ToDouble(property.GetValue(null))); //the double values were changed in the current program
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(null, property.GetValue(null).ToString() + "hello");
                    Assert.AreEqual(property.GetValue(defaults).ToString() + "hello", Convert.ToDouble(property.GetValue(null)).ToString()); //the string values were changed in the current program
                }
                else if (property.PropertyType == typeof(decimal))
                {
                    property.SetValue(null, Convert.ToDecimal(property.GetValue(null)) + 1);
                    Assert.AreEqual(Convert.ToDecimal(property.GetValue(defaults)) + 1, Convert.ToDecimal(property.GetValue(null))); //the decimal value were changed in the current program
                }
                else if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(null, !Convert.ToBoolean(property.GetValue(null)));
                    Assert.AreEqual(!Convert.ToBoolean(property.GetValue(defaults)), Convert.ToBoolean(property.GetValue(null))); //the bool value were changed in the current program
                }
                else continue;
            }

            SaveState.open_method(builder.ToString());
            foreach (PropertyInfo property in typeof(Lollipop).GetProperties())
            {
                if (property.PropertyType == typeof(int))
                    Assert.AreEqual(Convert.ToInt32(property.GetValue(defaults)), Convert.ToInt32(property.GetValue(null))); //the int values were changed back
                else if (property.PropertyType == typeof(double))
                    Assert.AreEqual(Convert.ToDouble(property.GetValue(defaults)), Convert.ToDouble(property.GetValue(null))); //the double values were changed back
                else if (property.PropertyType == typeof(string))
                    Assert.AreEqual(property.GetValue(defaults).ToString(), Convert.ToDouble(property.GetValue(null)).ToString()); //the string values were changed back
                else if (property.PropertyType == typeof(decimal))
                    Assert.AreEqual(Convert.ToDecimal(property.GetValue(defaults)), Convert.ToDecimal(property.GetValue(null))); //the decimal value were changed back
                else if (property.PropertyType == typeof(bool))
                    Assert.AreEqual(Convert.ToBoolean(property.GetValue(defaults)), Convert.ToBoolean(property.GetValue(null))); //the bool value were changed back
                else continue;
            }
        }
    }
}
