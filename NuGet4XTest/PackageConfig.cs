using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NuGet.Packaging.Core;

namespace Duality.Editor.PackageManagement
{
    public class PackageConfig
    {
        public List<PackageIdentity> Packages { get; } = new List<PackageIdentity>();

        public void Add(string id, string version) => Add(PackageIdentityParser.Parse(id, version));

        public void Add(PackageIdentity packageIdentity) => Packages.Add(packageIdentity);

        public PackageConfig()
        {

        }

        public PackageConfig(string path)
        {
            XElement packagesXml = XElement.Load(path);

            foreach (var variable in packagesXml.Nodes())
            {
                if (variable is XElement xElement)
                {
                    var idAttribute = xElement.Attribute("id");
                    var versionAttribute = xElement.Attribute("version");

                    var packageIdentity = PackageIdentityParser.Parse(idAttribute.Value, versionAttribute.Value);
                    Packages.Add(packageIdentity);
                }
            }
        }

        public void Serialize(string path)
        {
            var packagesXml = Packages.Select(x => new XElement("package", new XAttribute("id", x.Id), new XAttribute("version", x.Version)));

            var rootXml = new XElement("packages", packagesXml);

            new XDocument(rootXml).Save(path);
        }
    }
}
