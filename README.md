# DotNet Core SSH KeyGen

[![Build status](https://ci.appveyor.com/api/projects/status/b0jfpp2jvslo3mj8?svg=true)](https://ci.appveyor.com/project/ShawInnes/sshkeygenerator)

After searching around for a solution to creating SSH keys using C# and dotnet core I came across the following stackoverflow posts which explain how to convert from `RSACryptoServiceProvider` to values suitable for use with ssh.

## Supported Platforms

- .NET 4.5 (Desktop / Server)
- .NET Standard 2.0

## Installation

SshKeyGenerator is a library for .NET and is available on NuGet:

```
Install-Package SshKeyGenerator
```

or

```
dotnet add package SshKeyGenerator
```

## Usage Example

```
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

```
-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAs1O/9F2XlZVaVWEUYfflxx0wDG9FE09hQF2wngA6EemHpe6I
hvdwvQ7obrZ85jSYdBxBRjNAYF3T9+wU+8w6m4qZCVYvqqRjszN8j3VwUBLzWilt
...
wDGzJIkmbhANi+252pH6PAuWjZPfz8COGA6QPPH0/khpHPj1LY4mi5Ubi3YT1uRo
dOGttjtkj9fLX5IQ81G9HR0MdT1Gd4fPenYPOUCgCPB5adpT1BB+
-----END RSA PRIVATE KEY-----
```

#### keygen.ToRfcPublicKey()

```
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQCzU7/0XZeVlVpVYRRh9
...
+ohFsbcbAMcfLWGUdouyHvLvF21G0z50z2i2CrU7WW4WdrGGfxhU3TeRTXd7Skwk/K7iBBn3oc/xct generated-key
```

#### keygen.ToRfcPublicKey("user@domain.com")

```
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQCzU7/0XZeVlVpVYRRh9
...
+ohFsbcbAMcfLWGUdouyHvLvF21G0z50z2i2CrU7WW4WdrGGfxhU3TeRTXd7Skwk/K7iBBn3oc/xct user@domain.com
```

## References

- Exporting Private Key: https://stackoverflow.com/a/23739932
- Exporting Public Key: https://stackoverflow.com/a/25591659
- Exporting Public Key in RFC4716 Format: https://stackoverflow.com/a/47917364

## Contributors

Thanks to the following contributors for their assistance with this library.

- [@ryangribble](https://github.com/ryangribble)
- [@richardlawley](https://github.com/richardlawley)
