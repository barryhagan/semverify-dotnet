# semverify-dotnet
## A semantic version verification tool for dotnet assemblies

[![Nuget Package](https://badgen.net/nuget/v/semverify-dotnet-tool)](https://www.nuget.org/packages/semverify-dotnet-tool/)

## Examples

Compare two NuGet package versions:

```
> semverify Newtonsoft.Json@12.0.1 Newtonsoft@json@12.0.2

+ public Newtonsoft.Json.MissingMemberHandling Newtonsoft.Json.JsonObjectAttribute.MissingMemberHandling { get; set; }

+ public Newtonsoft.Json.Linq.JTokenReader.JTokenReader(Newtonsoft.Json.Linq.JToken token, string initialPath) { }

+ public Newtonsoft.Json.JsonConverter Newtonsoft.Json.Serialization.JsonContract.InternalConverter { get; internal set; }

+ public System.Nullable<Newtonsoft.Json.MissingMemberHandling> Newtonsoft.Json.Serialization.JsonObjectContract.MissingMemberHandling { get; set; }

+ public bool Newtonsoft.Json.Serialization.JsonProperty.IsRequiredSpecified { get; }

+ public override int Newtonsoft.Json.Serialization.NamingStrategy.GetHashCode() { }

+ public override bool Newtonsoft.Json.Serialization.NamingStrategy.Equals(object obj) { }

+ protected new bool Newtonsoft.Json.Serialization.NamingStrategy.Equals(Newtonsoft.Json.Serialization.NamingStrategy other) { }

The calculated semver for (12.0.1 => 12.0.2) was [Minor] (12.1.0)
```

Compare a NuGet package to a local assembly DLL:

`> semverify Newtonsoft.Json@11.0.2 ./Newtonsoft.Json.dll`


Output the public API of an assembly

`> semverify ./Newtonsoft.Json.dll`

## Usage

```
Usage:
  semverify [options] <assembly1> [<assembly2>]

Arguments:
  <assembly1>    The path or nuget package of the first assembly to compare
  <assembly2>    The path or nuget package of the second assembly to compare

Options:
  --deps, --common-deps <deps>                       The path to dependencies shared by both assemblies
  --a1-deps, --assembly1-deps-path <a1-deps>         The path to dependencies of assembly 1
  --a2-deps, --assembly2-deps-path <a2-deps>         The path to dependencies of assembly 2
  --output-api <output-api>                          The path to output the generated API text files
  --expected-change-type <Major|Minor|None|Patch>    The expected semver change type for this comparison
  --version                                          Display version information
  ```
