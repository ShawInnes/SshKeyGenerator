using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SshKeyGenerator.Cryptography;

namespace SshKeyGenerator
{
    internal static class PrivateKeyWriter
    {
        private const int BcryptRounds = 64;

        internal static string Pem(byte[] der, string passphrase)
        {
            if (string.IsNullOrEmpty(passphrase)) return Armor("RSA PRIVATE KEY", der);

            var iv = RandomBytes(16);
            var password = Encoding.UTF8.GetBytes(passphrase);
            var key = new byte[32];
            var previous = new byte[0];
            try
            {
                // OpenSSL EVP_BytesToKey: MD5(previous || password || first 8 IV bytes).
                // This derivation is required by the traditional encrypted PEM format.
                using (var md5 = MD5.Create())
                {
                    for (int offset = 0; offset < key.Length; offset += previous.Length)
                    {
                        var input = new byte[previous.Length + password.Length + 8];
                        Buffer.BlockCopy(previous, 0, input, 0, previous.Length);
                        Buffer.BlockCopy(password, 0, input, previous.Length, password.Length);
                        Buffer.BlockCopy(iv, 0, input, previous.Length + password.Length, 8);
                        Array.Clear(previous, 0, previous.Length);
                        previous = md5.ComputeHash(input);
                        Array.Clear(input, 0, input.Length);
                        Buffer.BlockCopy(previous, 0, key, offset, Math.Min(previous.Length, key.Length - offset));
                    }
                }

                var encrypted = Encrypt(der, key, iv, PaddingMode.PKCS7);
                return Armor("RSA PRIVATE KEY", encrypted,
                    "Proc-Type: 4,ENCRYPTED\nDEK-Info: AES-256-CBC," + BitConverter.ToString(iv).Replace("-", "") + "\n\n");
            }
            finally
            {
                Array.Clear(password, 0, password.Length);
                Array.Clear(key, 0, key.Length);
                Array.Clear(previous, 0, previous.Length);
            }
        }

        internal static string OpenSsh(RSAParameters parameters, string passphrase, string comment)
        {
            ValidateComment(comment);
            bool encrypted = !string.IsNullOrEmpty(passphrase);
            var salt = encrypted ? RandomBytes(16) : new byte[0];
            byte[] privateBlock;
            using (var stream = new MemoryStream())
            {
                var check = RandomBytes(4);
                stream.Write(check, 0, check.Length);
                stream.Write(check, 0, check.Length);
                WriteString(stream, "ssh-rsa");
                WriteMpint(stream, parameters.Modulus);
                WriteMpint(stream, parameters.Exponent);
                WriteMpint(stream, parameters.D);
                WriteMpint(stream, parameters.InverseQ);
                WriteMpint(stream, parameters.P);
                WriteMpint(stream, parameters.Q);
                WriteString(stream, comment ?? "");
                int blockSize = encrypted ? 16 : 8;
                int padding = blockSize - (int)(stream.Length % blockSize);
                for (int i = 1; i <= padding; i++) stream.WriteByte((byte)i);
                privateBlock = stream.ToArray();
                Array.Clear(stream.GetBuffer(), 0, (int)stream.Length);
            }

            try
            {
                if (encrypted)
                {
                    var password = Encoding.UTF8.GetBytes(passphrase);
                    var derived = new byte[48];
                    var key = new byte[32];
                    var iv = new byte[16];
                    try
                    {
                        new BCrypt().Pbkdf(password, salt, BcryptRounds, derived);
                        Buffer.BlockCopy(derived, 0, key, 0, key.Length);
                        Buffer.BlockCopy(derived, key.Length, iv, 0, iv.Length);
                        var ciphertext = Encrypt(privateBlock, key, iv, PaddingMode.None);
                        Array.Clear(privateBlock, 0, privateBlock.Length);
                        privateBlock = ciphertext;
                    }
                    finally
                    {
                        Array.Clear(password, 0, password.Length);
                        Array.Clear(derived, 0, derived.Length);
                        Array.Clear(key, 0, key.Length);
                        Array.Clear(iv, 0, iv.Length);
                    }
                }

                using (var output = new MemoryStream())
                using (var options = new MemoryStream())
                {
                    var magic = Encoding.ASCII.GetBytes("openssh-key-v1\0");
                    output.Write(magic, 0, magic.Length);
                    WriteString(output, encrypted ? "aes256-cbc" : "none");
                    WriteString(output, encrypted ? "bcrypt" : "none");
                    if (encrypted)
                    {
                        WriteString(options, salt);
                        WriteUInt32(options, BcryptRounds);
                    }
                    WriteString(output, options.ToArray());
                    WriteUInt32(output, 1);
                    WriteString(output, PublicBlob(parameters));
                    WriteString(output, privateBlock);
                    var bytes = output.ToArray();
                    try { return Armor("OPENSSH PRIVATE KEY", bytes); }
                    finally
                    {
                        Array.Clear(bytes, 0, bytes.Length);
                        Array.Clear(output.GetBuffer(), 0, (int)output.Length);
                    }
                }
            }
            finally { Array.Clear(privateBlock, 0, privateBlock.Length); }
        }

        internal static byte[] PublicBlob(RSAParameters parameters)
        {
            using (var stream = new MemoryStream())
            {
                WriteString(stream, "ssh-rsa");
                WriteMpint(stream, parameters.Exponent);
                WriteMpint(stream, parameters.Modulus);
                return stream.ToArray();
            }
        }

        internal static void ValidateComment(string comment)
        {
            if (comment != null && (comment.IndexOf('\r') >= 0 || comment.IndexOf('\n') >= 0 || comment.IndexOf('\0') >= 0))
                throw new ArgumentException("Key comments cannot contain newlines or NUL characters.", nameof(comment));
        }

        private static void WriteMpint(Stream stream, byte[] value)
        {
            int offset = 0;
            while (offset < value.Length && value[offset] == 0) offset++;
            bool prefix = offset < value.Length && (value[offset] & 0x80) != 0;
            WriteUInt32(stream, value.Length - offset + (prefix ? 1 : 0));
            if (prefix) stream.WriteByte(0);
            stream.Write(value, offset, value.Length - offset);
        }

        private static void WriteUInt32(Stream stream, int value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteString(Stream stream, string value) => WriteString(stream, Encoding.UTF8.GetBytes(value));

        private static void WriteString(Stream stream, byte[] value)
        {
            WriteUInt32(stream, value.Length);
            stream.Write(value, 0, value.Length);
        }

        private static byte[] RandomBytes(int length)
        {
            var bytes = new byte[length];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
            return bytes;
        }

        private static byte[] Encrypt(byte[] data, byte[] key, byte[] iv, PaddingMode padding)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = padding;
                using (var transform = aes.CreateEncryptor(key, iv))
                    return transform.TransformFinalBlock(data, 0, data.Length);
            }
        }

        private static string Armor(string label, byte[] bytes, string headers = "")
        {
            var base64 = Convert.ToBase64String(bytes);
            var output = new StringBuilder().Append("-----BEGIN ").Append(label).Append("-----\n").Append(headers);
            for (int i = 0; i < base64.Length; i += 64)
                output.Append(base64, i, Math.Min(64, base64.Length - i)).Append('\n');
            return output.Append("-----END ").Append(label).Append("-----\n").ToString();
        }
    }
}
