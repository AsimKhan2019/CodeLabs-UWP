# //build 2016 Workshops

This repo contains the three modules of the __UWP Code Lab__ created for BUILD 2016.

The modules are intended to be followed consecutively, but it is not required. You can choose to do the modules in any order.

##Setup##

Follow these steps to prepare your computer for the code labs:

1. Clone the repo or download as zip. If you download as zip, remember to unblock the zip archive before unzipping. To unblock a downloaded archive, right-click on the zip in Windows Explorer, click Properties, then check __Unblock__ near the bottom of the window, then click __OK__.
    
* You only need Windows 10 v1511 and the Windows 10586 SDK (as shipped in Visual Studio 2015 Update 1 or later) for modules 1 and 2. 
    
* For each module, go to the _labfolder_\CodeLabs-UWP\Workshop\ _module_\Source folder, rightclick on __Setup.cmd__ and click ‘Run as Administrator’. This installs the code snippets for the module. Do this for each module.

* For module 2, install the __Ink Toolbar preview__. To install it, double click on _labfolder_\CodeLabs-UWP\Workshop\Module2-MorePersonalComputing\Source\Setup\ExtensionSDKs\InkToolbarPreview.vsix. _Note: This InkToolbarPreview is provided for this lab only. It must not be used for production code. It will be replaced by the fully integrated Ink Toolbar in future releases of the Windows SDK._

* For module 3, you need the latest Windows insider Program Windows 10 update (14295 right now) and the 14295 SDK –and emulators 

* For module 3, you will also need to go into Visual Studio: Tools->Nuget Package Manager->Package Manager settings. In there, go to Package Sources and setup a local NuGet package source, pointing at the folder _labfolder_\CodeLabs-UWP\Workshop\Module3-ConnectedApps\Source\Setup\Nuget-local  
