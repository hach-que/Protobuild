namespace Protobuild.Tests
{
	using System;
    using System.IO;
    using Xunit;

    public class SigningKeyWorksTest : ProtobuildTest
    {
        [Fact]
        public void GenerationIsCorrect()
        {
			this.SetupTest("SigningKeyWorks");

			// Create a .codesignkey and .packagesignkey file in the HOME directory
			// where we are running.
            var home = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(home))
            {
                home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

			using (var writer = new StreamWriter(Path.Combine(home, ".codesignkey")))
			{
				writer.WriteLine("CODESIGNKEY-TEST");
            }

            using (var writer = new StreamWriter(Path.Combine(home, ".packagesignkey")))
            {
                writer.WriteLine("PACKAGESIGNKEY-TEST");
            }

            this.Generate("MacOS");

            Assert.True(File.Exists(this.GetPath(@"Console\Console.MacOS.csproj")));
            var consoleContents = this.ReadFile(@"Console\Console.MacOS.csproj");

            Assert.Contains("<EnableCodeSigning>True</EnableCodeSigning>", consoleContents);
            Assert.Contains("<CodeSigningKey>CODESIGNKEY-TEST</CodeSigningKey>", consoleContents);
            Assert.Contains("<EnablePackageSigning>True</EnablePackageSigning>", consoleContents);
            Assert.Contains("<PackageSigningKey>PACKAGESIGNKEY-TEST</PackageSigningKey>", consoleContents);
            Assert.Contains("<CreatePackage>True</CreatePackage>", consoleContents);

            File.Delete(Path.Combine(home, ".codesignkey"));
            File.Delete(Path.Combine(home, ".packagesignkey"));

            this.Generate("MacOS");

            Assert.True(File.Exists(this.GetPath(@"Console\Console.MacOS.csproj")));
            consoleContents = this.ReadFile(@"Console\Console.MacOS.csproj");

            Assert.DoesNotContain("<EnableCodeSigning>True</EnableCodeSigning>", consoleContents);
            Assert.DoesNotContain("<CodeSigningKey>CODESIGNKEY-TEST</CodeSigningKey>", consoleContents);
            Assert.DoesNotContain("<EnablePackageSigning>True</EnablePackageSigning>", consoleContents);
            Assert.DoesNotContain("<PackageSigningKey>PACKAGESIGNKEY-TEST</PackageSigningKey>", consoleContents);
            Assert.DoesNotContain("<CreatePackage>True</CreatePackage>", consoleContents);
            Assert.Contains("<EnableCodeSigning>False</EnableCodeSigning>", consoleContents);
            Assert.Contains("<EnablePackageSigning>False</EnablePackageSigning>", consoleContents);
            Assert.Contains("<CreatePackage>False</CreatePackage>", consoleContents);
        }
    }
}