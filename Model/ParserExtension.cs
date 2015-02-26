using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Office.Interop.Outlook;
using WPFDoistExtLib;
using System.Xml;

namespace WPFDoist.Model {
	public enum EXT_TYPE { TEXT, LINK, PROTO };
	public class ParserInstance {
		private static int id_cnter = 123;
		public ParserInstance(iParserExtension ext) {
			id = id_cnter++;
			this.ext = ext;
			type = EXT_TYPE.TEXT;
			ext_as_proto = ext as iParserExtensionProtocolHandler;
			if (ext_as_proto != null)
				type = EXT_TYPE.PROTO;
			ext_as_link = ext as iParserExtensionLink;
			if (ext_as_link != null)
				type = EXT_TYPE.LINK;

		}
		public iParserExtension ext;
		public int id;
		public EXT_TYPE type;
		public iParserExtensionProtocolHandler ext_as_proto;
		public iParserExtensionLink ext_as_link;
	}
	public class ExtensionManager {
		private void LoadNetPlugins(String path, List<ParserInstance> exts) {
			try {
				var dlls = Directory.GetFileSystemEntries(path, "*Extension.dll");
				foreach (String full_dll in dlls) {
					try {
						var file_info = new FileInfo(full_dll);
						String dll = file_info.Name;
						if (!dll.Contains("Extension"))
							continue;
						var asm = Assembly.LoadFrom(full_dll);
						if (asm == null)
							continue;
						foreach (Type type in asm.GetTypes()) {
							if (type.IsAbstract)
								continue;

							if (!type.GetInterfaces().Contains(typeof(iParserExtension)))
								continue;
							var plugin = asm.CreateInstance(type.FullName) as iParserExtension;
							exts.Add(new ParserInstance(plugin));
						}
					} catch (System.Exception e) {
						if (Settings.GetSettingB(SET_NAMES.JSDebug))
							MessageBox.Show("Plugin load extension on dll " + full_dll + " of: " + e.Message);
					}
				}

			} catch (System.Exception e) {
				if (Settings.GetSettingB(SET_NAMES.JSDebug))
					MessageBox.Show("Plugin loading error of: " + e.Message);
			}

		}
		private string GetXmlCont(XmlNode node, String name) {
			return node.SelectSingleNode(name).InnerText;
		}
		private void LoadXMLPlugins(String path, List<ParserInstance> exts) {
			try {
				var files = Directory.GetFileSystemEntries(path, "*Extension.xml");
				foreach (var file in files) {
					try {
						var xmldoc = new XmlDocument();
						xmldoc.Load(file);
						var nodes = xmldoc.DocumentElement.SelectNodes("/Extensions/*");
						foreach (XmlNode node in nodes) {
							var type = node.Name.ToLower();
							var regexp_to_find = GetXmlCont(node,"regexp_to_find");
							var regexp_replace_with_func_body = GetXmlCont(node,"regexp_replace_with_func_body");
							iParserExtension ext=null;
							if (type == "parserextension") {
								ext = new XmlParserExtension{regexp_replace_with_func_body = regexp_replace_with_func_body,regexp_to_find = regexp_to_find};
							} else if (type == "parserextensionprotocolhandler") {
								ext = new XmlProtoParserExtension { override_url_func_body = GetXmlCont(node, "override_url_func_body"), regexp_replace_with_func_body = regexp_replace_with_func_body, regexp_to_find = regexp_to_find };
							} else {
								Debug.WriteLine("unknown extension of type: " + type);
								continue;//unknown extension type
							}
							exts.Add(new ParserInstance(ext));
						}


					} catch (System.Exception e) {
						if (Settings.GetSettingB(SET_NAMES.JSDebug))
							MessageBox.Show("Plugin load extension on xml file " + file + " of: " + e.Message);
					}
				}

			} catch (System.Exception e) {
				if (Settings.GetSettingB(SET_NAMES.JSDebug))
					MessageBox.Show("XML Plugin loading error of: " + e.Message);
			}
		}

		private IEnumerable<ParserInstance> LoadPlugins() {
			Debug.WriteLine(DateTime.Now + ": Plugin load start");
			List<ParserInstance> exts = new List<ParserInstance>();
			String app_dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			String user_dir = Settings.GetUserAppDataPath();
			LoadNetPlugins(app_dir, exts);
			LoadNetPlugins(user_dir, exts);
			LoadXMLPlugins(app_dir, exts);
			LoadXMLPlugins(user_dir, exts);
			Debug.WriteLine(DateTime.Now + ": Plugin load end");
			return exts;
		}
		private IEnumerable<ParserInstance> _exts;
		public IEnumerable<ParserInstance> extensions { get { return _exts ?? (_exts = LoadPlugins()); } }
	}
	public class XmlParserExtension : iParserExtension {
		public string regexp_replace_with_func_body { get; set; }
		public string regexp_to_find { get; set; }
	}
	public class XmlProtoParserExtension : iParserExtensionProtocolHandler {
		public string override_url_func_body { get; set; }
		public string regexp_replace_with_func_body { get; set; }
		public string regexp_to_find { get; set; }
	}
}
