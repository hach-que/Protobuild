using System;
using System.IO;

namespace Protobuild
{
    public class CrossPackAttrPackageCommand : ICommand
    {
        public void Encounter(Execution pendingExecution, string[] args)
        {
            if (args.Length < 2)
            {
                throw new InvalidOperationException("You must provide the name and value to set for the cross-packaging attribute");
            }

            pendingExecution.PackageCrossPackAttributes.Add(args[0], args[1]);
        }

        public int Execute(Execution execution)
        {
            throw new NotSupportedException();
        }

        public string GetDescription()
        {
            return @"
Sets a cross-packaging attribute to a given value.
";
        }

        public int GetArgCount()
        {
            return 2;
        }

        public string[] GetArgNames()
        {
            return new[] { "name", "value" };
        }
    }
}

