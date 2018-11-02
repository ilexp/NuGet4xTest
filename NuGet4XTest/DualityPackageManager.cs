using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace Duality.Editor.PackageManagement
{
	public class DualityPackageManager
	{
		public string PackagesPath
		{
			get { return _manager.PackagesFolderNuGetProject.Root; }
		}

		private readonly NuGetPackageManager _manager;
		private readonly INuGetProjectContext _projectContext;
		private readonly List<SourceRepository> _sourceRepositories;
		private readonly SourceRepository _localRepository;

		public DualityPackageManager(string rootPath, string packagesPath)
		{
			DefaultFrameworkNameProvider frameworkNameProvider = new DefaultFrameworkNameProvider();
			string testAppFrameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
				.OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
				.Select(x => x.FrameworkName)
				.FirstOrDefault();

			NuGetFramework currentFramework = testAppFrameworkName == null
				? NuGetFramework.AnyFramework
				: NuGetFramework.ParseFrameworkName(testAppFrameworkName, frameworkNameProvider);
			List<Lazy<INuGetResourceProvider>> resourceProviders = new List<Lazy<INuGetResourceProvider>>();
			resourceProviders.AddRange(Repository.Provider.GetCoreV3());

			_projectContext = new CustomNuGetProjectContext();

			ISettings settings = new CustomNuGetSettings(rootPath);



			PackageSourceProvider sourceProvider = new PackageSourceProvider(settings);
			SourceRepositoryProvider repoProvider = new SourceRepositoryProvider(sourceProvider, resourceProviders);

			var project = new CustomNuGetProject(Path.Combine(rootPath, packagesPath), currentFramework);
			var localSource = new PackageSource(Path.GetFullPath(project.Root));
			_localRepository = new SourceRepository(localSource, resourceProviders);

			CustomSolutionManager solutionManager = new CustomSolutionManager(rootPath, project);
			//_manager = new NuGetPackageManager(repoProvider, settings, rootPath);
			_manager = new NuGetPackageManager(repoProvider, settings, solutionManager, new CustomDeleteManager());
			_manager.PackagesFolderNuGetProject = project;
			_sourceRepositories = repoProvider.GetRepositories().ToList();
		}

		public IEnumerable<PackageMetadata> Search(string searchTerm = "tags:Duality, Plugin", bool prerelease = false)
		{
			return Task.Run(() =>
			{
				var filter = new SearchFilter(prerelease);
				foreach (var source in _sourceRepositories)
				{
					var dependencyInfoResource = source.GetResourceAsync<PackageSearchResource>().GetAwaiter().GetResult();
					var result = dependencyInfoResource.SearchAsync(searchTerm, filter, 0, int.MaxValue, new CustomNuGetLogger(), CancellationToken.None).GetAwaiter().GetResult();
					return result.Select(x => new PackageMetadata(x));
				}
				return Enumerable.Empty<PackageMetadata>();
			}).GetAwaiter().GetResult();
		}

		public PackageMetadata GetInstalledPackage(PackageIdentity packageIdentity)
		{
			return new PackageMetadata(Task.Run(() =>
			  {
				  var dependencyInfoResource = _localRepository.GetResourceAsync<PackageMetadataResource>().GetAwaiter().GetResult();
				  return dependencyInfoResource.GetMetadataAsync(packageIdentity, new CustomNuGetLogger(), CancellationToken.None).GetAwaiter().GetResult();
			  }).GetAwaiter().GetResult());
		}

		public PackageMetadata[] GetInstalledPackages()
		{
			return Task.Run(() =>
			{
				var packageIdentities = GetInstalledPackageIdentities();
				return packageIdentities.Select(GetInstalledPackage).ToArray();
			}).GetAwaiter().GetResult();
		}

		public IEnumerable<PackageIdentity> GetInstalledPackageIdentities()
		{
			return Task.Run(() =>
			{
				var packageReferences = _manager.PackagesFolderNuGetProject.GetInstalledPackagesAsync(CancellationToken.None).GetAwaiter().GetResult();
				return packageReferences.Select(x => x.PackageIdentity);
			}).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Installs a package
		/// </summary>
		/// <param name="packageId">The id of the package</param>
		/// <param name="version">The required version. If not specified the latest version will be used</param>
		/// <param name="allowPrereleaseVersions"></param>
		/// <param name="allowUnlisted"></param>
		/// <returns></returns>
		public void InstallPackage(string packageId, NuGetVersion version, bool allowPrereleaseVersions = true, bool allowUnlisted = false)
		{
			Task.Run(() =>
			{
				ResolutionContext resolutionContext = new ResolutionContext(
					DependencyBehavior.Lowest,
					allowPrereleaseVersions,
					allowUnlisted,
					VersionConstraints.None);
				IEnumerable<NuGetProjectAction> installActions = _manager.PreviewInstallPackageAsync(
					_manager.PackagesFolderNuGetProject,
					new PackageIdentity(packageId, version),
					resolutionContext,
					_projectContext,
					_sourceRepositories,
					Enumerable.Empty<SourceRepository>(),
					CancellationToken.None).GetAwaiter().GetResult();

				_manager.ExecuteNuGetProjectActionsAsync(
					_manager.PackagesFolderNuGetProject,
					installActions,
					_projectContext,
					CancellationToken.None).GetAwaiter().GetResult();
			}).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Updates a package
		/// </summary>
		/// <param name="packageId">The id of the package</param>
		/// <param name="version">The required version. If not specified the latest version will be used</param>
		/// <param name="allowPrereleaseVersions"></param>
		/// <param name="allowUnlisted"></param>
		/// <returns></returns>
		public void UpdatePackage(string packageId, NuGetVersion version = null, bool allowPrereleaseVersions = true, bool allowUnlisted = false)
		{
			Task.Run(() =>
			{
				ResolutionContext resolutionContext = new ResolutionContext(DependencyBehavior.Lowest, allowPrereleaseVersions, allowUnlisted, VersionConstraints.None);

				IEnumerable<NuGetProjectAction> updateActions = _manager.PreviewUpdatePackagesAsync(
					new PackageIdentity(packageId, version),
					new[] { _manager.PackagesFolderNuGetProject },
					resolutionContext,
					_projectContext,
					_sourceRepositories,
					Enumerable.Empty<SourceRepository>(),
					CancellationToken.None).GetAwaiter().GetResult();

				_manager.ExecuteNuGetProjectActionsAsync(_manager.PackagesFolderNuGetProject, updateActions, _projectContext, CancellationToken.None).GetAwaiter().GetResult();
			}).GetAwaiter().GetResult();
		}

		public void UninstallPackage(string packageId)
		{
			Task.Run(() =>
			{
				UninstallationContext uninstallContext = new UninstallationContext(true, false);

				IEnumerable<NuGetProjectAction> uninstallActions = _manager.PreviewUninstallPackageAsync(
					_manager.PackagesFolderNuGetProject,
					packageId,
					uninstallContext,
					_projectContext,
					CancellationToken.None).GetAwaiter().GetResult();

				_manager.ExecuteNuGetProjectActionsAsync(
					_manager.PackagesFolderNuGetProject,
					uninstallActions,
					_projectContext,
					CancellationToken.None).GetAwaiter().GetResult();
			}).GetAwaiter().GetResult();
		}
	}
}