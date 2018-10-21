using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;

namespace Duality.Editor.PackageManagement
{
	public class CustomSolutionManager : ISolutionManager
	{
		public List<NuGetProject> NuGetProjects { get; set; } = new List<NuGetProject>();

		public CustomSolutionManager(string solutionPath, CustomNuGetProject customNuGetProject)
		{
			SolutionDirectory = solutionPath;
			NuGetProjects.Add(customNuGetProject);

		}

	    public Task<NuGetProject> GetNuGetProjectAsync(string nuGetProjectSafeName)
	    {
	        return Task.FromResult(NuGetProjects
	            .FirstOrDefault(p => string.Equals(nuGetProjectSafeName, p.GetMetadata<string>(NuGetProjectMetadataKeys.Name), StringComparison.OrdinalIgnoreCase)));
	    }

	    public Task<string> GetNuGetProjectSafeNameAsync(NuGetProject nuGetProject)
	    {
	        return Task.FromResult(nuGetProject.GetMetadata<string>(NuGetProjectMetadataKeys.Name));
	    }

	    public Task<IEnumerable<NuGetProject>> GetNuGetProjectsAsync()
	    {
	        return Task.FromResult(NuGetProjects.AsEnumerable());
	    }

	    public bool IsSolutionOpen
	    {
	        get { return NuGetProjects.Count > 0; }
	    }

	    public Task<bool> IsSolutionAvailableAsync()
	    {
	        return Task.FromResult(IsSolutionOpen);
	    }

	    public void EnsureSolutionIsLoaded()
	    {
	        // do nothing
	    }

	    public Task<bool> DoesNuGetSupportsAnyProjectAsync()
	    {
	        return Task.FromResult(true);
	    }

	    public void OnActionsExecuted(IEnumerable<ResolvedAction> actions)
	    {
	        if (ActionsExecuted != null)
	        {
	            ActionsExecuted(this, new ActionsExecutedEventArgs(actions));
	        }
	    }

        public string SolutionDirectory { get; }
		public INuGetProjectContext NuGetProjectContext { get; set; }
		public event EventHandler SolutionOpening;
		public event EventHandler SolutionOpened;
		public event EventHandler SolutionClosing;
		public event EventHandler SolutionClosed;
		public event EventHandler<NuGetEventArgs<string>> AfterNuGetCacheUpdated;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectAdded;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRemoved;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectRenamed;
		public event EventHandler<NuGetProjectEventArgs> NuGetProjectUpdated;
		public event EventHandler<NuGetProjectEventArgs> AfterNuGetProjectRenamed;
		public event EventHandler<ActionsExecutedEventArgs> ActionsExecuted;
	}
}
