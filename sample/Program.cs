using System;
using SshKeyGenerator;

namespace SshKeyGeneratorSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Supply a passphrase through your application's secret input mechanism.
            // This sample reads an environment variable and prints private keys for demonstration.
            var passphrase = Environment.GetEnvironmentVariable("SSH_KEY_PASSPHRASE");
            using (var keygen = new SshKeyGenerator.SshKeyGenerator(2048))
            {
                Console.WriteLine(keygen.ToPrivateKey(PrivateKeyFormat.OpenSsh, passphrase, "user@domain.com"));
                Console.WriteLine(keygen.ToPrivateKey(PrivateKeyFormat.Pem, passphrase));
                Console.WriteLine(keygen.ToRfcPublicKey("user@domain.com"));
            }
        }
    }
}
