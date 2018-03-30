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

namespace NuGet4XTest
{
	public class DualityPackageManager
	{
		private readonly NuGetPackageManager _manager;
		private readonly CustomNuGetProject _project;
		private readonly INuGetProjectContext _projectContext;
		private readonly List<SourceRepository> _sourceRepositories;

		public DualityPackageManager(string rootPath)
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
			string targetPath = Path.Combine(rootPath, "Packages");
			ISettings settings = new CustomNuGetSettings(rootPath);
			PackageSourceProvider sourceProvider = new PackageSourceProvider(settings);
			SourceRepositoryProvider repoProvider = new SourceRepositoryProvider(sourceProvider, resourceProviders);
			_project = new CustomNuGetProject(targetPath, currentFramework);
			CustomSolutionManager solutionManager = new CustomSolutionManager(rootPath, _project);
			_manager = new NuGetPackageManager(repoProvider, settings, solutionManager, new CustomDeleteManager());

			PackageSource packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
			SourceRepository sourceRepository = new SourceRepository(packageSource, resourceProviders);
			_sourceRepositories = new List<SourceRepository> { sourceRepository };
		}

		/// <summary>
		/// Installs a package
		/// </summary>
		/// <param name="packageId">The id of the package</param>
		/// <param name="version">The required version. If not specified the latest version will be used</param>
		/// <param name="allowPrereleaseVersions"></param>
		/// <param name="allowUnlisted"></param>
		/// <returns></returns>
		public async Task InstallPackage(string packageId, NuGetVersion version, bool allowPrereleaseVersions = true, bool allowUnlisted = false)
		{
			ResolutionContext resolutionContext = new ResolutionContext(
				DependencyBehavior.Lowest,
				allowPrereleaseVersions,
				allowUnlisted,
				VersionConstraints.ExactMajor);

			IEnumerable<NuGetProjectAction> installActions = await _manager.PreviewInstallPackageAsync(
				_project,
				new PackageIdentity(packageId, version),
				resolutionContext,
				_projectContext,
				_sourceRepositories,
				Enumerable.Empty<SourceRepository>(),
				CancellationToken.None);

			await _manager.ExecuteNuGetProjectActionsAsync(
				_project,
				installActions,
				_projectContext,
				CancellationToken.None);
		}

		public async Task<IEnumerable<PackageIdentity>> Foo()
		{
			return await _manager.GetInstalledPackagesInDependencyOrder(_project, CancellationToken.None);
		}

		/// <summary>
		/// Updates a package
		/// </summary>
		/// <param name="packageId">The id of the package</param>
		/// <param name="version">The required version. If not specified the latest version will be used</param>
		/// <param name="allowPrereleaseVersions"></param>
		/// <param name="allowUnlisted"></param>
		/// <returns></returns>
		public async Task UpdatePackage(string packageId, NuGetVersion version = null, bool allowPrereleaseVersions = true, bool allowUnlisted = false)
		{
			ResolutionContext resolutionContext = new ResolutionContext(DependencyBehavior.Lowest, allowPrereleaseVersions, allowUnlisted, VersionConstraints.ExactMajor);

			IEnumerable<NuGetProjectAction> updateActions = await _manager.PreviewUpdatePackagesAsync(
				new PackageIdentity(packageId, version), 
				new[] { _project },
				resolutionContext,
				_projectContext,
				_sourceRepositories,
				Enumerable.Empty<SourceRepository>(),
				CancellationToken.None);

			await _manager.ExecuteNuGetProjectActionsAsync(_project, updateActions, _projectContext, CancellationToken.None);
		}

		public async Task UninstallPackage(string packageId)
		{
			UninstallationContext uninstallContext = new UninstallationContext(true, false);

			IEnumerable<NuGetProjectAction> uninstallActions = await _manager.PreviewUninstallPackageAsync(
				_project,
				packageId,
				uninstallContext,
				_projectContext,
				CancellationToken.None);

			await _manager.ExecuteNuGetProjectActionsAsync(
				_project,
				uninstallActions,
				_projectContext,
				CancellationToken.None);
		}
	}
}