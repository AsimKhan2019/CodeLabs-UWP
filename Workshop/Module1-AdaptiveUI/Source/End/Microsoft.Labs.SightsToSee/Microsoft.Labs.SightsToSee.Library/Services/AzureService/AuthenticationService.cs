//#define SERVER_INITIATED

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
            // Authentication required for the cloud storage
            bool success = false;
            string message = string.Empty;
            try
            {
                // Sign-in 
                string token = await MSAAuthenticationHelper.GetTokenHelperAsync();

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
#endif
    }


    internal static class MSAAuthenticationHelper
    {
        // To obtain Microsoft account tokens, you must register your application online 
        // Then, you must associate the app with the store. 
        private const string MicrosoftAccountProviderId = "https://login.microsoft.com";

        private static WebAccountProvider accountProvider = null;
        private static WebAccount userAccount = null;

        //Use "consumers" as your authority when you want to get an MSA token.
        const string authority = "consumers";

        //Use "organizations" as your authority when you want the app to work on any Azure Tenant.
        //static string authority = "organizations";

        // Store account-specific settings so that the app can remember that a user has already signed in.
        public static ApplicationDataContainer _settings = ApplicationData.Current.RoamingSettings;

        const string AccountScopeRequested = "wl.basic";

        // Client ID is ignored for WAM requests for MSA
        const string AccountClientId = "none";


        // Get an access token for the given context and resourceId. An attempt is first made to 
        // acquire the token silently. If that fails, then we try to acquire the token by prompting the user.
        public static async Task<string> GetTokenHelperAsync()
        {

            string token = null;

            // FindAccountProviderAsync returns the WebAccountProvider of an installed plugin
            // The Provider and Authority specifies the specific plugin
            // This scenario only supports Microsoft accounts.

            // The Microsoft account provider is always present in Windows 10 devices, as is the Azure AD plugin.
            // If a non-installed plugin or incorect identity is specified, FindAccountProviderAsync will return null 
            accountProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId, authority);

            // Check if there's a record of the last account used with the app
            var userID = _settings.Values["userID"];

            if (userID != null)
            {

                WebTokenRequest webTokenRequest = new WebTokenRequest(accountProvider, AccountScopeRequested, AccountClientId);

                // Get an account object for the user
                userAccount = await WebAuthenticationCoreManager.FindAccountAsync(accountProvider, (string)userID);


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
                WebTokenRequest webTokenRequest = new WebTokenRequest(accountProvider, AccountScopeRequested, AccountClientId);

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
        }
    }
}
