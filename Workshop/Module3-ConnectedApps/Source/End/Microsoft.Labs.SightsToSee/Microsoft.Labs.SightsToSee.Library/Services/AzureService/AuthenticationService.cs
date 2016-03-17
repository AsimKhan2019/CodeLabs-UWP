using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;

namespace Microsoft.Labs.SightsToSee.Library.Services.AzureService
{
    public class AuthenticationService
    {
        private MobileServiceClient _mobileServiceClient;

        public AuthenticationService(MobileServiceClient mobileServiceClient)
        {
            _mobileServiceClient = mobileServiceClient;
        }

#if SERVER_INITIATED
        // Server-initiated authentication - cannot offer a SSO experience
        public async Task<Tuple<bool, string>> AuthenticateAsync()
        {
            // Authentication required for the cloud storage
            bool success = false;
            string message = string.Empty;
            try
            {
                // Sign-in using Microsoft Account authentication.
                MobileServiceUser user = await _mobileServiceClient
                    .LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                Debug.WriteLine(message = string.Format("You are now signed in - {0}", user.UserId));

                success = true;
            }
            catch (InvalidOperationException)
            {
                message = "You must log in. Login Required";
            }

            return Tuple.Create(success, message);
        }
#else
        // Client-initiated authentication - can offer a SSO experience
        public async Task<Tuple<bool, string>> AuthenticateAsync()
        {
            await CopyRedirectUriToClipboard();
            //#error Comment out this #error, then Run app once to get App Redirect Uri. Configure app in portal.

            // Authentication required for the cloud storage
            bool success = false;
            string message = string.Empty;
            try
            {
                // App client ID is how the app uniquely identifies itself to the Identity provider (Microsoft Azure Active Directory
                // or Microsoft Account Directory)
                // You must Associate your app with the Store, and then go to the Live Services site to get your app client Id
                string appClientId = "000000004818B85D";

                // Sign-in 
                string token = await AuthenticationHelper.GetTokenHelperAsync(appClientId);

                if (token != null)
                {
                    // Sign-in to the Mobile Apps service, passing the authentication token
                    JObject payload = new JObject();
                    payload["access_token"] = token;
                    MobileServiceUser user = await _mobileServiceClient.
                        LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount, payload);

                    Debug.WriteLine(message = string.Format("You are now signed in - {0}", user.UserId));

                    success = true;
                }
                else
                {
                    message = "AuthenticationHelper could not get auth token";
                    success = false;
                }
            }
            catch (InvalidOperationException)
            {
                message = "You must log in. Login Required";
            }

            return Tuple.Create(success, message);
        }


        private async Task CopyRedirectUriToClipboard()
        {
            // Run the app once calling this method to get the app redirect Uri - then register that app redirect Uri in the
            // -> App that authenticates against AAD (see https://azure.microsoft.com/en-gb/documentation/articles/app-service-mobile-how-to-configure-active-directory-authentication/)
            // -> App that authenticates against MSA (see https://azure.microsoft.com/en-gb/documentation/articles/app-service-mobile-how-to-configure-microsoft-authentication/)
            // EXCEPT that instructions for supplying the redirect URI apply to server-initiated authentication. For client authentication,
            // as used here, the redirect Uri is the one copied to the clipboard here.
            var redirectURI = AuthenticationHelper.GetAppRedirectURI();

            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(redirectURI);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            Windows.ApplicationModel.DataTransfer.Clipboard.Flush();

            var dialog = new Windows.UI.Popups.MessageDialog(redirectURI, "App Redirect URI copied to clipboard - Now configure yor app");
            await dialog.ShowAsync();

        }
    }
#endif

    internal static class AuthenticationHelper
    {
        private static WebAccountProvider aadAccountProvider = null;
        private static WebAccount userAccount = null;

        //Use "consumers" as your authority when you want to get an MSA token.
        static string authority = "consumers";

        //Use "organizations" as your authority when you want the app to work on any Azure Tenant.
        //static string authority = "organizations";

        // If you want to write a UWP app to allow users from a specific Azure AD tenant, e.g. developertenant.onmicrosoft.com,
        // to invoke the Directory Graph. Here the code you write for selecting the Azure AD provider. 
        //static string tenant = "developertenant.onmicrosoft.com";
        //static string authority = "https://login.microsoftonline.com/" + tenant;


        // Store account-specific settings so that the app can remember that a user has already signed in.
        public static ApplicationDataContainer _settings = ApplicationData.Current.RoamingSettings;

        // To authenticate to Microsoft Graph, the client needs to know its App ID URI. 
        public const string ResourceUrl = "https://graph.microsoft.com/";


        // Get an access token for the given context and resourceId. An attempt is first made to 
        // acquire the token silently. If that fails, then we try to acquire the token by prompting the user.
        public static async Task<string> GetTokenHelperAsync(string appClientId)
        {

            string token = null;

            aadAccountProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", authority);

            // Check if there's a record of the last account used with the app
            var userID = _settings.Values["userID"];

            if (userID != null)
            {

                WebTokenRequest webTokenRequest = new WebTokenRequest(aadAccountProvider, string.Empty, appClientId);
                webTokenRequest.Properties.Add("resource", ResourceUrl);

                // Get an account object for the user
                userAccount = await WebAuthenticationCoreManager.FindAccountAsync(aadAccountProvider, (string)userID);


                // Ensure that the saved account works for getting the token we need
                WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest, userAccount);
                if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success || webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.AccountSwitch)
                {
                    WebTokenResponse webTokenResponse = webTokenRequestResult.ResponseData[0];
                    userAccount = webTokenResponse.WebAccount;
                    token = webTokenResponse.Token;
                }
                else
                {
                    // The saved account could not be used for getting a token
                    // Make sure that the UX is ready for a new sign in
                    SignOut();
                }
            }
            else
            {
                // There is no recorded user. Start a sign in flow without imposing a specific account.
                WebTokenRequest webTokenRequest = new WebTokenRequest(aadAccountProvider, string.Empty, appClientId, WebTokenRequestPromptType.ForceAuthentication);
                webTokenRequest.Properties.Add("resource", ResourceUrl);

                // This is where most of the magic happens.The provider you specified takes over, trying to use the currently 
                // signed in user(or any account saved on the system) to obtain the token you requested without prompting the user. 
                // If the token can be obtained without interaction, as it will often be the case on cloud domain joined or 
                // classic domain joined machines, the call to RequestTokenAsync will return right away.
                // In case interaction is required, either for showing consent or for gathering authentication factors, 
                // the API will take care to automatically prompt the user with the correct experience.
                WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);

                if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    WebTokenResponse webTokenResponse = webTokenRequestResult.ResponseData[0];
                    userAccount = webTokenResponse.WebAccount;
                    token = webTokenResponse.Token;
                }
            }

            // We succeeded in getting a valid user.
            if (userAccount != null)
            {
                // save user ID in local storage
                _settings.Values["userID"] = userAccount.Id;
                _settings.Values["userEmail"] = userAccount.UserName;
                _settings.Values["userName"] = userAccount.Properties["DisplayName"];

                return token;
            }

            // We didn't succeed in getting a valid user. Clear the app settings so that another user can sign in.
            else
            {

                SignOut();
                return null;
            }
        }

        /// <summary>
        /// Signs the user out of the service.
        /// </summary>
        public static void SignOut()
        {
            //Clear stored values from last authentication.
            _settings.Values["userID"] = null;
            _settings.Values["userEmail"] = null;
            _settings.Values["userName"] = null;
        }

        public static string GetAppRedirectURI()
        {
            // Windows 10 universal apps require redirect URI in the format below. Add a breakpoint to this line and run the app before you register it, so that
            // you can supply the correct redirect URI value.
            return string.Format("ms-appx-web://microsoft.aad.brokerplugin/{0}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host).ToUpper();
        }
    }
}
