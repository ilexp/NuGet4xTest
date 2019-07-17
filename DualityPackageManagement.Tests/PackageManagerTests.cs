using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duality.Editor.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuGet4XTest;
using NUnit.Framework;

namespace DualityPackageManagement.Tests
{
    [TestFixture]
    public class PackageManagerTests
    {
        private string _rootPath = "ProjectRoot";
        private string _packagesPath = "Packages";
        private DualityPackageManager _dualityPackageManager;

     //   [SetUp]
     //   public void Init()
     //   {
     //       _dualityPackageManager = new DualityPackageManager(_rootPath, _packagesPath);
     //       if (Directory.Exists(_dualityPackageManager.PackagesPath))
     //           Directory.Delete(_dualityPackageManager.PackagesPath, true);
     //   }

	    //[TearDown]
	    //public void Cleanup()
	    //{
		   // _dualityPackageManager = new DualityPackageManager(_rootPath, _packagesPath);
		   // if (Directory.Exists(_dualityPackageManager.PackagesPath))
			  //  Directory.Delete(_dualityPackageManager.PackagesPath, true);
	    //}

		public static IEnumerable<TestCaseData> InstallTestCases
        {
            get
            {
                yield return new TestCaseData("Newtonsoft.Json", new NuGetVersion(10, 0, 1));
                yield return new TestCaseData("Pathfindax", new NuGetVersion(2, 2, 0, 479));
            }
        }

	    public static IEnumerable<TestCaseData> UpdateTestCases
	    {
		    get
		    {
			    yield return new TestCaseData(new PackageIdentity("Newtonsoft.Json", new NuGetVersion(9, 0, 1)), new NuGetVersion(10, 0, 2));
			    yield return new TestCaseData(new PackageIdentity("Pathfindax", new NuGetVersion(2, 2, 0, 479)), new NuGetVersion(2, 2, 2, 603));
		    }
	    }


		[Test, TestCaseSource("InstallTestCases")]
        public void InstallPackage(string packageId, NuGetVersion version)
        {
             //_dualityPackageManager.InstallPackage(packageId, version);

            //var installedPackagesInFolder = Directory.EnumerateDirectories(_dualityPackageManager.PackagesPath).ToArray();
            //var installedPackageInFolder = installedPackagesInFolder.FirstOrDefault(x => x.Contains(packageId));
            //if (installedPackageInFolder == null) Assert.Fail("No installed package found with id {0}", packageId);
            //Assert.IsTrue(installedPackageInFolder.Contains(version.ToString()), "No installed package found with version {0}", version);
        }

	    //[Test, TestCaseSource("UpdateTestCases")]
	    //public void UpdatePackage(PackageIdentity packageIdentity, NuGetVersion versionToUpdateTo)
	    //{			
		//    _dualityPackageManager.InstallPackage(packageIdentity.Id, packageIdentity.Version);
		//	_dualityPackageManager.UpdatePackage(packageIdentity.Id, versionToUpdateTo);
        //
		//    var installedPackagesInFolder = Directory.EnumerateDirectories(_dualityPackageManager.PackagesPath).ToArray();
		//    var installedPackageInFolder = installedPackagesInFolder.FirstOrDefault(x => x.Contains(packageIdentity.Id) && x.Contains(versionToUpdateTo.ToString()));
		//    if (installedPackageInFolder == null) Assert.Fail("No installed package found with id {0}", packageIdentity.Id);
		//    Assert.IsTrue(installedPackageInFolder.Contains(versionToUpdateTo.ToString()), "No installed package found with version {0}", versionToUpdateTo);
	    //}
        //
		//[Test, TestCaseSource("InstallTestCases")]
        //public void GetInstalledPackage(string packageId, NuGetVersion version)
        //{
        //    _dualityPackageManager.InstallPackage(packageId, version);
        //    var installedPackages = _dualityPackageManager.GetInstalledPackageIdentities();
        //
        //    var installedPackage = installedPackages.FirstOrDefault(x => x.Id == packageId);
        //    if (installedPackage == null) Assert.Fail("No installed package found with id {0}", packageId);
        //    Assert.IsTrue(installedPackage.Version == version, "No installed package found with version {0}", version);
        //
        //    var installedPackagesInFolder = Directory.EnumerateDirectories(_dualityPackageManager.PackagesPath).ToArray();
        //    Assert.AreEqual(installedPackagesInFolder.Length, installedPackages.Count());
        //}
    }
}
