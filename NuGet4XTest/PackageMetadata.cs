using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace Duality.Editor.PackageManagement
{
	public class PackageMetadata
	{
		private readonly IPackageSearchMetadata _packageSearchMetadata;

		public PackageMetadata(IPackageSearchMetadata packageSearchMetadata)
		{
			_packageSearchMetadata = packageSearchMetadata;
		}

		public IEnumerable<VersionInfo> GetVersions()
		{
			return Task.Run(() =>
			{
				return _packageSearchMetadata.GetVersionsAsync().GetAwaiter().GetResult();
			}).GetAwaiter().GetResult();
		}

		public string Authors
		{
			get
			{
				return _packageSearchMetadata.Authors;
			}
		}
		public IEnumerable<PackageDependencyGroup> DependencySets
		{
			get
			{
				return _packageSearchMetadata.DependencySets;
			}
		}
		public string Description
		{
			get
			{
				return _packageSearchMetadata.Description;
			}
		}
		public long? DownloadCount
		{
			get
			{
				return _packageSearchMetadata.DownloadCount;
			}
		}
		public Uri IconUrl
		{
			get
			{
				return _packageSearchMetadata.IconUrl;
			}
		}
		public PackageIdentity Identity
		{
			get
			{
				return _packageSearchMetadata.Identity;
			}
		}
		public Uri LicenseUrl
		{
			get
			{
				return _packageSearchMetadata.LicenseUrl;
			}
		}
		public Uri ProjectUrl
		{
			get
			{
				return _packageSearchMetadata.ProjectUrl;
			}
		}
		public Uri ReportAbuseUrl
		{
			get
			{
				return _packageSearchMetadata.ReportAbuseUrl;
			}
		}
		public DateTimeOffset? Published
		{
			get
			{
				return _packageSearchMetadata.Published;
			}
		}

		public string Owners
		{
			get
			{
				return _packageSearchMetadata.Owners;
			}
		}

		public bool RequireLicenseAcceptance
		{
			get
			{
				return _packageSearchMetadata.RequireLicenseAcceptance;
			}
		}

		public string Summary
		{
			get
			{
				return _packageSearchMetadata.Summary;
			}
		}

		public string Tags
		{
			get
			{
				return _packageSearchMetadata.Tags;
			}
		}

		public string Title
		{
			get
			{
				return _packageSearchMetadata.Title;
			}
		}
		public bool IsListed
		{
			get
			{
				return _packageSearchMetadata.IsListed;
			}
		}
		public bool PrefixReserved
		{
			get
			{
				return _packageSearchMetadata.PrefixReserved;
			}
		}
	}
}