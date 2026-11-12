# Key format examples and legacy exports

See the [package README](../README.md) for installation, passphrase options, and
format selection. The abbreviated examples below illustrate output structure;
they are not complete keys.

## Sample output

### keygen.ToPrivateKey()

```text
-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAs1O/9F2XlZVaVWEUYfflxx0wDG9FE09hQF2wngA6EemHpe6I
hvdwvQ7obrZ85jSYdBxBRjNAYF3T9+wU+8w6m4qZCVYvqqRjszN8j3VwUBLzWilt
...
wDGzJIkmbhANi+252pH6PAuWjZPfz8COGA6QPPH0/khpHPj1LY4mi5Ubi3YT1uRo
dOGttjtkj9fLX5IQ81G9HR0MdT1Gd4fPenYPOUCgCPB5adpT1BB+
-----END RSA PRIVATE KEY-----
```

### keygen.ToRfcPublicKey()

```text
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQCzU7/0XZeVlVpVYRRh9
...
+ohFsbcbAMcfLWGUdouyHvLvF21G0z50z2i2CrU7WW4WdrGGfxhU3TeRTXd7Skwk/K7iBBn3oc/xct generated-key
```

### keygen.ToRfcPublicKey("user@domain.com")

```text
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQCzU7/0XZeVlVpVYRRh9
...
+ohFsbcbAMcfLWGUdouyHvLvF21G0z50z2i2CrU7WW4WdrGGfxhU3TeRTXd7Skwk/K7iBBn3oc/xct user@domain.com
```

## Legacy export methods

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
using var keygen = new SshKeyGenerator.SshKeyGenerator(4096);

// Get Base64 encoded keys with private key
var base64Keys = keygen.ToB64Blob(true);

// Use in a SSH server. e.g. FxSsh
var server = new SshServer();
server.AddHostKey("ssh-rsa", base64Keys);
```
