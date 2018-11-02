using System;
using System.Linq;

namespace Duality.Editor.PackageManagement
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var packageManager = new DualityPackageManager("ProjectRoot", "Packages");
			var f = packageManager.Search().ToArray();
			var versions = f.ToArray()[0].GetVersions().ToArray();
			var package = f.FirstOrDefault();
			packageManager.InstallPackage(package.Identity.Id, package.Identity.Version);

			//var package2 = new PackageIdentity("Singularity.Duality.core", new NuGetVersion(0, 1, 3, 68));
			//await packageManager.InstallPackage(package2.Id, package2.Version);
			var result = packageManager.GetInstalledPackageIdentities();
			var m = packageManager.GetInstalledPackages();
			//await packageManager.InstallPackage("Newtonsoft.Json", new NuGetVersion(10, 0, 1));
			//await packageManager.InstallPackage("AdamsLair.Duality.Primitives", new NuGetVersion(2, 0, 4));

			//await packageManager.UpdatePackage("Newtonsoft.Json");
			//await packageManager.UninstallPackage("Newtonsoft.Json");
			Console.ReadLine();
		}
	}
}
