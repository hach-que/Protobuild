using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Protobuild
{
    public class SelfUpdate : ISelfUpdate
    {
        private readonly IPackageManager _packageManager;
        private readonly IPackageLookup _packageLookup;
        private readonly IHostPlatformDetector _hostPlatformDetector;

        internal SelfUpdate(
            IPackageManager packageManager,
            IPackageLookup packageLookup,
            IHostPlatformDetector hostPlatformDetector)
        {
            _packageManager = packageManager;
            _packageLookup = packageLookup;
            _hostPlatformDetector = hostPlatformDetector;
        }

        private string GetVersionsPath()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var versionsPath = Path.Combine(basePath, "Protobuild", "Versions");

            if (!Directory.Exists(versionsPath))
            {
                Directory.CreateDirectory(versionsPath);
            }

            return versionsPath;
        }

        public void EnsureDesiredVersionIsAvailableIfPossible()
        {
            if (Assembly.GetEntryAssembly()
                .Location.StartsWith(GetVersionsPath(), StringComparison.InvariantCultureIgnoreCase))
            {
                // We have been launched from the versions folder, so assume that we are the correct
                // version instead of performing any other logic.
                return;
            }

            var desiredVersionSearchOrder = new List<string>()
            {
                "stable",
                "master"
            };

            if (File.Exists(Path.Combine("Build", "Module.xml")))
            {
                var module = ModuleInfo.Load(Path.Combine("Build", "Module.xml"));
                if (string.IsNullOrWhiteSpace(module.ProtobuildVersion))
                {
                    // This module has no desired version (other than the one that is currently
                    // executing).
                    return;
                }

                // Add the desired version to the start of the desired version list.
                desiredVersionSearchOrder.Insert(0, module.ProtobuildVersion);
            }

            PackageRef? foundRef = null;
            ProtobuildPackageMetadata packageMetadata = null;

            // Try to lookup the latest package version map from the internet, if possible.
            try
            {
                var preferCache = false;

                foreach (var @ref in desiredVersionSearchOrder)
                {
                    var request = new PackageRequestRef(
                        "https://protobuild.org/Protobuild/Protobuild",
                        @ref,
                        _hostPlatformDetector.DetectPlatform(),
                        preferCache);

                    var found = false;

                    try
                    {
                        var response = _packageLookup.Lookup(request) as ProtobuildPackageMetadata;

                        if (response?.BinaryURI != null)
                        {
                            packageMetadata = response;
                            found = true;
                            foundRef = new PackageRef
                            {
                                Uri = "https://protobuild.org/Protobuild/Protobuild",
                                GitRef = @ref
                            };
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // No binary package found; no source URI set.
                    }

                    if (found)
                    {
                        break;
                    }

                    preferCache = true;
                }
            }
            catch
            {
                // Unable to perform online lookup.
            }

            if (foundRef == null)
            {
                // We couldn't resolve the desired reference, or we're offline and
                // can't perform online lookups.
                return;
            }

            // Install the desired version.
            _packageManager.Resolve(
                packageMetadata,
                foundRef.Value,
                null,
                false,
                true,
                false);

            // TODO: Set some state telling us that a relaunch is needed.
        }

        public bool RelaunchIfNeeded(string[] args, out int relaunchExitCode)
        {
            
        }
    }
}
