using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.Utility;

namespace Tests.Regression_Tests
{
    [TestClass]
    public class ProjectedCollectionRegressionTests
    {
        [TestMethod]
        public void TestNonNotifyingSourceCollection()
        {
            // Regression test for bug #5195: NullReferenceException if ProjectedCollection.SourceCollection does not implement INotifyCollectionChanged
            List<TestObject> source = new List<TestObject>(new[] { new TestObject { Value = 17 } });
            ProjectedCollection collection = new ProjectedCollection { SourceCollection = source, Path = "Value" };

            Assert.IsTrue(collection.Cast<int>().SequenceEqual(new[] { 17 }));
        }

        private sealed class TestObject : NotifyPropertyChangedBase<TestObject>
        {
            public TestObject()
            {
                this.value = 13;
            }

            private int value;
            public int Value
            {
                get { return this.value; }
                set
                {
                    this.value = value;
                    this.OnPropertyChanged(x => x.Value);
                }
            }

            private object child;
            public object Child
            {
                get { return this.child; }
                set
                {
                    this.child = value;
                    this.OnPropertyChanged(x => x.Child);
                }
            }
        }
    }
}
