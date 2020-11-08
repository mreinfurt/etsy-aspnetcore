using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using OAuth;

namespace etsy_aspnetcore.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EtsyController : ControllerBase
    {
        private const string RequestUrl = "https://openapi.etsy.com/v2/oauth/request_token";
        private const string RequestAccessTokenUrl = "https://openapi.etsy.com/v2/oauth/access_token";
        
        private const string ConsumerKey = "YOUR_CONSUMER_KEY";
        private const string ConsumerSecret = "YOUR_CONSUMER_SECRET";

        private static string TokenSecret { get; set; }
        
        [HttpGet]
        public IActionResult Login()
        {
            // Configure our OAuth client
            var client = new OAuthRequest
            {
                Method = "GET",
                Type = OAuthRequestType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                RequestUrl = RequestUrl,
                CallbackUrl = "https://localhost:5001/api/etsy/callback"
            };
            
            // Build request url and send the request
            var url = client.RequestUrl + "?" + client.GetAuthorizationQuery();
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            
            using var dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();

            // Parse login_url and oauth_token_secret from response
            var loginUrl = HttpUtility.ParseQueryString(responseFromServer).Get("login_url");
            TokenSecret = HttpUtility.ParseQueryString(responseFromServer).Get("oauth_token_secret");

            return Redirect(loginUrl);
        }

        private static string OAuthToken { get; set; }
        private static string OAuthTokenSecret { get; set; }

        [HttpGet]
        public IActionResult Callback()
        {
            // Read token and verifier
            string token = Request.Query["oauth_token"];
            string verifier = Request.Query["oauth_verifier"];

            // Create access token request
            var client = new OAuthRequest
            {
                Method = "GET",
                Type = OAuthRequestType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Token = token,
                TokenSecret = TokenSecret,
                RequestUrl = RequestAccessTokenUrl,
                Verifier = verifier
            };
            
            // Build request url and send the request
            var url = client.RequestUrl + "?" + client.GetAuthorizationQuery();
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            using var dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();
            
            // Parse and save access token and secret
            OAuthToken = HttpUtility.ParseQueryString(responseFromServer).Get("oauth_token");
            OAuthTokenSecret = HttpUtility.ParseQueryString(responseFromServer).Get("oauth_token_secret");
            
            return Ok();
        }
        
        [HttpGet]
        public async Task<IActionResult> OpenOrders()
        {
            const string requestUrl = "https://openapi.etsy.com/v2/shops/YOURSHOPNAME/receipts/open?";
            
            var client = new OAuthRequest
            {
                Method = "GET",
                Type = OAuthRequestType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Token = OAuthToken,
                TokenSecret = OAuthTokenSecret,
                RequestUrl = requestUrl,
            };
            
            var url = requestUrl + client.GetAuthorizationQuery();
            var result = await url.GetStringAsync();
            return Content(result, "application/json");
        }
    }
}