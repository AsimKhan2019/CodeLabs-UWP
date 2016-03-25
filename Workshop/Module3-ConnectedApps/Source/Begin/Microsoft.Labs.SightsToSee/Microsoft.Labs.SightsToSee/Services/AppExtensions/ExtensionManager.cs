using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppExtensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;
using Microsoft.Labs.SightsToSee.Models;

namespace Microsoft.Labs.SightsToSee.Facts
{
    public class ExtensionManager
    {

        private ObservableCollection<Extension> _extensions;
        private string _contract;
        private CoreDispatcher _dispatcher;
        private AppExtensionCatalog _catalog;

        public ExtensionManager(string contract)
        {
            // extensions list   
            _extensions = new ObservableCollection<Extension>();

            // catalog & contract
            _contract = contract;
            _catalog = AppExtensionCatalog.Open(_contract);

            // using a method that uses the UI Dispatcher before initializing will throw an exception
            _dispatcher = null;
        }

        public ObservableCollection<Extension> Extensions
        {
            get { return _extensions; }
        }

        public string Contract
        {
            get { return _contract; }
        }

        // this sets up UI dispatcher and does initial extension scan
        public void Initialize()
        {
            // check that we haven't already been initialized
            if (_dispatcher != null)
            {
                throw new ExtensionManagerException("Extension Manager for " + this.Contract + " is already initialized.");
            }

            // thread that initializes the extension manager has the dispatcher
            _dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

            // set up extension management events
            _catalog.PackageInstalled += Catalog_PackageInstalled;
            _catalog.PackageUninstalling += Catalog_PackageUninstalling;
            _catalog.PackageUpdating += Catalog_PackageUpdating;
            _catalog.PackageUpdated += Catalog_PackageUpdated;
            _catalog.PackageStatusChanged += Catalog_PackageStatusChanged;

            // Scan all extensions
            FindAllExtensions();
        }

        public async void FindAllExtensions()
        {
            // make sure we have initialized
            if (_dispatcher == null)
            {
                throw new ExtensionManagerException("Extension Manager for " + this.Contract + " is not initialized.");
            }

            // load all the extensions currently installed
            IReadOnlyList<AppExtension> extensions = await _catalog.FindAllAsync();
            foreach (AppExtension ext in extensions)
            {
                // load this extension
                await LoadExtension(ext);
            }
        }

        private async void Catalog_PackageInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
        {
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                foreach (AppExtension ext in args.Extensions)
                {
                    await LoadExtension(ext);
                }
            });
        }

        private async void Catalog_PackageUpdated(AppExtensionCatalog sender, AppExtensionPackageUpdatedEventArgs args)
        {
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                foreach (AppExtension ext in args.Extensions)
                {
                    await LoadExtension(ext);
                }
            });
        }

        // package is updating, so just unload the extensions
        private async void Catalog_PackageUpdating(AppExtensionCatalog sender, AppExtensionPackageUpdatingEventArgs args)
        {
            await UnloadExtensions(args.Package);
        }

        // package is removed, so unload all the extensions in the package and remove it
        private async void Catalog_PackageUninstalling(AppExtensionCatalog sender, AppExtensionPackageUninstallingEventArgs args)
        {
            await RemoveExtensions(args.Package);
        }

        // package status has changed, could be invalid, licensing issue, app was on USB and removed, etc
        private async void Catalog_PackageStatusChanged(AppExtensionCatalog sender, AppExtensionPackageStatusChangedEventArgs args)
        {
            // get package status
            if (!(args.Package.Status.VerifyIsOK()))
            {
                // if it's offline unload only
                if (args.Package.Status.PackageOffline)
                {
                    await UnloadExtensions(args.Package);
                }

                // package is being serviced or deployed
                else if (args.Package.Status.Servicing || args.Package.Status.DeploymentInProgress)
                {
                    // ignore these package status events
                }

                // package is tampered or invalid or some other issue, remove the extensions
                else
                {
                    await RemoveExtensions(args.Package);
                }

            }
            // if package is now OK, attempt to load the extensions
            else
            {
                // try to load any extensions associated with this package
                await LoadExtensions(args.Package);
            }
        }

        // loads an extension
        public async Task LoadExtension(AppExtension ext)
        {
            // get unique identifier for this extension
            string identifier = ext.AppInfo.AppUserModelId + "!" + ext.Id;

            // load the extension if the package is OK
            if (!(ext.Package.Status.VerifyIsOK()
                    // This is where we'd normally do signature verfication, but don't care right now
                    //&& extension.Package.SignatureKind == PackageSignatureKind.Store
                    ))
            {
                // if this package doesn't meet our requirements
                // ignore it and return
                return;
            }

            // if its already existing then this is an update
            Extension existingExt = _extensions.Where(e => e.UniqueId == identifier).FirstOrDefault();

            // new extension
            if (existingExt == null)
            {
                // get extension properties
                IPropertySet properties = await ext.GetExtensionPropertiesAsync();

                // get logo 
                var filestream = await (ext.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(1, 1))).OpenReadAsync();
                BitmapImage logo = new BitmapImage();
                logo.SetSource(filestream);

                // create new extension
                Extension nExt = new Extension(ext, properties, logo);

                // Add it to extension list
                _extensions.Add(nExt);

                // load it
                await nExt.Load();
            }
            // update
            else
            {
                // unload the extension
                existingExt.Unload();

                // update the extension
                await existingExt.Update(ext);
            }
        }

        // loads all extensions associated with a package - used for when package status comes back
        public async Task LoadExtensions(Package package)
        {
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _extensions.Where(ext => ext.AppExtension.Package.Id.FamilyName == package.Id.FamilyName).ToList().ForEach(async e => { await e.Load(); });
            });
        }

        // unloads all extensions associated with a package - used for updating and when package status goes away
        public async Task UnloadExtensions(Package package)
        {
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _extensions.Where(ext => ext.AppExtension.Package.Id.FamilyName == package.Id.FamilyName).ToList().ForEach(e => { e.Unload(); });
            });
        }

        // removes all extensions associated with a package - used when removing a package or it becomes invalid
        public async Task RemoveExtensions(Package package)
        {
            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _extensions.Where(ext => ext.AppExtension.Package.Id.FamilyName == package.Id.FamilyName).ToList().ForEach(e => { e.Unload(); _extensions.Remove(e); });
            });
        }


        public async void RemoveExtension(Extension ext)
        {
            await _catalog.RequestRemovePackageAsync(ext.AppExtension.Package.Id.FullName);
        }

        // For exceptions using the Extension Manager
        public class ExtensionManagerException : Exception
        {
            public ExtensionManagerException() { }

            public ExtensionManagerException(string message) : base(message) { }

            public ExtensionManagerException(string message, Exception inner) : base(message, inner) { }
        }
    }

    public class Extension
    {
        private AppExtension _extension;
        private IPropertySet _valueset;
        private string _version;
        private bool _supported;
        private bool _offline;
        private string _uniqueId;
        private BitmapImage _logo;
        private readonly object _sync = new object();

        public Extension(AppExtension ext, IPropertySet properties, BitmapImage logo)
        {
            _extension = ext;
            _valueset = properties;
            _offline = false;
            _supported = false;
            _logo = logo;
            _version = "Unknown";

            // get version from properties
            //if (_valueset != null)
            //{
            //    PropertySet versionProperty = _valueset["TargetVersion"] as PropertySet;
            //    if (versionProperty != null) _version = versionProperty["#text"].ToString();
            //}

            //// check for unsupported version
            //if (_version == "Unknown" || _version.CompareTo("1.4") > 0)
            //{
            //    _version += " (Unsupported Version!)";
            //}
            //else
            //{
            //    _supported = true;
            //}

            //AUMID + Extension ID is the unique identifier for an extension
            _uniqueId = ext.AppInfo.AppUserModelId + "!" + ext.Id;
        }

        public BitmapImage Logo
        {
            get { return _logo; }
        }

        public string UniqueId
        {
            get { return _uniqueId; }
        }

        public string Version
        {
            get { return _version; }
        }

        public bool Offline
        {
            get { return _offline; }
        }

        public AppExtension AppExtension
        {
            get { return _extension; }
        }

        public bool Enabled
        {
            get
            {
                return AppSettings.AppExtensionEnabled;
            }

            set
            {
                if (value != AppSettings.AppExtensionEnabled)
                {
                    if (value)
                    {
                        AppSettings.AppExtensionEnabled = true;
                        Enable();
                    }
                    else
                    {
                        AppSettings.AppExtensionEnabled = false;
                        Disable();
                    }
                }
            }
        }

        public bool Loaded
        {
            get
            {
                return AppSettings.AppExtensionLoaded;
            }

            set
            {
                AppSettings.AppExtensionLoaded = value;
            }
        }

        public async Task Update(AppExtension ext)
        {
            // ensure this is the same uid
            string identifier = ext.AppInfo.AppUserModelId + "!" + ext.Id;
            if (identifier != this.UniqueId)
            {
                return;
            }

            // get extension properties
            ValueSet properties = await ext.GetExtensionPropertiesAsync() as ValueSet;

            // get logo 
            var filestream = await (ext.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(1, 1))).OpenReadAsync();
            BitmapImage logo = new BitmapImage();
            logo.SetSource(filestream);

            // update the extension
            this._extension = ext;
            this._valueset = properties;
            this._logo = logo;
            /*
            // get version from properties
            if (_props.ContainsKey("Version"))
            {
                this._version = this._props["Version"] as string;
            }
            else
            { */
            this._version = "Unknown";
            //}

            // load it
            await Load();
        }

        // this controls loading of the extension
        public async Task Load()
        {
            // if it's not enabled or already loaded, don't load it
            if (!Enabled || Loaded)
            {
                return;
            }

            // make sure package is OK to load
            if (!_extension.Package.Status.VerifyIsOK())
            {
                return;
            }

            // Extension is not loaded and enabled - load it
            StorageFolder folder = await _extension.GetPublicFolderAsync();
            if (folder != null)
            {

                // load file from extension package
                String fileName = @"SanFranciscoSights.json";
                StorageFile file = await folder.GetFileAsync(fileName);
                var extensionsTrip = await SeedDataFactory.CreateSampleTrip(file);
                foreach (var sight in extensionsTrip.Sights)
                {
                    sight.TripId = AppSettings.LastTripId;
                }

                var dataModelService = DataModelServiceFactory.CurrentDataModelService(); 

                await dataModelService.InsertSights(extensionsTrip.Sights);
            }
            Loaded = true;
            _offline = false;

        }

        // This enables the extension
        public async Task Enable()
        {
            // indicate desired state is enabled
            Enabled = true;

            // load the extension
            await Load();
        }

        // this is different from Disable, example: during updates where we only want to unload -> reload
        // Enable is user intention. Load respects enable, but unload doesn't care
        public async void Unload()
        {
            StorageFolder folder = await _extension.GetPublicFolderAsync();

            if (folder == null)
                return; //nothing to unload

            // load file from extension package
            string fileName = @"SanFranciscoSights.json";
            StorageFile file = await folder.GetFileAsync(fileName);
            var extensionsTrip = await SeedDataFactory.CreateSampleTrip(file);

            var modelService = DataModelServiceFactory.CurrentDataModelService();
            var currentTrip = await modelService.LoadTripAsync(AppSettings.LastTripId);

            // unload it
            lock (_sync)
            {
                if (Loaded)
                {
                    foreach (var sight in extensionsTrip.Sights)
                    {
                        var importedSight = currentTrip.Sights.Single(s => s.Name == sight.Name);
                        if (!importedSight.IsMySight)
                            modelService.DeleteSightAsync(importedSight);
                    }

                    // see if package is offline
                    if (!_extension.Package.Status.VerifyIsOK() && !_extension.Package.Status.PackageOffline)
                    {
                        _offline = true;
                    }

                    Loaded = false;
                }
            }
        }

        // user-facing action to disable the extension
        public void Disable()
        {
            // only disable if it is enabled
            if (Enabled)
            {
                // set desired state to disabled
                Enabled = false;
                // unload the app
                Unload();
            }
        }
    }
}
