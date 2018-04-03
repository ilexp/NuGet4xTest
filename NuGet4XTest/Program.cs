using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Resolver;
using NuGet.Versioning;

namespace NuGet4XTest
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Task mainTask = MainAsync(args);
			mainTask.Wait();
			Console.ReadLine();
		}
		private static async Task MainAsync(string[] args)
		{
			var packageManager = new DualityPackageManager("ProjectRoot", "Packages");
			var f =  await packageManager.Search();
			var versions = await f.ToArray()[0].GetVersionsAsync();
			var package = f.FirstOrDefault();
			await packageManager.InstallPackage(package.Identity.Id, package.Identity.Version);

			//await packageManager.InstallPackage("Newtonsoft.Json", new NuGetVersion(10, 0, 1));
			//await packageManager.InstallPackage("AdamsLair.Duality.Primitives", new NuGetVersion(2, 0, 4));
			//await packageManager.UpdatePackage("Newtonsoft.Json");
			//await packageManager.UninstallPackage("Newtonsoft.Json");
		}
	}
}
