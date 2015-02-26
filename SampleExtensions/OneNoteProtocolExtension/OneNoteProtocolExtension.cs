using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFDoistExtLib;

namespace OneNoteProtocolExtension {
	public class OneNoteProtocolExtension : iParserExtensionProtocolHandler {
		public string regexp_to_find { get { return @"(onenote:[^ #]+?([^\\\/]+)[\\\/]([a-zA-Z0-9]+)\.one#(?:([^&]+)&amp;section)?[^ ]+)"; } }
		public string regexp_replace_with_func_body { get { return "return decodeURIComponent(arguments[1]) +': ' + decodeURIComponent(arguments[2]) + (arguments[3] !== undefined ? ' - ' + decodeURIComponent(arguments[3]) : '');"; } }
		public string override_url_func_body { get { return null; } }
	}

}
