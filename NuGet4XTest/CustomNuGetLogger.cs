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
	public class CustomNuGetLogger : LoggerBase
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
