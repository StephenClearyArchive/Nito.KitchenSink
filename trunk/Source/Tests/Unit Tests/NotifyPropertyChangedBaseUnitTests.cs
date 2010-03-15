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
    public class NotifyPropertyChangedBaseUnitTests
    {
        [TestMethod]
        public void Subscription_MatchingPropertyChanged_InvokesHandler()
        {
            TestDerivedObject test = new TestDerivedObject();
            bool called = false;

            test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);
            test.ValueProperty = 13;

            Assert.IsTrue(called, "SubscribeToPropertyChanged should invoke handler when property changes");
        }

        [TestMethod]
        public void Subscription_MismatchingPropertyChanged_DoesNotInvokeHandler()
        {
            TestDerivedObject test = new TestDerivedObject();
            bool called = false;

            test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);
            test.ObjectProperty = new object();

            Assert.IsFalse(called, "SubscribeToPropertyChanged should not invoke handler when other properties change");
        }

        [TestMethod]
        public void Subscription_ReturnsUnsubscribableDelegate()
        {
            TestDerivedObject test = new TestDerivedObject();
            bool called = false;

            var unsubscribe = test.SubscribeToPropertyChanged(x => x.ValueProperty, x => called = true);
            test.PropertyChanged -= unsubscribe;
            test.ValueProperty = 13;

            Assert.IsFalse(called, "SubscribeToPropertyChanged should not invoke handler when unsubscribed");
        }

        [TestMethod]
        public void OnItemsPropertyChanged_InvokesHandler_WithCorrectPropertyName()
        {
            TestDerivedObject test = new TestDerivedObject();
            bool called = false;

            test.PropertyChanged += (_, x) => { if (x.PropertyName == "Items[]") called = true; };
            test.RaiseItems();

            Assert.IsTrue(called, "OnItemsPropertyChanged should invoke handler with property name 'Items[]'");
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

            public void RaiseItems()
            {
                this.OnItemsPropertyChanged();
            }
        }
    }
}

