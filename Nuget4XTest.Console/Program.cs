using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Editor.PackageManagement;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet4XTest;

namespace Nuget4XTest.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var settings = Settings.LoadDefaultSettings(root: null);
            var nuGetFramework = NuGetFramework.ParseFolder("net472");
            var packagePath = Path.GetFullPath("packages");
            var packageId = "Singularity.Duality.core";
            var packageVersion = "0.13.0";

            var f = new DualityPackageManager(nuGetFramework, logger, settings, packagePath);
            f.InstallPackage(packageId, packageVersion).Wait();

            var installedPackages = f.GetInstalledPackages();

            f.UninstallPackage(packageId, packageVersion).Wait();
            f.UninstallPackage("Singularity", "0.13.0").Wait();

            //Search for packages that have both duality and plugin as tags, also includes prereleases.
            var results = f.Search("tag:duality,plugin", true).Result;

            //var packageManager = new DualityPackageManager("ProjectRoot", "Packages");
            //var f = packageManager.Search().ToArray();
            //var versions = f.ToArray()[0].GetVersions().ToArray();
            //var package = f.FirstOrDefault();
            //packageManager.InstallPackage(package.Identity.Id, package.Identity.Version);

            //var package2 = new PackageIdentity("Singularity.Duality.core", new NuGetVersion(0, 1, 3, 68));
            //await packageManager.InstallPackage(package2.Id, package2.Version);
            //var result = packageManager.GetInstalledPackageIdentities();
            //var m = packageManager.GetInstalledPackages();
            //await packageManager.InstallPackage("Newtonsoft.Json", new NuGetVersion(10, 0, 1));
            //await packageManager.InstallPackage("AdamsLair.Duality.Primitives", new NuGetVersion(2, 0, 4));

            //await packageManager.UpdatePackage("Newtonsoft.Json");
            //await packageManager.UninstallPackage("Newtonsoft.Json");
            System.Console.ReadLine();
        }
    }
}
