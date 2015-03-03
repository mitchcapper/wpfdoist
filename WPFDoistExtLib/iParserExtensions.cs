using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFDoistExtLib
{
	public interface iParserExtension {
		string regexp_to_find { get; } //regexp to find the initial link
		string regexp_replace_with_func_body { get; }
	}
	public interface iParserExtensionLink : iParserExtension {
		void HandleLink(String link);
	}
	public interface iParserExtensionProtocolHandler : iParserExtension {
		string override_url_func_body { get; } //only required if $1 won't match the url for use
	}

}
