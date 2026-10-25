# Authoring a tool

A tool is a folder: one assembly plus one manifest. The host reads the manifest and puts a button on
the ribbon; no host change is required.

## 1. Create the project

```
src/ArKrPlugFR.Plugins.<Name>/
    ArKrPlugFR.Plugins.<Name>.csproj
    <Name>Command.cs
    <Name>.plugin.json
```

The project targets `net48`, enables `UseWPF` (the shared window shell is WPF) and references the
three shared projects:

```xml
<PropertyGroup>
  <TargetFramework>net48</TargetFramework>
  <RootNamespace>ArKrPlugFR.Plugins.MyTool</RootNamespace>
  <AssemblyName>ArKrPlugFR.Plugins.MyTool</AssemblyName>
  <UseWPF>true</UseWPF>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="..\ArKrPlugFR.Core\ArKrPlugFR.Core.csproj" />
  <ProjectReference Include="..\ArKrPlugFR.Revit\ArKrPlugFR.Revit.csproj" />
  <ProjectReference Include="..\ArKrPlugFR.Ui\ArKrPlugFR.Ui.csproj" />
</ItemGroup>

<ItemGroup>
  <None Include="MyTool.plugin.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## 2. Write the command

Derive from `PluginCommand`. The base class already handles the active-document guard, the culture,
logging and turning an unexpected exception into a readable message.

```csharp
using ArKrPlugFR.Revit.Commands;
using Autodesk.Revit.UI;

public sealed class MyToolCommand : PluginCommand
{
    protected override Result Run(PluginContext context)
    {
        // context.Document, context.UiDocument, context.Log, context.Culture
        return Result.Succeeded;
    }
}
```

Rules worth keeping:

- Put anything that can be expressed without the Revit API into `ArKrPlugFR.Core` and test it.
- Wrap document changes in a `Transaction`; use `TransactionMode.Manual`, which the base class
  declares.
- Report what happened. A tool that changes a model silently is a tool nobody trusts.

## 3. Write the manifest

`MyTool.plugin.json`:

```json
{
  "id": "my-tool",
  "assembly": "ArKrPlugFR.Plugins.MyTool.dll",
  "entryType": "ArKrPlugFR.Plugins.MyTool.MyToolCommand",
  "titles": { "en": "My tool", "ru": "Мой инструмент" },
  "descriptions": {
    "en": "One sentence describing what the tool does.",
    "ru": "Одна фраза о том, что делает инструмент."
  },
  "ribbon": { "tab": "ArKrPlugFR", "panel": "Elements", "buttonSize": "large", "order": 10 },
  "supportedRevitVersions": [ "2020", "2022", "2024" ]
}
```

| Field | Required | Notes |
| --- | --- | --- |
| `id` | yes | Stable identifier, unique across the deployment; used as the button name |
| `assembly` | yes | File name of the tool assembly, resolved next to the host assembly |
| `entryType` | yes | Full name of the command type |
| `titles` | yes | Button captions keyed by culture; `en` is the fallback |
| `descriptions` | no | Tooltip texts keyed by culture |
| `ribbon.tab` | yes | Tab name; created if missing |
| `ribbon.panel` | yes | Panel inside the tab; created if missing |
| `ribbon.buttonSize` | no | `large` (default), `medium`, `small` |
| `ribbon.order` | no | Sort order inside the panel; ties are broken by caption |
| `supportedRevitVersions` | no | Omitted means every supported release |

Current limitation: only `large` buttons are placed; a manifest requesting another size is logged as
a warning and shown as a large button.

A malformed manifest never breaks the add-in. The file is skipped, and the reason is written to the
log and shown in the **About** window.

## 4. Build and deploy

```powershell
./scripts/pack.ps1 -RevitVersion 2024
```

Copy `artifacts/deploy/2024/` into `%APPDATA%\Autodesk\Revit\Addins\2024\` and start Revit.

## Checklist before committing a tool

- [ ] The command derives from `PluginCommand` and returns a meaningful `Result`.
- [ ] Model changes happen inside a transaction.
- [ ] Rules that do not need the Revit API live in `ArKrPlugFR.Core` and have tests.
- [ ] The manifest has both `en` and `ru` captions.
- [ ] `dotnet test tests/ArKrPlugFR.Core.Tests` passes.
- [ ] The build for every supported release produces no warnings:
      `dotnet build ArKrPlugFR.sln -p:RevitVersion=2020` (repeat for 2022 and 2024).
