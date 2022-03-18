# DotNet Core SSH KeyGen

[![Build status](https://ci.appveyor.com/api/projects/status/b0jfpp2jvslo3mj8?svg=true)](https://ci.appveyor.com/project/ShawInnes/sshkeygenerator) [![NuGet version (SshKeyGenerator)](https://img.shields.io/nuget/v/SshKeyGenerator.svg?style=flat-square)](https://www.nuget.org/packages/SshKeyGenerator/)

After searching around for a solution to creating SSH keys using C# and dotnet core I came across the following stackoverflow posts which explain how to convert from `RSACryptoServiceProvider` to values suitable for use with ssh.

## Supported Platforms

- .NET 4.5 (Desktop / Server)
- .NET Standard 2.0
- .NET Standard 2.1
- .NET 5.0
- .NET 6.0

## Installation

SshKeyGenerator is a library for .NET and is available on NuGet:

```powershell
Install-Package SshKeyGenerator
```

or

```bash
dotnet add package SshKeyGenerator
```

## Usage Example

```csharp
var keygen = new SshKeyGenerator.SshKeyGenerator(2048);

var privateKey = keygen.ToPrivateKey();
Console.WriteLine(privateKey);

var publicSshKey = keygen.ToRfcPublicKey();
Console.WriteLine(publicSshKey);

var publicSshKeyWithComment = keygen.ToRfcPublicKey("user@domain.com");
Console.WriteLine(publicSshKeyWithComment);
```

### Sample Output

#### keygen.ToPrivateKey()

```text
-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAs1O/9F2XlZVaVWEUYfflxx0wDG9FE09hQF2wngA6EemHpe6I
hvdwvQ7obrZ85jSYdBxBRjNAYF3T9+wU+8w6m4qZCVYvqqRjszN8j3VwUBLzWilt
...
wDGzJIkmbhANi+252pH6PAuWjZPfz8COGA6QPPH0/khpHPj1LY4mi5Ubi3YT1uRo
dOGttjtkj9fLX5IQ81G9HR0MdT1Gd4fPenYPOUCgCPB5adpT1BB+
-----END RSA PRIVATE KEY-----
```

#### keygen.ToRfcPublicKey()

```text
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQCzU7/0XZeVlVpVYRRh9
...
+ohFsbcbAMcfLWGUdouyHvLvF21G0z50z2i2CrU7WW4WdrGGfxhU3TeRTXd7Skwk/K7iBBn3oc/xct generated-key
```

#### keygen.ToRfcPublicKey("user@domain.com")

```text
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQCzU7/0XZeVlVpVYRRh9
...
+ohFsbcbAMcfLWGUdouyHvLvF21G0z50z2i2CrU7WW4WdrGGfxhU3TeRTXd7Skwk/K7iBBn3oc/xct user@domain.com
```

### Export Methods

There are three methods which can be used to export SSH key data for use by other libraries such as [FxSsh](https://github.com/Aimeast/FxSsh):

1. byte[] ToBlobs : Export as Blobs
1. string ToB64Blob : Export blobs as base64 string
1. string ToXml : Export as XML string

Export methods usage:

```csharp
// Create a new instance 
var keygen = new SshKeyGenerator.SshKeyGenerator(4096);

// Get Base64 encoded keys with private key
var base64Keys = keygen.ToB64Blob(true);

// Use in a SSH server. e.g. FxSsh
var server = new SshServer();
server.AddHostKey("ssh-rsa", base64Keys);
```

## References

- Exporting Private Key: https://stackoverflow.com/a/23739932
- Exporting Public Key: https://stackoverflow.com/a/25591659
- Exporting Public Key in RFC4716 Format: https://stackoverflow.com/a/47917364

## Contributors

Thanks to the following contributors for their assistance with this library.

- [@ryangribble](https://github.com/ryangribble)
- [@richardlawley](https://github.com/richardlawley)
- [@sauditore](https://github.com/sauditore)
