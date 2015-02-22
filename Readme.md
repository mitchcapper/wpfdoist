# Why?
-	Todoist Windows app has some decent issues and less than great dependencies.

# Features
-	WPF Native browser control (so zoom and other IE controls work)
-	Windows keyboard shortcut support for adding tasks
-	Allows adding of additional javascript/css to the page (to have your code after full load add a js function called UserOnLoaded
-	Outlook email links
-	Support for arbitrary link patterns in code


# Known Issues
-	Note while this works with outlook email links it only works with the official email ID.  This ID changes when the email changes folders.
-	Primarily tested with outlook 2013/2016 and windows 8.1/10.
-	No settings ui yet, edit: C:\Users\[YOUR_USER]\AppData\Local\Mitch Capper\WPFDoist\program_settings.xml
-	after first launch
-	The apps IE compat mode will not be set right on first launch (so exit and re-launch the very first time to get the browser to behave right).