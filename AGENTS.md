# Project principles for contributors and AI agents

The goal of SshKeyGenerator is a dependency-free, self-contained .NET SSH key
generator. Treat this as an architectural requirement for every change.

## Production implementation

- Use the .NET Base Class Library (BCL) only, especially
  `System.Security.Cryptography` for cryptographic operations.
- Do not add third-party cryptography packages or libraries to the production
  implementation, directly or transitively. This includes Bouncy Castle, SSH.NET,
  and wrappers around external crypto libraries.
- Do not invoke `ssh-keygen`, OpenSSL, or other executables from production code,
  or add native packages or platform-specific interop as an implementation shortcut.
- When a required SSH format or derivation operation is missing from the BCL,
  maintain a focused local managed implementation using BCL APIs. Do not vendor
  entire third-party libraries to bypass the dependency rule.
- Preserve attribution and license notices for existing adapted source, including
  the internal bcrypt_pbkdf implementation and `THIRD-PARTY-NOTICES.md`.
- Preserve existing public APIs, supported framework targets, and interoperable
  output formats. Document observable compatibility changes explicitly.

## Verification and tooling

- Test-only independent readers (such as the existing SSH.NET test reference),
  test frameworks, and external interoperability tools are allowed in tests.
  Never reference test dependencies from the production project or ship them as
  runtime dependencies.
- Build-only reference assemblies are allowed with `PrivateAssets="All"` and
  must not become dependencies of the distributed NuGet package.
- Verify encrypted exports with independent readers, including correct and
  incorrect passphrases. Use `ssh-keygen` and OpenSSL in opt-in interoperability
  tests when available.
- Check the packed NuGet dependency groups when changing project references or
  package configuration; the shipped library must retain no runtime package
  dependencies.
