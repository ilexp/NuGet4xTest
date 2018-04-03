using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        [SetUp]
        public void Init()
        {
            _dualityPackageManager = new DualityPackageManager(_rootPath, _packagesPath);
            if (Directory.Exists(_dualityPackageManager.PackagesPath))
                Directory.Delete(_dualityPackageManager.PackagesPath, true);
        }

        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData("Newtonsoft.Json", new NuGetVersion(10, 0, 1));
                yield return new TestCaseData("Pathfindax", new NuGetVersion(2, 2, 0, 479));
            }
        }


        [Test, TestCaseSource(nameof(TestCases))]
        public async Task InstallPackage(string packageId, NuGetVersion version)
        {
            await _dualityPackageManager.InstallPackage(packageId, version);

            var installedPackagesInFolder = Directory.EnumerateDirectories(_dualityPackageManager.PackagesPath).ToArray();
            var installedPackageInFolder = installedPackagesInFolder.FirstOrDefault(x => x.Contains(packageId));
            if (installedPackageInFolder == null) Assert.Fail("No installed package found with id {0}", packageId);
            Assert.IsTrue(installedPackageInFolder.Contains(version.ToString()), "No installed package found with version {0}", version);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public async Task GetInstalledPackage(string packageId, NuGetVersion version)
        {
            await _dualityPackageManager.InstallPackage(packageId, version);
            var installedPackages = await _dualityPackageManager.GetInstalledPackages();

            var installedPackage = installedPackages.FirstOrDefault(x => x.Id == packageId);
            if (installedPackage == null) Assert.Fail("No installed package found with id {0}", packageId);
            Assert.IsTrue(installedPackage.Version == version, "No installed package found with version {0}", version);

            var installedPackagesInFolder = Directory.EnumerateDirectories(_dualityPackageManager.PackagesPath).ToArray();
            Assert.AreEqual(installedPackagesInFolder.Length, installedPackages.Count());
        }
    }
}
