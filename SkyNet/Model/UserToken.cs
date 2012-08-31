namespace SkyNet.Model
{
    public class UserToken
    {
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public long Expires_In { get; set; }
        public string Scope { get; set; }
        public string Token_Type { get; set; }
    }
}