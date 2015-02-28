# Why?
-	Todoist Windows app has some decent issues and less than great dependencies.

# Features
-	WPF Native browser control
-	Windows keyboard shortcut support for adding tasks/bringing to front
-	Allows adding of additional javascript/css to the page (to have your code after full load add a js function called UserOnLoaded)
-	Outlook email links
-	Support for arbitrary link patterns in code
-	Extension support for custom handling, custom formatting, and custom protocol recognition (for example onenote: link support!). See [SampleExtensions\Readme.md](https://github.com/mitchcapper/wpfdoist/blob/master/SampleExtensions/Readme.md)  for details.


# Known Issues
-	Note while this works with outlook email links it only works with the official email ID.  This ID changes when the email changes folders.
-	Primarily tested with outlook 2013/2016 and windows 8.1/10.
-	The apps IE compat mode will not be set right on first launch (so exit and re-launch the very first time to get the browser to behave right).
-	Rarely (maybe slow load?) the extensions are not properly loaded. I am not yet sure why this happens, but waiting seems to fix it (you can also right click on the systray and reload otherwise).
-	If you get the error when running setup of: "You cannot start application WPFDoist from this location because it is already installed from a different location."  It is because the installer is at a different location from where you last installed from (yay one click installers) to fix just uninstall from add/remove programs before re-running the installer.  You will not lose your settings or user data folder.

# Directions
-	Install app (can run standalone in a folder as portable also, its just a rar archive as an exe)
	-	You can install by using the setup exe under releases.
	-	Double click it, it will ask for a temporary folder to copy the install files to and will then run setup.
	-	Once it installs you can find it in your start menu under WPFDoist.
	-	Primarily designed for Windows 8 / IE10 or newer (but may work on older editions)
-	For first launch after install be sure to restart the app after starting it, as it must set compatibility settings for the browser
-	Just login to the browser frame for the first load (should not have to do every start)
-	Options are accessible by right clicking on the systray icon
-	Browser controls work as normal IE,  you can use control +/- to zoom
-	Keyboard shortcut to bring to front/add task: (defaults to control+shift+t)
-	You can change the data directory from the c:\users\appdata\.... to any folder you want by setting the registry key: HKEY_CURRENT_USER\Software\WPFDoist\DataDir (string) to whatever you like (ie r:\wpfdoist_shared\)