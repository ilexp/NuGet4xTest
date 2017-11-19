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
	public class CustomNuGetProject : FolderNuGetProject
	{
		public CustomNuGetProject(string path, NuGetFramework framework) : base(path, new PackagePathResolver(path), framework)
		{

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
			return base.GetInstalledPackagesAsync(token);
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
