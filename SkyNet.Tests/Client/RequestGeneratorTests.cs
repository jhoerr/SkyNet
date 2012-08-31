using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RestSharp;
using SkyNet.Client;

namespace SkyNet.Tests.Client
{
    [TestFixture]
    public class RequestGeneratorTests
    {
        private readonly RequestGenerator _requestGenerator = new RequestGenerator();

        private const string UrlBase = "http://localhost";
        private const string ClientId = "ClientId";
        private const string ClientSecret = "ClientSecret";
        private const string RedirectUri = "RedirectUri";
        private const string AuthorizationCode = "AuthorizationCode";
        private const string RefreshToken = "RefreshToken";

        [Test]
        public void GetAccessRequestUrl()
        {
            var authorizeRequest = _requestGenerator.Authorize(ClientId, RedirectUri, new[]{Scope.SkyDriveUpdate, Scope.OfflineAccess });
            var uri = new RestClient(UrlBase).BuildUri(authorizeRequest);
            Assert.That(uri.AbsolutePath, Is.EqualTo("/oauth20_authorize.srf"));
            AssertHasMethod(authorizeRequest, Method.GET);
            AssertHasParameter(authorizeRequest, "client_id", ClientId);
            AssertHasParameter(authorizeRequest, "redirect_uri", RedirectUri);
            AssertHasParameter(authorizeRequest, "scope", "wl.offline_access wl.skydrive_update");
            AssertHasParameter(authorizeRequest, "response_type", "code");
        }

        [TestCase(Result = @"http://localhost/oauth20_token.srf")]
        public string GetAccessToken()
        {
            var accessTokenRequest = _requestGenerator.GetAccessToken(ClientId, ClientSecret, RedirectUri, AuthorizationCode);
            AssertHasMethod(accessTokenRequest, Method.POST);
            AssertHasParameter(accessTokenRequest, "client_id", ClientId);
            AssertHasParameter(accessTokenRequest, "client_secret", ClientSecret);
            AssertHasParameter(accessTokenRequest, "redirect_uri", RedirectUri);
            AssertHasParameter(accessTokenRequest, "code", AuthorizationCode);
            AssertHasParameter(accessTokenRequest, "grant_type", "authorization_code");
            return GetAbsoluteUri(accessTokenRequest);
        }

        [TestCase(Result = @"http://localhost/oauth20_token.srf")]
        public string GetRefreshToken()
        {
            var refreshTokenRequest = _requestGenerator.RefreshAccessToken(ClientId, ClientSecret, RedirectUri, RefreshToken);
            AssertHasMethod(refreshTokenRequest, Method.POST);
            AssertHasParameter(refreshTokenRequest, "client_id", ClientId);
            AssertHasParameter(refreshTokenRequest, "client_secret", ClientSecret);
            AssertHasParameter(refreshTokenRequest, "redirect_uri", RedirectUri);
            AssertHasParameter(refreshTokenRequest, "refresh_token", RefreshToken);
            AssertHasParameter(refreshTokenRequest, "grant_type", "refresh_token");
            return GetAbsoluteUri(refreshTokenRequest);
        }

        [TestCase(null, Result = "http://localhost/me/skydrive")]
        [TestCase("", Result = "http://localhost/me/skydrive")]
        [TestCase("123", Result = "http://localhost/123")]
        public string Get(string id)
        {
            var restRequest = _requestGenerator.Get(id);
            AssertHasMethod(restRequest, Method.GET);
            return GetAbsoluteUri(restRequest);
        }

        [TestCase(null, Result = "http://localhost/me/skydrive/files")]
        [TestCase("", Result = "http://localhost/me/skydrive/files")]
        [TestCase("123", Result = "http://localhost/123/files")]
        public string GetContents(string id)
        {
            var restRequest = _requestGenerator.GetContents(id);
            AssertHasMethod(restRequest, Method.GET);
            return GetAbsoluteUri(restRequest);
        }

        [TestCase("123", "folder", Result = "http://localhost/123")]
        public string CreateFolder(string parentId, string name)
        {
            var restRequest = _requestGenerator.CreateFolder(parentId, name);
            AssertHasMethod(restRequest, Method.POST);
            AssertHasParameter(restRequest, "name", name);
            AssertHasParameter(restRequest, "description", string.Empty);
            return GetAbsoluteUri(restRequest);
        }

        [TestCase("123", "file.txt", Result = "http://localhost/123/files")]
        public string FileCreateOrWrite(string parentId, string name)
        {
            using (var stream = new MemoryStream())
            {
                var restRequest = _requestGenerator.Write(parentId, stream, name, "content");
                AssertHasMethod(restRequest, Method.POST);
                AssertHasFileWithName(restRequest, name);
                return GetAbsoluteUri(restRequest);
            }
        }

        [TestCase("123", Result = "http://localhost/123")]
        public string Delete(string id)
        {
            var restRequest = _requestGenerator.Delete(id);
            AssertHasMethod(restRequest, Method.DELETE);
            return GetAbsoluteUri(restRequest);
        }

        [TestCase("123", "456", Result = "http://localhost/123")]
        public string Copy(string id, string newParentId)
        {
            var restRequest = _requestGenerator.Copy(id, newParentId);
            AssertHasMethod(restRequest, Method.POST); // This is a function of how RestSharp processes non-standard HTTP methods...
            AssertHasParameter(restRequest, "destination", newParentId);
            return GetAbsoluteUri(restRequest);
        }

        [TestCase("123", "456", Result = "http://localhost/123")]
        public string Move(string id, string newParentId)
        {
            var restRequest = _requestGenerator.Move(id, newParentId);
            AssertHasMethod(restRequest, Method.POST); // This is a function of how RestSharp processes non-standard HTTP methods...
            AssertHasParameter(restRequest, "destination", newParentId);
            return GetAbsoluteUri(restRequest);
        }

        [TestCase("123", "newName", Result = "http://localhost/123")]
        public string Rename(string id, string newName)
        {
            var restRequest = _requestGenerator.Rename(id, newName);
            AssertHasMethod(restRequest, Method.PUT);
            AssertHasParameter(restRequest, "name", newName);
            return GetAbsoluteUri(restRequest);
        }

        private static void AssertHasMethod(RestRequest restRequest, Method expected)
        {
            Assert.That(restRequest.Method, Is.EqualTo(expected));
        }

        [TestCase("123", Result = "http://localhost/123/content")]
        public string Read(string id)
        {
            var restRequest = _requestGenerator.Read(id, 0, 10);
            AssertHasMethod(restRequest, Method.GET);
            AssertHasHeader(restRequest, "Range", "bytes=0-10");
            return GetAbsoluteUri(restRequest);
        }

        private void AssertHasHeader(RestRequest restRequest, string key, string value)
        {
            AssertHasParameterOfType(restRequest, key, value, ParameterType.HttpHeader);
        }

        private void AssertHasParameter(RestRequest restRequest, string key, string value)
        {
            AssertHasParameterOfType(restRequest, key, value, ParameterType.GetOrPost);
        }

        private static void AssertHasParameterOfType(RestRequest restRequest, string key, string value, ParameterType parameterType)
        {
            var parameter = restRequest.Parameters.SingleOrDefault(p => p.Name.Equals(key));
            Assert.That(parameter, Is.Not.Null);
            Assert.That(parameter.Value, Is.EqualTo(value));
            Assert.That(parameter.Type, Is.EqualTo(parameterType));
        }

        private void AssertHasFileWithName(RestRequest restRequest, string name)
        {
            var file = restRequest.Files.Any(f => f.FileName.Equals(name));
            Assert.That(file, Is.Not.Null);
        }

        private static string GetAbsoluteUri(IRestRequest accessTokenRequest)
        {
            return new RestClient(UrlBase).BuildUri(accessTokenRequest).AbsoluteUri;
        }
    }
}
