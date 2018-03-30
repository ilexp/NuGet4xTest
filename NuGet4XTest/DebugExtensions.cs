using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NuGet4XTest
{
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
}
