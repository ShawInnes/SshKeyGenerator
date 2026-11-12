# Third-party notices

`src/Cryptography/BCrypt.cs` is adapted from [SSH.NET](https://github.com/sshnet/SSH.NET/blob/3476f739fdf5957c6af397da668a01e36c2130ea/src/Renci.SshNet/Security/Cryptography/Bcrypt.cs),
commit `3476f739fdf5957c6af397da668a01e36c2130ea`. Only its namespace, unused imports, and trailing whitespace were changed.
It implements OpenBSD bcrypt_pbkdf, originally ported through SSHJ. The source retains this license:

```text
Copyright (c) 2006 Damien Miller <djm@mindrot.org>
Copyright (c) 2010 Ryan D. Emerle

Permission to use, copy, modify, and distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
```
