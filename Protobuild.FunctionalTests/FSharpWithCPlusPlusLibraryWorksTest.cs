namespace Protobuild.Tests
{
    using System.IO;
    using Xunit;

    public class FSharpWithCPlusPlusLibraryWorksTest : ProtobuildTest
    {
        [Fact]
        public void GenerationIsCorrect()
        {
            this.SetupTest("FSharpWithCPlusPlusLibraryWorks");

            this.Generate("Windows");
        }
    }
}
