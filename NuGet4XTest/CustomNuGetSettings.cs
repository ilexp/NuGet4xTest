using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

using NuGet.Common;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Resolver;
using System.Xml.Linq;

namespace NuGet4XTest
{
	public class CustomNuGetSettings : ISettings
	{
		private struct PackageSourceItem
		{
			public string Name;
			public string Path;
			public string ProtocolVersion;

			public PackageSourceItem(string name, string path, string protocolVersion)
			{
				this.Name = name;
				this.Path = path;
				this.ProtocolVersion = protocolVersion;
			}
		}
		
		private static readonly PackageSourceItem[] DefaultPackageSources = new PackageSourceItem[]
		{
			new PackageSourceItem("nuget.org", "https://api.nuget.org/v3/index.json", "3")
		};


		private string projectRootPath = null;
		private List<PackageSourceItem> packageSources = new List<PackageSourceItem>();


		public string Root
		{
			get { return this.projectRootPath; }
		}
		string ISettings.FileName
		{
			get { return "CustomNuGetSettings"; }
		}
		IEnumerable<ISettings> ISettings.Priority
		{
			get { return new[] { this }; }
		}


		public CustomNuGetSettings(string projectRootPath)
		{
			this.projectRootPath = projectRootPath;
			this.packageSources.AddRange(DefaultPackageSources);
		}

		
		string ISettings.GetValue(string section, string key, bool isPath)
		{
			string result = null;
			if (section == "config" && key == "globalPackagesFolder") { }
			else
			{
				Console.WriteLine("Unresolved GetValue({0}, {1}, {2})", section, key, isPath);
				string baseValue = new Settings(this.projectRootPath).GetValue(section, key, isPath);
				Console.WriteLine("  {0}", baseValue);
			}
			return result;
		}
		IList<SettingValue> ISettings.GetSettingValues(string section, bool isPath)
		{
			List<SettingValue> result = new List<SettingValue>();
			if (section == "packageSources")
			{
				foreach (PackageSourceItem item in this.packageSources)
				{
					SettingValue value = new SettingValue(item.Name, item.Path, this, false);
					value.AdditionalData.Add("protocolVersion", item.ProtocolVersion);
					result.Add(value);
				}
			}
			else if (section == "disabledPackageSources") { }
			else
			{
				Console.WriteLine("Unresolved GetSettingValues({0}, {1})", section, isPath);
				IList<SettingValue> baseValue = new Settings(this.projectRootPath).GetSettingValues(section, isPath);
				foreach (SettingValue item in baseValue)
				{
					Console.WriteLine("  {0}: {1}, + {2} additional", item.Key, item.Value, item.AdditionalData.Count);
				}
			}
			return result;
		}
		IList<KeyValuePair<string, string>> ISettings.GetNestedValues(string section, string subSection)
		{
			List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
			if (section == "packageSourceCredentials") { }
			else
			{
				Console.WriteLine("Unresolved GetNestedValues({0}, {1})", section, subSection);
				IList<KeyValuePair<string, string>> baseValue = new Settings(this.projectRootPath).GetNestedValues(section, subSection);
				foreach (KeyValuePair<string, string> item in baseValue)
				{
					Console.WriteLine("  {0}", item);
				}
			}
			return result;
		}
		
		event EventHandler ISettings.SettingsChanged
		{
			add { }
			remove { }
		}

		bool ISettings.DeleteSection(string section)
		{
			throw new NotImplementedException();
		}
		bool ISettings.DeleteValue(string section, string key)
		{
			throw new NotImplementedException();
		}

		void ISettings.SetValue(string section, string key, string value)
		{
			throw new NotImplementedException();
		}
		void ISettings.SetValues(string section, IReadOnlyList<SettingValue> values)
		{
			throw new NotImplementedException();
		}
		void ISettings.SetNestedValues(string section, string subSection, IList<KeyValuePair<string, string>> values)
		{
			throw new NotImplementedException();
		}

		void ISettings.UpdateSections(string section, IReadOnlyList<SettingValue> values)
		{
			throw new NotImplementedException();
		}
	}
}
