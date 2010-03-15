using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink;

namespace UnitTests
{
    [TestClass]
    public class DequeUnitTests
    {
        [TestMethod]
        public void Deque_DefaultConstructed_IsEmpty()
        {
            Deque<int> q = new Deque<int>();
            int result = q.Count;
            Assert.AreEqual(0, result, "Default constructed deque should be empty");
        }

        [TestMethod]
        public void Deque_ConstructedWithCapacity_RemembersCapacity()
        {
            Deque<int> q = new Deque<int>(13);
            int result = q.Capacity;
            Assert.AreEqual(13, result, "Deque constructed with capacity should remember that capacity");
        }

        [TestMethod]
        public void Deque_ConstructedWithCollection_RemembersCollection()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            Assert.AreEqual(5, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_ConstructedWithEmptyCollection_HasNonZeroCapacity()
        {
            Deque<int> q = new Deque<int>(new int[] { });
            Assert.AreEqual(0, q.Count, "Deque should remember its items");
            Assert.AreNotEqual(0, q.Capacity, "Deque should not have a zero Capacity");
        }

        [TestMethod]
        public void Deque_AddToFront_AddsItem()
        {
            Deque<int> q = new Deque<int>();
            q.AddToFront(13);
            Assert.AreEqual(1, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_AddToFront_AddsItemInOrder()
        {
            Deque<int> q = new Deque<int>();
            q.AddToFront(13);
            q.AddToFront(17);
            Assert.AreEqual(2, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 17, 13 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_AddToBack_AddsItem()
        {
            Deque<int> q = new Deque<int>();
            q.AddToBack(13);
            Assert.AreEqual(1, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_AddToBack_AddsItemInOrder()
        {
            Deque<int> q = new Deque<int>();
            q.AddToBack(13);
            q.AddToBack(17);
            Assert.AreEqual(2, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 17 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_AfterBackResize_RemembersItems()
        {
            Deque<int> q = new Deque<int>(1);
            q.Add(13);
            q.Add(17);
            Assert.AreEqual(2, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 17 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_AfterFrontResize_RemembersItems()
        {
            Deque<int> q = new Deque<int>(1);
            q.AddToFront(13);
            q.AddToFront(17);
            Assert.AreEqual(2, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 17, 13 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_InsertRange_InsertsRange()
        {
            Deque<int> q = new Deque<int>();
            q.InsertRange(0, new[] { 13, 17 });
            Assert.AreEqual(2, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 17 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_InsertAtFront_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.Insert(0, 13);
            Assert.AreEqual(6, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 1, 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_InsertBeforeMidpoint_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.Insert(1, 13);
            Assert.AreEqual(6, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] {1, 13, 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_InsertAfterMidpoint_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.Insert(4, 13);
            Assert.AreEqual(6, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 13, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_InsertAtBack_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.Insert(5, 13);
            Assert.AreEqual(6, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5, 13 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_RemoveAtFront_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.RemoveAt(0);
            Assert.AreEqual(4, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_RemoveBeforeMidpoint_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.RemoveAt(1);
            Assert.AreEqual(4, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_RemoveAfterMidpoint_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.RemoveAt(3);
            Assert.AreEqual(4, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_RemoveAtBack_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.RemoveAt(4);
            Assert.AreEqual(4, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Deque_RemoveRange_RemovesRange()
        {
            Deque<int> q = new Deque<int>(new[] { 13, 17 });
            q.RemoveRange(0, 2);
            Assert.AreEqual(0, q.Count, "Deque should remember its items");
        }

        [TestMethod]
        public void InsertRange_AtFront_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.InsertRange(0, new [] { 13, 17 });
            Assert.AreEqual(7, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 17, 1, 2, 3, 4, 5 }), "Deque should remember its items");
        }
        
        [TestMethod]
        public void InsertRange_BeforeMidpoint_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.InsertRange(1, new[] { 13, 17 });
            Assert.AreEqual(7, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 13, 17, 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void InsertRange_AfterMidpoint_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.InsertRange(4, new[] { 13, 17 });
            Assert.AreEqual(7, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 13, 17, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void InsertRange_AtBack_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.InsertRange(5, new[] { 13, 17 });
            Assert.AreEqual(7, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5, 13, 17 }), "Deque should remember its items");
        }

        [TestMethod]
        public void InsertEmptyRange_DoesNothing()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.InsertRange(3, new int[] { });
            Assert.AreEqual(5, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void RemoveRange_AtFront_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5, 6, 7 });
            q.RemoveRange(0, 2);
            Assert.AreEqual(5, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 3, 4, 5, 6, 7 }), "Deque should remember its items");
        }

        [TestMethod]
        public void RemoveRange_BeforeMidpoint_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5, 6, 7 });
            q.RemoveRange(1, 2);
            Assert.AreEqual(5, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 4, 5, 6, 7 }), "Deque should remember its items");
        }

        [TestMethod]
        public void RemoveRange_AfterMidpoint_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5, 6, 7 });
            q.RemoveRange(4, 2);
            Assert.AreEqual(5, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 7 }), "Deque should remember its items");
        }

        [TestMethod]
        public void RemoveRange_AtEnd_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5, 6, 7 });
            q.RemoveRange(5, 2);
            Assert.AreEqual(5, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void RemoveEmptyRange_DoesNothing()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.RemoveRange(3, 0);
            Assert.AreEqual(5, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Setting Capacity to a value less than Count should be rejected")]
        public void Capacity_LessThanCount_IsRejected()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.Capacity = 4;
        }

        [TestMethod]
        public void Capacity_SetToItself_DoesNothing()
        {
            Deque<int> q = new Deque<int>(3);
            q.Capacity = 3;
            Assert.AreEqual(3, q.Capacity, "Deque should remember its capacity");
        }

        [TestMethod]
        public void Capacity_SetToZero_IsNotZero()
        {
            Deque<int> q = new Deque<int>(3);
            q.Capacity = 0;
            Assert.AreNotEqual(0, q.Capacity, "Deque should not allow its capacity to be set to zero");
        }

        [TestMethod]
        public void Capacity_ResizeWhileSplit_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(4);
            q.InsertRange(0, new[] { 13, 17, 19 });
            q.Insert(0, 11);
            q.Add(23);
            Assert.IsTrue(q.SequenceEqual(new[] { 11, 13, 17, 19, 23 }), "Deque should remember its items");
        }

        [TestMethod]
        public void Clear_ClearsItems()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.Clear();
            Assert.AreEqual(0, q.Count, "Deque should remember its items");
        }

        [TestMethod]
        public void SetItem_UpdatesItems()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.AddToFront(13);
            q[3] = 17;
            Assert.AreEqual(6, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 13, 1, 2, 17, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        public void RemoveFromBack_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.RemoveFromBack();
            Assert.AreEqual(4, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 1, 2, 3, 4 }), "Deque should remember its items");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Should not be able to remove elements from an empty deque")]
        public void RemoveFromBack_WhenEmpty_IsRejected()
        {
            Deque<int> q = new Deque<int>();
            q.RemoveFromBack();
        }

        [TestMethod]
        public void RemoveFromFront_MaintainsOrder()
        {
            Deque<int> q = new Deque<int>(new[] { 1, 2, 3, 4, 5 });
            q.RemoveFromFront();
            Assert.AreEqual(4, q.Count, "Deque should remember its items");
            Assert.IsTrue(q.SequenceEqual(new[] { 2, 3, 4, 5 }), "Deque should remember its items");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Should not be able to remove elements from an empty deque")]
        public void RemoveFromFront_WhenEmpty_IsRejected()
        {
            Deque<int> q = new Deque<int>();
            q.RemoveFromFront();
        }
    }
}
