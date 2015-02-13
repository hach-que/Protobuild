using System.IO;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;

namespace Protobuild
{
    public class CrossPackager : ICrossPackager
    {
        const string NUSPEC_NAMESPACE = "http://schemas.microsoft.com/packaging/2011/10/nuspec.xsd";

        public void Package(
            string packageSourceFile, 
            string packageDestinationFile,
            string packageFormat,
            string[] projectReferences,
            Dictionary<string, string> attributes,
            string platform)
        {
            var temporaryWorkingArea = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(Path.Combine(temporaryWorkingArea, "lib"));

            var package = new BinaryPackageContent();
            package.Format = 
                string.IsNullOrEmpty(packageFormat) ? 
                PackageManager.ARCHIVE_FORMAT_TAR_LZMA : 
                packageFormat;

            using (var stream = new FileStream(packageSourceFile, FileMode.Open))
            {
                package.PackageData = new byte[stream.Length];
                stream.Read(package.PackageData, 0, package.PackageData.Length);
            }

            package.ExtractTo(Path.Combine(temporaryWorkingArea, "lib"));

            var moduleInfo = ModuleInfo.Load(Path.Combine(Path.Combine(temporaryWorkingArea, "lib", "Build", "Module.xml")));

            var nuspec = new XmlDocument();
            nuspec.AppendChild(nuspec.CreateXmlDeclaration("1.0", "utf-8", null));

            var root = nuspec.CreateElement("package", NUSPEC_NAMESPACE);
            nuspec.AppendChild(root);

            var metadata = nuspec.CreateElement("metadata", NUSPEC_NAMESPACE);
            root.AppendChild(metadata);

            var now = DateTime.Now;

            var yearAsString = now.Year.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var autoversion = string.Format(
                "{0}.{1}.{2}",
                yearAsString.Substring(2),
                now.DayOfYear,
                (now.Hour * 3600) + (now.Minute * 60) + now.Second);

            var framework = this.GetAttrOrDefault(attributes, "framework", "net40");

            this.AddStringElement(metadata, attributes, "id", moduleInfo.Name + "." + platform);
            this.AddStringElement(metadata, attributes, "version", autoversion);
            this.AddStringElement(metadata, attributes, "title", moduleInfo.Name + "." + platform);
            this.AddStringElement(metadata, attributes, "authors", moduleInfo.Name + " Developers");
            this.AddStringElement(metadata, attributes, "owners", moduleInfo.Name + " Developers");
            this.AddStringElement(metadata, attributes, "licenseUrl", null);
            this.AddStringElement(metadata, attributes, "projectUrl", null);
            this.AddStringElement(metadata, attributes, "iconUrl", null);
            this.AddStringElement(metadata, attributes, "requireLicenseAcceptance", "false");
            this.AddStringElement(metadata, attributes, "description", "This package was automatically created by Protobuild.");
            this.AddStringElement(metadata, attributes, "summary", "This package was automatically created by Protobuild.");
            this.AddStringElement(metadata, attributes, "releaseNotes", "This package was automatically created by Protobuild.");
            this.AddStringElement(metadata, attributes, "copyright", "Copyright " + now.Year);
            this.AddStringElement(metadata, attributes, "language", "en-US");

            var references = nuspec.CreateElement("references", NUSPEC_NAMESPACE);
            metadata.AppendChild(references);

            var files = nuspec.CreateElement("files", NUSPEC_NAMESPACE);
            root.AppendChild(files);

            var addedTargets = new List<string>();
            foreach (var project in projectReferences)
            {
                foreach (var fileSrcC in this.GetFilesRecursively(Path.Combine(temporaryWorkingArea, "lib", project)))
                {
                    var fileSrc = fileSrcC;
                    if (fileSrc.StartsWith("AnyCPU", StringComparison.Ordinal))
                    {
                        fileSrc = fileSrc.Substring (7);
                    }

                    var target = Path.Combine("lib", framework, fileSrc);
                    if (addedTargets.Contains(target))
                    {
                        continue;
                    }

                    addedTargets.Add(target);

                    var file = nuspec.CreateElement("file", NUSPEC_NAMESPACE);
                    file.SetAttribute("src", Path.Combine("lib", project, fileSrcC));
                    file.SetAttribute("target", target);
                    files.AppendChild(file);

                    if (fileSrc.EndsWith(".dll", StringComparison.Ordinal))
                    {
                        var reference = nuspec.CreateElement("reference", NUSPEC_NAMESPACE);
                        reference.SetAttribute("file", new FileInfo(fileSrcC).Name);
                        references.AppendChild(reference);
                    }
                }
            }

            var targetNuspec = Path.Combine(temporaryWorkingArea, "target.nuspec");
            using (var writer = XmlWriter.Create(targetNuspec, new XmlWriterSettings { Indent = true, IndentChars = "  " }))
            {
                nuspec.WriteTo(writer);
            }

            using (var writer = XmlWriter.Create(Console.Out, new XmlWriterSettings { Indent = true, IndentChars = "  " }))
            {
                nuspec.WriteTo(writer);
            }
            Console.WriteLine();

            Console.WriteLine("Running nuget to create package...");

            var startInfo = new ProcessStartInfo
            {
                FileName = "nuget",
                Arguments = "pack ",
                WorkingDirectory = temporaryWorkingArea,
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();

            // Find the resulting .nupkg file.
            foreach (var filePkg in new DirectoryInfo(temporaryWorkingArea).GetFiles("*.nupkg"))
            {
                if (File.Exists(packageDestinationFile))
                {
                    File.Delete(packageDestinationFile);
                }

                filePkg.MoveTo(packageDestinationFile);
                break;
            }

            Console.WriteLine("Package build complete, resulting file is " + packageDestinationFile);
        }

        private void AddStringElement(XmlNode elem, Dictionary<string, string> attrs, string id, string str)
        {
            var value = this.GetAttrOrDefault(attrs, id, str);

            if (value == null)
            {
                return;
            }

            var child = elem.OwnerDocument.CreateElement(id, NUSPEC_NAMESPACE);
            child.InnerText = value;
            elem.AppendChild(child);
        }

        private IEnumerable<string> GetFilesRecursively(string path, string current = "")
        {
            foreach (var dir in new DirectoryInfo(path).GetDirectories())
            {
                foreach (var sub in this.GetFilesRecursively(dir.FullName, Path.Combine(current, dir.Name)))
                {
                    yield return sub;
                }
            }

            foreach (var file in new DirectoryInfo(path).GetFiles())
            {
                yield return Path.Combine(current, file.Name);
            }
        }

        private string GetAttrOrDefault(Dictionary<string, string> attrs, string name, string @default)
        {
            if (attrs.ContainsKey(name))
            {
                return attrs[name];
            }
            else
            {
                return @default;
            }
        }
    }
}

