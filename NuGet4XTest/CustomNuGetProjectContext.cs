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
	public class CustomNuGetProjectContext : INuGetProjectContext
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
}
