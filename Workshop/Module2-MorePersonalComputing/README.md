<a name="HOLTop" ></a>

# More Personal Computing and Shell Integration #
---

<a name="Overview"></a>
## Overview ##

Windows 10 is the first step to an era of more personal computing. UWP and the Windows 10 platform enables development of apps that interact with users in more human, natural ways, through speech, pen and touch.

In this lab, we will enhance the Sights2See Travel app that was created in Module 1 of this UWP code lab. We will add inking and speech support to allow users to interact with the app in more natural ways. We will add an Interactive Toast which allows the user deeper integration with your app right from the Notification pane.

<a name="Objectives"></a>
### Objectives ###

In this module, we’re going to get hands-on experience implementing more personal ways for your C# UWP app to interact with the user. You will see how to:

- Add inking support
- Provide speech integration through Cortana
- Build an interactive notification that enables direct user actions.

<a name="Prerequisites"></a>
### Prerequisites ###

The following is required to complete this module:

- Microsoft Windows 10 Build 14279 or later
- [Microsoft Visual Studio 2015 with Update 2 or later][1]

[1]: https://www.visualstudio.com/products/visual-studio-community-vs

---

<a name="Exercises" ></a>

## Exercises ##

This module includes the following exercises:

1.	[Inking](#Exercise1)
1.	[Cortana Integration and Speech Commands](#Exercise2)
1.	[Implement an interactive toast](#Exercise3)


Estimated time to complete this module:  **40 to 50 minutes**

<a name="Exercise1"></a>
### Exercise 1: Inking ###

We will be implementing the following Inking features:
- Notes recorded using a simplified OneNote model
- Inking on photos
- Ink Toolbar (preview of the RS1 Ink Toolbar for color and pen selection)
- Optical character recognition

<a name="Ex1Task1"></a>
#### Task 1 - Add the ability for users to enter written notes ####

We'll start by adding an InkCanvas that can be used for taking notes.

1. Open the starter project at **&lt;Lab Root&gt;\Module 2\Begin\**.

1. Run the starter project on the **Local Machine** and then exit the application.
    
	> **Note:** You may notice there is a new prompt for location as well as a BackgroundTasks project in the Solution. We'll be using these features later in the Module.
    
1.	We're going to enable Inking as an alternative to typing notes in the Sight Detail view.

	To that end, we've already added the boolean property **NotesAreInk** to the **Sight** model in the starter code to keep track of whether the user saved  the Notes field as text or Ink.
   
	We've also added an **IsNotesInking** boolean to the ViewModel to determine if we should display the notes entry field as text or Ink.
   
	Let's get started by added a simple InkCanvas to the Notes field.

1. Open **SightsDetailControl.xaml**.
   
1. Expand the **M2_EnableInkButton** snippet below the **Notes** title TextBlock. The button will go in the second column of the grid.

	> **Note:** The visibility of this button is tied to the **NotesAreInking** boolean through a **BooleanToVisibility** converter.

1. Expand the **M2_EnableInk** snippet in the **SightsDetailPageViewModel**. This method will set the **IsNotesInking** bool to true. We're going to use this property to handle visibility for the Ink and Text elements.

1. Return to **SightsDetailControl.xaml**. Expand the **M2_NotesInkCanvas** snippet below the Notes TextBox.

	> **Note:** The InkCanvas is contained in a Grid with a white background, because the InkCanvas on its own would display with a transparent background.
    
1. In the **SightsDetailControl** code-behind, expand the **M2_NotesInputs** snippet after `InitializeComponent()` in the constructor.

1. Build and run your app. 

1. Open the **SightDetailControl** for a Sight and use the **EnableInkButton** to change the Notes field to an InkCanvas. 

	The default pen for the InkCanvas is a simple black line. Right now, we are also not saving the Ink. In the next task, we're going to set up an Ink Toolbar to handle pen color, saving, and clearing the Notes Ink Canvas.

<a name="Ex1Task2"></a>
####  Task 2 – Add the Redstone Ink Toolbar ####

In Redstone, you'll have the option of adding the Redstone Ink Toolbar to any InkCanvas. We're going to use a preview of the toolbar for our Notes InkCanvas. The toolbar is customizable, so we'll also add our custom Save button to it.

1. Notice that the `xmlns:c="using:InkToolbarPreview"` namespace has been added to the top-level **UserControl** in **SightDetailControl.xaml**. We've added an **InkToolbar** example for image annotation, and it is also using this namespace. We've also added the `using InkToolbarPreview` namespace to the **SightsDetailControl** code-behind.
    
1. Expand the **M2_InkToolbar** snippet after the EnableInkButton in **SightDetailControl.xaml**.

    > **Notes:** One of the buttons on the Toolbar is commented out. We'll enable it in the next task. For now, you can ignore it.

1. Open the **SightDetailControl** code-behind.
    
1. Scroll down to the `#region NotesInkToolbar` and expand the **M2_SaveUndo** snippet in the region.

1. Locate and review the **NotesSaveButton_Click** method which sets the **NotesAreInk** property on the Sight to true and saves the ink strokes in a storage file.
    
1. Locate and review the **NotesUndoButton_Click** method  which clears all strokes, sets **NotesAreInk** and **IsNotesInking** to false, and returns to the text input method for Notes.
    
    > **Note:** The **EraserClearAll()** method already exists for the image annotation InkToolbar, so we are reusing it for the Notes Ink Toolbar clear method as well.
    
    > There is also a style already defined for the image annotation InkToolbar, which makes Red, Green, and Blue ink available. The style is defined in **SightDetailControl.xaml**.
    
1. Expand the **M2_SetupNotes** snippet inside the **SetupNotesInkAsync** task in the code-behind. This method restores Ink that has been saved to the Sight.
    
1. Build and run the app. Use the Notes InkToolbar to change properties for the Ink canvas, save the Ink, and undo.

<a name="Ex1Task3"></a>
#### Task 3 - Add OCR Ink to text capability ####

Now that we've added the ability to record notes with Ink, it would be useful to recognize those notes as text. In this task, we're going to add the ability to use Optical Character Recognition to convert Ink notes to text.

1. Uncomment the remaining **InkToolbarCustomToggleButton** on the Notes InkToolbar in **SightDetailControl.xaml**. This button will pop open a dialog where the user can complete the speech recognition process.

1. Next, let's create the dialog. Expand the **M2_OcrDialog** snippet at the bottom of the main Grid in the **SightsDetailControl** XAML.

    This dialog lets the user choose a speech recognizer from those installed on the machine and use it to parse the Ink to text.
    
    If the result is acceptable, the user can select the primary key on the dialog to finalize the conversion. If not acceptable, the user can cancel and return to the InkCanvas.
    
1. Open the **SightDetailControl** code-behind and expand the **M2_Recognizers** snippet above the constructor.
    
1. In the constructor, find the `#region SetupRecognizers` and expand the **M2_SetupRecognizers** snippet inside of it. This code gets the list of all available recognizers on the device.
    
1. Scroll down to the `#region OCR`. Expand the **M2_RecognizerMethods** snippet inside the region.

1. Then expand the **M2_RecognizerChanged** snippet inside the **OnRecognizerChanged** method.

    - The **TryOCR** method opens the **OCRDialog** when the user selects the button from the Notes InkToolbar. It awaits the result from the dialog: will the user accept the recognized text or cancel?

    - The **OnRecognizerChanged** method clears the Status TextBlock in the OCRDialog and calls the SetRecognizerByName method.
    
    - The **SetRecognizerByNameMethod** sets the default recognizer to the user's selection if it exists on the device.
    
1. Expand the **M2_OnRecognize** snippet inside the **OnRecognizeAsync** method.

    - The **OnRecognizeAsync** method sends the ink strokes (if any) to the ink recognizer container and awaits the results of **RecognizeAsync**.
    
    - If results are returned, they are built into a string and displayed in the **Status** TextBlock in the OCR Dialog.
    
    - Results also enable the Primary dialog button, which allows the user to accept the conversion.
    
    - If there are no results, an error message is given.
    
    - If there is no ink to recognize, an error message is given.
    
1. Build and run the app. Write text on the Notes InkCanvas, and try converting it with the recognizer.

1. When you receive a result from the recognizer, accept the results to return to a Notes TextBox with the new results appended.

<a name="Exercise2"></a>
### Exercise 2: Cortana Integration and Speech Commands ###

<a name="Ex2Task1"></a>
#### Task 1 -  Voice Commands to launch App in the foreground ####

Voice commands give your users a convenient, hands-free way to interact with your app. We're going to begin by adding a simple voice command that launches the app. Then we'll explore more advanced scenarios with a voice command service that returns results in the Cortana window without launching the app.

1. You'll notice that there is a new **BackgroundTasks** project in the solution. We'll be using that later on for the **VoiceCommandService**, but for now we'll be working in the **SightsToSee** project.

1. Create a new XML file in the main directory of the **SightsToSee** project and give it the name **VoiceCommands.xml**. This file is the voice command definition file that will define the voice command schema. We're going to create a simple schema with a voice command that launches the app.

1. Expand the **M2_LaunchCommand** snippet below the XML namespace declaration. This code creates a voice command set for en-us.
    
    > **Note:** We have added en-us as the language for this example, but you can add additional command sets within the same VCD. For instance, the language tag for Germany would be `xml:lang="de-de"`. The list of regions and languages that Cortana supports is at http://windows.microsoft.com/en-us/windows-10/cortanas-regions-and-languages.
    
    > If you choose to add another command set in a supported language, make sure to add an equivalent command in that language every time you add one to the en-us command set throughout this demo.
    
    We've given the command set the prefix "Sights To See." This prefix tells Cortana to listen for commands related to the app.
    
    The explicitly specified `<AppName>`
    
    - Acts as a command prefix
    
    - Lets you specify a user-friendly name for the app
    
    - Enables explicit placement of the app name within a voice command
    
    Within the command,
    
    - The square [] brackets denote optional words
    
    - The curly {} brackets act as a placeholder for a phrase list. Any item in the phrase list can be used in the command.
    
    - The `RequireAppName="BeforeOrAfterPhrase"` attribute means the app name can be spoken at the beginning or end of the command.
 
    > **Note:** The Navigate element signifies that the app will launch in the foreground. The alternative to launching in the foreground is to define a WinRT component to handle behind-the-scenes interactions with app data through Cortana. You can learn more about Voice Command Definitions at https://msdn.microsoft.com/en-us/library/windows/apps/dn722331.aspx

1. Open **App.xaml.cs**. We are going to register the VCD file. Expand the **M2_LoadVCD** snippet below the `// Insert the M2_LoadVCD snippet here` comment in the **OnLaunched()** method.

    > **Note:** You will need to launch the app once normally to register the VCD each time you make changes to it.

1. Expand the **M2_VoiceActivation** snippet below the `// Insert the M2_VoiceActivation snippet here` comment in the **OnActivated()** method. This creates a switch based on **ActivationKind.VoiceCommand**.

1. Expand the **M2_HandleVoiceCommand** snippet anywhere in **App.xaml.cs**. This snippet creates a switch based on the voice command as it is understood by Cortana. Additionally, it writes the recognized command and the spoken text to the console.

1. Build and run the app to register the VCD. Close the app.

1. Speak a variation of the launch command. The app should launch and navigate to the main page. 

<a name="Ex2Task2"></a>
#### Task 2 -  Voice Commands to interact with your App in the background ####

1. Now that we've created a typical launch scenario, let's take a look at more advanced scenarios.

    We've added a **BackgroundTasks** project to the starting code.
    
    - The project is a Windows Runtime Component
    
    - It contains a class called **VoiceCommandService**, which serves as the entry point to the background task
    
    - The **VoiceCommandService** is registered in the app manifest as an app service.

1. Let's take a look at the app service declaration. Open the **Package.appxmanifest**. On the **Declarations** tab, the **VoiceCommandService** is declared as an app service with the entrypoint **BackgroundTasks.VoiceCommandService**.

   The **BackgroundTasks** project has also been added as a reference in the **SightsToSee** project.
   
   The **SightsToSee.Library** project is referenced in the BackgroundTasks project.

1. We're going to add a second voice command to the VCD. This command will be handled by the service.

    Open **VoiceCommands.xml**. Expand the **M2_NearbyCommand** snippet below the **LaunchApp** command.
    
    This command uses phrase lists and several `<ListenFor>` nodes to provide a variety of options for the user to access the command.
    
    The `RequireAppName="ExplicitlySpecified"` attribute allows you to place the app name within the voice command instead of at the beginning or end.
    
    > **Note:** The app name is defined in the `<AppName>` tag at the top of the VCD file. The app name is required for the **RequireAppName** attribute to work. Reference the placement of the app name within the command using `{builtin:AppName}`.

1. Open **App.xaml.cs** and expand the **M2_NearbyCase** snippet in the **voiceCommandName** switch under the **LaunchApp** case.

    Now we are ready to handle the incoming voice command in the **VoiceCommandService**.

1. Open **Background Tasks > VoiceCommandService.cs**.

    - We've set up the **VoiceCommandService** to implement the **IBackgroundTask** interface and handle task cancellation.
    
    - The deferral in the background task ensures that the task will run uninterrupted to completion.
    
    - The **Run()** method is the entry point to the task.
    
1. Expand the **M2_Using** snippet below the existing using statements.

    We'll be adding code that relies on these dependencies.

1. Expand the **M2_ServiceConnection** snippet as the first item in the VoiceCommandService class.VoiceCommandService

    > **Note:** The voice service connection is maintained for the lifetime of a Cortana session.

1. Make the **Run()** method **async**.

1. Expand the **M2_TriggerDetails** snippet inside the **Run()** method. 

    - We are checking the trigger details to see if the name matches the name of the App Service registration from the app manifest. If so, we implement a try-catch block.
    
    > **Note:** The subscription to the **VoiceCommandCompleted** event is in this code block, because it must take place after the **voiceServiceConnection** is set.
    
1. Expand the **M2_CommandCompleted** snippet above the **OnTaskCanceled** method.

1. Our voice service connection is set up and we are handling completion and cancellation. Now we can handle the particular case of the NearbySights command.

    We've added a **SightsHelper** to the **Services** directory in the **SightsToSee.Library** project to assist with locating nearby sights. Let's take a look at the helper.
    
    What this helper does:
    
    - Finds the shortest route from the current location to a sight based on walking distance or driving distance
    
    - Provides tasks that iterate through the Sights in the trip to find the closest Sight or Sights

1. Expand the **M2_GetNearest** snippet after the **Run()** method.

    - The **GetNearestSights** task loads the default trip and passes it to the SightsHelper to find the closest Sights
    
    - **ReportFailureToGetCurrentLocation()** task returns a message to the user when the user hasn't granted location permissions to the app
    
    >**Note:** In the starter project, location is enabled in the app manifest and we ask the user on startup to confirm location access for the app. We'll use the result from that user dialog to determine if we have location access.
    
    - The **ReportFailureToGetSights()** task returns a message to the user when there are no Sights in the trip.
    
1. Expand the **M2_ShowNearest** snippet after the tasks you just added.

    The **ShowNearestResults** task 
    
    - Returns a written and spoken message to the user
    
    - Creates **VoiceCommandContentTiles** that display the name, description, and image for the nearest Sights
    
    - Displays the content tiles to the user in the Cortana pane
    
1. Now we can set up the switch to handle the **NearbySights** case in the **Run()** method. We're going to call the tasks we just added.
    
    Expand the **M2_HandleNearbySights** snippet inside the **Try** block in the **Run()** method.
    
    What it does:
    
    - Checks to see if we have permission to use the user's location
    
    - If we have permission, get the location and use it to get the nearest Sights
    
    - If nearby Sights are returned, call the **ShowNearestResults** task to display them to the user.

1. Build and run your app to register the new VCD. Close the app.

1. Ask Cortana to show nearby Sights. Cortana will display the closest Sights as content tiles.

<a name="Exercise3"></a>
### Exercise 3: Interactive Toast Notifications ###

<a name="Ex3Task1"></a>
#### Task 1 – Implement an interactive toast ####

Toast notifications are a great way to quickly interact with a user outside of an app. In this task, we're going to build and trigger a toast notification for a Sight when it is added to My Sights.

1.	Open **SightDetailControl.xaml** and expand the **M2_DatePicker** snippet after the **Caption** TextBlock. This snippet includes a **CalendarDatePicker** and **TimePicker**.
  
    We've created variables in the ViewModel for **CurrentSightTime** and **CurrentSightDate** that return the visit date and time stored for the sight. If the date is null, it will return **DateTime.Now**.
    
1. In the **Services > TileNotificationsService** folder, open **ScheduledNotificationService.cs**. We've provided this class to help build the notification.

    What it does:

    - Builds the XML for the toast
    
    - Creates the toast notifier
    
    - Adds the toast to the schedule to display 30 seconds from now
    
    - Defines four actions the user can take
    
    > **Note:** The toast is set to always display 30 seconds from the time it is triggered to make it easy to test.
    
1. Open the **SightDetailPageViewModel**. In the **AddSightAsync()** method, expand the **M2_ScheduleToast** snippet as the first item in the method. This snippet calls the **ScheduledNotificationService** to create and send a toast notification and it passes it the current Sight.
    
1. When the toast arrives, it would be useOnActivatedful if the user could go straight into the Sight Details from the toast.

1. Open **App.xaml.cs**. Expand the **M2_ToastActivation** snippet as a case in the `switch (args.Kind)` in the **OnActivated()** override.

1. Build and run your app. Open the Sight details for a sight that has not been added to My Sights. 

1. Use the **+** button in the app bar to add the sight to your sights. This action will trigger the **ScheduleNotificationService** to send a toast notification. The notification will appear in 30 seconds with the Sight details.
    
1. Click the toast to launch the app.


<a name="Summary"></a>
## Summary ##

By completing this module, you should have:

- Added inking support
- Provided speech integration through Cortana
- Built an interactive notification that enables direct user actions.