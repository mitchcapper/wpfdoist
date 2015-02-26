using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFDoistExtLib;

namespace HighlightExtension {
	public class HighlightExtension : iParserExtension {
		public string regexp_to_find { get { return "_([^ ]+)_"; } }
		public string regexp_replace_with_func_body { get { return "return \"<b style='color:yellow'>\" + arguments[1] + '</b>';"; } }
	}
}
