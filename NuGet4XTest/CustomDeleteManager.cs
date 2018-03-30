using System;
using System.Collections.Generic;
using NuGet.PackageManagement;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;

namespace NuGet4XTest
{
	public class CustomDeleteManager : IDeleteOnRestartManager
	{
		public IReadOnlyList<string> GetPackageDirectoriesMarkedForDeletion()
		{
			return new List<string>();
		}

		public void CheckAndRaisePackageDirectoriesMarkedForDeletion()
		{
			
		}

		public void MarkPackageDirectoryForDeletion(PackageIdentity package, string packageDirectory, INuGetProjectContext projectContext)
		{
			
		}

		public void DeleteMarkedPackageDirectories(INuGetProjectContext projectContext)
		{
			
		}

		public event EventHandler<PackagesMarkedForDeletionEventArgs> PackagesMarkedForDeletionFound;
	}
}