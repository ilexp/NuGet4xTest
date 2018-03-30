using System;
using NuGet.Packaging;
using NuGet.ProjectManagement;
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
