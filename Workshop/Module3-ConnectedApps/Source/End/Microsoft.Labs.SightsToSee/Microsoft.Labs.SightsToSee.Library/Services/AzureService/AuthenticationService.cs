//#define SERVER_INITIATED

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;

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
                string token = await new MSAAuthenticationHelper().GetAuthTokenAsync();

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


    internal class MSAAuthenticationHelper
    {
        // To obtain Microsoft account tokens, you must register your application online 
        // Then, you must associate the app with the store. 
        private const string MicrosoftAccountProviderId = "https://login.microsoft.com";

        private WebAccountProvider accountProvider = null;
        private WebAccount userAccount = null;

        //Use "consumers" as your authority when you want to get an MSA token.
        const string consumerAuthority = "consumers";

        // Store account-specific settings so that the app can remember that a user has already signed in.
        public ApplicationDataContainer _settings = ApplicationData.Current.RoamingSettings;

        const string AccountScopeRequested = "wl.basic";

        // Client ID is ignored for WAM requests for MSA
        const string AccountClientId = "none";

        // Event to return the token
        public event EventHandler<AuthenticatedEventArgs> OnAuthenticated;

        public Task<string> GetAuthTokenAsync()
        {
            var tcs = new TaskCompletionSource<string>();
            EventHandler<AuthenticatedEventArgs> subscription = null;
            subscription = (_, e) =>
            {
                this.OnAuthenticated -= subscription;
                tcs.TrySetResult(e.Token);
            };
            this.OnAuthenticated += subscription;
            this.BeginGetAuthTokenAsync();
            return tcs.Task;

        }

        // Get an access token for the given context and resourceId. An attempt is first made to 
        // acquire the token silently. If that fails, then we try to acquire the token by prompting the user.
        private async Task BeginGetAuthTokenAsync()
        {
            string token = null;

            // FindAccountProviderAsync returns the WebAccountProvider of an installed plugin
            // The Provider and Authority specifies the specific plugin
            // This scenario only supports Microsoft accounts.

            // The Microsoft account provider is always present in Windows 10 devices, as is the Azure AD plugin.
            // If a non-installed plugin or incorect identity is specified, FindAccountProviderAsync will return null 
            accountProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId, consumerAuthority);

            // Check if there's a record of the last account used with the app
            var userID = _settings.Values["userID"];

            if (userID != null)
            {

                WebTokenRequest webTokenRequest = new WebTokenRequest(accountProvider, AccountScopeRequested, AccountClientId);

                // Get an account object for the user
                userAccount = await WebAuthenticationCoreManager.FindAccountAsync(accountProvider, (string)userID);


                // Ensure that the saved account works for getting the token we need
                //WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest, userAccount);
                WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest, userAccount);
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
                // There is no recorded user. 
                // Check if a default MSA account has been set already.
                accountProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId);

                // Check if the returned authority is "consumers" 
                if (accountProvider?.Authority == consumerAuthority)
                {
                    // If it is, then there’s a default MSA account present and there’s no need to show account control
                    WebTokenRequest webTokenRequest = new WebTokenRequest(accountProvider, AccountScopeRequested, AccountClientId);

                    // This is where most of the magic happens.The provider you specified takes over, trying to use the currently 
                    // signed in user(or any account saved on the system) to obtain the token you requested without prompting the user. 
                    //WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);
                    WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest);

                    if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
                    {
                        WebTokenResponse webTokenResponse = webTokenRequestResult.ResponseData[0];
                        userAccount = webTokenResponse.WebAccount;
                        token = webTokenResponse.Token;
                    }
                }
                else
                {
                    // There is no default account or the returned authority is not "consumer", so we must show account control
                    // The AccountCommandsRequested event triggers before the Accounts settings pane is displayed 
                    AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += OnAccountCommandsRequested;
                    AccountsSettingsPane.Show();
                    return;
                }
            }

            // We succeeded in getting a valid user.
            if (userAccount != null)
            {
                // save user ID in local storage
                _settings.Values["userID"] = userAccount.Id;
                _settings.Values["userEmail"] = userAccount.UserName;

                OnAuthenticated?.Invoke(this, new AuthenticatedEventArgs(token));
            }

            // We didn't succeed in getting a valid user. Clear the app settings so that another user can sign in.
            else
            {

                SignOut();
                OnAuthenticated?.Invoke(this, new AuthenticatedEventArgs(string.Empty));
            }
        }

        // This event handler is called when the Account settings pane is to be launched.
        private async void OnAccountCommandsRequested(
            AccountsSettingsPane sender,
            AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            // In order to make async calls within this callback, the deferral object is needed
            AccountsSettingsPaneEventDeferral deferral = e.GetDeferral();

            // The Microsoft account provider is always present in Windows 10 devices, as is the Azure AD plugin.
            // If a non-installed plugin or incorect identity is specified, FindAccountProviderAsync will return null   
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId, consumerAuthority);

            WebAccountProviderCommand providerCommand = new WebAccountProviderCommand(provider, WebAccountProviderCommandInvoked);
            e.WebAccountProviderCommands.Add(providerCommand);

            e.HeaderText = "Signing in with your Microsoft account allows syncing of your Sights2See data across all your devices";

            // You can add links such as privacy policy, help, general account settings
            e.Commands.Add(new SettingsCommand("privacypolicy", "Privacy policy", PrivacyPolicyInvoked));
            e.Commands.Add(new SettingsCommand("otherlink", "Other link", OtherLinkInvoked));
            deferral.Complete();
        }

        private async void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
        {
            string token = string.Empty;

            // AccountClientID is ignored by MSA
            WebTokenRequest webTokenRequest = new WebTokenRequest(command.WebAccountProvider, AccountScopeRequested, AccountClientId);

            // If the user selected a specific account, RequestTokenAsync will return a token for that account.
            // The user may be prompted for credentials or to authorize using that account with your app
            // If the user selected a provider, the user will be prompted for credentials to login to a new account
            WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);

            // If a token was successfully returned, then store the WebAccount Id into local app data
            // This Id can be used to retrieve the account whenever needed. To later get a token with that account
            // First retrieve the account with FindAccountAsync, and include that webaccount 
            // as a parameter to RequestTokenAsync or RequestTokenSilentlyAsync
            if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
            {
                WebTokenResponse webTokenResponse = webTokenRequestResult.ResponseData[0];
                userAccount = webTokenResponse.WebAccount;
                token = webTokenResponse.Token;
            }

            // We succeeded in getting a valid user.
            if (userAccount != null)
            {
                // save user ID in local storage
                _settings.Values["userID"] = userAccount.Id;
                _settings.Values["userEmail"] = userAccount.UserName;
            }

            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= OnAccountCommandsRequested;

            OnAuthenticated?.Invoke(this, new AuthenticatedEventArgs(token));
        }

        private async void PrivacyPolicyInvoked(IUICommand command)
        {
            await (new MessageDialog("Privacy policy clicked by user")).ShowAsync();
        }

        private async void OtherLinkInvoked(IUICommand command)
        {
            await(new MessageDialog("Other link pressed by user")).ShowAsync();
        }

        /// <summary>
        /// Signs the user out of the service.
        /// </summary>
        public void SignOut()
        {
            //Clear stored values from last authentication.
            _settings.Values["userID"] = null;
            _settings.Values["userEmail"] = null;
        }
    }

    public class AuthenticatedEventArgs : EventArgs
    {
        public string Token { get; internal set; }
        public AuthenticatedEventArgs(string token)
        {
            Token = token;
        }
    }
}
