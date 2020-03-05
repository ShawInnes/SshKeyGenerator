using System;

namespace SshKeyGeneratorSample
{
  class Program
  {
    static void Main(string[] args)
    {
      var keygen = new SshKeyGenerator.SshKeyGenerator(2048);

      var privateKey = keygen.ToPrivateKey();
      Console.WriteLine(privateKey);

      var publicSshKey = keygen.ToRfcPublicKey();
      Console.WriteLine(publicSshKey);

      var publicSshKeyWithComment = keygen.ToRfcPublicKey("user@domain.com");
      Console.WriteLine(publicSshKeyWithComment);
    }
  }
}
