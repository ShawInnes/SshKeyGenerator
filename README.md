# DotNet Core SSH KeyGen

[![Build and test](https://github.com/ShawInnes/SshKeyGenerator/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/ShawInnes/SshKeyGenerator/actions/workflows/build.yml) [![NuGet version (SshKeyGenerator)](https://img.shields.io/nuget/v/SshKeyGenerator.svg?style=flat-square)](https://www.nuget.org/packages/SshKeyGenerator/)

Generate RSA SSH keys in managed C# on Windows, Linux, and macOS. Export private keys as OpenSSH or PKCS#1 RSA/PEM, with optional passphrase protection. The library has no runtime package dependencies and does not invoke external tools.

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

### Legacy exports

`ToBlobs(bool includePrivateKey)`, `ToB64Blob(bool includePrivateKey)`, and `ToXml()`
export CSP blobs or RSA XML for integrations with other libraries. These exports
are unencrypted. CSP blobs depend on platform support for
`RSACryptoServiceProvider`.

See [key format examples and legacy integrations](https://github.com/ShawInnes/SshKeyGenerator/blob/master/docs/KEY-FORMATS.md)
for sample output and usage.

## Contributing

Development principles, build/test instructions, references, and contributor
credits are in the [contributor guide](https://github.com/ShawInnes/SshKeyGenerator/blob/master/docs/CONTRIBUTING.md).
Package maintainers can find publishing setup and release instructions in the
[release guide](https://github.com/ShawInnes/SshKeyGenerator/blob/master/docs/RELEASE.md).
