using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Linq;

using NuGet.Common;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Resolver;
using NuGet.Versioning;

namespace NuGet4XTest
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Task mainTask = MainAsync(args);
			mainTask.Wait();
			Console.ReadLine();
		}
		private static async Task MainAsync(string[] args)
		{
			CustomNuGetLogger logger = new CustomNuGetLogger();
			
			DefaultFrameworkNameProvider frameworkNameProvider = new DefaultFrameworkNameProvider();
			string testAppFrameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
				.OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
				.Select(x => x.FrameworkName)
				.FirstOrDefault();
			//string folderName = "netstandard1.1";
			//NuGetFramework currentFramework = folderName == null
			//	? NuGetFramework.AnyFramework
			//	: NuGetFramework.ParseFolder(folderName, frameworkNameProvider);
			NuGetFramework currentFramework = testAppFrameworkName == null
				? NuGetFramework.AnyFramework
				: NuGetFramework.ParseFrameworkName(testAppFrameworkName, frameworkNameProvider);

			List<Lazy<INuGetResourceProvider>> resourceProviders = new List<Lazy<INuGetResourceProvider>>();
			resourceProviders.AddRange(Repository.Provider.GetCoreV3());

			PackageSource packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
			SourceRepository sourceRepository = new SourceRepository(packageSource, resourceProviders);

			//Console.WriteLine("Getting metadata resource...");
			//PackageMetadataResource metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
			//Console.WriteLine("Getting search resource...");
			//PackageSearchResource searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>();
			//Console.WriteLine("Getting auto complete resource...");
			//AutoCompleteResource autoCompleteResource = await sourceRepository.GetResourceAsync<AutoCompleteResource>();
			//Console.WriteLine("Getting dependency info resource...");
			//DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
			//Console.WriteLine("Getting download resource...");
			//DownloadResource downloadResource = await sourceRepository.GetResourceAsync<DownloadResource>();
			//
			//Console.WriteLine();
			//Console.WriteLine("-----------------------------------------------------------------------------");
			//Console.WriteLine();
			//Console.WriteLine("Getting metadata...");
			//IEnumerable<IPackageSearchMetadata> metadata = await metadataResource.GetMetadataAsync("Newtonsoft.Json", false, false, logger, CancellationToken.None);
			//metadata.Dump();
			//
			//Console.WriteLine();
			//Console.WriteLine("-----------------------------------------------------------------------------");
			//Console.WriteLine();
			//Console.WriteLine("Searching metadata...");
			//SearchFilter searchFilter = new SearchFilter(false, null);
			//metadata = await searchResource.SearchAsync("Newtonsoft.Json", searchFilter, 0, 10, logger, CancellationToken.None);
			//metadata.Dump();
			//
			//Console.WriteLine();
			//Console.WriteLine("-----------------------------------------------------------------------------");
			//Console.WriteLine();
			//Console.WriteLine("Resolving dependencies...");
			//IEnumerable<RemoteSourceDependencyInfo> dependencyInfo = await dependencyInfoResource.ResolvePackages("Newtonsoft.Json", logger, CancellationToken.None);
			//dependencyInfo.Dump();
			//
			//Console.WriteLine();
			//Console.WriteLine("-----------------------------------------------------------------------------");
			//Console.WriteLine();
			//Console.WriteLine("Resolving for target framework {0}...", currentFramework);
			//IEnumerable<SourcePackageDependencyInfo> dependencyInfo2 = await dependencyInfoResource.ResolvePackages("Newtonsoft.Json", currentFramework, logger, CancellationToken.None);
			//dependencyInfo2.Dump();

			Console.WriteLine();
			Console.WriteLine("-----------------------------------------------------------------------------");
			Console.WriteLine();
			Console.WriteLine("Installing for target framework {0}...", currentFramework);

			string rootPath = "ProjectRoot";
			string packagesPath = Path.Combine(rootPath, "Packages");
			string targetPath = Path.Combine(rootPath, "Target");
			ISettings settings = new CustomNuGetSettings(rootPath);
			PackageSourceProvider sourceProvider = new PackageSourceProvider(settings);
			SourceRepositoryProvider repoProvider = new SourceRepositoryProvider(sourceProvider, resourceProviders);
			NuGetPackageManager manager = new NuGetPackageManager(repoProvider, settings, packagesPath);
			CustomNuGetProject project = new CustomNuGetProject(targetPath, currentFramework);

			bool allowPrereleaseVersions = true;
			bool allowUnlisted = false;
			ResolutionContext resolutionContext = new ResolutionContext(
				DependencyBehavior.Lowest, 
				allowPrereleaseVersions, 
				allowUnlisted, 
				VersionConstraints.ExactMajor);
			UninstallationContext uninstallContext = new UninstallationContext(
				true,
				false);
			INuGetProjectContext projectContext = new CustomNuGetProjectContext();
			List<SourceRepository> sourceRepositories = new List<SourceRepository>();
			sourceRepositories.Add(sourceRepository);

			Console.WriteLine("Preview for package install...");
			IEnumerable<NuGetProjectAction> installActions = await manager.PreviewInstallPackageAsync(
				project,
				new PackageIdentity("Newtonsoft.Json", new NuGetVersion(10, 0, 2)),
				resolutionContext,
				projectContext,
				sourceRepositories,
				Enumerable.Empty<SourceRepository>(),
				CancellationToken.None);
			Console.WriteLine("Execute package install...");
			await manager.ExecuteNuGetProjectActionsAsync(
				project,
				installActions,
				projectContext,
				CancellationToken.None);

			Console.WriteLine("Preview for package update...");
			IEnumerable<NuGetProjectAction> updateActions = await manager.PreviewUpdatePackagesAsync(
				new PackageIdentity("Newtonsoft.Json", new NuGetVersion(10, 0, 3)),
				new[] { project },
				resolutionContext,
				projectContext,
				sourceRepositories,
				Enumerable.Empty<SourceRepository>(),
				CancellationToken.None);
			IEnumerable<NuGetProjectAction> updateActions2 = await manager.PreviewUpdatePackagesAsync(
				new PackageIdentity("Newtonsoft.Json", new NuGetVersion(10, 0, 2)),
				new[] { project },
				resolutionContext,
				projectContext,
				sourceRepositories,
				Enumerable.Empty<SourceRepository>(),
				CancellationToken.None);
			IEnumerable<NuGetProjectAction> updateActions3 = await manager.PreviewUpdatePackagesAsync(
				"Newtonsoft.Json",
				new[] { project },
				resolutionContext,
				projectContext,
				sourceRepositories,
				Enumerable.Empty<SourceRepository>(),
				CancellationToken.None);
			Console.WriteLine("Execute package update...");
			await manager.ExecuteNuGetProjectActionsAsync(
				project,
				updateActions,
				projectContext,
				CancellationToken.None);

			Console.WriteLine("Preview for package uninstall...");
			IEnumerable<NuGetProjectAction> uninstallActions = await manager.PreviewUninstallPackageAsync(
				project,
				new PackageIdentity("Newtonsoft.Json", new NuGetVersion(10, 0, 3)),
				uninstallContext,
				projectContext,
				CancellationToken.None);
			Console.WriteLine("Execute package uninstall...");
			await manager.ExecuteNuGetProjectActionsAsync(
				project,
				uninstallActions,
				projectContext,
				CancellationToken.None);

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Done");
		}
	}
}
