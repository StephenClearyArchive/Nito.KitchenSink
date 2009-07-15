using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.Utility;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace UnitTests
{
    [TestClass]
    public class ProjectedCollectionUnitTests
    {
        [TestMethod]
        public void TestSimpleProjection()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17 }));
        }

        [TestMethod]
        public void TestNestedSimpleProjection()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Child = new TestObject { Value = 17 } } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Child.Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17 }));
        }

        [TestMethod]
        public void TestBadPath()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Child.Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 0 }));
        }

        [TestMethod]
        public void TestSourceContainingNull()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 }, null });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 0 }));
        }

        [TestMethod]
        public void TestPropertyChange()
        {
            TestObject test = new TestObject { Value = 17 };
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { test });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17 }));

            test.Value = 19;

            Assert.IsTrue(collection.SequenceEqual(new[] { 19 }));

            test.Value = 21;

            Assert.IsTrue(collection.SequenceEqual(new[] { 21 }));
        }

        [TestMethod]
        public void TestAddItem()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>();
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            Assert.AreEqual(0, collection.Count);

            source.Add(new TestObject { Value = 17 });

            Assert.IsTrue(collection.SequenceEqual(new[] { 17 }));

            source.Add(new TestObject { Value = 19 });

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 19 }));

            source.Insert(1, new TestObject { Value = 21 });

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 21, 19 }));
        }

        [TestMethod]
        public void TestRemoveItem()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new [] { new TestObject { Value = 17 }, new TestObject { Value = 19 }, new TestObject { Value = 21 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 19, 21 }));

            source.RemoveAt(1);

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 21 }));

            source.RemoveAt(0);

            Assert.IsTrue(collection.SequenceEqual(new[] { 21 }));

            source.RemoveAt(0);

            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void TestClearItems()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 }, new TestObject { Value = 19 }, new TestObject { Value = 21 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 19, 21 }));

            source.Clear();

            Assert.AreEqual(0, collection.Count);
        }

        [TestMethod]
        public void TestMoveItem()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 }, new TestObject { Value = 19 }, new TestObject { Value = 21 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 19, 21 }));

            source.Move(0, 1);

            Assert.IsTrue(collection.SequenceEqual(new[] { 19, 17, 21 }));

            source.Move(0, 2);

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 21, 19 }));

            source.Move(1, 0);

            Assert.IsTrue(collection.SequenceEqual(new[] { 21, 17, 19 }));

            source.Move(2, 0);

            Assert.IsTrue(collection.SequenceEqual(new[] { 19, 21, 17 }));
        }

        [TestMethod]
        public void TestReplaceItem()
        {
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 }, new TestObject { Value = 19 }, new TestObject { Value = 21 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 19, 21 }));

            source[1] = new TestObject { Value = 199 };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 199, 21 }));

            source[2] = new TestObject { Value = 1 };

            Assert.IsTrue(collection.SequenceEqual(new[] { 17, 199, 1 }));

            source[0] = source[2];

            Assert.IsTrue(collection.SequenceEqual(new[] { 1, 199, 1 }));
        }

        [TestMethod]
        public void TestPropertyChangeNotifications()
        {
            bool sawItemsPropertyChanged = false;
            bool sawCollectionChanged = false;
            TestObject test = new TestObject { Value = 17 };
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { null, test });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            collection.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "Items[]")
                        sawItemsPropertyChanged = true;
                };

            collection.CollectionChanged += (sender, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Replace &&
                        args.OldStartingIndex == 1 &&
                        args.OldItems.Cast<object>().SequenceEqual(new object[] { 17 }) &&
                        args.NewItems.Cast<object>().SequenceEqual(new object[] { 19 }))
                        sawCollectionChanged = true;
                };

            test.Value = 19;

            Assert.IsTrue(sawItemsPropertyChanged);
            Assert.IsTrue(sawCollectionChanged);
        }

        [TestMethod]
        public void TestItemAddNotifications()
        {
            bool sawItemsPropertyChanged = false;
            bool sawCountPropertyChanged = false;
            bool sawCollectionChanged = false;
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new TestObject[] { null });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            collection.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Items[]")
                    sawItemsPropertyChanged = true;
                else if (args.PropertyName == "Count")
                    sawCountPropertyChanged = true;
            };

            collection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add &&
                    args.NewStartingIndex == 1 &&
                    args.NewItems.Cast<object>().SequenceEqual(new object[] { 17 }))
                    sawCollectionChanged = true;
            };

            source.Add(new TestObject { Value = 17 });

            Assert.IsTrue(sawItemsPropertyChanged);
            Assert.IsTrue(sawCountPropertyChanged);
            Assert.IsTrue(sawCollectionChanged);
        }

        [TestMethod]
        public void TestItemRemoveNotifications()
        {
            bool sawItemsPropertyChanged = false;
            bool sawCountPropertyChanged = false;
            bool sawCollectionChanged = false;
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 }, new TestObject { Value = 19 }, new TestObject { Value = 21 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            collection.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Items[]")
                    sawItemsPropertyChanged = true;
                else if (args.PropertyName == "Count")
                    sawCountPropertyChanged = true;
            };

            collection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Remove &&
                    args.OldStartingIndex == 1 &&
                    args.OldItems.Cast<object>().SequenceEqual(new object[] { 19 }))
                    sawCollectionChanged = true;
            };

            source.RemoveAt(1);

            Assert.IsTrue(sawItemsPropertyChanged);
            Assert.IsTrue(sawCountPropertyChanged);
            Assert.IsTrue(sawCollectionChanged);
        }

        [TestMethod]
        public void TestItemResetNotifications()
        {
            bool sawItemsPropertyChanged = false;
            bool sawCountPropertyChanged = false;
            bool sawCollectionChanged = false;
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 }, new TestObject { Value = 19 }, new TestObject { Value = 21 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            collection.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Items[]")
                    sawItemsPropertyChanged = true;
                else if (args.PropertyName == "Count")
                    sawCountPropertyChanged = true;
            };

            collection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset)
                    sawCollectionChanged = true;
            };

            source.Clear();

            Assert.IsTrue(sawItemsPropertyChanged);
            Assert.IsTrue(sawCountPropertyChanged);
            Assert.IsTrue(sawCollectionChanged);
        }

        [TestMethod]
        public void TestItemMoveForwardNotifications()
        {
            bool sawItemsPropertyChanged = false;
            bool sawCollectionChanged = false;
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 }, new TestObject { Value = 19 }, new TestObject { Value = 21 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            collection.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Items[]")
                    sawItemsPropertyChanged = true;
            };

            collection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Move &&
                    args.OldStartingIndex == 1 &&
                    args.NewStartingIndex == 2 &&
                    args.NewItems.Cast<object>().SequenceEqual(new object[] { 19 }))
                    sawCollectionChanged = true;
            };

            source.Move(1, 2);

            Assert.IsTrue(sawItemsPropertyChanged);
            Assert.IsTrue(sawCollectionChanged);
        }

        [TestMethod]
        public void TestItemMoveBackwardNotifications()
        {
            bool sawItemsPropertyChanged = false;
            bool sawCollectionChanged = false;
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { new TestObject { Value = 17 }, new TestObject { Value = 19 }, new TestObject { Value = 21 } });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            collection.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Items[]")
                    sawItemsPropertyChanged = true;
            };

            collection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Move &&
                    args.OldStartingIndex == 2 &&
                    args.NewStartingIndex == 1 &&
                    args.NewItems.Cast<object>().SequenceEqual(new object[] { 21 }))
                    sawCollectionChanged = true;
            };

            source.Move(2, 1);

            Assert.IsTrue(sawItemsPropertyChanged);
            Assert.IsTrue(sawCollectionChanged);
        }

        [TestMethod]
        public void TestItemReplaceNotifications()
        {
            bool sawItemsPropertyChanged = false;
            bool sawCollectionChanged = false;
            TestObject test = new TestObject { Value = 17 };
            ObservableCollection<TestObject> source = new ObservableCollection<TestObject>(new[] { null, test });
            ProjectedCollection<int> collection = new ProjectedCollection<int> { SourceCollection = source, Path = "Value" };

            collection.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Items[]")
                    sawItemsPropertyChanged = true;
            };

            collection.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Replace &&
                    args.OldStartingIndex == 1 &&
                    args.OldItems.Cast<object>().SequenceEqual(new object[] { 17 }) &&
                    args.NewItems.Cast<object>().SequenceEqual(new object[] { 19 }))
                    sawCollectionChanged = true;
            };

            source[1] = new TestObject { Value = 19 };

            Assert.IsTrue(sawItemsPropertyChanged);
            Assert.IsTrue(sawCollectionChanged);
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
