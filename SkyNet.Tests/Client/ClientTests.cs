using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SkyNet.Model;

namespace SkyNet.Tests.Client
{
    [TestFixture]
    public class ClientTests
    {
        private readonly SkyNet.Client.Client _client = new SkyNet.Client.Client(TestInfo.ApiKey, TestInfo.ApiSecret, TestInfo.CallbackUrl, TestInfo.AccessToken, TestInfo.RefreshToken);
        
        [Test]
        public void GetRootFolder()
        {
            var contents = _client.GetContents(Folder.Root);
            Assert.That(contents.Any(f => f.Name.Equals("Public")));
        }
        
        [Test]
        public void CreateDeleteFolder()
        {
            var createdFolder = _client.CreateFolder(Folder.Root, "theFolder", "the description");
            Assert.That(createdFolder, Is.Not.Null);
            Assert.That(createdFolder.Name, Is.EqualTo("theFolder"));
            Assert.That(createdFolder.Description, Is.EqualTo("the description"));
            Cleanup(createdFolder.Id);
        }

        [Test]
        public void CreateDeleteFile()
        {
            var createdFile = _client.CreateFile(Folder.Root, "testFile", "text/plain");
            Assert.That(createdFile, Is.Not.Null);
            Assert.That(createdFile.Name, Is.EqualTo("testFile"));
            Cleanup(createdFile.Id);
        }

        [Test]
        public void WriteNewFileStream()
        {
            var content = new byte[] {1, 2, 3};
            using (var stream = new MemoryStream(content))
            {
                var writtenFile = _client.Write(Folder.Root, stream, "testFile", "text/plain");
                var actual = _client.Get(writtenFile.Id);
                Assert.That(actual, Is.Not.Null);
                Assert.That(actual.Name, Is.EqualTo("testFile"));
                Assert.That(actual.Size, Is.EqualTo(content.Length));
                Cleanup(actual.Id);
            }
        }

        [Test]
        public void WriteNewFileBytes()
        {
            var content = new byte[] { 1, 2, 3 };
            var writtenFile = _client.Write(Folder.Root, content, "testFile", "text/plain");
            var actual = _client.Get(writtenFile.Id);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Name, Is.EqualTo("testFile"));
            Assert.That(actual.Size, Is.EqualTo(content.Length));
            Cleanup(actual.Id);
        }

        private void Cleanup(string id)
        {
            _client.Delete(id);
            var contents = _client.GetContents(Folder.Root);
            Assert.That(contents.Any(f => f.Id.Equals(id)), Is.False);
        }


        [Test]
        public void CopyFile()
        {
            var file = _client.CreateFile(Folder.Root, "theFile", "text/plain");
            var folder = _client.CreateFolder(Folder.Root, "theFolder");
            try
            {
                var copy = _client.Copy(file.Id, folder.Id);
                Assert.That(copy, Is.Not.Null);
                Assert.That(copy.Id, Is.Not.EqualTo(file.Id));
                Assert.That(copy.Name, Is.EqualTo("theFile"));
                Assert.That(copy.Parent_Id, Is.EqualTo(folder.Id));
            }
            finally 
            {
                _client.Delete(folder.Id);
            }
        }

        [Test]
        public void MoveFile()
        {
            var file = _client.CreateFile(Folder.Root, "theFile", "text/plain");
            var folder = _client.CreateFolder(Folder.Root, "theFolder");
            try
            {
                var moved = _client.MoveFile(file.Id, folder.Id);
                Assert.That(moved, Is.Not.Null);
                Assert.That(moved.Name, Is.EqualTo("theFile"));
                Assert.That(moved.Id, Is.EqualTo(file.Id));
                Assert.That(moved.Parent_Id, Is.EqualTo(folder.Id));
            }
            finally
            {
                _client.Delete(folder.Id);
            }
        }

        [Test]
        public void MoveFolder()
        {
            var folder = _client.CreateFolder(Folder.Root, "theFolder");
            var otherFolder = _client.CreateFolder(Folder.Root, "otherFolder", "text/plain");
            try
            {
                var moved = _client.MoveFile(otherFolder.Id, folder.Id);
                Assert.That(moved, Is.Not.Null);
                Assert.That(moved.Name, Is.EqualTo("otherFolder"));
                Assert.That(moved.Id, Is.EqualTo(otherFolder.Id));
                Assert.That(moved.Parent_Id, Is.EqualTo(folder.Id));
            }
            finally
            {
                _client.Delete(folder.Id);
            }
        }

        [Test]
        public void RenameFile()
        {
            var file = _client.CreateFile(Folder.Root, "theFile", "text/plain");
            try
            {
                var renamed = _client.RenameFile(file.Id, "newName");
                Assert.That(renamed, Is.Not.Null);
                Assert.That(renamed.Id, Is.EqualTo(file.Id));
                Assert.That(renamed.Name, Is.EqualTo("newName"));
            }
            finally
            {
                _client.Delete(file.Id);
            }
        }

        [Test]
        public void RenameFolder()
        {
            var folder = _client.CreateFolder(Folder.Root, "theFolder");
            try
            {
                var renamed = _client.RenameFolder(folder.Id, "newName");
                Assert.That(renamed, Is.Not.Null);
                Assert.That(renamed.Id, Is.EqualTo(folder.Id));
                Assert.That(renamed.Name, Is.EqualTo("newName"));
            }
            finally
            {
                _client.Delete(folder.Id);
            }
        }


        [Test, Ignore("run this if a test fails and you need to get rid of the theFolder that it created")]
        public void DeleteTestFolder()
        {
            DeleteFromRootFolder("theFile");
            DeleteFromRootFolder("theFolder");
            DeleteFromRootFolder("otherFolder");
            DeleteFromRootFolder("newName");
        }

        private void DeleteFromRootFolder(string itemName)
        {
            var contents = _client.GetContents(Folder.Root);
            var actual = contents.SingleOrDefault(f => f.Name.Equals(itemName));
            if (actual != null)
            {
                _client.Delete(actual.Id);
            }
        }
    }
}
