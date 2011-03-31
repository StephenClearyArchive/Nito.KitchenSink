using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.KitchenSink.FileSystemPaths;

namespace Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void EmptyPathProperties()
        {
            var path = string.Empty.AsFileSystemPath();
            Assert.AreEqual(string.Empty, path.Path);
            Assert.AreEqual(string.Empty, path.Root);
            Assert.AreEqual(string.Empty, path.DirectoryName);
            Assert.AreEqual(string.Empty, path.FileName);
            Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
            Assert.AreEqual(string.Empty, path.Extension);
            Assert.IsFalse(path.HasExtension);
            Assert.IsFalse(path.IsAbsolute);
        }

        [TestMethod]
        public void RootPathWithBackslashProperties()
        {
            var path = @"C:\".AsFileSystemPath();
            Assert.AreEqual(@"C:\", path.Path);
            Assert.AreEqual(@"C:\", path.Root);
            Assert.AreEqual(string.Empty, path.DirectoryName);
            Assert.AreEqual(string.Empty, path.FileName);
            Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
            Assert.AreEqual(string.Empty, path.Extension);
            Assert.IsFalse(path.HasExtension);
            Assert.IsTrue(path.IsAbsolute);
        }

        [TestMethod]
        public void RootPathWithoutBackslashProperties()
        {
            var path = @"C:".AsFileSystemPath();
            Assert.AreEqual(@"C:", path.Path);
            Assert.AreEqual(@"C:", path.Root);
            Assert.AreEqual(string.Empty, path.DirectoryName);
            Assert.AreEqual(string.Empty, path.FileName);
            Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
            Assert.AreEqual(string.Empty, path.Extension);
            Assert.IsFalse(path.HasExtension);
            Assert.IsTrue(path.IsAbsolute);
        }

        [TestMethod]
        public void FileNameWithExtensionProperties()
        {
            var path = @"test.tmp".AsFileSystemPath();
            Assert.AreEqual(@"test.tmp", path.Path);
            Assert.AreEqual(string.Empty, path.Root);
            Assert.AreEqual(string.Empty, path.DirectoryName);
            Assert.AreEqual(@"test.tmp", path.FileName);
            Assert.AreEqual(@"test", path.FileNameWithoutExtension);
            Assert.AreEqual(@".tmp", path.Extension);
            Assert.IsTrue(path.HasExtension);
            Assert.IsFalse(path.IsAbsolute);
        }

        [TestMethod]
        public void FileNameWithoutExtensionProperties()
        {
            var path = @"test".AsFileSystemPath();
            Assert.AreEqual(@"test", path.Path);
            Assert.AreEqual(string.Empty, path.Root);
            Assert.AreEqual(string.Empty, path.DirectoryName);
            Assert.AreEqual(@"test", path.FileName);
            Assert.AreEqual(@"test", path.FileNameWithoutExtension);
            Assert.AreEqual(string.Empty, path.Extension);
            Assert.IsFalse(path.HasExtension);
            Assert.IsFalse(path.IsAbsolute);
        }

        [TestMethod]
        public void ExtensionWithoutFileNameProperties()
        {
            var path = @".tmp".AsFileSystemPath();
            Assert.AreEqual(@".tmp", path.Path);
            Assert.AreEqual(string.Empty, path.Root);
            Assert.AreEqual(string.Empty, path.DirectoryName);
            Assert.AreEqual(@".tmp", path.FileName);
            Assert.AreEqual(string.Empty, path.FileNameWithoutExtension);
            Assert.AreEqual(@".tmp", path.Extension);
            Assert.IsTrue(path.HasExtension);
            Assert.IsFalse(path.IsAbsolute);
        }
    }
}
