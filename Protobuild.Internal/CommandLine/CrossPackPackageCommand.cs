using System;
using System.IO;

namespace Protobuild
{
    public class CrossPackPackageCommand : ICommand
    {
        private readonly ICrossPackager m_CrossPackager;

        public CrossPackPackageCommand(
            ICrossPackager crossPackager)
        {
            this.m_CrossPackager = crossPackager;
        }

        public void Encounter(Execution pendingExecution, string[] args)
        {
            pendingExecution.SetCommandToExecuteIfNotDefault(this);

            if (args.Length < 4 || args[0] == null || args[1] == null || args[2] == null || args[3] == null)
            {
                throw new InvalidOperationException("You must provide the source Protobuild package, target NuGet package, references and platform.");
            }

            pendingExecution.PackageSourceFile = new FileInfo(args[0]).FullName;
            pendingExecution.PackageDestinationFile = new FileInfo(args[1]).FullName;
            pendingExecution.PackageNuGetReferences = args[2].Split(',');
            pendingExecution.Platform = args[3];
        }

        public int Execute(Execution execution)
        {
            if (!File.Exists(execution.PackageSourceFile))
            {
                throw new InvalidOperationException("The source file " + execution.PackageSourceFolder + " does not exist.");
            }

            this.m_CrossPackager.Package(
                execution.PackageSourceFile,
                execution.PackageDestinationFile,
                execution.PackageFormat,
                execution.PackageNuGetReferences,
                execution.PackageCrossPackAttributes,
                execution.Platform);

            return 0;
        }

        public string GetDescription()
        {
            return @"
Cross-packages the source Protobuild package into a NuGet package.  You
need to specify the Protobuild project names that should be added as
references when this package is added to a project through NuGet.  The
references should be specified as a comma-delimited string.
";
        }

        public int GetArgCount()
        {
            return 4;
        }

        public string[] GetArgNames()
        {
            return new[] { "module_path", "package_file", "references", "platform" };
        }
    }
}

