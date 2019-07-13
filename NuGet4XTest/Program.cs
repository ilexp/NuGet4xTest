using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace Duality.Editor.PackageManagement
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var packageId = "Singularity.Duality.core";
            var packageVersion = "0.13.0";

            var f = new DualityPackageManager();
            f.InstallPackage(packageId, packageVersion).Wait();
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
            Console.ReadLine();
        }
    }

    public class DualityPackageManager
    {
        private ILogger _logger;
        private ISettings _settings;

        public DualityPackageManager()
        {
            _logger = NullLogger.Instance;
            _settings = Settings.LoadDefaultSettings(root: null);
        }

        public async Task InstallPackage(string id, string version)
        {
            var nugetVersion = NuGetVersion.Parse(version);
            var identity = new PackageIdentity(id, nugetVersion);
            await InstallPackage(identity);
        }

        public async Task InstallPackage(PackageIdentity packageIdentity)
        {
            var nuGetFramework = NuGetFramework.ParseFolder("net472");
            var sourceRepositoryProvider = new SourceRepositoryProvider(_settings, Repository.Provider.GetCoreV3());

            using (var cacheContext = new SourceCacheContext())
            {
                var repositories = sourceRepositoryProvider.GetRepositories();
                var availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
                await GetPackageDependencies(
                    packageIdentity,
                    nuGetFramework, cacheContext, _logger, repositories, availablePackages);

                var resolverContext = new PackageResolverContext(
                    DependencyBehavior.Lowest,
                    new[] { packageIdentity.Id },
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<PackageReference>(),
                    Enumerable.Empty<PackageIdentity>(),
                    availablePackages,
                    sourceRepositoryProvider.GetRepositories().Select(s => s.PackageSource),
                    NullLogger.Instance);

                var resolver = new PackageResolver();
                var packagesToInstall = resolver.Resolve(resolverContext, CancellationToken.None)
                    .Select(p => availablePackages.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
                var packagePathResolver = new PackagePathResolver(Path.GetFullPath("packages"));
                var packageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Defaultv3,
                    XmlDocFileSaveMode.None,
                    ClientPolicyContext.GetClientPolicy(_settings, _logger),
                    _logger);

                var frameworkReducer = new FrameworkReducer();

                foreach (var packageToInstall in packagesToInstall)
                {
                    PackageReaderBase packageReader;
                    var installedPath = packagePathResolver.GetInstalledPath(packageToInstall);
                    if (installedPath == null)
                    {
                        var downloadResource =
                            await packageToInstall.Source.GetResourceAsync<DownloadResource>(CancellationToken.None);
                        var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                            packageToInstall,
                            new PackageDownloadContext(cacheContext),
                            SettingsUtility.GetGlobalPackagesFolder(_settings),
                            NullLogger.Instance, CancellationToken.None);

                        await PackageExtractor.ExtractPackageAsync(
                            downloadResult.PackageSource,
                            downloadResult.PackageStream,
                            packagePathResolver,
                            packageExtractionContext,
                            CancellationToken.None);

                        packageReader = downloadResult.PackageReader;
                    }
                    else
                    {
                        packageReader = new PackageFolderReader(installedPath);
                    }

                    var libItems = packageReader.GetLibItems();
                    var nearest = frameworkReducer.GetNearest(nuGetFramework, libItems.Select(x => x.TargetFramework));
                    Console.WriteLine(string.Join("\n", libItems
                        .Where(x => x.TargetFramework.Equals(nearest))
                        .SelectMany(x => x.Items)));

                    var frameworkItems = packageReader.GetFrameworkItems();
                    nearest = frameworkReducer.GetNearest(nuGetFramework,
                        frameworkItems.Select(x => x.TargetFramework));
                    Console.WriteLine(string.Join("\n", frameworkItems
                        .Where(x => x.TargetFramework.Equals(nearest))
                        .SelectMany(x => x.Items)));
                }
            }
        }

        public async Task GetPackageDependencies(PackageIdentity package,
            NuGetFramework framework,
            SourceCacheContext cacheContext,
            ILogger logger,
            IEnumerable<SourceRepository> repositories,
            ISet<SourcePackageDependencyInfo> availablePackages)
        {
            if (availablePackages.Contains(package)) return;

            foreach (var sourceRepository in repositories)
            {
                var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
                var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                    package, framework, cacheContext, logger, CancellationToken.None);

                if (dependencyInfo == null) continue;

                availablePackages.Add(dependencyInfo);
                foreach (var dependency in dependencyInfo.Dependencies)
                {
                    await GetPackageDependencies(
                        new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                        framework, cacheContext, logger, repositories, availablePackages);
                }
            }
        }
    }
}
