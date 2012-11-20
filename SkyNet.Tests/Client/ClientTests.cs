using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SkyNet.Tests.Client
{
    [TestFixture]
    public class ClientTests
    {
        private readonly SkyNet.Client.Client _client = new SkyNet.Client.Client(TestInfo.ApiKey, TestInfo.ApiSecret, TestInfo.CallbackUrl, TestInfo.AccessToken, TestInfo.RefreshToken);
        [Test]
        public void CreateFolder()
        {
            _client.CreateFolder(null, "testFolder");
            var contents = _client.GetContents(null);
            Assert.That(contents.Any(f => f.Name.Equals("testFolder")));
        }
    }
}
