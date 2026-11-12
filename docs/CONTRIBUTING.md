# Contributing

[Package usage](../README.md) · [Release guide](RELEASE.md)

This guide covers development and maintenance of SshKeyGenerator. Run the commands
below from the repository root. AI agents and contributors should also follow
[AGENTS.md](../AGENTS.md).

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
[THIRD-PARTY-NOTICES.md](../THIRD-PARTY-NOTICES.md) for the vendored bcrypt license.

## References

- Exporting Private Key: https://stackoverflow.com/a/23739932
- Exporting Public Key: https://stackoverflow.com/a/25591659
- Exporting Public Key in RFC4716 Format: https://stackoverflow.com/a/47917364

## Contributors

Thanks to the following contributors for their assistance with this library.

- [@ryangribble](https://github.com/ryangribble)
- [@richardlawley](https://github.com/richardlawley)
- [@sauditore](https://github.com/sauditore)
