using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RestSharp;
using RestSharp.Validation;
using SkyNet.Util;

namespace SkyNet.Client
{
    public class RequestGenerator
    {
        private static readonly UTF8Encoding UTF8Encoder = new UTF8Encoding(); 

        private const string OAuthResource = "oauth20_{verb}.srf";
        private const string AuthorizeVerb = "authorize";
        private const string TokenVerb = "token";
        private const string SkyDriveRootFolder = "me/skydrive";

        public RestRequest Authorize(string clientId, string callbackUrl, IEnumerable<Scope> requestedScopes)
        {
            var request = new RestRequest(Method.GET) {Resource = OAuthResource};
            request.AddParameter("verb", AuthorizeVerb, ParameterType.UrlSegment);
            request.AddParameter("client_id", clientId);
            request.AddParameter("scope", string.Join(" ", requestedScopes.OrderBy(s => s).Select(s => s.GetDescription())));
            request.AddParameter("response_type", "code");
            request.AddParameter("redirect_uri", callbackUrl);
            return request;
        }

        public RestRequest GetAccessToken(string clientId, string clientSecret, string callbackUrl, string authorizationCode)
        {
            var request = new RestRequest(Method.POST) {Resource = OAuthResource};
            request.AddParameter("verb", TokenVerb, ParameterType.UrlSegment);
            request.AddParameter("client_id", clientId);
            request.AddParameter("redirect_uri", callbackUrl);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("code", authorizationCode);
            request.AddParameter("grant_type", "authorization_code");
            return request;
        }

        public RestRequest RefreshAccessToken(string clientId, string clientSecret, string callbackUrl, string refreshToken)
        {
            var request = new RestRequest(Method.POST) {Resource = OAuthResource};
            request.AddParameter("verb", TokenVerb, ParameterType.UrlSegment);
            request.AddParameter("client_id", clientId);
            request.AddParameter("redirect_uri", callbackUrl);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("refresh_token", refreshToken);
            request.AddParameter("grant_type", "refresh_token");
            return request;
        }

        public RestRequest Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return ContentRequest(Method.GET, SkyDriveRootFolder);
            }

            var request = ContentRequest(Method.GET, "{id}");
            request.AddUrlSegment("id", id);
            return request;
        }

        public RestRequest GetContents(string id)
        {
            var path = string.IsNullOrEmpty(id) ? SkyDriveRootFolder : id;
            var request = ContentRequest(Method.GET, "{id}/files");
            request.AddUrlSegment("id", path);
            return request;
        }

        public RestRequest CreateFolder(string parentFolderId, string name, string description = null)
        {
            Require.Argument("parentFolderId", parentFolderId);
            Require.Argument("name", name);

            var request = ContentRequest(Method.POST, "{id}");
            request.AddUrlSegment("id", parentFolderId);
            request.AddBody(new {name, description});
            return request;
        }

        public RestRequest Copy(string sourceId, string newParentId)
        {
            var request = CopyMove(sourceId, newParentId);
            return request;
        }

        public RestRequest Delete(string id)
        {
            Require.Argument("id", id);

            var request = ContentRequest(Method.DELETE, "{id}");
            request.AddParameter("id", id, ParameterType.UrlSegment);
            return request;
        }

        public RestRequest Write(string parentFolderId, Stream content, string name, string contentType)
        {
            Require.Argument("id", parentFolderId);
            Require.Argument("name", name);

            var request = ContentRequest(Method.POST, "{id}/files");
            request.AddUrlSegment("id", parentFolderId);
            request.AddFile("file", content.CopyTo, Encode(name), contentType);
            return request;
        }

        public RestRequest Read(string id, long startByte, long endByte)
        {
            Require.Argument("id", id);

            var request = ContentRequest(Method.GET, "{id}/content");
            request.AddUrlSegment("id", id);
            request.AddHeader("Range", string.Format("bytes={0}-{1}", startByte, endByte));
            return request;
        }

        public RestRequest Move(string sourceId, string newParentId)
        {
            var request = CopyMove(sourceId, newParentId);
            return request;
        }

        private RestRequest CopyMove(string sourceId, string newParentId)
        {
            Require.Argument("sourceId", sourceId);
            Require.Argument("newParentId", newParentId);

            var request = ContentRequest(Method.POST, "{id}");
            request.AddUrlSegment("id", sourceId);
            request.AddBody(new{destination= newParentId});
            return request;
        }

        public RestRequest Rename(string id, string name)
        {
            Require.Argument("id", id);
            Require.Argument("name", name);

            var request = ContentRequest(Method.PUT, "{id}");
            request.AddUrlSegment("id", id);
            request.AddBody(new {name});
            return request;
        }

        private static string Encode(string name)
        {
            return UTF8Encoder.GetString(UTF8Encoder.GetBytes(name));
        }

        private RestRequest ContentRequest(Method method, string resource)
        {
            Require.Argument("resource", resource);

            var request = new RestRequest(method) {Resource = Append(resource), RequestFormat = DataFormat.Json};
            return request;
        }

        private string Append(string apendage)
        {
            return string.Format("/{0}", apendage.Trim(new[] { '/' }));
        }
    }
}