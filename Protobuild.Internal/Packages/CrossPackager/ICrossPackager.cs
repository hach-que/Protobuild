using System.Collections.Generic;

namespace Protobuild
{
    public interface ICrossPackager
    {
        void Package(
            string packageSourceFile,
            string packageDestinationFile,
            string packageFormat,
            string[] references,
            Dictionary<string, string> attributes,
            string platform);
    }
}

