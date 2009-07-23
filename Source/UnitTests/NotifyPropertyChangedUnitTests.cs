using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.Utility;
using System.ComponentModel;
using System.Linq.Expressions;

namespace UnitTests
{
    [TestClass]
    public class NotifyPropertyChangedUnitTests
    {
        [TestMethod]
        public void TestSubscription()
        {
            TestObject test = new TestObject();
            bool called = false;

            test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);

            Assert.AreEqual(false, called);

            test.ObjectProperty = new object();

            Assert.AreEqual(false, called);

            test.ValueProperty = 13;

            Assert.AreEqual(true, called);
        }

        [TestMethod]
        public void TestDerivedSubscription()
        {
            TestDerivedObject test = new TestDerivedObject();
            bool called = false;

            // Ensure the PropertyChangedEventHandler.Raise extension method will not fail when its "this" parameter is null
            test.ObjectProperty = new object();

            test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);

            Assert.AreEqual(false, called);

            test.ObjectProperty = new object();

            Assert.AreEqual(false, called);

            test.ValueProperty = 13;

            Assert.AreEqual(true, called);
        }

        [TestMethod]
        public void TestIncludedSubscription()
        {
            TestIncludedObject test = new TestIncludedObject();
            bool called = false;

            // Ensure the PropertyChangedEventHandler.Raise extension method will not fail when its "this" parameter is null
            test.ObjectProperty = new object();

            test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);

            Assert.AreEqual(false, called);

            test.ObjectProperty = new object();

            Assert.AreEqual(false, called);

            test.ValueProperty = 13;

            Assert.AreEqual(true, called);
        }

        /// <summary>
        /// Test object that has its own INotifyPropertyChanged implementation (used to test SubscribeToPropertyChanged)
        /// </summary>
        private sealed class TestObject : INotifyPropertyChanged
        {
            private int valueProperty;
            private object objectProperty;

            public int ValueProperty
            {
                get { return this.valueProperty; }
                set
                {
                    this.valueProperty = value;
                    this.OnPropertyChanged(this.GetPropertyName(x => x.ValueProperty));
                }
            }

            public object ObjectProperty
            {
                get { return this.objectProperty; }
                set
                {
                    this.objectProperty = value;
                    this.OnPropertyChanged(this.GetPropertyName(x => x.ObjectProperty));
                }
            }

            #region INotifyPropertyChanged Members

            public void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion
        }

        /// <summary>
        /// Test object that uses an inherited implementation of INotifyPropertyChanged.
        /// </summary>
        private sealed class TestDerivedObject : NotifyPropertyChangedBase<TestDerivedObject>
        {
            private int valueProperty;
            private object objectProperty;

            public int ValueProperty
            {
                get { return this.valueProperty; }
                set
                {
                    this.valueProperty = value;
                    this.OnPropertyChanged(x => x.ValueProperty);
                }
            }

            public object ObjectProperty
            {
                get { return this.objectProperty; }
                set
                {
                    this.objectProperty = value;
                    this.OnPropertyChanged(x => x.ObjectProperty);
                }
            }
        }

        /// <summary>
        /// Test object that uses the contained implementation of INotifyPropertyChanged.
        /// </summary>
        private sealed class TestIncludedObject : INotifyPropertyChanged
        {
            private NotifyPropertyChangedCore<TestIncludedObject> propertyChanged;
            private int valueProperty;
            private object objectProperty;

            public TestIncludedObject()
            {
                this.propertyChanged = new NotifyPropertyChangedCore<TestIncludedObject>(this);
            }

            public int ValueProperty
            {
                get { return this.valueProperty; }
                set
                {
                    this.valueProperty = value;
                    this.propertyChanged.OnPropertyChanged(x => x.ValueProperty);
                }
            }

            public object ObjectProperty
            {
                get { return this.objectProperty; }
                set
                {
                    this.objectProperty = value;
                    this.propertyChanged.OnPropertyChanged(x => x.ObjectProperty);
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged
            {
                add { this.propertyChanged.PropertyChanged += value; }
                remove { this.propertyChanged.PropertyChanged -= value; }
            }

            #endregion
        }
    }
}
