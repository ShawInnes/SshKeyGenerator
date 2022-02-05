# SSHKeyGenerator

A native .NET and .NET Core library for creating SSH RSA keys suitable for use with SSH clients and Git+SSH authentication.

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
