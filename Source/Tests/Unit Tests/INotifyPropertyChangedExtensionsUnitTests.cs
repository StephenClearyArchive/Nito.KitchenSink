using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;
using System.ComponentModel;

namespace UnitTests
{
    [TestClass]
    public class INotifyPropertyChangedExtensionsUnitTests
    {
        [TestMethod]
        public void Subscription_MatchingPropertyChanged_InvokesHandler()
        {
            TestObject test = new TestObject();
            bool called = false;

            test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);
            test.ValueProperty = 13;

            Assert.IsTrue(called, "SubscribeToPropertyChanged should invoke handler when property changes");
        }

        [TestMethod]
        public void Subscription_MismatchingPropertyChanged_DoesNotInvokeHandler()
        {
            TestObject test = new TestObject();
            bool called = false;

            test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);
            test.ObjectProperty = new object();

            Assert.IsFalse(called, "SubscribeToPropertyChanged should not invoke handler when other properties change");
        }

        [TestMethod]
        public void Subscription_ReturnsUnsubscribableDelegate()
        {
            TestObject test = new TestObject();
            bool called = false;

            var unsubscribe = test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);
            test.PropertyChanged -= unsubscribe;
            test.ValueProperty = 13;

            Assert.IsFalse(called, "SubscribeToPropertyChanged should not invoke handler when unsubscribed");
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
    }
}

