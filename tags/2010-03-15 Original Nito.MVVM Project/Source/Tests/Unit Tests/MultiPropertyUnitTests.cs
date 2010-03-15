using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Nito.MVVM;

namespace UnitTests
{
    [TestClass]
    public class MultiPropertyUnitTests
    {
        [TestMethod]
        public void TestSimpleRead()
        {
            ObservableCollection<FakeVM> source = new ObservableCollection<FakeVM>(new[] { new FakeVM { Value = 13 }, new FakeVM { Value = 13 }, new FakeVM { Value = 13 } });
            MultiProperty prop = new MultiProperty { SourceCollection = source, Path = "Value" };

            Assert.AreEqual(13, prop.Value);
        }

        [TestMethod]
        public void TestUnequalRead()
        {
            ObservableCollection<FakeVM> source = new ObservableCollection<FakeVM>(new[] { new FakeVM { Value = 13 }, new FakeVM { Value = 15 }, new FakeVM { Value = 13 } });
            MultiProperty prop = new MultiProperty { SourceCollection = source, Path = "Value" };

            Assert.IsNull(prop.Value);
        }

        [TestMethod]
        public void TestSimpleWrite()
        {
            ObservableCollection<FakeVM> source = new ObservableCollection<FakeVM>(new[] { new FakeVM { Value = 13 }, new FakeVM { Value = 13 }, new FakeVM { Value = 13 } });
            MultiProperty prop = new MultiProperty { SourceCollection = source, Path = "Value" };

            prop.Value = 17;

            Assert.IsTrue(source.Select(x => x.Value).SequenceEqual(new [] { 17, 17, 17 }));
        }

        [TestMethod]
        public void TestUnequalWrite()
        {
            ObservableCollection<FakeVM> source = new ObservableCollection<FakeVM>(new[] { new FakeVM { Value = 13 }, new FakeVM { Value = 15 }, new FakeVM { Value = 13 } });
            MultiProperty prop = new MultiProperty { SourceCollection = source, Path = "Value" };

            prop.Value = 17;

            Assert.IsTrue(source.Select(x => x.Value).SequenceEqual(new[] { 17, 17, 17 }));
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
