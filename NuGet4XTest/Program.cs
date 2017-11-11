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
			NuGetLogger logger = new NuGetLogger();
			
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
			DebugNuGetProject project = new DebugNuGetProject(targetPath, new PackagePathResolver(targetPath, false), currentFramework);

			bool allowPrereleaseVersions = true;
			bool allowUnlisted = false;
			ResolutionContext resolutionContext = new ResolutionContext(
				DependencyBehavior.Lowest, 
				allowPrereleaseVersions, 
				allowUnlisted, 
				VersionConstraints.ExactMajor);    
			INuGetProjectContext projectContext = new ProjectContext();
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

	public static class DebugExtensions
	{
		public static void Dump(this IEnumerable<IPackageSearchMetadata> metadata)
		{
			foreach (IPackageSearchMetadata item in metadata)
			{
				Console.WriteLine("{0}", item.Identity);
				Console.WriteLine("  Tags: {0}", item.Tags);
				Console.WriteLine("  Summary: {0}", item.Summary);
				foreach (PackageDependencyGroup group in item.DependencySets)
				{
					Console.WriteLine("  Dependencies for '{0}':", group.TargetFramework);
					foreach (PackageDependency dependency in group.Packages)
					{
						Console.WriteLine("    {0} {1}", dependency.Id, dependency.VersionRange);
					}
				}
				if (item.DependencySets.Any())
					Console.WriteLine();
			}
		}
		public static void Dump(this IEnumerable<RemoteSourceDependencyInfo> dependencyInfo)
		{
			foreach (RemoteSourceDependencyInfo item in dependencyInfo)
			{
				Console.WriteLine("{0}", item.Identity);
				foreach (PackageDependencyGroup group in item.DependencyGroups)
				{
					Console.WriteLine("  Dependencies for '{0}':", group.TargetFramework);
					foreach (PackageDependency dependency in group.Packages)
					{
						Console.WriteLine("    {0} {1}", dependency.Id, dependency.VersionRange);
					}
				}
				if (item.DependencyGroups.Any())
					Console.WriteLine();
			}
		}
		public static void Dump(this IEnumerable<SourcePackageDependencyInfo> dependencyInfo)
		{
			foreach (SourcePackageDependencyInfo item in dependencyInfo)
			{
				Console.WriteLine("{0} {1}", item.Id, item.Version);
				foreach (PackageDependency dependency in item.Dependencies)
				{
					Console.WriteLine("  {0} {1}", dependency.Id, dependency.VersionRange);
				}
				if (item.Dependencies.Any())
					Console.WriteLine();
			}
		}
	}

	public class DebugNuGetProject : FolderNuGetProject
	{
		public DebugNuGetProject(string path, PackagePathResolver resolver, NuGetFramework framework) : base(path, resolver, framework) { }

		public override Task PreProcessAsync(INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			Console.WriteLine("PreProcessAsync");
			return base.PostProcessAsync(nuGetProjectContext, token);
		}
		public override Task PostProcessAsync(INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			Console.WriteLine("PostProcessAsync");
			return base.PostProcessAsync(nuGetProjectContext, token);
		}
		public override Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
		{
			Console.WriteLine("GetInstalledPackagesAsync");
			return base.GetInstalledPackagesAsync(token);
		}
		public override Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			Console.WriteLine("InstallPackageAsync({0})", packageIdentity);
			return base.InstallPackageAsync(packageIdentity, downloadResourceResult, nuGetProjectContext, token);
		}
		public override Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
		{
			Console.WriteLine("UninstallPackageAsync({0})", packageIdentity);
			return base.UninstallPackageAsync(packageIdentity, nuGetProjectContext, token);
		}
	}
	public class ProjectContext : INuGetProjectContext
	{
		public PackageExtractionContext PackageExtractionContext { get; set; }
		public XDocument OriginalPackagesConfig { get; set; }
		public NuGetActionType ActionType { get; set; }
		public TelemetryServiceHelper TelemetryService { get; set; }
		public ISourceControlManagerProvider SourceControlManagerProvider
		{
			get { return null; }
		}
		public NuGet.ProjectManagement.ExecutionContext ExecutionContext
		{
			get { return null; }
		}
		
		public void Log(MessageLevel level, string message, params object[] args)
		{
			Console.WriteLine("{0}: {1}", level, string.Format(message, args));
		}
		public void ReportError(string message)
		{
			Console.WriteLine("Reported Error: {0}", message);
		}
		public FileConflictAction ResolveFileConflict(string message)
		{
			return FileConflictAction.Overwrite;
		}
	}

	public class NuGetLogger : LoggerBase
	{
		public override void Log(ILogMessage message)
		{
			StringBuilder builder = new StringBuilder();
			//builder.Append(message.Time);
			//builder.Append(' ');
			builder.Append(message.Level);
			if (message.Level == LogLevel.Warning)
			{
				builder.Append(" (");
				builder.Append(message.WarningLevel);
				builder.Append(")");
			}
			if (message.Code != NuGetLogCode.Undefined)
			{
				builder.Append(" Code ");
				builder.Append(message.Code);
			}
			if (!string.IsNullOrEmpty(message.ProjectPath))
			{
				builder.Append(" '");
				builder.Append(message.ProjectPath);
				builder.Append("'");
			}
			builder.Append(": ");
			builder.Append(message.Message);
			Console.WriteLine(builder.ToString());
		}
		public override Task LogAsync(ILogMessage message)
		{
			this.Log(message);
			return Task.FromResult<object>(null);
		}
	}
}
