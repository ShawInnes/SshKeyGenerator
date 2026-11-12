namespace SshKeyGenerator
{
    /// <summary>RSA private key output formats.</summary>
    public enum PrivateKeyFormat
    {
        /// <summary>PKCS#1 RSA PEM, with OpenSSL-compatible encryption when a passphrase is supplied.</summary>
        Pem,
        /// <summary>OpenSSH openssh-key-v1, with bcrypt and AES-256-CBC when encrypted.</summary>
        OpenSsh
    }
}
