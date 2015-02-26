# Extensions
Extensions can go in the application folder or the user folder.
Extensions can be one of the following types:
-	ParserExtension - Just replaces the regex match with html/text returned by the function
-	ParserExtensionLink - calls a .net function and passes the first regex match to it when clicked
-	ParserExtensionProtocolHandler - used to primarily convert regex matches to valid links (ie to support custom protocols like onenote:)
-	Plugins can go in the application folder (a bit trickier to figure out C:\Users\YOUR_USERNAME\AppData\Local\Apps\2.0\random.....) or in the app data folder for the application: C:\Users\YOUR_USERNAME\AppData\Local\Mitch_Capper\WPFDoist
-	Careful of your javascript, if its invalid it will probably prevent todoist from working until you remove your bad plugin
-	Extensions in .net or xml (xml extensions cannot be used for link type as the point of the link type is to call a .net function in code)

# Extension Members
-	All extensions have the following members:
--	regexp_to_find - This should be a regex representing the text in the task we want to be replacing.  Note todist converts certain entities like & to &amp;.  You should have atleast one capture group (part in parens in the regex) that represents the 
--	regexp_replace_with_func_body - this function is called (passing additional captures to it accessible in the arguments array) and you should return the Text/html you want to replace the text you extracted out.  This should not be a full <a href link in the case of the other extension types (we will handle that wiring for you) just the display html
-	ParserExtensionLink (.net only) has one additional function:
--		HandleLink(String link) - This will be called when the user clicks on that part of the task, and link will be the first capture in your regex
-	ParserExtensionProtocolHandler has one additional (optional) member:
--		override_url_func_body - This you can leave empty (but must still specify it).  If not empty the code will call this function and pass in the regex matches.  This is primarily useful if you need to do more than just a regex to get your url into a usable form. You should return a valid link here (it will be what document.location is set to).

## .NET Plugins
-	Create a new project of type "Class Library" add a reference to the WPFDoistExtLib.dll then make sure your class inherits from the proper interface for your type: iParserExtension, iParserExtensionLink, iParserExtensionProtocolHandler
-	Make sure your project name and DLL both end in Extension (so it should result in YourPluginNameExtension.dll) or else it wont be loaded

## XML Plugins
-	For xml enclose your fields with <![CDATA[YOUR_CONTENT_HERE]]> to avoid having to worry about xml escaping
-	Make sure you name your nodes properly see examples for how

## Examples
-	There are .net and javascript examples for each type.  Look at the SampleExtensions folder for how to write plugins.  *.xml files in that folder are the xml examples (also see the OneNoteProtocolExtension.xml in the main folder).  The Project files are .net examples.


## Debugging
-	To help with debugging enable debugging in options.  c:\temp\js_debug.js will include the JS we are injecting into the page.
-	Also you can use the external logger in your function bodies to help with debugging like:
window.external.log(obj_str(arguments));
(obj_str just serializes objects).

