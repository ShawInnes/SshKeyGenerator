using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using XC.RSAUtil;

namespace SshKeyGenerator
{

  class Program
  {


    static void Main(string[] args)
    {
      Serilog.Log.Logger = new Serilog.LoggerConfiguration()
              .WriteTo.LiterateConsole()
              .CreateLogger();

      var keygen = new SshKeyGenerator(2048);

      Log.Information("{thing}", keygen.ToPrivateKey());
      Log.Information("{thing}", keygen.ToPublicKey());
      Log.Information("{thing}", keygen.ToRfcPublicKey());
    }
  }
}
