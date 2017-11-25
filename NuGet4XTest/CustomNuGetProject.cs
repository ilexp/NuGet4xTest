using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Linq;

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
using NuGet.Versioning;

namespace NuGet4XTest
{
	public class CustomNuGetProject : FolderNuGetProject
	{
		private NuGetFramework framework;

		public CustomNuGetProject(string path, NuGetFramework framework) : base(path, new PackagePathResolver(path), framework)
		{
			this.framework = framework;
		}

		public override Task PreProcessAsync(INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			Console.WriteLine("PreProcessAsync");
			return base.PostProcessAsync(nuGetProjectContext, token);
		}
		public override Task PostProcessAsync(INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			Console.WriteLine("PostProcessAsync");
			return base.PostProcessAsync(nuGetProjectContext, token);
		}
		public override Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
		{
			Console.WriteLine("GetInstalledPackagesAsync");

			List<PackageReference> installedPackages = new List<PackageReference>();
			foreach (string packageFolder in Directory.EnumerateDirectories(this.Root))
			{
				string packageFolderName = Path.GetFileName(packageFolder);
				string packageFileName = packageFolderName + ".nupkg";
				string packageFilePath = Path.Combine(packageFolder, packageFileName);
				if (!File.Exists(packageFilePath)) continue;

				int dotIndex = packageFolderName.Length;
				int dotCount = 0;
				while (true)
				{
					dotIndex = packageFolderName.LastIndexOf('.', dotIndex - 1);
					if (dotIndex == -1) break;

					dotCount++;
					if (dotCount < 3) continue;

					string potentialVersionString = packageFolderName.Substring(
						dotIndex + 1, 
						packageFolderName.Length - dotIndex - 1);
					NuGetVersion version;
					if (!NuGetVersion.TryParse(potentialVersionString, out version))
						continue;

					string packageName = packageFolderName.Remove(dotIndex);
					PackageIdentity identity = new PackageIdentity(packageName, version);

					installedPackages.Add(new PackageReference(identity, this.framework));
					break;
				}
			}

			foreach (PackageReference item in installedPackages)
			{
				Console.WriteLine("- {0}", item);
			}

			return Task.FromResult(installedPackages as IEnumerable<PackageReference>);
		}
		public override Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			Console.WriteLine("InstallPackageAsync({0})", packageIdentity);
			return base.InstallPackageAsync(packageIdentity, downloadResourceResult, nuGetProjectContext, token);
		}
		public override Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			Console.WriteLine("UninstallPackageAsync({0})", packageIdentity);
			return base.UninstallPackageAsync(packageIdentity, nuGetProjectContext, token);
		}
	}
}
