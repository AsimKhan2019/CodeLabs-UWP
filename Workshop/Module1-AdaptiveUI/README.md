<a name="HOLTop" ></a>

# Building an Adaptive UI for Multiple Windows Devices #
---

<a name="Overview"></a>
## Overview ##

**Universal Windows Platform** (UWP) apps may run on a number of device families that differ significantly in screen size and features. To give a great user experience on all devices, the design must adapt to the device. An adaptive UI can detect information about the device it is running on and deliver a layout based on the characteristics of that device.

An adaptive UI differs from a responsive UI, because it can deliver an individualized layout for each device family or screen size snap point. A responsive UI typically takes a single, flexible design and displays it gracefully on all devices. One drawback to responsive UI can be a slower load time – elements are loaded for all devices and resolutions even though they may not be needed.

With adaptive UI, you can deliver a responsive design, but you also have the ability to deliver unique views to devices that have little in common with each other. For example, an Xbox view may be completely distinct from the desktop and mobile views for an app, because the device UI and interactions are so different.

In this module, we will evolve a fixed layout into an adaptive UI and view it on Desktop, Mobile, and Continuum. We’ll also use some new tools to easily generate default tiles, build adaptive tiles from XML, and enhance the Maps experience.

<a name="Objectives"></a>
### Objectives ###

In this module, you will see how to:

- Evolve a fixed layout into an adaptive UI
- Create visual states to support narrow, medium, and wide screen widths
- Use a RelativePanel to reposition content in the different states
- Use Setters to better adapt styles to a smaller screen
- Demonstrate Continuum for Mobile
- Create eye-catching tiles using adaptive tile templates
- Create great Maps experiences using 3D and Streetside

<a name="Prerequisites"></a>
### Prerequisites ###

The following is required to complete this module:

- Microsoft Windows 10
- Microsoft Visual Studio 2015 Update 1 or later

---

<a name="Exercises" ></a>

## Exercises ##

This module includes the following exercises:

1.	[Building an Adaptive UI](#Exercise1)
1.	[Default and Adaptive Tiles](#Exercise2)
1.	[Use Maps features](#Exercise3)


Estimated time to complete this module:  **40 to 50 minutes**

<a name="Exercise1"></a>
### Exercise 1: Building an Adaptive UI ###

The **SightsToSee** app is a demo travel app that recommends sights and landmarks around a trip destination. In the app, you can view the sights on the map, browse suggested sights and save them to your trip, and explore 3D and Streetside views of the locations.

The app has a navigation menu, and selecting elements on that menu will navigate to pages within the application. These pages are hosted within a frame that handles navigation, the back stack, and allows you to save that history through suspend/resume cycles.

We’re going to begin with a version of the SightsToSee app that has a fixed layout and adapt it for tablet and mobile views.

<a name="Ex1Task1"></a>
#### Task 1 – Explore the Desktop layout ####

Let’s take a look at how the app is set up and plan an adaptive layout. We’ll begin by walking through the starter project.

1.	In a new instance of Visual Studio 2015, choose **File > Open> Project/Solution**. Browse to **C:\Labs\CodeLabs-UWP\Workshop\Module 1-AdaptiveUI\Begin\Microsoft.Labs.SightsToSee** and open the solution file.

1.	Once the project has opened, set your Solution Configuration to **Debug** and your Solution Platform to **x86**. Select **Local Machine** from the Debug Target dropdown menu.

	![Configure your app to run on the Local Machine](Images/debug_mode.png?raw=true "Configure your app to run on the Local Machine")

	_Configure your app to run on the Local Machine_

1.	Run the project. After the splash page, the startup experience will display. In a fully-realized app, users would begin by creating a new trip. In this version of the app, there is a single, preloaded San Francisco trip that uses static data. Use the airplane button to select the trip.

	![The startup experience in the SightsToSee app](Images/startup.png?raw=true "The startup experience in the SightsToSee app")

	_The startup experience in the SightsToSee app_

1.	The app will open the trip detail page for San Francisco. Once you are in the trip, you will see three sections: a navigation pane, a list of Sights with thumbnail images, and a map control.

	![Fixed layout in the starter app](Images/fixed_layout.png?raw=true "Fixed layout in the starter app")

	_Fixed layout in the starter app_

1.	Resize the window to view the UI behavior. Since we are starting with a fixed UI, the content gets cut off as the window gets smaller.

	![Content in the fixed UI is cut off for smaller window sizes](Images/fixed_layout_cut.png?raw=true "Content in the fixed UI is cut off for smaller window sizes")

	_Content in the fixed UI is cut off for smaller window sizes_

1.	Click the hamburger to observe the menu behavior. The menu will collapse, and the content moves to the left.

1.	Return to Visual Studio and stop debugging.

1.	Let’s take a look at the XAML that defines the layout. Open **AppShell.xaml** in the main project folder. The app shell contains the navigation pane, which is built with a SplitView, and a frame for content. There is a single visual state in the VisualStateManager.

    > **Note:** A visual state is used to apply different values to properties on controls. The state is triggered by an adaptive trigger that can fire either based upon MinWindowWidth or MinWindowHeight. Within a visual state, there are Setters that specify the control and its property and the value to assign.
    > 
    > Here you can see that the visual state sets the split view pane called RootSplitView to Open and the display mode to CompactInline.
    > 
    > CompactInline mode displaces the app content when the menu is open, and it shows the narrow nav pane when the menu is closed.

1.	Open **Views > TripDetailPage.xaml**. The controls in this Grid get loaded into the frame in the app shell. Notice that the Sights and Map are laid out in a RelativePanel inside of a Grid. A RelativePanel is a layout container that allows you to control the placement of its child elements in spatial relation to each other.

    The single visual state in the Visual State Manager arranges the controls for the Desktop view.

    > **Note:** There are certain controls that expose their properties to child controls. These properties are known as attached properties. The RelativePanel is an example of such a control. It exposes properties such as RightOf that allows child elements to position themselves within the RelativePanel relative to other child elements or the panel itself.
    > 
    > You’ll notice in the Visual State that there is a special syntax in the Setter target attribute where attached properties are surrounded by parentheses.

1.	Stop debugging and return to Visual Studio.

<a name="Ex1Task2"></a>
#### Task 2 – Incorporate additional layouts ####

Now that we’ve explored the fixed UI in the SightsToSee starter app, we can add visual states for Tablet and Mobile. Our goal is to make better use of the available real estate by rearranging and resizing controls, and by hiding and showing content as needed.

1. We’re going to begin by adding the tablet visual state to the app shell using code snippets.

    > **Note**: Code snippets are blocks of reusable code that can be quickly inserted using a unique string, hotkey, or context menu. There are two kinds of snippets: expansion snippets and surround-with snippets.

    > **Note**: Expansion snippets contain contiguous blocks of code that insert at the point of the cursor. Surround-with snippets can wrap around existing code. In this module, we will use expansion snippets. For more on code snippets, visit https://msdn.microsoft.com/en-us/library/ms165392.aspx

1. Open **AppShell.xaml**.

1. Find the `<--Desktop State-->` in the **VisualStateGroup**. Change the **MinWindowWidth** adaptive trigger for the Desktop state from 0 to **1024**.

1. Look for the `<!--Tablet State -->` section in the **VisualStateGroup**. Insert a new line beneath this comment. Type **M1_ShellTablet** and hit the Tab key to expand the snippet.

    This state will control the behavior of the navigation pane for mid-size windows and tablets. It will set the SplitView pane to closed by default and keep the SplitView in **CompactInline** mode.

	(Code Snippet - _M1_ShellTablet_)

    ````XAML
    <!-- Tablet State -->

    <VisualState>
        <VisualState.StateTriggers>
            <AdaptiveTrigger MinWindowWidth="720" />
        </VisualState.StateTriggers>
        <VisualState.Setters>
            <Setter Target="RootSplitView.DisplayMode" Value="CompactInline"/>
            <Setter Target="RootSplitView.IsPaneOpen" Value="False"/>
        </VisualState.Setters>
    </VisualState>
    ````

1. The snippet we just added will control the Tablet state for the navigation pane, but we also need to add a tablet state for the content. but we also need to add a tablet state for the content. Open **Views > TripDetailPage.xaml**.

1.	Look for the `<!--Tablet State -->` section in the VisualStateGroup. Type **M1_TripTablet** below the comment and hit the Tab key to expand the snippet.
This state moves the Map control above the Sights GridViews, and it anchors the top, left, and right sides of the Map to the panel.

	(Code Snippet - _M1_TripTablet_)

    ````XAML
    <!-- Tablet State -->

    <VisualState>
        <VisualState.StateTriggers>
            <AdaptiveTrigger MinWindowWidth="720" />
        </VisualState.StateTriggers>
        <VisualState.Setters>
            <Setter Target="title.Visibility" Value="Visible" />

            <Setter Target="MobileHeader.Visibility" Value="Collapsed" />

            <Setter Target="MapGrid.Height" Value="360" />
            <Setter Target="MapGrid.(RelativePanel.RightOf)" Value="" />
            <Setter Target="MapGrid.(RelativePanel.AlignTopWith)" Value="" />
            <Setter Target="MapGrid.(RelativePanel.AlignTopWithPanel)" 
                Value="True" />
            <Setter Target="MapGrid.(RelativePanel.AlignRightWithPanel)" 
                Value="True" />
            <Setter Target="MapGrid.(RelativePanel.AlignLeftWithPanel)" 
                Value="True" />
            <Setter Target="MapGrid.(RelativePanel.AlignBottomWithPanel)" 
                Value="False" />
            <Setter Target="MapGrid.Margin" Value="24,0,24,28" />
            <Setter Target="MapGrid.Padding" Value="0" />

            <Setter Target="SightsGrid.(RelativePanel.Below)" Value="MapGrid" />
            <Setter Target="SightsGrid.(RelativePanel.AlignRightWithPanel)" 
                Value="True" />
            <Setter Target="SightsGrid.(RelativePanel.AlignLeftWithPanel)" 
                Value="True" />
            <Setter Target="SightsGrid.Width" Value="Auto" />
            <Setter Target="SightsGrid.Margin" Value="24,0,0,0" />

            <Setter Target="LayoutPanel.Padding" Value="0" />

        </VisualState.Setters>
    </VisualState>
    ````
	> **Note**:  Visual states sometimes conflict with each other if certain properties aren’t cleared or overridden. With RelativePanels, it is easy to inadvertently set up a circular reference. Explicit layouts and properties directly on controls can also cause conflicts in visual states.
You can override properties in a visual state by setting them to different values. To clear out a RelativePanel alignment state without setting it to a new value, set it to the empty string.

1.	Build and run the app. Resize the window to view the new Tablet visual state. Take a look at the menu behavior and Sight detail popup behavior. If you are using a device with Tablet mode, turn it on. Although the adaptive layout may not change, since it is triggered by screen size, the back button experience will change from the shell back button to the global back button.

	![The Tablet visual state in the SightsToSee app](Images/tablet_state.png?raw=true "The Tablet visual state in the SightsToSee app")

	_The Tablet visual state in the SightsToSee app_ 

    You may notice that the UI is still cut off for window sizes smaller than the Tablet state we’ve defined. In the following steps, we will add the Mobile state.
    
1.	Stop debugging and return to Visual Studio.

1.	Return to **AppShell.xaml**. 

1.	Expand the **M1_ShellMobile** snippet into the `<!-- Mobile State -->` section in the VisualStateGroup.
    
    This state sets the SplitView pane to Overlay mode. Overlay mode means the menu is invisible when closed and lays over the content when open. Note that the hamburger button is not included in the SplitView, so it will always display. This state also sets the nav pane to closed by default, so only the hamburger button will be visible when the user arrives at the page.
	
	(Code Snippet - _M1_ShellMobile_)

    ````XAML
    <!-- Mobile State -->

    <VisualState>
        <VisualState.StateTriggers>
            <AdaptiveTrigger MinWindowWidth="0" />
        </VisualState.StateTriggers>
        <VisualState.Setters>
            <Setter Target="RootSplitView.DisplayMode" Value="Overlay"/>
            <Setter Target="RootSplitView.IsPaneOpen" Value="False"/>
        </VisualState.Setters>
    </VisualState>
    ````
    
1.	Open **Views > TripDetailPage.xaml**. 
1.	Expand the **M1_TripMobile** snippet into the `<!--Mobile State -->` section in the VisualStateGroup.

    This state sets the map to full-bleed width with no margins. The large page title from the Tablet and Desktop states is hidden, and the Mobile header is shown instead. Some of the setters are set to the empty string to clear out conflicting RelativePanel properties from other states.

	(Code Snippet - _M1_TripMobile_)

	````XAML
	<!-- Mobile State -->

    <VisualState>
        <VisualState.StateTriggers>
            <AdaptiveTrigger MinWindowWidth="0" />
        </VisualState.StateTriggers>
        <VisualState.Setters>
            <Setter Target="title.Visibility" Value="Collapsed" />

            <Setter Target="MobileHeader.Visibility" Value="Visible" />

            <Setter Target="MapGrid.Height" Value="300" />
            <Setter Target="MapGrid.(RelativePanel.RightOf)" Value="" />
            <Setter Target="MapGrid.(RelativePanel.AlignTopWith)" Value="" />
            <Setter Target="MapGrid.(RelativePanel.AlignRightWithPanel)" 
                Value="True" />
            <Setter Target="MapGrid.(RelativePanel.AlignLeftWithPanel)" 
                Value="True" />
            <Setter Target="MapGrid.(RelativePanel.AlignBottomWithPanel)" 
                Value="False" />
            <Setter Target="MapGrid.Margin" Value="0,0,0,12" />
            <Setter Target="MapGrid.Padding" Value="0" />

            <Setter Target="SightsGrid.(RelativePanel.Below)" Value="MapGrid" />
            <Setter Target="SightsGrid.(RelativePanel.AlignRightWithPanel)" 
                Value="True" />
            <Setter Target="SightsGrid.(RelativePanel.AlignLeftWithPanel)" 
                Value="True" />
            <Setter Target="SightsGrid.Width" Value="Auto" />
            <Setter Target="SightsGrid.Margin" Value="12,0,0,0" />

            <Setter Target="LayoutPanel.Padding" Value="0" />
        </VisualState.Setters>
    </VisualState>
    ````
    
1. Build and run the app. Resize the window to see the app adapt from Desktop to Tablet to Mobile states.

	![The Mobile visual state in the SightsToSee app](Images/mobile_state.png?raw=true "The Mobile visual state in the SightsToSee app")

	_The Mobile visual state in the SightsToSee app_

1. Stop debugging and return to Visual Studio. In the next task, we’ll view the app on an actual Mobile device.

<a name="Ex1Task3"></a>
#### Task 3 – Mobile and Continuum ####

With the new Visual States, we’ve seen how the app is adaptive on Desktop for different window sizes. Let’s take a look at the experience on Mobile and Continuum.

1. Plug your Continuum-enabled mobile device into your development machine with the provided USB-C cable.

1. Deploy the app to the Mobile device from Visual Studio using Debug mode, the ARM solution platform, and Device as the solution target.

1. Once it has deployed, browse the app on the Mobile device. The SplitView menu is in Overlay mode, and the Mobile header is visible. Select a Sight to open its detail popup and test the hardware back button to dismiss it.

1. Stop debugging and unplug the Mobile device from the development machine.

1. Now that the app is installed on the Mobile device, we can use Continuum to browse an enhanced experience.

	> **Note:** Continuum gives your Mobile device the power to behave like a PC when connected to an external display. Your Mobile device must be Continuum-enabled, connected to a Microsoft Display Dock, and the app you are running needs an adaptive UI.  Continuum can display one running app at a time. For more information, visit [https://www.microsoft.com/en-us/windows/Continuum](https://www.microsoft.com/en-us/windows/Continuum)

1. Plug your Continuum-enabled Mobile device into Microsoft display dock, and plug the display dock into the external display.

1. Tap the **Tap to control &lt;device name&gt;** bar at top of Mobile screen.

	![Tap the bar at the top of the screen to use the Mobile device as a touchpad](Images/continuum.png?raw=true "Tap the bar at the top of the screen to use the Mobile device as a touchpad")

	_Tap the bar at the top of the screen to use the Mobile device as a touchpad_

	> **Note:** Do not tap the big finger icon – it only serves to point to the control bar; it doesn’t navigate to the touchpad.

1. When the touchpad opens, follow the directions on the screen and use one finger to move the mouse, a tap to select, and two fingers to scroll.

	![The Mobile device becomes a touchpad to control the external Continuum display](Images/continuum1.png?raw=true "The Mobile device becomes a touchpad to control the external Continuum display")

    _The Mobile device becomes a touchpad to control the external Continuum display_

1. Using the phone as a touchpad, open the app from the list of all apps in the Mobile Start Menu. You can also pin the app to the Start menu as a tile.

	> **Note:** If you already had the app running when you connected to an external display through Continuum, the running instance of the app will open on Continuum.

	If you pin the app to the Start Menu while viewing it in Continuum, it will remain pinned to the Mobile Start Menu when you disconnect from Continuum.

1. The adaptive UI will display according to the dimensions of the external display. The app layout will look similar desktop experience on a larger monitor, but will have the global back in the task bar. Open the detail view for a Sight to test out the global back behavior.

<a name="Exercise2"></a>
### Exercise 2: Default and Adaptive Tiles ###

Tiles are an essential part of the Windows 10 experience. They offer a unique and creative options for interacting with users outside of the traditional app experience.

Default tiles brand your app and make it visible and easy to access. Adaptive tiles are powerful tools for extending your app experience and providing small moments of what is called delight in user experience design.

In the next two tasks, we will explore tools that make it easier to design and generate default and adaptive tiles.

<a name="Ex2Task1"></a>
#### Task 1 – Generate default tiles ####

Providing assets for default tiles is a quick and easy way to brand your app, and there are tools that can make it even easier. The UWP Tile Generator is a Visual Studio extension that takes a single logo asset and generates all the different assets you need for a range of default tiles.

In this task, we will add a logo asset to the project, and use the UWP Tile Generator to generate default tiles and splash assets.

1. Open **Tools > Extensions and Updates** in Visual Studio and browse to the **Installed** tab.

1. Use the search box to search for **UWP Tile Generator**.

1. The UWP Tile Generator extension is already installed on the workshop machines. If you need to install it on a dev machine, you can switch to the Online tab, find the extension and then use the **Download** button to download and install the extension. 

	![The UWP Tile Generator extension](Images/uwp_tile_extension.png?raw=true "The UWP Tile Generator extension")

	_The UWP Tile Generator extension_

1. The UWP Tile Generator extension is a tool to quickly and easily create Tile assets for a UWP project. We can add a logo asset to the project and use the extension to generate default tile and splash assets.

	First, open the **Package.appxmanifest** and browse to the **Visual Assets** tab. Select **All Image Assets** to view the current tiles and splash assets. You’ll see that the placeholder UWP app tile appears for the recommended tile sizes.

1. Right-click the **Assets** directory in the Solution Explorer and choose **Add > Existing Item**. Add the logo asset from **C:\Labs\CodeLabs-UWP\Workshop\Module 1-AdaptiveUI\Begin\Assets\Tile_Logo.png**.

	> **Note:** The new assets that will be generated using the extension will appear in the same folder as the original image. You may choose to add a subfolder to the Assets directory and place the new logo image in the subfolder for better organization.
	> 
	> When using a generator extension, it is a good idea to use a high resolution seed image in order to generate good quality results for the largest tile and splash assets.

1. Right-click the **Tile_Logo.png** asset in the Solution Explorer and select **Generate UWP Tiles** from the context menu.

1. Right-click the asset again. This time, select **Generate UWP Splash** from the context menu. You will see the new assets appear in the same folder as the original logo image.

1. Return to the package manifest editor. The Visual Assets tab should now be filled out with the new default tile and splash assets.

	The Tile_Logo asset has a transparent background, so we’ll need to define the brand color in the app manifest. If we don’t define a background color, it will default to the user’s system theme color.

1. Under **All Image Assets**, set the **Tile background color** and the **Splash background color** to **#2dA092** in hex notation.

1. Run the app on the Local Machine. You’ll see the new splash screen show up. Pin the app to the Start Menu to view the default tiles. Resize to view the Wide and Large tiles.

1. Stop debugging and return to Visual Studio.

> **Note:** The UWP Tile Generator tool is a quick and easy way to generate Tile assets. In many cases, a designer might well want to use professional image editing tools to create customized artwork for different sizes.

<a name="Ex2Task2"></a>
#### Task 2 – Create adaptive tiles ####

While default tiles are a great way to start branding your app, adaptive tiles take the user experience to the next level. There are tools available that can help you design and implement adaptive tiles in your UWP app. We will use the Notifications Visualizer app to preview and build adaptive tile XML and the NotificationsExtension NuGet package to generate the XML in the app using code.

The **Notifications Visualizer** app is available from the Windows Store. You can install this and use it to help you design tiles and notifications.

![The Notifications Visualizer app is available from the Windows Store](Images/notifications_visualizer_store2.png?raw=true "The Notifications Visualizer app is available from the Windows Store")

_The Notifications Visualizer app is available from the Windows Store_

Do not install it from the store now - it is pre-installed on your computer.

#### Instructor Demo: Using the Notifications Visualizer app

1. Open the Notifications Visualizer app. Use the **Payloads** dropdown menu to browse through the sample tile XML.

	![Browse sample XML for adaptive tiles in the Notifications Visualizer app](Images/notifications_visualizer.png?raw=true "Browse sample XML for adaptive tiles in the Notifications Visualizer app")

	_Browse sample XML for adaptive tiles in the Notifications Visualizer app_

	> **Note:** The image path in the sample XML is defined as a relative path from the Notifications Visualizer app folder.

1. Use the **Pick Folder of XML Files** button to select the **C:\Labs\CodeLabs-UWP\Workshop\Module 1-AdaptiveUI\Begin\Microsoft.Labs.SightsToSee\Assets** folder in the file explorer. The **AdaptiveTiles.xml** file will automatically load into the XML Payload window.

    > **Note:** For more on adaptive tile and toast schema and implementation, check out the **Help (?)** section of the Notifications Visualizer app or visit https://msdn.microsoft.com/en-us/library/windows/apps/xaml/mt185606.aspx

1. Open the **Settings** pane in the Notifications Visualizer app. Set the tile background color to **#2da092**.

1. Click the placeholder **Square44x44Logo** and use the file picker to replace it with the **Square44x44Logo.scale-100.png** asset from the SightsToSee app Assets folder. You can repeat the process with the other logos declared in the Settings pane.

1. Change the app preview name to **SightsToSee**.

1. Return to the tile preview. 

    ##### End of Instructor Demo #####
    
#### Workshop Steps ####

Now that we’ve examined the structure of the adaptive tile XML, let’s add adaptive tiles to the SightsToSee app. We will use the **Notifications Extensions** NuGet package to help to generate tile, toast, and badge notifications for Windows 10 using code instead of XML, which also gives you access to Intellisense.

The structure of the XML we’re going to generate with code will reflect the structure of the XML file we just previewed in the Notifications Visualizer app.

1. Return to Visual Studio and right-click the **SightsToSee** project name in the Solution Explorer. Select **Manage NuGet packages** from the context menu.

1. On the **Browse** tab of the NuGet Package Manager, search for **Notifications Extensions**.

1. Install the **NotificationsExtensions.Win10** NuGet Package.

1. In the Solution Explorer, right-click the **Services > TileNotificationService** folder and choose **Add > Existing Item**. Browse to the **C:\Labs\CodeLabs-UWP\Workshop\Module 1-AdaptiveUI\Begin\Assets** folder and add **TileHelper.cs**.

    > **Note:** The TileHelper generates XML similar to the XML we previewed in the Notifications Visualizer app. It looks for the first five Sights added to a Trip, and displays a peek image from each Sight along with the Sight name and description on an adaptive tile. The TileHelper also generates the tile notification.

1. Now that the tile helper is part of the project, we can call it from our code. Open **ViewModels > TripDetailPageViewModel.cs**.

1. Find the comment telling you where to insert the **M1_CreateTiles** snippet and expand the **M1_CreateTiles** snippet there.

	(Code Snippet - _M1_CreateTiles_)

	````C#
	TileHelper.SetInteractiveTilesForTrip(CurrentTrip);
	// Also whenever the MySights collection changes
	Sights.CollectionChanged += (s, a) =>
		TileHelper.SetInteractiveTilesForTrip(CurrentTrip);
	````

1. Build and run the app. Pin the app to the Start Menu to see the adaptive tile behavior. Resize the tile to wide or large to see the adaptive behavior.

	![The wide adaptive tile and its peek image](Images/adaptive_tile1.png?raw=true "The wide adaptive tile and its peek image")

	_The wide adaptive tile and its peek image_

1. Stop debugging and return to Visual Studio.

<a name="Exercise3"></a>
### Exercise 3: Maps ###

Maps provide a great way to add visual interaction with the Sights in the app. We already have a Map control in the app layout. Let’s add some features to the map to enhance the user experience.

<a name="Ex3Task1"></a>
#### Task 1 – Use Maps features ####

In this task, we will display the Sights as PushPins on the map, enable Aerial3D Map View, and enable ShowStreet mode.

1. Open TripDetailPage.xaml. Expand the **M1_MapItems** snippet inside the MapControl. The Map items in the MapItemsControl are bound to the list of Sights. Sights added to **My Sights** will display as larger PushPins with borders. **Suggested Sights** will display as smaller PushPins without borders.

	(Code Snippet - _M1_MapItems_)

	````XAML
	<maps:MapControl x:Name="Map"
						Grid.Row="1"
						MapServiceToken="{x:Bind ViewModel.MapServiceToken}"
						ZoomInteractionMode="GestureAndControl"
						TiltInteractionMode="GestureAndControl">
		<maps:MapItemsControl ItemsSource="{x:Bind ViewModel.CurrentTrip.Sights, Mode=OneWay}">
			<maps:MapItemsControl.ItemTemplate>
				 <DataTemplate x:DataType="models:Sight">
					  <Grid FlyoutBase.AttachedFlyout="{StaticResource     
							  SightMapFlyout}"
							  Tapped="MapPinTapped">
						  <Ellipse Stroke="{StaticResource
								  NavMenuForegroundPressedBrush}"
								  StrokeThickness="2"
								  maps:MapControl.Location="{x:Bind Location}"
								  maps:MapControl.NormalizedAnchorPoint="0.5,0.5"
								  Height="36"
								  Width="36"
								  Visibility="{x:Bind
								  IsMySight, Mode=OneWay, Converter={StaticResource
								  BooleanToVisibilityConverter}}"
								  Fill="{StaticResource SightGridItemHoverBrush}"/>
							<Ellipse Stroke="{StaticResource
								  NavMenuForegroundPressedBrush}"
								  StrokeThickness="0"
								  maps:MapControl.Location="{x:Bind Location}"
								  maps:MapControl.NormalizedAnchorPoint="0.5,0.5"
								  Height="18"
								  Width="18"
								  Opacity=".75"
								  Visibility="{x:Bind
								  IsMySight, Mode=OneWay, Converter={StaticResource
								  InverseBooleanToVisibilityConverter}}"
								  Fill="{StaticResource SightGridItemHoverBrush}"/>
							<Ellipse maps:MapControl.Location="{x:Bind Location}"
								  maps:MapControl.NormalizedAnchorPoint="0.5,0.5"
								  Visibility="{x:Bind IsMySight, Mode=OneWay,
								  Converter={StaticResource
								  BooleanToVisibilityConverter}}"
								  Height="8"
								  Width="8"
								  Fill="Black" />
							<Ellipse maps:MapControl.Location="{x:Bind Location}"
								  maps:MapControl.NormalizedAnchorPoint="0.5,0.5"
								  Visibility="{x:Bind IsMySight, Mode=OneWay,
								  Converter={StaticResource
								  InverseBooleanToVisibilityConverter}}"
								  Height="4"
								  Width="4"
								  Fill="Black" />
					  </Grid>
				 </DataTemplate>
			</maps:MapItemsControl.ItemTemplate>
		</maps:MapItemsControl>
	</maps:MapControl>
	````

1. Open **TripDetailPage.xaml**. Expand the **M1_Flyout** snippet immediately after the GridViewHeaderItem style. This Flyout will pop up when a map PushPin is selected and show icons to enable Aerial3D and ShowStreet modes.

	(Code Snippet - _M1_Flyout_)

	````XAML
	<Flyout x:Key="SightMapFlyout"
				 Placement="Top">
		<Grid DataContext="{Binding}">
			<Grid.ColumnDefinitions>
				 <ColumnDefinition Width="Auto" />
				 <ColumnDefinition Width="*" />
			</Grid.ColumnDefinitions>
			<Image Source="{Binding ImageUri, Mode=OneWay}"
								Grid.Column="0"
								Height="72"
								Width="72"
								Margin="0,0,4,0"
								Stretch="UniformToFill" />
			<StackPanel Grid.Column="1">
				 <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
										  Text="{Binding Name, Mode=OneWay}"
										  Margin="0,0,0,8" />
				 <StackPanel Orientation="Horizontal">
					  <Button Style="{StaticResource CircleButtonStyle}"
											Click="{x:Bind ViewModel.Show3D}">
							<FontIcon
											x:Name="View3d"
											VerticalAlignment="Center"
											FontFamily="{ThemeResource
											SymbolThemeFontFamily}"
											FontSize="16"
											Glyph="&#xEC07;" />
					  </Button>
					  <Button Style="{StaticResource CircleButtonStyle}"
											Click="{x:Bind ViewModel.ShowStreet}">
							<FontIcon
											x:Name="StreetView"
											VerticalAlignment="Center"
											FontFamily="{ThemeResource
											SymbolThemeFontFamily}"
											FontSize="16"
											Glyph="&#xE803;" />
					  </Button>
					  <Button Style="{StaticResource CircleButtonStyle}"
											Click="{x:Bind ViewModel.ShowDetail}">
							<FontIcon
											x:Name="Detail"
											VerticalAlignment="Center"
											FontFamily="{ThemeResource
											SymbolThemeFontFamily}"
											FontSize="16"
											Glyph="&#xE10C;" />
					  </Button>
				 </StackPanel>
			</StackPanel>
		</Grid>
		<Flyout.FlyoutPresenterStyle>
			<Style TargetType="FlyoutPresenter">
				 <Setter Property="ScrollViewer.ZoomMode" Value="Enabled" />
				 <Setter Property="Background" Value="White" />
				 <Setter Property="BorderBrush" Value="Gray" />
				 <Setter Property="BorderThickness" Value="2" />
				 <Setter Property="MinHeight" Value="80" />
				 <Setter Property="MinWidth" Value="80" />
				 <Setter Property="Padding" Value="4" />
			</Style>
		</Flyout.FlyoutPresenterStyle>
	</Flyout>
	````

1. Add the **FlyoutBase** attribute to the Grid in the DataTemplate for the MapItemsControl.ItemTemplate. There is no code snippet for this attribute, but you can use Intellisense or copy it from the code sample below.

	````XAML
	<maps:MapItemsControl.ItemTemplate>
		<DataTemplate x:DataType="models:Sight">
			<Grid FlyoutBase.AttachedFlyout="{StaticResource SightMapFlyout}"
														 Tapped="MapPinTapped">
	````

1. Open **ViewModels > TripDetailPageViewModel.cs**. Expand the **M1_Show3D** snippet where indicated by the comment towards the bottom of the view model.

	The **Show3D()** method hides the flyout and sets the Map style to **Aerial3DWithRoads**. It also sets the scene by controlling the pitch, direction, and radius of the 3D view.

	(Code Snippet - _M1_Show3D_)

	````C#
	public async void Show3D(object sender, RoutedEventArgs e)
	{
		Flyout?.Hide();
		// sender is the button - and the data context is the Sight
		_currentSight = ((Button)sender).DataContext as Sight;
		if (_currentSight == null)
			return;

		if (Map.Is3DSupported)
		{
			// Set the aerial 3D view.
			Map.Style = MapStyle.Aerial3DWithRoads;
			// Create the map scene.
			var hwScene =
				 MapScene.CreateFromLocationAndRadius(_currentSight.Location,
				 200, /* show this many meters around */
				 0, /* looking at it to the North*/
				 60 /* degrees pitch */);
			// Set the 3D view with animation.
			await Map.TrySetSceneAsync(hwScene, MapAnimationKind.Bow);
			IsDisplay3D = true;
		}
		else
		{
			// If 3D views are not supported, display dialog.
			var viewNotSupportedDialog = new ContentDialog
			{
				 Title = "3D is not supported",
				 Content = "\n3D views are not supported on this device.",
				 PrimaryButtonText = "OK"
			};
			await viewNotSupportedDialog.ShowAsync();
		}
	}

	public async void ShowStreet(object sender, RoutedEventArgs e)
	{

	}
	````

	> **Note:** The M1_Show3D code snippet includes a stubbed method for ShowStreet(). We will add the contents of the ShowStreet method in a later step.

1. Build and run the app. The PushPins will animate in with Bow animation. Click a PushPin to view its flyout. Use the building icon to open Aerial3D mode.

	![Aerial3D view](Images/aerial3d.png?raw=true "Aerial3D view")

	_Aerial3D view_

1. Stop debugging and return to Visual Studio.

1. Let’s add the code to enable Streetside mode. The button in the XAML flyout and the stubbed method in the view model are already in place. Expand the **M1_ShowStreet** code snippet inside the **ShowStreet()** method in the view model.

	This method hides the flyout and turns on the Streetside overlay if it is available.

	(Code Snippet - _M1_ShowStreet_)

	````C#
	public async void ShowStreet(object sender, RoutedEventArgs e)
	{
		Flyout?.Hide();
		// sender is the button - and the data context is the Sight
		_currentSight = ((Button)sender).DataContext as Sight;
		if (_currentSight == null)
			return;

		// Check if Streetside is supported.
		if (Map.IsStreetsideSupported)
		{
			var panorama = await
				 StreetsidePanorama.FindNearbyAsync(_currentSight.Location);

			// Set the Streetside view if a panorama exists.
			if (panorama != null)
			{
				 // Create the Streetside view.
				 var ssView = new StreetsideExperience(panorama) {
								  OverviewMapVisible = true };
				 Map.CustomExperience = ssView;
			}
			else
			{
				 var viewNotSupportedDialog = new ContentDialog
				 {
					  Title = "Streetside not available",
					  Content = "\nNo Streetside view found for this location.",
					  PrimaryButtonText = "OK"
				 };
				 await viewNotSupportedDialog.ShowAsync();
			}
		}
		else
		{
			// If Streetside is not supported
			var viewNotSupportedDialog = new ContentDialog
			{
				 Title = "Streetside is not supported",
				 Content = "\nStreetside views are not supported on this device.",
				 PrimaryButtonText = "OK"
			};
			await viewNotSupportedDialog.ShowAsync();
		}
	}
	````

1. Build and run the app. Select a Sight on the map and use the street icon to open the Streetside overlay.

	![Streetside view](Images/streetside.png?raw=true "Streetside view")

	_Streetside view_

1. Stop debugging and return to Visual Studio.

<a name="Summary"></a>
## Summary ##

In this module, you learned techniques to design and build an adaptive UI for multiple Windows devices. You used tools to generate and build default and adaptive tiles that enhance the user experience and extend interactions with the app onto the start menu.
