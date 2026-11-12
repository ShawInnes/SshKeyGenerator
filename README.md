# DotNet Core SSH KeyGen

[![Build status](https://ci.appveyor.com/api/projects/status/b0jfpp2jvslo3mj8?svg=true)](https://ci.appveyor.com/project/ShawInnes/sshkeygenerator) [![NuGet version (SshKeyGenerator)](https://img.shields.io/nuget/v/SshKeyGenerator.svg?style=flat-square)](https://www.nuget.org/packages/SshKeyGenerator/)

Generate RSA SSH keys in managed C# on Windows, Linux, and macOS. Export private keys as OpenSSH or PKCS#1 RSA/PEM, with optional passphrase protection. The library has no runtime package dependencies and does not invoke external tools.

## Project principles

- **Keep the library dependency-free.** Production code must use the .NET Base
  Class Library (BCL), including `System.Security.Cryptography`, rather than
  third-party cryptography libraries such as Bouncy Castle or SSH.NET.
- **Keep key generation self-contained.** Do not require external executables,
  native crypto packages, or platform-specific interop for generation or export.
- **Implement missing format support locally.** Where the BCL does not provide
  an SSH encoding or derivation operation, use a focused managed implementation
  built on the BCL. The internal bcrypt_pbkdf implementation is one such case;
  preserve its attribution and license notices.
- **Verify interoperability independently.** Third-party readers and command-line
  tools may be used in tests only; they must never become dependencies of the
  shipped library. Test/build packages must remain separate from runtime code.
- **Preserve existing functionality.** Keep existing APIs and key formats
  compatible, and document any observable behavior changes.

## Supported Platforms

- .NET Framework 4.5.2 (legacy)
- .NET Standard 2.0
- .NET Standard 2.1
- .NET 5.0
- .NET 6.0
- .NET 8.0
- .NET 10.0

.NET Core applications can use the .NET Standard assets. Existing legacy targets are retained for compatibility; new applications should use a supported .NET runtime.

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
using var keygen = new SshKeyGenerator.SshKeyGenerator(2048);

var privateKey = keygen.ToPrivateKey();
Console.WriteLine(privateKey);

var publicSshKey = keygen.ToRfcPublicKey();
Console.WriteLine(publicSshKey);

var publicSshKeyWithComment = keygen.ToRfcPublicKey("user@domain.com");
Console.WriteLine(publicSshKeyWithComment);
```

### Passphrases and private key formats

```csharp
using SshKeyGenerator;

using var keygen = new SshKeyGenerator.SshKeyGenerator(2048);
// Obtain this from your application's secret input mechanism.
string passphrase = GetPassphrase();

// Modern OpenSSH container: -----BEGIN OPENSSH PRIVATE KEY-----
string openSsh = keygen.ToPrivateKey(PrivateKeyFormat.OpenSsh, passphrase, "user@domain.com");
// Equivalent convenience method:
string sameFormat = keygen.ToOpenSshPrivateKey(passphrase, "user@domain.com");

// Traditional PKCS#1 PEM: -----BEGIN RSA PRIVATE KEY-----
string pem = keygen.ToPrivateKey(PrivateKeyFormat.Pem, passphrase);
// Equivalent convenience method:
string samePemFormat = keygen.ToPrivateKey(passphrase);
```

`ToPrivateKey()` still returns an **unencrypted PKCS#1 PEM** key. A null or empty
passphrase exports an unencrypted key in either format. Whitespace and Unicode
passphrases are preserved and encoded as UTF-8. Encryption applies to each export;
it does not change the key pair or encrypt XML/CSP blob exports. OpenSSH comments
are optional; CR, LF, and NUL are rejected.

| Format | Encryption with a passphrase | Key derivation |
| --- | --- | --- |
| OpenSSH (`openssh-key-v1`) | AES-256-CBC | bcrypt_pbkdf, 64 rounds, random 16-byte salt |
| RSA/PEM (PKCS#1) | AES-256-CBC with PKCS#7 padding | OpenSSL EVP_BytesToKey, MD5, random IV |

Prefer OpenSSH for new integrations. The PEM derivation is the legacy algorithm
required for compatibility with traditional encrypted `RSA PRIVATE KEY` files;
it provides less resistance to passphrase guessing than bcrypt. This is not
PKCS#8 (`ENCRYPTED PRIVATE KEY`). Every encrypted export uses fresh randomness.

`ToRfcPublicKey()` retains its existing name and returns a single-line OpenSSH
`authorized_keys` entry, not an RFC4716 block. `ToPublicKey()` returns a
SubjectPublicKeyInfo `PUBLIC KEY` PEM block. RSA is the key algorithm in both
private key formats.

Save private keys using UTF-8 without a BOM and restrict file access (for example,
`chmod 600 id_rsa` on Unix). To verify a saved key:

```bash
ssh-keygen -y -f id_rsa
# For RSA/PEM only:
openssl rsa -in id_rsa.pem -check -noout
```

Both commands prompt for an encrypted key's passphrase.

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

These legacy exports are unencrypted. CSP blob exports use a temporary
`RSACryptoServiceProvider` and depend on platform support for that provider; PEM
and OpenSSH generation use `RSA.Create()`.

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

## Building and testing

Install the .NET 10 SDK (and the .NET 8 runtime to run both test targets).

```bash
dotnet build SshKeyGenerator.sln -c Release
dotnet test tests/SshKeyGenerator.Tests.csproj -c Release
# Also verify decryption using installed ssh-keygen and openssl (Unix shell):
SSH_KEYGEN_INTEROP_TESTS=1 dotnet test tests/SshKeyGenerator.Tests.csproj -c Release
# Exercise the .NET Standard implementation on a .NET runtime:
SSH_KEYGEN_INTEROP_TESTS=1 dotnet test tests/SshKeyGenerator.Tests.csproj -c Release -f net8.0 -p:KeyGeneratorTargetFramework=netstandard2.0
dotnet pack src/SshKeyGenerator.csproj -c Release
```

Tests use SSH.NET as an independent reader, check incorrect passphrases, compare
public keys, and verify RSA signatures. Native-tool checks are opt-in locally and
run in Linux/macOS CI. Framework 4.5.2 and .NET 5/6 assets are compile-checked;
they require their respective runtimes for execution. See
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for the vendored bcrypt license.

## References

- Exporting Private Key: https://stackoverflow.com/a/23739932
- Exporting Public Key: https://stackoverflow.com/a/25591659
- Exporting Public Key in RFC4716 Format: https://stackoverflow.com/a/47917364

## Contributors

Thanks to the following contributors for their assistance with this library.

- [@ryangribble](https://github.com/ryangribble)
- [@richardlawley](https://github.com/richardlawley)
- [@sauditore](https://github.com/sauditore)
