using RestSharp;

namespace SkyNet.Client
{
    public class AccessTokenAuthenticator : OAuth2Authenticator
    {
        public AccessTokenAuthenticator(string accessToken) : base(accessToken)
        {
        }

        public override void Authenticate(IRestClient client, IRestRequest request)
        {
            request.Resource += string.Format("?access_token={0}", AccessToken);
        }
    }
}