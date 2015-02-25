# Why?
-	Todoist Windows app has some decent issues and less than great dependencies.

# Features
-	WPF Native browser control
-	Windows keyboard shortcut support for adding tasks/bringing to front
-	Allows adding of additional javascript/css to the page (to have your code after full load add a js function called UserOnLoaded)
-	Outlook email links
-	Support for arbitrary link patterns in code


# Known Issues
-	Note while this works with outlook email links it only works with the official email ID.  This ID changes when the email changes folders.
-	Primarily tested with outlook 2013/2016 and windows 8.1/10.
-	The apps IE compat mode will not be set right on first launch (so exit and re-launch the very first time to get the browser to behave right).

# Directions
-	Install app (can run standalone in a folder as portable also, its just a rar archive as an exe)
-	For first launch after install be sure to restart the app after starting it, as it must set compatibility settings for the browser
-	Options are accessible by right clicking on the systray icon
-	Browser controls work as normal IE,  you can use control +/- to zoom
-	Keyboard shortcut to bring to front/add task: (defaults to control+shift+t)