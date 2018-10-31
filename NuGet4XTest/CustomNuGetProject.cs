using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Duality.Editor.PackageManagement
{
	public class CustomNuGetProject : FolderNuGetProject
	{
		private NuGetFramework framework;

		public CustomNuGetProject(string root, NuGetFramework framework) : base(root, new PackagePathResolver(root), framework)
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
			if (!Directory.Exists(this.Root)) Directory.CreateDirectory(this.Root);
			foreach (string packageFolder in Directory.EnumerateDirectories(this.Root))
			{
			    string packageFolderName = Path.GetFileName(packageFolder);
                var versionStartIndex = packageFolderName.Length - 1;
			    for (var i = packageFolderName.Length - 1; i >= 0; i--)
			    {
			        if (char.IsDigit(packageFolderName[i]) || packageFolderName[i] == '.')
			        {
			            versionStartIndex = i;
                    }
			        else
			        {
			            break;
			        }
			    }

			    var versionString = packageFolderName.Substring(versionStartIndex + 1);
                var idString = packageFolderName.Substring(0, versionStartIndex);

				NuGetVersion version;
				if (!NuGetVersion.TryParse(versionString, out version))
			        continue;

			    PackageIdentity identity = new PackageIdentity(idString, version);
			    installedPackages.Add(new PackageReference(identity, this.framework));
			}

			foreach (PackageReference item in installedPackages)
			{
				Console.WriteLine("- {0}", item);
			}

			return Task.FromResult((IEnumerable<PackageReference>) installedPackages);
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
