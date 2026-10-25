# Architecture

## Goal

Keep the part that decides *what should happen to the model* separate from the part that talks to
Revit. Rules change often and are the interesting part; the API calls around them are repetitive.
Separating the two is what makes this code base testable without Revit and portable across releases.

## Layers

| Project | Target | References Revit | Responsibility |
| --- | --- | --- | --- |
| `ArKrPlugFR.Core` | netstandard2.0 | no | Units, geometry rules, join analysis, logging, plugin manifests |
| `ArKrPlugFR.Revit` | net48 | yes | API access, command base class, version compatibility |
| `ArKrPlugFR.Ui` | net48 | no | WPF shell, palette and control styles, host-window interop |
| `ArKrPlugFR.Host` | net48 | yes | Revit entry point, ribbon built from manifests |
| `ArKrPlugFR.Plugins.*` | net48 | yes | One command per tool, plus its manifest |

Dependency direction is strictly upwards in that table. Nothing depends on a tool project, and the
host discovers tools at runtime instead of referencing them.

## Why `Core` has no Revit reference

Domain types such as `Length`, `SnapSettings`, `StepSnap` and `JoinPairs` express model behaviour in
plain .NET. They can therefore be covered by fast unit tests that run on a modern runtime in CI:

```
dotnet test tests/ArKrPlugFR.Core.Tests
```

Every rule that a user could ask about ("why was this element moved?", "which joins will be
removed?") is decided in this assembly, so the answer is verifiable without opening Revit.

## Multi-version strategy

The Revit API differs between 2020, 2022 and 2024. Three rules keep that manageable:

1. **Prefer APIs that exist in every supported release.** For example, filtering by several
   categories uses `ElementMulticategoryFilter`, which is available in all three, instead of a
   version-specific overload of `ElementCategoryFilter`.
2. **Isolate unavoidable differences.** Conditional compilation lives only in
   `ArKrPlugFR.Revit/Compatibility/`. The release symbol (`REVIT_2020`, `REVIT_2022`, `REVIT_2024`)
   is defined in `Directory.Build.props` from the `RevitVersion` property.
3. **Fail loudly on unknown input.** `RevitVersion` maps to a reference-assembly package version; an
   unmapped value stops the build with an explicit message.

Example of rule 2, `ElementIdCompat`:

```csharp
public static long GetValue(ElementId id)
{
#if REVIT_2024
    return id.Value;          // long accessor, Revit 2024
#else
    return id.IntegerValue;   // int accessor, Revit 2020 and 2022
#endif
}
```

## Reference assemblies instead of an installed Revit

The Revit API is referenced through the `Nice3point.Revit.Api.*` NuGet packages, which ship
`RevitAPI.dll` and `RevitAPIUI.dll` under `ref/`. Consequences:

- the solution builds, tests and packs on a machine without Revit;
- CI can build all supported releases;
- the packages are marked `PrivateAssets="all"` and `ExcludeAssets="runtime"`, so `RevitAPI.dll` is
  **not** copied into the deployment. Revit loads the API into its process, and a second copy in the
  add-in folder would break type identity at runtime.

## Runtime composition

1. Revit reads `installer/ArKrPlugFR.addin` and creates `ArKrPlugFR.Host.App`.
2. `App.OnStartup` asks `PluginCatalog` for the manifests next to the host assembly.
3. `PluginCatalog` filters them by the running Revit release and checks that each tool assembly is
   present; problems are collected, not thrown.
4. `RibbonBuilder` creates the tab and panels once and adds one button per tool.
5. Clicking a button makes Revit load the tool assembly and create the command declared by
   `entryType`.

The host never loads a tool assembly itself, so a broken tool cannot prevent the add-in from
starting, and adding or removing a tool is a file operation.

## Error handling and logging

- `PluginCommand` is the boundary for tool errors: it logs the failure, shows the message and the
  log folder, and returns `Result.Failed` instead of letting an exception escape into Revit.
- `Log` fans out to sinks and swallows sink failures: logging must never be the reason a tool
  breaks. `FileLogSink` writes one file per day and prunes files older than the retention period.
- All user data lives under `%LOCALAPPDATA%\ArKrPlugFR` (`AppPaths`); nothing is written into the
  Revit installation folder.

## Testing strategy

| Layer | How it is covered |
| --- | --- |
| `Core` | Unit tests: units and conversions, snapping rules, join de-duplication, manifest parsing and validation, logging and retention |
| `Ui` | `scripts/smoke-ui.ps1` loads the built assembly with the .NET Framework WPF stack, creates the tool window and exercises its properties, which catches malformed XAML and pack URIs that do not resolve |
| `Revit`, `Host`, tools | Build verification across the three supported releases, plus manual smoke testing in Revit |

The tests deliberately include failure paths (malformed manifests, unwritable directories, unknown
button sizes), because those are the paths that decide whether a tool fails politely or crashes.
