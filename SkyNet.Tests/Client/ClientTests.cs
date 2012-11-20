using System;
using System.Collections.Generic;
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
            _client.CreateFolder(Folder.Root, "testFolder", "the description");
            var contents = _client.GetContents(Folder.Root);
            var createdFolder = contents.SingleOrDefault(f => f.Name.Equals("testFolder"));
            Assert.That(createdFolder, Is.Not.Null);
            Assert.That(createdFolder.Name, Is.EqualTo("testFolder"));
            Assert.That(createdFolder.Description, Is.EqualTo("the description"));
            _client.Delete(createdFolder.Id);
            contents = _client.GetContents(Folder.Root);
            Assert.That(contents.Any(f => f.Id.Equals(createdFolder.Id)), Is.False);
        }

        [Test]
        public void CreateDeleteFile()
        {
            _client.CreateFile(Folder.Root, "testFile", "text/plain");
            var contents = _client.GetContents(Folder.Root);
            var createdFile = contents.SingleOrDefault(f => f.Name.Equals("testFile"));
            Assert.That(createdFile, Is.Not.Null);
            Assert.That(createdFile.Name, Is.EqualTo("testFile"));
            _client.Delete(createdFile.Id);
            contents = _client.GetContents(Folder.Root);
            Assert.That(contents.Any(f => f.Id.Equals(createdFile.Id)), Is.False);
        }

    }
}
