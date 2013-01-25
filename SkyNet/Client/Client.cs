using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using Microsoft.Win32;
using RestSharp;
using RestSharp.Deserializers;
using SkyNet.Model;
using File = SkyNet.Model.File;

namespace SkyNet.Client
{
    public class Client
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _callbackUrl;
        private string _refreshToken;
        private readonly RestClient _restAuthorizationClient;
        private readonly RestClient _restContentClient;
        private readonly RequestGenerator _requestGenerator;

        private const string OAuthUrlBase = @"https://login.live.com";
        private const string ContentUrlBase = @"https://apis.live.net/v5.0/";

        public Client(string clientId, string clientSecret, string callbackUrl, string accessToken = null, string refreshToken = null)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _callbackUrl = callbackUrl;

            _restAuthorizationClient = new RestClient(OAuthUrlBase);
            _restAuthorizationClient.ClearHandlers();
            _restAuthorizationClient.AddHandler("*", new JsonDeserializer());

            _restContentClient = new RestClient(ContentUrlBase);
            _restContentClient.ClearHandlers();
            _restContentClient.AddHandler("*", new JsonDeserializer());

            _requestGenerator = new RequestGenerator();

            SetUserToken(new UserToken {Access_Token = accessToken, Refresh_Token = refreshToken});
        }

        public string GetAuthorizationRequestUrl(IEnumerable<Scope> requestedScopes)
        {
            var request = _requestGenerator.Authorize(_clientId, _callbackUrl, requestedScopes);
            return _restAuthorizationClient.BuildUri(request).AbsoluteUri;
        }

        public UserToken GetAccessToken(string authorizationCode)
        {
            var getAccessToken = _requestGenerator.GetAccessToken(_clientId, _clientSecret, _callbackUrl, authorizationCode);
            var token = ExecuteAuthorizationRequest<UserToken>(getAccessToken);
            SetUserToken(token);
            return token;
        }

        public UserToken RefreshAccessToken()
        {
            var refreshAccessToken = _requestGenerator.RefreshAccessToken(_clientId, _clientSecret, _callbackUrl, _refreshToken);
            var token = ExecuteAuthorizationRequest<UserToken>(refreshAccessToken);
            SetUserToken(token);
            return token;
        }

        public File Get(string id = null)
        {
            return ExecuteContentRequest<File>(_requestGenerator.Get(id));
        }

        public IEnumerable<File> GetContents(string id)
        {
            var result = ExecuteContentRequest<File>(_requestGenerator.GetContents(id));
            return result.Data;
        }

        public Folder CreateFolder(string parentFolderId, string name, string description = null)
        {
            return ExecuteContentRequest<Folder>(_requestGenerator.CreateFolder(parentFolderId, name, description));
        }

        public File CreateFile(string parentFolderId, string name, string contentType)
        {
            return Write(parentFolderId, new byte[0], name, contentType);
        }

        public File Write(string parentFolderId, byte[] content, string name, string contentType)
        {
            using (var stream = new MemoryStream(content))
            {
                return Write(parentFolderId, stream, name, contentType);
            }
        }

        public File Write(string parentFolderId, Stream content, string name, string contentType)
        {
            return ExecuteContentRequest<File>(_requestGenerator.Write(parentFolderId, content, name, contentType));
        }

        public byte[] Read(string id, long startByte, long endByte)
        {
            var response = ExecuteContentRequest(_requestGenerator.Read(id, startByte, endByte));
            return response.RawBytes;
        }

        public File Copy(string sourceId, string newParentId)
        {
            return ExecuteContentRequestAsPost<File>(_requestGenerator.Copy(sourceId, newParentId), "COPY");
        }

        public void Rename(string id, string name)
        {
            ExecuteContentRequest(_requestGenerator.Rename(id, name));
        }

        public File RenameFile(string id, string name)
        {
            return ExecuteContentRequest<File>(_requestGenerator.Rename(id, name));
        }

        public Folder RenameFolder(string id, string name)
        {
            return ExecuteContentRequest<Folder>(_requestGenerator.Rename(id, name));
        }

        public void Move(string id, string newParentId)
        {
            ExecuteContentRequestAsPost(_requestGenerator.Move(id, newParentId), "MOVE");
        }

        public File MoveFile(string id, string newParentId)
        {
            return ExecuteContentRequestAsPost<File>(_requestGenerator.Move(id, newParentId), "MOVE");
        }

        public Folder MoveFolder(string id, string newParentId)
        {
            return ExecuteContentRequestAsPost<Folder>(_requestGenerator.Move(id, newParentId), "MOVE");
        }

        public void Delete(string id)
        {
            ExecuteContentRequest(_requestGenerator.Delete(id));
        }

        private void SetUserToken(UserToken token)
        {
            _refreshToken = token.Refresh_Token;
            _restContentClient.Authenticator = new AccessTokenAuthenticator(token.Access_Token);
        }

        private T ExecuteAuthorizationRequest<T>(IRestRequest restRequest) where T : new()
        {
            return ExecuteRequest<T>(_restAuthorizationClient, restRequest);
        }

        private IRestResponse ExecuteContentRequest(IRestRequest restRequest)
        {
            var restResponse = _restContentClient.Execute(restRequest);
            CheckForError(restResponse);
            return restResponse;
        }

        private void ExecuteContentRequestAsPost(IRestRequest restRequest, string method)
        {
            var restResponse = _restContentClient.ExecuteAsPost(restRequest, method);
            CheckForError(restResponse);
        }

        private T ExecuteContentRequestAsPost<T>(IRestRequest restRequest, string method) where T:new()
        {
            var restResponse = _restContentClient.ExecuteAsPost<T>(restRequest, method);
            CheckForError(restResponse);
            return restResponse.Data;
        }

        private T ExecuteContentRequest<T>(IRestRequest restRequest) where T : new()
        {
            var restResponse = _restContentClient.Execute<T>(restRequest);
            CheckForError(restResponse);
            return restResponse.Data;
        }

        private static void CheckForError(IRestResponse restResponse)
        {
            var statusCode = restResponse.StatusCode;

            if (statusCode == HttpStatusCode.InternalServerError
                || statusCode == HttpStatusCode.BadGateway
                || statusCode == HttpStatusCode.BadRequest
                || statusCode == HttpStatusCode.Unauthorized)
                throw new HttpException((int) statusCode, restResponse.Content);
        }

        private static T ExecuteRequest<T>(RestClient restContentClient, IRestRequest restRequest) where T : new()
        {
            return restContentClient.Execute<T>(restRequest).Data;
        }

        private static void Copy(byte[] input, Stream output)
        {
            using (var memoryStream = new MemoryStream(input))
            {
                Copy(memoryStream, output);
            }
        }

        private static void Copy(Stream input, Stream output)
        {
            var buffer = new byte[16*1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
    }
}