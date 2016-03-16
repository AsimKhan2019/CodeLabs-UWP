# Hands-On Lab: More Personal Computing and Shell Integration
## Instructor Guide
March 2016

# Contents

### [Overview](#overview)

### [Part 1: Inking](#part1)
- [Task 1 – Add the ability for users to enter written notes](#task1)
- [Task 2 – Add the Redstone Ink control](#task2)
- [Task 3 – Add OCR Ink to text capability](#task3)

### [Part 2: Cortana Integration and Speech Commands](#part2)
- [Task 4 – Voice commands](#task4)

### [Part 3: Interactive Toast](#part3)
- [Task 5 – Implement an interactive toast](#task6)

### [Summary](#summary)

<a name="overview"></a>
# Overview

__Module 2 – More Personal Computing and Shell Integration (Instructor)__

In this module, we’re going to get hands-on experience implementing more personal ways for your C# UWP app to interact with the user. We will
-	Add inking support
- Provide speech integration through Cortana
- Build an interactive notification that enables direct user actions.

Let’s get started with Inking.

# Part 1: Inking

We will be implementing the following Inking features:
- Notes recorded using a simplified OneNote model
- Inking on photos
- Ink Toolbar (preview of the RS1 Ink Toolbar for color and pen selection)
- Optical character recognition

<a name="task1"></a>
### Task 1 - Add the ability for users to enter written notes

We'll start by adding an InkCanvas that can be used for taking notes.

#### Steps

1. Open the starter project at __&lt;Lab Root&gt;\Module 2\Begin\__.

1.	Run the project

    -	Run the starter project on the __Local Machine__.

    -	Exit the application.
    
    >__Note:__ You may notice there is a new prompt for location as well as a BackgroundTasks project in the Solution. We'll be using these features later in the Module.
    
1.	We're going to enable Inking as an alternative to typing notes in the Sight Detail view.

   To that end, we've already added the boolean property __NotesAreInk__ to the __Sight__ model in the starter code to keep track of whether the user saved  the Notes field as text or Ink.
   
   We've also added an __IsNotesInking__ boolean to the ViewModel to determine if we should display the notes entry field as text or Ink.
   
   Let's get started by added a simple InkCanvas to the Notes field.

1. Open __SightsDetailControl.xaml__.
   
1. Expand the __M2_EnableInkButton__ snippet below the __Notes__ title TextBlock. The button will go in the second column of the grid.

    >__Note:__ The visibility of this button is tied to the __NotesAreInking__ boolean through a __BooleanToVisibility__ converter.

1. Expand the __M2_EnableInk__ snippet in the __SightsDetailPageViewModel__. This method will set the __IsNotesInking__ bool to true. We're going to use this property to handle visibility for the Ink and Text elements.

1. Return to __SightsDetailControl.xaml__. Expand the __M2_NotesInkCanvas__ snippet below the Notes TextBox.

    >__Note:__ The InkCanvas is contained in a Grid with a white background, because the InkCanvas on its own would display with a transparent background.
    
1. In the SightsDetailControl code-behind, expand the __M2_NotesInputs__ snippet after ```InitializeComponent()``` in the constructor.

1. Build and run your app. 

    - Open the SightDetailControl for a Sight and use the EnableInkButton to change the Notes field to an InkCanvas. 

    - The default pen for the InkCanvas is a simple black line. Right now, we are also not saving the Ink. 
    
    In the next task, we're going to set up an Ink Toolbar to handle pen color, saving, and clearing the Notes Ink Canvas.

<a name="task2"></a>
### Task 2 – Add the Redstone Ink Toolbar

In Redstone, you'll have the option of adding the Redstone Ink Toolbar to any InkCanvas. We're going to use a preview of the toolbar for our Notes InkCanvas. The toolbar is customizable, so we'll also add our custom Save button to it.

__Steps__

1. Notice that the ```xmlns:c="using:InkToolbarPreview"``` namespace has been added to the top-level UserControl in __SightDetailControl.xaml__. We've added an InkToolbar example for image annotation, and it is also using this namespace.

    We've also added the ```using InkToolbarPreview``` namespace to the SightsDetailControl code-behind.
    
1. Expand the __M2_InkToolbar__ snippet after the EnableInkButton in SightDetailControl.xaml.

    >__Notes:__ One of the buttons on the Toolbar is commented out. We'll enable it in the next task. For now, you can ignore it.


1. Open the __SightDetailControl__ code-behind.
    
1. Scroll down to the ```#region NotesInkToolbar```. Expand the __M2_SaveUndo__ snippet in the region.

    - __The NotesSaveButton_Click__ method sets the __NotesAreInk__ property on the Sight to true and saves the ink strokes in a storage file.
    
    - The __NotesUndoButton_Click__ method clears all strokes, sets __NotesAreInk__ and __IsNotesInking__ to false, and returns to the text input method for Notes.
    
    >__Note:__ The __EraserClearAll()__ method already exists for the image annotation InkToolbar, so we are reusing it for the Notes Ink Toolbar clear method as well.
    
    There is also a style already defined for the image annotation InkToolbar, which makes Red, Green, and Blue ink available. The style is defined in __SightDetailControl.xaml__.
    
1. Expand the __M2_SetupNotes__ snippet inside the __SetupNotesInkAsync__ task in the code-behind.

    This method restores Ink that has been saved to the Sight.
    
1. Build and run the app. Use the Notes InkToolbar to change properties for the Ink canvas, save the Ink, and undo.

<a name="task3"></a>
### Task 3 - Add OCR Ink to text capability

Now that we've added the ability to record notes with Ink, it would be useful to recognize those notes as text. In this task, we're going to add the ability to use Optical Character Recognition to convert Ink notes to text.

__Steps__

1. Uncomment the remaining __InkToolbarCustomToggleButton__ on the Notes InkToolbar in __SightDetailControl.xaml__.

    This button will pop open a dialog where the user can complete the speech recognition process.
    
    Next, let's create the dialog.

1. Expand the __M2_OcrDialog__ snippet at the bottom of the main Grid in the SightsDetailControl XAML.

    This dialog lets the user choose a speech recognizer from those installed on the machine and use it to parse the Ink to text.
    
    If the result is acceptable, the user can select the primary key on the dialog to finalize the conversion. If not acceptable, the user can cancel and return to the InkCanvas.
    
1. Open the SightDetailControl code-behind.
    
    Expand the __M2_Recognizers__ snippet above the constructor.
    
1. In the constructor, find the ```#region SetupRecognizers``` and expand the __M2_SetupRecognizers__ snippet inside of it.

    This code gets the list of all available recognizers on the device.
    
1. Scroll down to the ```#region OCR```. Expand the __M2_RecognizerMethods__ snippet inside the region.

1. Then expand the __M2_RecognizerChanged__ snippet inside the __OnRecognizerChanged__ method.

    - The __TryOCR__ method opens the __OCRDialog__ when the user selects the button from the Notes InkToolbar. It awaits the result from the dialog: will the user accept the recognized text or cancel?

    - The __OnRecognizerChanged__ method clears the Status TextBlock in the OCRDialog and calls the SetRecognizerByName method.
    
    - The __SetRecognizerByNameMethod__ sets the default recognizer to the user's selection if it exists on the device.
    
1. Expand the __M2_OnRecognize__ snippet inside the __OnRecognizeAsync__ method.

    - The __OnRecognizeAsync__ method sends the ink strokes (if any) to the ink recognizer container and awaits the results of __RecognizeAsync__.
    
    - If results are returned, they are built into a string and displayed in the __Status__ TextBlock in the OCR Dialog.
    
    - Results also enable the Primary dialog button, which allows the user to accept the conversion.
    
    - If there are no results, an error message is given.
    
    - If there is no ink to recognize, an error message is given.
    
1. Build and run the app. Write text on the Notes InkCanvas, and try converting it with the recognizer.

    When you receive a result from the recognizer, accept the results to return to a Notes TextBox with the new results appended.

<a name="part2"></a>
#Part 2: Cortana Integration and Speech Commands

<a name="task4"></a>
### Task 4 -  Voice Commands

Voice commands give your users a convenient, hands-free way to interact with your app. We're going to begin by adding a simple voice command that launches the app. Then we'll explore more advanced scenarios with a voice command service that returns results in the Cortana window without launching the app.

Let's get started.

1. You'll notice that there is a new __BackgroundTasks__ project in the solution. We'll be using that later on for the VoiceCommandService, but for now we'll be working in the __SightsToSee__ project.

1. Create a new XML file in the main directory of the SightsToSee project and give it the name __VoiceCommands.xml__. This file is the voice command definition file that will define the voice command schema. We're going to create a simple schema with a voice command that launches the app.

1. Expand the __M2_LaunchCommand__ snippet below the XML namespace declaration.

    This code creates a voice command set for en-us.
    
    >__Note:__ We have added en-us as the language for this example, but you can add additional command sets within the same VCD. For instance, the language tag for Germany would be xml:lang="de-de". The list of regions and languages that Cortana supports is at http://windows.microsoft.com/en-us/windows-10/cortanas-regions-and-languages.
    
    >If you choose to add another command set in a supported language, make sure to add an equivalent command in that language every time you add one to the en-us command set throughout this demo.
    
    We've given the command set the prefix "Sights To See." This prefix tells Cortana to listen for commands related to the app.
    
    The explicitly specifed ```<AppName>```
    
    - Acts as a command prefix
    
    - Lets you specify a user-friendly name for the app
    
    - Enables explicit placement of the app name within a voice command
    
    Within the command,
    
    - The square [] brackets denote optional words
    
    - The curly {} brackets act as a placeholder for a phrase list. Any item in the phrase list can be used in the command.
    
    - The ```RequireAppName="BeforeOrAfterPhrase"``` attribute means the app name can be spoken at the beginning or end of the command.
 
    >__Note:__ The Navigate element signifies that the app will launch in the foreground. The alternative to launching in the foreground is to define a WinRT component to handle behind-the-scenes interactions with app data through Cortana. You can learn more about Voice Command Definitions at https://msdn.microsoft.com/en-us/library/windows/apps/dn722331.aspx

1. Open __App.xaml.cs__. We are going to register the VCD file. Expand the __M2_LoadVCD__ snippet below the ```// Insert the M2_LoadVCD snippet here``` comment in the __OnLaunched()__ method.

    >__Note:__ You will need to launch the app once normally to register the VCD each time you make changes to it.

1. Expand the __M2_VoiceActivation__ snippet below the ```// Insert the M2_VoiceActivation snippet here``` comment in the OnActivated() method. 
    
    What it does:
    - Creates a switch based on ActivationKind.VoiceCommand

1. Expand the __M2_HandleVoiceCommand__ snippet anywhere in App.xaml.cs. 

    What it does:
    - Creates a switch based on the voice command as it is understood by Cortana
    - Writes the recognized command and the spoken text to the console

1. Build and run the app to register the VCD. Close the app.

1. Speak a variation of the launch command. The app should launch and navigate to the main page. 

1. Now that we've created a typical launch scenario, let's take a look at more advanced scenarios.

    We've added a BackgroundTasks project to the starting code.
    
    - The project is a Windows Runtime Component
    
    - It contains a class called VoiceCommandService, which serves as the entry point to the background task
    
    - The VoiceCommandService is registered in the app manifest as an app service.
    
    Let's take a look at the app service declaration.
    
1. Open the __Package.appxmanifest__. On the __Declarations__ tab, the VoiceCommandService is declared as an app service with the entrypoint __BackgroundTasks.VoiceCommandService__.

   The BackgroundTasks project has also been added as a reference in the SightsToSee project.
   
   The __SightsToSee.Library__ project is referenced in the BackgroundTasks project.

1. We're going to add a second voice command to the VCD. This command will be handled by the service.

    Open __VoiceCommands.xml__. Expand the __M2_NearbyCommand__ snippet below the __LaunchApp__ command.
    
    This command uses phrase lists and several ```<ListenFor>``` nodes to provide a variety of options for the user to access the command.
    
    The ```RequireAppName="ExplicitlySpecified"``` attribute allows you to place the app name within the voice command instead of at the beginning or end.
    
    >__Note:__ The app name is defined in the ```<AppName>``` tag at the top of the VCD file. The app name is required for the __RequireAppName__ attribute to work. Reference the placement of the app name within the command using ```{builtin:AppName}```.

1. Open __App.xaml.cs__ and expand the __M2_NearbyCase__ snippet in the __voiceCommandName__ switch under the __LaunchApp__ case.

    Now we are ready to handle the incoming voice command in the VoiceCommandService.

1. Open __Background Tasks > VoiceCommandService.cs__.

    - We've set up the VoiceCommandService to implement the IBackgroundTask interface and handle task cancellation.
    
    - The deferral in the background task ensures that the task will run uninterrupted to completion.
    
    - The __Run()__ method is the entry point to the task.
    
1. Expand the __M2_Using__ snippet below the existing using statements.

    We'll be adding code that relies on these dependencies.

1. Expand the __M2_ServiceConnection__ snippet as the first item in the VoiceCommandService class.

    >__Note:__ The voice service connection is maintained for the lifetime of a Cortana session.

1. Make the __Run()__ method __async__.

1. Expand the __M2_TriggerDetails__ snippet inside the __Run()__ method. 

    - We are checking the trigger details to see if the name matches the name of the App Service registration from the app manifest. If so, we implement a try-catch block.
    
    >__Note:__ The subscription to the __VoiceCommandCompleted__ event is in this code block, because it must take place after the __voiceServiceConnection__ is set.
    
1. Expand the __M2_CommandCompleted__ snippet above the __OnTaskCanceled__ method.

1. Our voice service connection is set up and we are handling completion and cancellation. Now we can handle the particular case of the NearbySights command.

    We've added a SightsHelper to the Services directory in the SightsToSee.Library project to assist with locating nearby sights. Let's take a look at the helper.
    
    What this helper does:
    
    - Finds the shortest route from the current location to a sight based on walking distance or driving distance
    
    - Privides tasks that iterate through the Sights in the trip to find the closest Sight or Sights

1. Expand the __M2_GetNearest__ snippet after the __Run()__ method.

    - The __GetNearestSights__ task loads the default trip and passes it to the SightsHelper to find the closest Sights
    
    - __ReportFailureToGetCurrentLocation()__ task returns a message to the user when the user hasn't granted location permissions to the app
    
    >__Note:__ In the starter project, location is enabled in the app manifest and we ask the user on startup to confirm location access for the app. We'll use the result from that user dialog to determine if we have location access.
    
    - The __ReportFailureToGetSights()__ task returns a message to the user when there are no Sights in the trip.
    
1. Expand the __M2_ShowNearest__ snippet after the tasks you just added.

    The __ShowNearestResults__ task 
    
    - Returns a written and spoken message to the user
    
    - Creates __VoiceCommandContentTiles__ that display the name, description, and image for the nearest Sights
    
    - Displays the content tiles to the user in the Cortana pane
    
1. Now we can set up the switch to handle the __NearbySights__ case in the __Run()__ method. We're going to call the tasks we just added.
    
    Expand the __M2_HandleNearbySights__ snippet inside the __Try__ block in the __Run()__ method.
    
    What it does:
    
    - Checks to see if we have permission to use the user's location
    
    - If we have permission, get the location and use it to get the nearest Sights
    
    - If nearby Sights are returned, call the __ShowNearestResults__ task to display them to the user.

1. Build and run your app to register the new VCD. Close the app.

1. Ask Cortana to show nearby Sights. Cortana will display the closest Sights as content tiles.


<a name="part3"></a>
#Part 3: Interactive Toast Notifications

<a name="task5"></a>
### Task 5 – Implement an interactive toast

Toast notifications are a great way to quickly interact with a user outside of an app. In this task, we're going to build and trigger a toast notification for a Sight when it is added to My Sights.

1.	Open __SightDetailControl.xaml__ and expand the __M2_DatePicker__ snippet after the __Caption__ TextBlock.

    This snippet includes a CalendarDatePicker and TimePicker.
  
    We've created variables in the ViewModel for __CurrentSightTime__ and __CurrentSightDate__ that return the visit date and time stored for the sight. If the date is null, it will return __DateTime.Now__.
    
1. In the __Services > TileNotificationsService__ folder, open __ScheduledNotificationService.cs__.

    We've provided this class to help build the notification.

    What it does:

    - Builds the XML for the toast
    
    - Creates the toast notifier
    
    - Adds the toast to the schedule to display 30 seconds from now
    
    - Defines four actions the user can take
    
    >__Note:__ The toast is set to always display 30 seconds from the time it is triggered to make it easy to test.
    
1. Open the __SightDetailPageViewModel__. In the __AddSightAsync()__ method, expand the __M2_ScheduleToast__ snippet as the first item in the method.

    What it does:
    
    - Calls the __ScheduledNotificationService__ to create and send a toast notification
    - Passes it the current Sight
    
1. When the toast arrives, it would be useful if the user could go straight into the Sight Details from the toast.

    Open __App.xaml.cs__. Expand the __M2_ToastActivation__ snippet as a case in the ```switch (args.Kind)``` in the __OnActivated()__ override.

1. Build and run your app. Open the Sight details for a sight that has not been added to My Sights. 

1. Use the + button in the app bar to add the sight to your sights. 

    This action will trigger the ScheduleNotificationService to send a toast notification. The notification will appear in 30 seconds with the Sight details.
    
    Click the toast to launch the app.
