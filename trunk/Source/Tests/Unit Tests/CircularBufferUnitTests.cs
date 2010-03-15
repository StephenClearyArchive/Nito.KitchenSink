using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class CircularBufferUnitTests
    {
        [TestMethod]
        public void CircularBuffer_ConstructedWithCapacity_RemembersCapacity()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(13);
            int result = q.Capacity;
            Assert.AreEqual(13, result, "CircularBuffer constructed with capacity should remember that capacity");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "CircularBuffer constructed with an invalid capacity should be rejected")]
        public void CircularBuffer_ConstructedWithInvalidCapacity_IsRejected()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(0);
        }

        [TestMethod]
        public void CircularBuffer_Add_AddsItem()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8);
            q.Add(13);
            Assert.AreEqual(1, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void CircularBuffer_Add_AddsItemInOrder()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8);
            q.Add(13);
            q.Add(17);
            Assert.AreEqual(2, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 17 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void CircularBuffer_AfterBackOverflow_RemembersItems()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(1);
            q.Add(13);
            q.Add(17);
            Assert.AreEqual(1, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 17 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void CircularBuffer_AfterBackResize_RemembersItems()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(1);
            q.Add(13);
            q.Capacity = 2;
            q.Add(17);
            Assert.AreEqual(2, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 17 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void CircularBuffer_AddRange_InsertsRange()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8);
            q.AddRange(new[] { 13, 17 });
            Assert.AreEqual(2, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 17 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void CircularBuffer_AddRangeToOverflow_InsertsRange()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.AddRange(new[] { 13, 17, 19, 23, 29 });
            Assert.AreEqual(8, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 3, 4, 5, 13, 17, 19, 23, 29 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void CircularBuffer_AddRangeLargerThanCapacity_InsertsRange()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.AddRange(new[] { 13, 17, 19, 23, 29, 31, 37, 39, 41 });
            Assert.AreEqual(8, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 17, 19, 23, 29, 31, 37, 39, 41 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void CircularBuffer_InsertAtBack_MaintainsOrder()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.Insert(5, 13);
            Assert.AreEqual(6, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5, 13 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void CircularBuffer_InsertAtBack_OverwritesOldElements()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(5) { 1, 2, 3, 4, 5 };
            q.Insert(5, 13);
            Assert.AreEqual(5, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 2, 3, 4, 5, 13 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Inserting not at the back should be rejected.")]
        public void CircularBuffer_InsertNotAtBack_IsRejected()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(5) { 1, 2, 3, 4, 5 };
            q.Insert(0, 13);
        }

        [TestMethod]
        public void CircularBuffer_RemoveAtFront_MaintainsOrder()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.RemoveAt(0);
            Assert.AreEqual(4, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 2, 3, 4, 5 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Removing not at the front should be rejected.")]
        public void CircularBuffer_RemoveNotAtFront_IsRejected()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(5) { 1, 2, 3, 4, 5 };
            q.Remove(5);
        }

        [TestMethod]
        public void CircularBuffer_RemoveRange_RemovesRange()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 13, 17 };
            q.RemoveRange(2);
            Assert.AreEqual(0, q.Count, "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void AddRange_AtBack_MaintainsOrder()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.AddRange(new[] { 13, 17 });
            Assert.AreEqual(7, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5, 13, 17 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void AddEmptyRange_DoesNothing()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.AddRange(new int[] { });
            Assert.AreEqual(5, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void RemoveRange_AtFront_MaintainsOrder()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5, 6, 7 };
            q.RemoveRange(2);
            Assert.AreEqual(5, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 3, 4, 5, 6, 7 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void RemoveEmptyRange_DoesNothing()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.RemoveRange(0);
            Assert.AreEqual(5, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void Capacity_LessThanCount_TruncatesList()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.Capacity = 3;
            Assert.AreEqual(3, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 3, 4, 5 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void Capacity_SetToLessThanOrEqualToZero_DoesNothing()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.Capacity = 0;
            Assert.AreEqual(5, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void Capacity_SetToItself_DoesNothing()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(3);
            q.Capacity = 3;
            Assert.AreEqual(3, q.Capacity, "CircularBuffer should remember its capacity");
        }

        [TestMethod]
        public void Capacity_ResizeWhileSplit_MaintainsOrder()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(4) { 13, 17, 19 };
            q.Add(23);
            q.Add(29);
            q.Capacity = 8;
            q.Add(31);
            Assert.IsTrue(q.SequenceEqual(new[] { 17, 19, 23, 29, 31 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void Clear_ClearsItems()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.Clear();
            Assert.AreEqual(0, q.Count, "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void SetItem_UpdatesItems()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.Add(13);
            q[3] = 17;
            Assert.AreEqual(6, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 17, 5, 13 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        public void Remove_MaintainsOrder()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8) { 1, 2, 3, 4, 5 };
            q.Remove();
            Assert.AreEqual(4, q.Count, "CircularBuffer should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 2, 3, 4, 5 }), "CircularBuffer should remember its items");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Should not be able to remove elements from an empty CircularBuffer")]
        public void Remove_WhenEmpty_IsRejected()
        {
            CircularBuffer<int> q = new CircularBuffer<int>(8);
            q.Remove();
        }
    }
}
