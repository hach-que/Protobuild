namespace Protobuild.Tests
{
    using System.IO;
    using Xunit;

    public class FSharpConsoleWorksTest : ProtobuildTest
    {
        [Fact]
        public void GenerationIsCorrect()
        {
            this.SetupTest("FSharpConsoleWorks");

            this.Generate("Windows");
        }
    }
}
