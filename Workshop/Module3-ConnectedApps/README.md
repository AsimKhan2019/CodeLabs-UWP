<a name="HOLTop" ></a>

# Connected Apps: Across Devices and App-to-App #
---

<a name="Overview"></a>
## Overview ##

Windows 10 Redstone comes with more ways than ever before to create great user experiences across devices and across apps. 
In this lab, you will learn how to connect your app to the cloud so users can get their data on whichever device they pick up. You will enhance the app to load additional app data through an App Extension. You will use the LaunchUri and LaunchForResults APIs to get directions from BingMaps and to connect to a photo processing apps to add effects to pictures.

You will add capabilities to the app to share files and content with other apps by adding support for Drag and Drop and the Share contract.

<a name="Objectives"></a>
### Objectives ###

In this module, you will see how to:

- Connect the app to the cloud
- Fetch resources from an App Extension
- Launch another app using LaunchUri
- Connect to another app using LaunchUriForResults
- Add drag and drop support to the app
- Use the Share contract to share content with other apps

<a name="Prerequisites"></a>
### Prerequisites ###

The following is required to complete this module:

- Microsoft Windows 10 Build 14279 or later
- Microsoft Visual Studio 2015 Update 2 or later
- Windows SDK Build 14279 or later

---

<a name="Exercises" ></a>

## Exercises ##

This module includes the following exercises:

1.	[Connect the app to an Azure App Service Mobile Apps cloud backend](#Exercise1)
1.	[Load resources from App Extensions](#Exercise2)
1.	[Launch other apps using Launch Uri and Launch for Results](#Exercise3)
1.  [Share files and content with other apps](#Exercise4)


Estimated time to complete this module:  **40 to 50 minutes**


Let’s get started with Azure.

<a name="Exercise1"></a>
### Exercise 1: Azure App Service Mobile Apps ###

<a name="Ex1Task1"></a>
#### Task 1 - Connect the app to the cloud ####

Introduction

##### Steps #####

1. tbd


<a name="Exercise2"></a>
### Exercise 2: App Extensions ###

<a name="Ex2Task1"></a>
#### Task 1 – Fetch resources from an App Extension ####

App Extensions allow you to add data to your application from other UWP Store apps. An extension app exposes its extension in the app manifest and indicates the available data. The host app indicates in its manifest that it is looking to consume that type of data.

You can install and uninstall extension apps while the host app is running, and it will raise appropriate events to add and remove data as required without relaunching.

##### Steps #####

1.	For this module, we've added a project called __AdditionalSights__ to the solution. The AdditionalSights app is an extension app that provides six more Sights in a __json__ file. 

   Let's take a look at how the extension app is set up.
   
   Open the AdditionalSights __Package.appxmanifest__ as code. 
   
   >__Note:__ The extension type we're using is in preview, so it isn't yet available in the Manifest Editor.
   
   ![View the Package manifest as code](Images/open_as_code.png "View the Package manifest as code")
    
    *__Figure__: View the Package manifest as code.*
   
   The extension is declared in the manifest with the category __windows.appExtension__. It exposes up a name, display name, description, and __Public__ folder. The Public folder is where we'll find the consumable data.
   
   #### XML
   
   ```XML
   <Extensions>
        <uap3:Extension Category="windows.appExtension">
          <uap3:AppExtension Name="SanFranPack.1.0" Id="base" PublicFolder="Public" DisplayName="Sights To See San Francisco" Description="Additional sights to see in San Francisco" />
        </uap3:Extension>
   </Extensions>
   ```
   
   Expand the __Public__ folder. You will see the json file containing additional Sights.
   
   We'll install the extension app later on.
   
1. Let's move over to __SightsToSee__, which is our host app.

    Open the SightsToSee __Package.appxmanifest__ as code.
    
    Expand the __M3_ExtensionHost__ snippet inside the __Extension__ node.
    
    #### XML
    ```XML
    <uap3:Extension Category="windows.appExtensionHost">
        <uap3:AppExtensionHost>
            <uap3:Name>SanFranPack.1.0</uap3:Name>
        </uap3:AppExtensionHost>
    </uap3:Extension>
    ```
    
    What it does:
    
    - Declares the SightsToSee app as a host for extensions with the name SanFranPack.1.0

1. In the SightsToSee app, open the helper __Services > App Extensions > ExtensionManager.cs__.

    We've provided this helper to manage the loading and unloading of extensions. 
    
    ##### __Initialize()__ #####

    - We're going to call __Initialize__ from __App.xaml.cs__.
     
    -  This is where we hook up event handlers to deal with adding and removing app extensions.
     
    - Extensions can be loaded and unloaded dynamically while the host app is running.
    
    - By default, when you call __Initialize__ on app startup, it will load extensions that have already been installed. The event handlers handle loading and unloading of extensions that are installed or uninstalled after that point.
    
    ##### Scroll down to the __Load__ task. #####
    
    - If an extension is enabled but not yet loaded, this task will check to make sure it is OK to load.
    
    - It will then create a local copy of the json file from the extension app and load the new Sights into the trip.
    
    ##### Unload #####
    
    - The __Unload__ task first gets the data again from the extension app
    
    - It uses a lock to make sure the user can't rerun the code until it has completed
    
    >__Note:__ Code inside a __lock__ can't be awaited.
    
    - It then checks the Sights in the extension file against the existing Sights in the app. If they are not in __My Sights__, the extension Sights will be removed.
    
1. Now that we've seen how extensions work and that we have the helper to support loading and unloading, we can set up the app to do the work.
    
    - Open __App.xaml.cs__
    
    - Expand the __M3_ExtensionManager__ snippet anywhere in the __App__ class.
    
        This code creates a new ExtensionManager to handle extensions with the name __SanFranPack.1.0__ and sets up an ExtensionManager property that we can access. 
    
    - In the __OnLaunched__ method, expand the __M3_Initialize__ snippet after the VCD load. This line will call the __Initialize()__ method in the ExtensionManager helper on app startup.
    
1. We've added some basic options to the app settings page to support loading and unloading of app extensions. 

    - Open __Views > SettingsPage.xaml__. Uncomment the __Extensions ListView__.  
    
        This list will populate when extensions are available.
        
    - Open __ViewModels > SettingsPageViewModel.cs__. Uncomment the __Extensions ObservableCollection__.
    
1. Deploy the __SightsToSee__ app and run it from the Start Menu. Navigate to the __Settings__ page.
    
    You can see that the Extensions list is empty.

    Before we can load the extension, it first needs to be installed on the machine. You can install an extension app directly from the Windows Store, sideload it, or deploy it if you have the source code.
    
    ![The Empty Extensions List](Images/empty_extensions_list.png "The Empty Extensions List")
    
    *__Figure__: The extensions list is empty until an extension app is installed.*

1. Since we have the source code, Deploy the __AdditionalSights__ project to install it. Keep the __Settings__ page open as it deploys.
    
    >__Note:__ You can sideload an appx bundle with PowerShell commands.
    
    - As soon as the __AddtionalSights__ app is installed, it appears in the __Extensions__ list. Use the toggle to enable it.
    
    ![Enable the extension app](Images/enable_extension.png "Enable the extension app")
    
    *__Figure__: Enable the extension app.*
    
    - Open your San Francisco trip to see the new Sights that have been added. Add one to __My Sights__.
    
1. Find the AdditionalSights app in the Start Menu. Right-click and uninstall it.

    ![Uninstall the extension app](Images/uninstall_extension.png "Uninstall the extension app")
    
    *__Figure__: Uninstall the extension app.*

1. Return to the San Francisco trip. Any additional Sights that weren't added to My Sights will have been removed.

<a name="Exercise3"></a>
### Exercise 3: Launch URI and Launch for Results ###

Inter-app communication can be used to

- Launch links in a Web browser
    
- Send an email with context from an app
    
- Report errors

- Call out to services

Your system has default programs set to open certain URI protocols. You can also define custom protocols.

We're going launch the Maps app and use it to get directions to a Sight. Then we'll use a custom URI protocol and package family name to launch a specific app, pass in a photo, and return the processed photo to one of our Sights.

<a name="Ex3Task1"></a>
#### Task 1 - Launch another app using LaunchUri ####

Let's start with a simple LaunchUri scenario.

__Steps__

1.	Open __SightDetailPage.xaml__ and expand the __M3_DirectionsButton__ snippet in the __TitleCommandBar__.

1. Expand the __M3_MobileDirectionsButton__ snippet in the __MobileCommandBar__. 

    >__Note:__ There are two command bars because of the adaptive design of the app. Current design guidelines recommend that Mobile command bars appear at the bottom of the page and that Desktop command bars appear at the top.
    
1. The app bar buttons you just added are hooked up to a __GetDirectionsAsync__ method in the view model. In the next step, we'll create that method.

1. Open the __SightDetailPageViewModel__. Expand the __M3_GetDirections__ snippet anywhere in the view model.

    What it does:
    
    - The mapsUri is set to use the bingmaps: protocol.
    
    - The mapsUri uses a query to pass the current Sight's latitude, longitude, and name.
    
        >__Note:__ the mapsUri string is constructed using C# 6 string interpolation.
    
    - We're using __LauncherOptions__ to specify a package family name for the target app. The package family name ensures that only the official Maps app will launch.
    
    >__Note:__ If you leave out the package family name, your user can choose between all apps on the system that uses the protocol specified in the launch URI.
    
    - __LaunchUriAsync__ starts the app associated with the bingmaps protocol and the package family name defined in the LauncherOptions.
    
1. Build and run your app. Open a Sight detail page and use the directions button on the app bar to launch the Maps app.

    ![The Directions Button](Images/directions_button.png "The Directions Button")
    
    *__Figure__: Use the Directions button on the app bar to launch the Maps app and get directions to the Sight.*

<a name="Ex3Task2"></a>
#### Task 2 -  Connect to another app using LaunchUriForResults ####

Beyond launching a target app and passing it data, we can launch an app and receive results back. 

- We're going to add a button to the InkToolbar we're using for image annotation and use it to launch a photoprocessing app. 

- The photoprocessing app will apply a Lumia filter to the image and return the altered version to our SightsToSee app.

##### Steps #####

1. Before we can launch the photoprocessing app, we need to install it on the system.

    Open the __&lt;LabRoot&gt;\Module3\Begin\ImageProcessingApp\PhotoEditingLaunchForResults.sln__ solution.
    
1. Set the __QuickStart__ project as the __StartUp Project__ if it isn't already. The QuickStart app applies a black and white Lumia filter to a photograph and allows the user to adjust brightness.

1. Build and deploy the __QuickStart__ app.

    ![The QuickStart App](Images/quickstart_app.png "The QuickStart App")
    
    *__Figure__: The QuickStart app.*

1. Return to the __SightsToSee__ Module 3 solution and open __SightDetailPage.xaml__.

    Add a new button to the ImageInkToolbar by expanding the __M3_LaunchButton__ snippet after the Undo button.
    
1. Open the __SightDetailPage__ code-behind. Find the __OnLaunchForResults__ event handler.

1. Expand the __M3_OpenPicker__ snippet inside the event handler.

    What it does:
    
    - Creates a new __FileOpenPicker__ in thumbnail mode
    
    - Sets the suggested start location to the user's __Picture Library__
    
    - Looks for files of type __jpg__, __jpeg__, and __png__
    
    - Creates a storage file to save the user's image selection locally

1. Now that we have an image, we can send it to our photoprocessing app.

    Expand the __M3_LaunchForResults__ snippet below the previous snippet in the __OnLaunchForResults__ event handler.

    What it does:
    
    - If the user has chosen a valid file, it is added as a token to a ValueSet
    
    - The target app Uri is specified
    
    - The package family name is specified in the LauncherOptions
    
    - We await the results of LaunchUriForResultsAsync
    
    - If the response status is Success, we copy the new image file to the local folder and add it to the Sight record.

1. Build and run your app. Open a Sight detail page and use the __LaunchForResults__ button on the ImageInkToolbar to launch the file picker.

    ![The LaunchForResults Button](Images/launchforresults_button.png "The LaunchForResults Button")
    
    *__Figure__: The LaunchForResults Button.*

1. Select an image from the filesystem. When the QuickStart app opens, set a brightness level for the modified image.

1. Use the __Save the image__ button to save and return the image to the SightsToSee app.

    You will see the modified image appear in the Sight gallery.


<a name="Exercise4"></a>
### Exercise 4: Additional Sharing Features ###

<a name="Ex4Task1"></a>
#### Task 1 – Add drag and drop support to the app ####

Adding drag and drop is a quick way to make your app more user-friendly. We're going to add drag and drop capability to the gallery grid in the SightDetailPage so users can easily add new photos to a Sight.

1. Open __SightDetailPage.xaml__ and find the __GalleryGrid__.

1. Add the __AllowDrop__, __Drop__, and __DragOver__ attributes to the opening tag of the GalleryGrid (type or copy/paste):

    #### XAML
    
    ```XAML
    AllowDrop="True"
    Drop="{x:Bind ViewModel.SightFile_DropAsync}"
    DragOver="{x:Bind ViewModel.SightFile_DragOver}"
    ```
    
    What it does:
    
    - __AllowDrop="True"__ enables the GalleryGrid as a drop target
    
    - We are wiring up the Drop and DragOver events to methods in the view model. We'll add those methods in the following steps.
    
1. Open the __SightDetailPageViewModel__. Expand the __M3_DragOver__ snippet anywhere in the view model.

    What it does:
    
    - Enables the __Copy__ operation for the originator of the drag event
    
    - Handles the __DragOver__ event
    
    - Customizes the __DragUI__ with a caption and content preview
    
1. Expand the __M3_DropAsync__ snippet below the DragOver event handler.

    What it does:
    
    - Copies the incoming storage items to the local folder
    
    - Calls the __AddSightFileAsync__ task, which creates and saves a SightFile record for each new image
    
    - __AddSightFileAsync__ also adds each new image to the CurrentSightFiles observable collection, so they will appear immediately in the gallery grid.
    
    >__Note:__ You can drag and drop multiple images at once.

1. Build and run the app. Drag and drop an image or multiple images onto the gallery grid in a Sight detail view.

    ![Drag and Drop](Images/drag_and_drop.png "Drag and Drop")
    
    *__Figure__: Drag and drop to add images to the Sight gallery.*

<a name="Ex4Task2"></a>
#### Task 2 – Use the Share contract to share content with other apps ####

The Share contract is an easy way to share data between apps. You can share links, text, photos, and videos. We're going to add a Share button to the Sight detail app bar to share the sight name, photo, and description in HTML format.

>__Note:__ Visit http://msdn.microsoft.com/en-us/library/windows/apps/hh465251.aspx to read the guidelines on sharing content in UWP apps.

1. Add the app share button to the command bar in __SightDetailPage.xaml__. There are two command bars: one for Mobile and one for larger windows.

    - Expand the __M3_ShareButton__ snippet into the __TitleCommandBar__.
    
    - Expand the __M3_MobileShareButton__ snippet into the __MobileCommandBar__.
    
1. Now let's add the code to support the share button. Open the __SightDetailPageViewModel__ and expand the __M3_ShareSight__ snippet anywhere in the ViewModel.

    __What it does:__

    - The DataTransferManager initiates an exchange of content with other apps. 
    
    - __GetForCurrentView()__ returns the DataTransferManager associated with the current window.
    
    - We're also subscribing to the __DataRequested__ event, which occurs when a share operation starts.
    
    - __ShowShareUI__ opens the system Share flyout.
    
1. With the __ShareSight()__ method, we've initiated the share operation. Next, we'll handle the __DataRequested__ event.

    Expand the __M3_DataRequested__ snippet below the __ShareSight__ method.
    
    >__Note:__ The Request property lets you access the DataRequest object and give it data or a failure message.
    
    __What it does:__
    
    -  Sets the Data title and description fields to the current Sight name and description
    
    - Creates an HTML payload string with the __src__ of the ```<img>``` tag set to the absolute URI of the Sight image
    
    - Formats the payload string as HTML
    
    - Adds the localImage to the ResourceMap

    
1. Build and run the app. Open a Sight detail view and use the Share button to initiate the Share process.

    ![The Share Contract](Images/share_button.png "The Share Contract")
    
    *__Figure__: Use the Share button to open the Share contract.*


<a name="Summary"></a>
## Summary ##

In this lab, we connected the app to the cloud and loaded additional app data through an App Extension. We also added LaunchUri and LaunchForResults scenarios to get directions from BingMaps and process photos that we added to the Sights.

Finally, we enhanced the user experience by adding Drag and Drop and the Share contract.