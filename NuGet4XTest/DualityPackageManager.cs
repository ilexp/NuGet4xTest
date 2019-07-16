using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;

namespace Duality.Editor.PackageManagement
{
    public class DualityPackageManager
    {
        private readonly ILogger _logger;
        private readonly ISettings _settings;
        private readonly NuGetFramework _nuGetFramework;

        private readonly PackagePathResolver _packagePathResolver;
        private readonly string _packagePath;
        private readonly string _globalPackagesFolder;

        private readonly SourceRepository[] _repositories;

        public DualityPackageManager(NuGetFramework nuGetFramework, ILogger logger, ISettings settings, string packagePath)
        {
            _logger = logger;
            _settings = settings;
            _nuGetFramework = nuGetFramework;
            _packagePath = packagePath;
            _packagePathResolver = new PackagePathResolver(packagePath);
            var sourceRepositoryProvider = new SourceRepositoryProvider(_settings, Repository.Provider.GetCoreV3());
            _repositories = sourceRepositoryProvider.GetRepositories().ToArray();
            _globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(_settings);
        }

        public async Task InstallPackage(string id, string version)
        {
            var identity = PackageIdentityParser.Parse(id, version);
            await InstallPackage(identity);
        }

        public async Task InstallPackage(PackageIdentity packageIdentity)
        {
            using (var cacheContext = new SourceCacheContext())
            {
                var availablePackages = await GetPackageDependencies(packageIdentity, cacheContext);

                var resolverContext = new PackageResolverContext(
                    DependencyBehavior.Lowest,
                    new[] { packageIdentity.Id },
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<PackageReference>(),
                    Enumerable.Empty<PackageIdentity>(),
                    availablePackages,
                    _repositories.Select(s => s.PackageSource),
                    NullLogger.Instance);

                var resolver = new PackageResolver();
                var packagesToInstall = resolver.Resolve(resolverContext, CancellationToken.None)
                    .Select(p => availablePackages.Single(x => PackageIdentityComparer.Default.Equals(x, p)));

                var packageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Defaultv3,
                    XmlDocFileSaveMode.None,
                    ClientPolicyContext.GetClientPolicy(_settings, _logger),
                    _logger);

                //var frameworkReducer = new FrameworkReducer();

                foreach (var packageToInstall in packagesToInstall)
                {
                    //PackageReaderBase packageReader;
                    var installedPath = _packagePathResolver.GetInstalledPath(packageToInstall);
                    if (installedPath == null)
                    {
                        var downloadResource = await packageToInstall.Source.GetResourceAsync<DownloadResource>(CancellationToken.None);
                        var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                            packageToInstall,
                            new PackageDownloadContext(cacheContext),
                            _globalPackagesFolder,
                            NullLogger.Instance, CancellationToken.None);

                        await PackageExtractor.ExtractPackageAsync(
                            downloadResult.PackageSource,
                            downloadResult.PackageStream,
                            _packagePathResolver,
                            packageExtractionContext,
                            CancellationToken.None);

                        //packageReader = downloadResult.PackageReader;
                    }
                    else
                    {
                        //packageReader = new PackageFolderReader(installedPath);
                    }

                    //var libItems = packageReader.GetLibItems();
                    //var nearest = frameworkReducer.GetNearest(_nuGetFramework, libItems.Select(x => x.TargetFramework));
                    //Console.WriteLine(string.Join("\n", libItems
                    //    .Where(x => x.TargetFramework.Equals(nearest))
                    //    .SelectMany(x => x.Items)));

                    //var frameworkItems = packageReader.GetFrameworkItems();
                    //nearest = frameworkReducer.GetNearest(_nuGetFramework,
                    //    frameworkItems.Select(x => x.TargetFramework));
                    //Console.WriteLine(string.Join("\n", frameworkItems
                    //    .Where(x => x.TargetFramework.Equals(nearest))
                    //    .SelectMany(x => x.Items)));
                }
            }
        }

        public IEnumerable<PackageIdentity> GetInstalledPackages()
        {
            var packages = Directory.GetDirectories(_packagePath).Select(Path.GetFileName).Select(PackageIdentityParser.Parse).ToArray();
            return packages;
        }

        public async Task UninstallPackage(string id, string version)
        {
            var identity = PackageIdentityParser.Parse(id, version);
            await UninstallPackage(identity);
        }

        public async Task UninstallPackage(PackageIdentity packageIdentity)
        {
            var installedPackages = GetInstalledPackages();

            using (var cacheContext = new SourceCacheContext())
            {
                foreach (var installedPackage in installedPackages)
                {
                    if (installedPackage.Equals(packageIdentity)) continue;

                    var dependencies = await GetPackageDependencies(installedPackage, cacheContext);

                    if (dependencies.Contains(packageIdentity)) throw new Exception($"Cannot uninstall {packageIdentity} because {installedPackage} depends on it");
                }
            }

            var path = _packagePathResolver.GetInstallPath(packageIdentity);
            Directory.Delete(path, true);
        }

        public async Task<HashSet<SourcePackageDependencyInfo>> GetPackageDependencies(PackageIdentity package,
            SourceCacheContext cacheContext)
        {
            var dependencyInfoResources = _repositories.Select(x => x.GetResource<DependencyInfoResource>()).ToArray();
            var availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

            await GetPackageDependencies(package, _nuGetFramework, cacheContext, _logger, dependencyInfoResources,
                availablePackages);

            return availablePackages;
        }

        private async Task GetPackageDependencies(PackageIdentity package,
            NuGetFramework framework,
            SourceCacheContext cacheContext, ILogger logger, DependencyInfoResource[] dependencyInfoResources, HashSet<SourcePackageDependencyInfo> availablePackages)
        {
            if (availablePackages.Contains(package)) return;

            foreach (var dependencyInfoResource in dependencyInfoResources)
            {
                var dependencyInfo = await dependencyInfoResource.ResolvePackage(package, framework, cacheContext, logger, CancellationToken.None);

                if (dependencyInfo == null) continue;

                availablePackages.Add(dependencyInfo);

                foreach (var dependency in dependencyInfo.Dependencies)
                {
                    await GetPackageDependencies(
                        new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                        framework, cacheContext, logger, dependencyInfoResources, availablePackages);
                }
            }
        }
    }
}