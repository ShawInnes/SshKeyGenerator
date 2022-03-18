using System;

namespace SshKeyGeneratorSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new instance 
            var keygen = new SshKeyGenerator.SshKeyGenerator(2048);

            // Get native private key
            var privateKey = keygen.ToPrivateKey();
            Console.WriteLine(privateKey);

            // Get native public key
            var publicSshKey = keygen.ToRfcPublicKey();
            Console.WriteLine(publicSshKey);

            // Get public key with comment
            var publicSshKeyWithComment = keygen.ToRfcPublicKey("user@domain.com");
            Console.WriteLine(publicSshKeyWithComment);

            // Get Base64 encoded keys with private key
            var base64Keys = keygen.ToB64Blob(true);
            Console.WriteLine(base64Keys);

            // Use in a SSH server. e.g. FxSsh
            /*
            var server = new SshServer();
            server.AddHostKey("ssh-rsa", base64Keys);
            */
        }
    }
}