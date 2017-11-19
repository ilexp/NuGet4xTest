using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

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
using System.Xml.Linq;

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
			ISettings settings = new Settings(rootPath);
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
			INuGetProjectContext projectContext = new CustomNuGetProjectContext();
			List<SourceRepository> sourceRepositories = new List<SourceRepository>();
			sourceRepositories.Add(sourceRepository);

			await manager.InstallPackageAsync(
				project,
				new PackageIdentity("Newtonsoft.Json", new NuGet.Versioning.NuGetVersion(10, 0, 3)), 
				resolutionContext, 
				projectContext, 
				sourceRepositories,
				Enumerable.Empty<SourceRepository>(),
				CancellationToken.None);

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Done");
		}
	}
}
