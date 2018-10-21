using System;
using System.Text;
using System.Threading.Tasks;
using NuGet.Common;

namespace Duality.Editor.PackageManagement
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
