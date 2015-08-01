namespace Protobuild.Tests
{
    using System.IO;
    using Xunit;

    public class FSharpLibraryWorksTest : ProtobuildTest
    {
        [Fact]
        public void GenerationIsCorrect()
        {
            this.SetupTest("FSharpLibraryWorks");

            this.Generate("Windows");
        }
    }
}
