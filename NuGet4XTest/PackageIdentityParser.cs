using System;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Duality.Editor.PackageManagement
{
    public static class PackageIdentityParser
    {
        public static PackageIdentity Parse(string id, string version)
        {
            var nugetVersion = NuGetVersion.Parse(version);
            return new PackageIdentity(id, nugetVersion);
        }

        public static PackageIdentity Parse(string fullname)
        {
            int dotIndex = fullname.Length;
            int dotCount = 0;
            while (true)
            {
                dotIndex = fullname.LastIndexOf('.', dotIndex - 1);
                if (dotIndex == -1) break;

                dotCount++;
                if (dotCount < 3) continue;

                string potentialVersionString = fullname.Substring(
                    dotIndex + 1,
                    fullname.Length - dotIndex - 1);
                NuGetVersion version;
                if (!NuGetVersion.TryParse(potentialVersionString, out version))
                    continue;

                string packageName = fullname.Remove(dotIndex);
                return new PackageIdentity(packageName, version);
            }
            throw new Exception($"{fullname} is not a correct package identity");
        }
    }
}
