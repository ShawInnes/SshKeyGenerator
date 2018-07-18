# DotNet Core SSH KeyGen

[![Build status](https://ci.appveyor.com/api/projects/status/b0jfpp2jvslo3mj8?svg=true)](https://ci.appveyor.com/project/ShawInnes/sshkeygenerator)

After searching around for a solution to creating SSH keys using C# and dotnet core I came across the following stackoverflow posts which explain how to convert from `RSACryptoServiceProvider` to values suitable for use with ssh.

# References

- Exporting Private Key: https://stackoverflow.com/a/23739932
- Exporting Public Key: https://stackoverflow.com/a/25591659

```

  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net452</TargetFrameworks>
  </PropertyGroup>

```