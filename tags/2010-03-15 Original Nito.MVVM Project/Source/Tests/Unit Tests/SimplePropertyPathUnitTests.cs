using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using Nito.Utility;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for SimplePropertyPath.
    /// </summary>
    [TestClass]
    public class SimplePropertyPathUnitTests
    {
        [TestMethod]
        public void TestSimpleRead()
        {
            FakeVM obj = new FakeVM { Value = 13 };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Value" };

            Assert.AreEqual(13, path.Value);
        }

        [TestMethod]
        public void TestSimpleWrite()
        {
            FakeVM obj = new FakeVM { Value = 13 };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Value" };
            path.Value = 17;

            Assert.AreEqual(17, obj.Value);
        }

        [TestMethod]
        public void TestChildRead()
        {
            FakeVM obj = new FakeVM { Child = new FakeVM { Value = 10 } };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Child.Value" };

            Assert.AreEqual(10, path.Value);
        }

        [TestMethod]
        public void TestChildWrite()
        {
            FakeVM obj = new FakeVM { Child = new FakeVM { Value = 10 } };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Child.Value" };
            path.Value = 17;

            Assert.AreEqual(17, obj.Child.Value);
        }

        [TestMethod]
        public void TestInvalidRead()
        {
            FakeVM obj = new FakeVM { Value = 13 };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "value" };

            Assert.IsNull(path.Value);
        }

        [TestMethod]
        public void TestInvalidWrite()
        {
            FakeVM obj = new FakeVM { Value = 13 };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "value" };
            path.Value = 17;

            Assert.AreEqual(13, obj.Value);
            Assert.IsNull(path.Value);
        }

        [TestMethod]
        public void TestInvalidChildRead()
        {
            FakeVM obj = new FakeVM { Child = new FakeVM { Value = 10 } };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Child.value" };

            Assert.IsNull(path.Value);
        }

        [TestMethod]
        public void TestInvalidChildWrite()
        {
            FakeVM obj = new FakeVM { Child = new FakeVM { Value = 10 } };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Child.value" };
            path.Value = 17;

            Assert.AreEqual(10, obj.Child.Value);
            Assert.IsNull(path.Value);
        }

        [TestMethod]
        public void TestSimpleChange()
        {
            bool sawChange = false;
            FakeVM obj = new FakeVM { Value = 11 };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Value" };
            path.PropertyChanged += (sender, args) => { if (args.PropertyName == "Value") sawChange = true; };

            Assert.AreEqual(11, path.Value);
            Assert.AreEqual(false, sawChange);

            obj.Value = 12;

            Assert.AreEqual(12, path.Value);
            Assert.AreEqual(true, sawChange);
        }

        [TestMethod]
        public void TestChildPropertyChange()
        {
            bool sawChange = false;
            FakeVM obj = new FakeVM { Child = new FakeVM { Value = 8 } };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Child.Value" };
            path.PropertyChanged += (sender, args) => { if (args.PropertyName == "Value") sawChange = true; };

            Assert.AreEqual(8, path.Value);
            Assert.AreEqual(false, sawChange);

            obj.Child.Value = 13;

            Assert.AreEqual(13, path.Value);
            Assert.AreEqual(true, sawChange);
        }

        [TestMethod]
        public void TestChildObjectChange()
        {
            bool sawChange = false;
            FakeVM obj = new FakeVM { Child = new FakeVM { Value = 100 } };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Child.Value" };
            path.PropertyChanged += (sender, args) => { if (args.PropertyName == "Value") sawChange = true; };

            Assert.AreEqual(100, path.Value);
            Assert.AreEqual(false, sawChange);

            obj.Child = null;

            Assert.IsNull(path.Value);
            Assert.AreEqual(true, sawChange);

            obj.Child = new FakeVM { Value = 113 };

            Assert.AreEqual(113, path.Value);
        }

        [TestMethod]
        public void TestLongChain()
        {
            FakeVM obj = new FakeVM { Value = 1 };
            SimplePropertyPath path = new SimplePropertyPath { Root = obj, Path = "Child.Child.Child.Value" };

            Assert.IsNull(path.Value);

            obj.Child = new FakeVM { Value = 2 };

            Assert.IsNull(path.Value);

            obj.Child.Child = new FakeVM { Value = 3 };

            Assert.IsNull(path.Value);

            obj.Child.Child.Child = new FakeVM { Value = 4 };

            Assert.AreEqual(4, path.Value);

            obj.Child = new FakeVM { Value = 52, Child = new FakeVM { Value = 53, Child = new FakeVM { Value = 54 } } };

            Assert.AreEqual(54, path.Value);
        }

        private sealed class FakeVM : INotifyPropertyChanged
        {
            private int value;
            private FakeVM child;

            public int Value
            {
                get { return this.value; }
                set
                {
                    this.value = value;
                    this.OnPropertyChanged("Value");
                }
            }

            public FakeVM Child
            {
                get { return this.child; }
                set
                {
                    this.child = value;
                    this.OnPropertyChanged("Child");
                }
            }

            #region INotifyPropertyChanged Members

            private void OnPropertyChanged(string propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion
        }

    }
}
