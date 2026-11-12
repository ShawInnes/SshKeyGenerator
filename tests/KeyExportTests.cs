using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Renci.SshNet;
using Xunit;

namespace SshKeyGenerator.Tests
{
    public class KeyExportTests
    {
        [Theory]
        [InlineData(PrivateKeyFormat.Pem, null)]
        [InlineData(PrivateKeyFormat.Pem, "")]
        [InlineData(PrivateKeyFormat.Pem, "test passphrase")]
        [InlineData(PrivateKeyFormat.Pem, "päss🔑 phrase")]
        [InlineData(PrivateKeyFormat.OpenSsh, null)]
        [InlineData(PrivateKeyFormat.OpenSsh, "")]
        [InlineData(PrivateKeyFormat.OpenSsh, "test passphrase")]
        [InlineData(PrivateKeyFormat.OpenSsh, "päss🔑 phrase")]
        public void SshNetCanReadEveryFormatAndSign(PrivateKeyFormat format, string passphrase)
        {
            using var generator = new SshKeyGenerator(2048);
            string pem = generator.ToPrivateKey(format, passphrase, "test@example.com");
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pem));
            using var key = new PrivateKeyFile(stream, passphrase);
            Assert.Equal(generator.ToRfcPublicKey(null).Split(' ')[1], Convert.ToBase64String(key.HostKeyAlgorithms.First().Data));
            var algorithm = key.HostKeyAlgorithms.Single(a => a.Name == "rsa-sha2-256");
            var data = Encoding.UTF8.GetBytes("independent reader signature test");
            var signature = algorithm.Sign(data);
            // SSH signatures contain a string algorithm name followed by a string signature.
            using var encoded = new MemoryStream(signature);
            int ReadLength() => (encoded.ReadByte() << 24) | (encoded.ReadByte() << 16) | (encoded.ReadByte() << 8) | encoded.ReadByte();
            int nameLength = ReadLength();
            encoded.Position += nameLength;
            var rawSignature = new byte[ReadLength()];
            Assert.Equal(rawSignature.Length, encoded.Read(rawSignature, 0, rawSignature.Length));
            using var publicKey = RSA.Create();
            publicKey.ImportFromPem(generator.ToPublicKey());
            Assert.True(publicKey.VerifyData(data, rawSignature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

            if (!string.IsNullOrEmpty(passphrase))
            {
                using var wrong = new MemoryStream(Encoding.UTF8.GetBytes(pem));
                Assert.ThrowsAny<Exception>(() => new PrivateKeyFile(wrong, "wrong"));
                using var missing = new MemoryStream(Encoding.UTF8.GetBytes(pem));
                Assert.ThrowsAny<Exception>(() => new PrivateKeyFile(missing));
                Assert.NotEqual(pem, generator.ToPrivateKey(format, passphrase));
            }
        }

        [Fact]
        public void ExistingExportsDescribeTheSameKey()
        {
            using var generator = new SshKeyGenerator(2048);
            using var privateKey = RSA.Create();
            using var publicKey = RSA.Create();
            privateKey.ImportFromPem(generator.ToPrivateKey());
            publicKey.ImportFromPem(generator.ToPublicKey());
            var data = Encoding.UTF8.GetBytes("key export verification");
            var signature = privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            Assert.True(publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            using var xml = RSA.Create();
            xml.FromXmlString(generator.ToXml());
            Assert.Equal(privateKey.ExportParameters(false).Modulus, xml.ExportParameters(false).Modulus);
            Assert.Equal(generator.ToPrivateKey(), generator.ToPrivateKey((string)null));
            Assert.Equal("-----BEGIN OPENSSH PRIVATE KEY-----", generator.ToOpenSshPrivateKey("secret").Split('\n')[0]);
        }

        [Fact]
        public void LegacyCspBlobsRoundTrip()
        {
            using var generator = new SshKeyGenerator(2048);
            using var provider = new RSACryptoServiceProvider();
            provider.PersistKeyInCsp = false;
            provider.ImportCspBlob(generator.ToBlobs(true));
            Assert.Equal(generator.ToXml(), provider.ToXmlString(true));
            provider.ImportCspBlob(Convert.FromBase64String(generator.ToB64Blob(false)));
            Assert.True(provider.PublicOnly);
        }

        [Fact]
        public void InvalidArgumentsAndDisposedInstancesAreRejected()
        {
            using var generator = new SshKeyGenerator(2048);
            Assert.Throws<ArgumentOutOfRangeException>(() => generator.ToPrivateKey((PrivateKeyFormat)123));
            Assert.Throws<ArgumentException>(() => generator.ToRfcPublicKey("bad\ncomment"));
            Assert.Throws<ArgumentException>(() => generator.ToOpenSshPrivateKey(null, "bad\0comment"));
            generator.Dispose();
            generator.Dispose();
            Assert.Throws<ObjectDisposedException>(() => generator.ToPrivateKey());
            Assert.Throws<ObjectDisposedException>(() => generator.ToRfcPublicKey());
            Assert.Throws<ObjectDisposedException>(() => generator.ToPublicKey());
            Assert.Throws<ObjectDisposedException>(() => generator.ToXml());
            Assert.Throws<ObjectDisposedException>(() => generator.ToBlobs(false));
        }

        // Enable when ssh-keygen and openssl are installed. CI runs these on Linux and macOS.
        [Theory]
        [InlineData(PrivateKeyFormat.Pem, "")]
        [InlineData(PrivateKeyFormat.Pem, "test passphrase")]
        [InlineData(PrivateKeyFormat.Pem, "päss🔑 phrase")]
        [InlineData(PrivateKeyFormat.OpenSsh, "")]
        [InlineData(PrivateKeyFormat.OpenSsh, "test passphrase")]
        [InlineData(PrivateKeyFormat.OpenSsh, "päss🔑 phrase")]
        public void NativeToolsCanDecrypt(PrivateKeyFormat format, string passphrase)
        {
            if (Environment.GetEnvironmentVariable("SSH_KEYGEN_INTEROP_TESTS") != "1") return;
            var directory = Path.Combine(Path.GetTempPath(), "sshkey-tests-" + Guid.NewGuid());
            Directory.CreateDirectory(directory);
            try
            {
                using var generator = new SshKeyGenerator(2048);
                var path = Path.Combine(directory, "key");
                File.WriteAllText(path, generator.ToPrivateKey(format, passphrase), new UTF8Encoding(false));
                if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                var result = Run("ssh-keygen", "-y", "-P", passphrase, "-f", path);
                Assert.True(result.Code == 0, result.Error);
                Assert.StartsWith(generator.ToRfcPublicKey(null), result.Output.Trim());
                if (passphrase.Length > 0)
                    Assert.NotEqual(0, Run("ssh-keygen", "-y", "-P", "wrong", "-f", path).Code);
                if (format == PrivateKeyFormat.Pem)
                {
                    var check = Run("openssl", "rsa", "-in", path, "-passin", "pass:" + passphrase, "-check", "-noout");
                    Assert.True(check.Code == 0, check.Error);
                    Assert.Contains("RSA key ok", check.Output);
                }
            }
            finally { Directory.Delete(directory, true); }
        }

        private static (int Code, string Output, string Error) Run(string tool, params string[] args)
        {
            var info = new ProcessStartInfo(tool) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            foreach (var arg in args) info.ArgumentList.Add(arg);
            using var process = Process.Start(info);
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(30000))
            {
                process.Kill(true);
                throw new TimeoutException(tool);
            }
            return (process.ExitCode, output.GetAwaiter().GetResult(), error.GetAwaiter().GetResult());
        }
    }
}
