using System;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Duality.Editor.PackageManagement
{
	internal class Program
	{
		public static async Task Main(string[] args)
		{
			Task mainTask = MainAsync(args);
			await mainTask;
			Console.ReadLine();
		}
		private static async Task MainAsync(string[] args)
		{
			var packageManager = new DualityPackageManager("ProjectRoot", "Packages");
			var f =  (await packageManager.Search()).ToArray();
			var versions = (await f.ToArray()[0].GetVersionsAsync()).ToArray();
			var package = f.FirstOrDefault();
			//await packageManager.InstallPackage(package.Identity.Id, package.Identity.Version);

			//var package2 = new PackageIdentity("Singularity.Duality.core", new NuGetVersion(0, 1, 3, 68));
			//await packageManager.InstallPackage(package2.Id, package2.Version);
			var result = await packageManager.GetInstalledPackageIdentities();
			var m = await packageManager.GetInstalledPackages();
			//await packageManager.InstallPackage("Newtonsoft.Json", new NuGetVersion(10, 0, 1));
			//await packageManager.InstallPackage("AdamsLair.Duality.Primitives", new NuGetVersion(2, 0, 4));

			//await packageManager.UpdatePackage("Newtonsoft.Json");
			//await packageManager.UninstallPackage("Newtonsoft.Json");
		}
	}
}
