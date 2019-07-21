﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duality.Editor.PackageManagement;
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

namespace NuGet4XTest
{
    public class DualityPackageManager
    {
        private readonly ILogger _logger;
        private readonly ISettings _settings;
        private readonly NuGetFramework _nuGetFramework;

        private readonly PackagePathResolver _packagePathResolver;
        private readonly string _packagePath;
        private readonly string _globalPackagesFolder;

        private readonly string _packageConfigPath;

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

            _packageConfigPath = "package.config";
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> Search(string searchTerm, bool includePrereleases = false)
        {
            var packageMetadataResources = _repositories.Select(x => x.GetResource<PackageSearchResource>()).ToArray();

            IEnumerable<IPackageSearchMetadata> result = Enumerable.Empty<IPackageSearchMetadata>();
            foreach (var packageMetadataResource in packageMetadataResources)
            {
                result = result.Concat(await packageMetadataResource.SearchAsync(searchTerm, new SearchFilter(includePrereleases), 0, 1000, _logger, CancellationToken.None));
            }

            return result;
        }

        public async Task UpdatePackage(string id)
        {
            var packageMetadata = (await Search($"id:{id}")).First();
            var latestVersion = (await packageMetadata.GetVersionsAsync()).Max(x => x.Version);
            await InstallPackage(new PackageIdentity(id, latestVersion));
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
                var packagesToInstall = resolver.Resolve(resolverContext, CancellationToken.None).ToArray();

                var packageConfig = new PackageConfig(_packageConfigPath);

                foreach (var identity in packagesToInstall)
                {
                    var previouslyInstalledPackages = packageConfig.Packages.Where(x => x.Id == identity.Id).ToArray(); //Take a copy to avoid modifying the enumerable
                    foreach (var previouslyInstalledPackage in previouslyInstalledPackages)
                    {
                        if (previouslyInstalledPackage.Version == identity.Version) continue;
                        packageConfig.Remove(previouslyInstalledPackage);
                        var path = _packagePathResolver.GetInstallPath(previouslyInstalledPackage);
                        if (Directory.Exists(path)) Directory.Delete(path, true);
                    }
                    packageConfig.Add(identity);
                }

                packageConfig.Serialize(_packageConfigPath);

                var packageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Defaultv3,
                    XmlDocFileSaveMode.None,
                    ClientPolicyContext.GetClientPolicy(_settings, _logger),
                    _logger);

                var dependencyInfoResources = _repositories.Select(x => x.GetResource<DependencyInfoResource>()).ToArray();
                foreach (var packageToInstall in packagesToInstall)
                {
                    await RestorePackage(dependencyInfoResources, packageToInstall, cacheContext, packageExtractionContext);
                }
                //var packageExtractionContext = new PackageExtractionContext(
                //    PackageSaveMode.Defaultv3,
                //    XmlDocFileSaveMode.None,
                //    ClientPolicyContext.GetClientPolicy(_settings, _logger),
                //    _logger);
                //
                ////var frameworkReducer = new FrameworkReducer();
                //
                //foreach (var packageToInstall in packagesToInstall)
                //{
                //    //PackageReaderBase packageReader;
                //    var installedPath = _packagePathResolver.GetInstalledPath(packageToInstall);
                //    if (installedPath == null)
                //    {
                //        var downloadResource = await packageToInstall.Source.GetResourceAsync<DownloadResource>(CancellationToken.None);
                //        var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                //            packageToInstall,
                //            new PackageDownloadContext(cacheContext),
                //            _globalPackagesFolder,
                //            NullLogger.Instance, CancellationToken.None);
                //
                //        await PackageExtractor.ExtractPackageAsync(
                //            downloadResult.PackageSource,
                //            downloadResult.PackageStream,
                //            _packagePathResolver,
                //            packageExtractionContext,
                //            CancellationToken.None);
                //
                //        //packageReader = downloadResult.PackageReader;
                //    }
                //    else
                //    {
                //        //packageReader = new PackageFolderReader(installedPath);
                //    }
                //
                //    //var libItems = packageReader.GetLibItems();
                //    //var nearest = frameworkReducer.GetNearest(_nuGetFramework, libItems.Select(x => x.TargetFramework));
                //    //Console.WriteLine(string.Join("\n", libItems
                //    //    .Where(x => x.TargetFramework.Equals(nearest))
                //    //    .SelectMany(x => x.Items)));
                //
                //    //var frameworkItems = packageReader.GetFrameworkItems();
                //    //nearest = frameworkReducer.GetNearest(_nuGetFramework,
                //    //    frameworkItems.Select(x => x.TargetFramework));
                //    //Console.WriteLine(string.Join("\n", frameworkItems
                //    //    .Where(x => x.TargetFramework.Equals(nearest))
                //    //    .SelectMany(x => x.Items)));
                //}
            }
        }

        public async Task RestorePackages()
        {
            var packageConfig = new PackageConfig(_packageConfigPath);
            var dependencyInfoResources = _repositories.Select(x => x.GetResource<DependencyInfoResource>()).ToArray();

            using var cacheContext = new SourceCacheContext();

            var packageExtractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv3,
                XmlDocFileSaveMode.None,
                ClientPolicyContext.GetClientPolicy(_settings, _logger),
                _logger);

            foreach (var packageIdentity in packageConfig.Packages)
            {
                await RestorePackage(dependencyInfoResources, packageIdentity, cacheContext, packageExtractionContext);
            }
        }

        private async Task RestorePackage(DependencyInfoResource[] dependencyInfoResources, PackageIdentity packageIdentity, SourceCacheContext cacheContext, PackageExtractionContext packageExtractionContext)
        {
            foreach (var dependencyInfoResource in dependencyInfoResources)
            {
                var dependencyInfo = await dependencyInfoResource.ResolvePackage(packageIdentity, _nuGetFramework, cacheContext, _logger, CancellationToken.None);
                if (dependencyInfo != null)
                {
                    var downloadResource = await dependencyInfo.Source.GetResourceAsync<DownloadResource>(CancellationToken.None);
                    var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                        dependencyInfo,
                        new PackageDownloadContext(cacheContext),
                        _globalPackagesFolder,
                        NullLogger.Instance, CancellationToken.None);

                    await PackageExtractor.ExtractPackageAsync(
                        downloadResult.PackageSource,
                        downloadResult.PackageStream,
                        _packagePathResolver,
                        packageExtractionContext,
                        CancellationToken.None);
                    break;
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

        public async Task UninstallPackage(PackageIdentity packageIdentity, bool ignoreDependencies = false)
        {
            var installedPackages = GetInstalledPackages();

            if (ignoreDependencies == false)
            {
                using (var cacheContext = new SourceCacheContext())
                {
                    foreach (var installedPackage in installedPackages)
                    {
                        if (installedPackage.Equals(packageIdentity)) continue;

                        var dependencies = await GetPackageDependencies(installedPackage, cacheContext);

                        if (dependencies.Contains(packageIdentity))
                            throw new Exception(
                                $"Cannot uninstall {packageIdentity} because {installedPackage} depends on it");
                    }
                }
            }

            var path = _packagePathResolver.GetInstallPath(packageIdentity);
            Directory.Delete(path, true);
            var packageConfig = new PackageConfig(_packageConfigPath);
            packageConfig.Remove(packageIdentity);
            packageConfig.Serialize(_packageConfigPath);

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
                if (dependencyInfoResource is LocalDependencyInfoResource && package.HasVersion == false) continue;
                var dependencyInfo = await dependencyInfoResource.ResolvePackage(package, framework, cacheContext, logger, CancellationToken.None);
                if (dependencyInfo != null)
                {
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
}