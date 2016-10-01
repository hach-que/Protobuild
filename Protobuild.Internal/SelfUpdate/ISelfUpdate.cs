namespace Protobuild
{
    public interface ISelfUpdate
    {
        void EnsureDesiredVersionIsAvailableIfPossible();

        bool RelaunchIfNeeded(string[] args, out int relaunchExitCode);
    }
}