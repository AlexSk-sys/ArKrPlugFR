# ArKrPlugFR

Independent BIM tooling for Autodesk Revit — architecture and structure (АР / КР) in one ribbon.
Open source, MIT licensed, with small focused tools and a testable core.

[English](#english) · [Русский](#русский)

---

## English

### What it is

ArKrPlugFR is a set of small, focused tools for Revit that remove repetitive work from an
architectural or structural model: joining and unjoining elements, aligning geometry to a grid,
collecting views and sheets, and keeping types clean.

The name is the scope: **Ar**chitecture + **Kr**onstruktsii (structural sections) + **Plug**in.

### Why this repository exists

The tools started as a way to remove repetitive model cleanup from real projects. Each tool
does one job and can be switched off, so a team can adopt one workflow without changing its
whole template.

This repository is the public home for the code, tests and documentation, released under MIT.
Third-party dependencies are limited to general-purpose open-source libraries such as Json.NET
and xUnit, referenced through NuGet under their own licences.

### Highlights

| Area | What it demonstrates |
| --- | --- |
| Layered architecture | Domain rules in a Revit-free assembly, API access in an adapter assembly, UI in a separate assembly, host that only composes them |
| Multi-version support | One source tree builds against the Revit 2020, 2022 and 2024 APIs; version differences isolated in one folder |
| Testable domain | 590+ unit tests run without Revit, because the rules never touch the Revit API |
| Manifest-driven UI | The ribbon is built from `*.plugin.json` files: adding a tool does not require recompiling the host |
| Bilingual UI | Captions, tooltips and reports resolve from culture-keyed manifests with a safe fallback |
| Operational care | Daily rolling logs with retention, per-user data paths, no writes inside the Revit installation |
| Warning-free build | The build is expected to produce zero warnings and CI enforces it |

### Architecture

```
ArKrPlugFR.Core      domain: units, geometry rules, join analysis, logging, manifests
        ▲            (netstandard2.0, no reference to the Revit API)
        │
ArKrPlugFR.Revit     adapters: Revit API access, command base class, version compatibility
        ▲            (net48)
        │
ArKrPlugFR.Ui        WPF shell, palette and control styles, window owner interop (net48)
        ▲
        │
ArKrPlugFR.Host      Revit entry point: reads manifests, builds the ribbon (net48)
        ▲
        │
ArKrPlugFR.Plugins.* one project per tool: a command plus a manifest (net48)
```

Dependency rules that the solution relies on:

- `Core` never references Revit, so its rules are unit-testable and portable.
- `Revit` is the only place allowed to call the Revit API.
- A tool never references another tool; shared behaviour is promoted into `Core`, `Revit` or `Ui`.
- The host has no compile-time reference to any tool: tools are discovered at runtime.

More detail: [docs/architecture.md](docs/architecture.md).

### Supported Revit versions

Revit 2020, 2022 and 2024, from a single source tree. The API reference assemblies are taken from
NuGet (`Nice3point.Revit.Api.*`), so the project builds and packs on a machine without Revit
installed, and CI builds all three targets.

```
dotnet build ArKrPlugFR.sln -p:RevitVersion=2020
dotnet build ArKrPlugFR.sln -p:RevitVersion=2022   # default
dotnet build ArKrPlugFR.sln -p:RevitVersion=2024
```

An unsupported release fails the build with an explicit message instead of silently compiling
against the wrong API.

Where the API genuinely differs, the difference is isolated in
`src/ArKrPlugFR.Revit/Compatibility/`, for example `ElementId` changing from `int` to `long`
accessors in Revit 2024.

Three tools are declared for Revit 2022 and 2024 only — `AnalyticalAlignment`,
`AnalyticalExtension` and `SheetExport` — because the platform features they use do not exist
in Revit 2020. Each manifest lists the releases it supports, and the host leaves the button of
a tool out on a release the manifest does not name.

### Requirements

- .NET SDK 8 or 9 (used for the test project and for building .NET Framework targets)
- .NET Framework 4.8 targeting pack (installs with Visual Studio 2022)
- Revit is **not** required to build, test or pack; it is required only to run the tools

### Build, test, pack

```powershell
# build everything for the default release (2022)
dotnet build ArKrPlugFR.sln

# run the domain tests (no Revit needed)
dotnet test tests/ArKrPlugFR.Core.Tests

# check that the WPF layer loads outside Revit
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/smoke-ui.ps1

# produce a deployable folder for one Revit release
./scripts/pack.ps1 -RevitVersion 2024
```

`pack.ps1` writes a flat deployment to `artifacts/deploy/<release>/` containing the host, the
shared assemblies and every tool with its manifest.

### Deploy

Copy the content of `artifacts/deploy/<release>/` into

```
%APPDATA%\Autodesk\Revit\Addins\<release>\
```

The add-in manifest is relative to its own folder, so the deployment carries no absolute path.
Start Revit: the **ArKrPlugFR** tab appears with the registered tools.

### Add a tool

1. Add a `ArKrPlugFR.Plugins.<Name>` project that references `Core`, `Revit` and `Ui`.
2. Derive the command from `PluginCommand` and implement `Run(PluginContext)`.
3. Add a `<Name>.plugin.json` manifest next to the assembly.
4. Build and pack. The ribbon picks the tool up without any change to the host.

Schema and a worked example: [docs/plugin-authoring.md](docs/plugin-authoring.md).

### Tools

Each tool is its own folder under `src/`, its own manifest and its own tests.

| Tool | What it does |
| --- | --- |
| `UnjoinElements` | Removes the cuts that columns and beams make in walls, one undoable step, reporting every pair that could not be unjoined. |
| `DuplicateTypeFinder` | Compares the project's family types and families, lists what repeats, and deletes only the items with no placed instances — after counting the instances once more at the moment of the delete. |
| `SheetDuplicator` | Duplicates the selected sheets of the current project together with the views they carry, giving every copy a free sheet number and a marked name. |
| `SheetCopy` | Copies the sheets of the current project with their placed views into the other projects that are open, one transaction per project, under free sheet numbers. |
| `ViewTemplateTransfer` | Copies views and view templates from the current project into another project that is open, grouped so whole branches can be ticked at once. |
| `DuplicateViews` | Creates many copies of the views, schedules and view templates of the current project in one action, with a copy count per row and an optional view template for every copy. |
| `HoleCuttingPlugin` | Cuts a rectangular opening in a wall where a duct, pipe, cable tray or conduit crosses it, with clearance on every side; nothing is loaded and no temporary element is created. |
| `WallOpeningsPlugin` | Cuts openings in the finish walls lining the walls you select, opposite their doors and windows, with the options remembered between sessions. |
| `CutHolesInConcrete` | Cuts openings in concrete walls, floors and columns where the equipment and services of a linked model pass through them; nothing is loaded into the project and the link is never changed. |
| `RefreshAnnotations` | Makes the tags of the active view recompute their text — with a selection only the tags of the selected elements, otherwise the whole view; every tag ends where it started. |
| `AlignToGrids` | Pulls the picked columns exactly onto the picked grids: a column lands on the nearest axis within the capture distance, or on the crossing of two axes; group members are skipped. |
| `CountByMark` | Counts the structures of a chosen kind that carry the same mark and writes the number into every matching bar; bars without a match are counted, never written zero. |
| `ReassignRebar` | Attaches the picked rebar to the picked column, wall, beam or floor: real bars are re-hosted, bars from IFC get the host's mark, bars inside systems are reported and skipped. |
| `SectionCollector` | Collects the concrete columns, walls, floors and beams of the 3D view, groups them by level, category and section read from the type names, selects them all and copies the report. |
| `AssignWorksets` | Puts the model elements into worksets by ordered rules — category, family or type prefix, or the value of a parameter — and creates the worksets that do not exist yet in the same step. |
| `SplitFloors` | Splits the picked floors: every outer contour of a flat slab becomes its own floor with the openings it holds, on the same level and with the same type, height offset, structural flag and workset. |
| `TransferParameters` | Three transfers in one window: one value onto the ticked sheets, the layer build-up of a wall or floor into a text parameter, and the value of a room onto the structures around it. |
| `SelectTypes` | Finds the family types of the project — all of them, the placed ones or the ones visible in the current view — and selects the instances of the ticked types. |
| `SheetBrowser` | Shows every sheet under the folders of the active browser scheme, in the order the scheme sorts them, and copies the path of any row as Group → Subgroup → Sheet. |
| `CouplerMarks` | Finds the bar nearest to every picked coupler and writes the mark and the category of the bar's host into two named parameters of the coupler. |
| `MarkStructures` | Decides the kind of every column, beam, floor, wall, foundation and pile from its category and shape and writes the configured mark — a code per kind plus a suffix for the material family. |
| `CompareParameters` | Puts two elements side by side: lists the parameters of both, marks what differs, and copies the values of the source into the target — filling, never clearing. |
| `AreaRebarMarks` | Writes a chosen value of every area reinforcement into the same parameter of the bars that belong to it, keeping the values the bars already carry unless asked otherwise. |
| `ConcreteExport` | Writes the concrete structures of the project or of the selection into a comma-separated file: category, family, type, level, materials, volume, sizes and mark. |
| `PileData` | Numbers the picked piles from the chosen corner and writes the raft bottom, every pile bottom and the height cut off under the raft. |
| `CageMetres` | Works out the running metres of a rebar cage from its picked layout lines and its own parameters — longitudinal bars along the lines, inclined and transverse bars by their steps — and writes the total into the cage. |
| `ScheduleHeaderText` | Writes a value into the header of a schedule — taken from the elements the schedule lists or typed in the window — so a designation lives in one place and not two. |
| `WallLineLoads` | Puts the weight of the chosen walls onto the slabs as line loads at the bottom of every wall — a force, a moment, a load case and an optional comment — for the calculation model. |
| `AnalyticalVisibility` | Hides or shows the analytical model of the bearing structures in the model views without deleting anything: the analytical elements, their loads and their boundary conditions stay in the project. |
| `AnalyticalAlignment` | Brings analytical members to the height of a picked target member and analytical nodes onto its line, dragging the members of a node with it. |
| `AnalyticalExtension` | Extends the analytical model of the picked columns and walls up to the nearest floor or beam above them, so no member of the calculation model stops in the air. |
| `SheetExport` | Exports the ticked sheets as one PDF file per sheet, named after the sheet, into a chosen folder, and reports every file it produced. |
| `BarSketches` | Makes a sheet per picked bar: a view cropped to the bar and a note with its shape, nominal diameter, length and mark. |
| `ScheduleColumns` | Hides the columns of a schedule whose every row is empty or zero, and shows the hidden columns that carry values again, so the schedule follows its content. |

### Repository layout

```
src/ArKrPlugFR.Core                  domain rules, no Revit dependency
src/ArKrPlugFR.Revit                 Revit API adapters, command base class, compatibility
src/ArKrPlugFR.Ui                    WPF shell, theme, interop
src/ArKrPlugFR.Host                  Revit entry point and ribbon builder
src/ArKrPlugFR.Plugins.UnjoinElements   unjoin columns and beams from walls
src/ArKrPlugFR.Plugins.DuplicateTypeFinder  find and remove duplicate types and families
src/ArKrPlugFR.Plugins.SheetDuplicator   duplicate sheets with their views in place
src/ArKrPlugFR.Plugins.SheetCopy         copy the sheets of this project into other open projects
src/ArKrPlugFR.Plugins.ViewTemplateTransfer  copy views and view templates into another open project
src/ArKrPlugFR.Plugins.DuplicateViews      duplicate views, schedules and templates in one action
src/ArKrPlugFR.Plugins.HoleCuttingPlugin   cut an opening where a service crosses a wall
src/ArKrPlugFR.Plugins.WallOpeningsPlugin  cut openings in finish walls opposite doors and windows
src/ArKrPlugFR.Plugins.CutHolesInConcrete  cut openings where a linked model's services cross concrete
src/ArKrPlugFR.Plugins.RefreshAnnotations  make the tags of the active view recompute their text
src/ArKrPlugFR.Plugins.AlignToGrids        pull columns exactly onto their grids
src/ArKrPlugFR.Plugins.CountByMark         count structures per mark into matching bars
src/ArKrPlugFR.Plugins.ReassignRebar       attach rebar to the structure it belongs to
src/ArKrPlugFR.Plugins.SectionCollector    collect concrete structures by level, category and section
src/ArKrPlugFR.Plugins.AssignWorksets      put model elements into worksets by ordered rules
src/ArKrPlugFR.Plugins.SplitFloors         split a floor into its parts as separate floors
src/ArKrPlugFR.Plugins.TransferParameters  copy values onto sheets, into layer text and around rooms
src/ArKrPlugFR.Plugins.SelectTypes         find family types and select their instances
src/ArKrPlugFR.Plugins.SheetBrowser        show the browser tree of the sheets and copy a path
src/ArKrPlugFR.Plugins.CouplerMarks        mark couplers with the identity of the nearest bar
src/ArKrPlugFR.Plugins.MarkStructures       mark structures by their kind and material
src/ArKrPlugFR.Plugins.CompareParameters    compare two elements and copy the differing values
src/ArKrPlugFR.Plugins.AreaRebarMarks       fill the bars of every area reinforcement from its field
src/ArKrPlugFR.Plugins.ConcreteExport       export concrete structures as a data table
src/ArKrPlugFR.Plugins.PileData             number piles and write their elevations and cut heights
src/ArKrPlugFR.Plugins.CageMetres           measure a rebar cage along its layout lines
src/ArKrPlugFR.Plugins.ScheduleHeaderText   write a value into the header of a schedule
src/ArKrPlugFR.Plugins.WallLineLoads        line loads at the bottom of the chosen walls
src/ArKrPlugFR.Plugins.AnalyticalVisibility hide or show the analytical model in the views
src/ArKrPlugFR.Plugins.AnalyticalAlignment  align analytical members and nodes to a target
src/ArKrPlugFR.Plugins.AnalyticalExtension  extend analytical columns and walls up to a floor or beam
src/ArKrPlugFR.Plugins.SheetExport         export sheets as one PDF file per sheet
src/ArKrPlugFR.Plugins.BarSketches          make a sketch sheet per picked bar
src/ArKrPlugFR.Plugins.ScheduleColumns      hide the empty columns of a schedule
tests/ArKrPlugFR.Core.Tests          unit tests for the domain
installer/ArKrPlugFR.addin           Revit add-in manifest
scripts/pack.ps1                     build and assemble a deployment
scripts/smoke-ui.ps1                 load the WPF layer outside Revit
docs/                                architecture, authoring guide, specifications
```

### Licence

MIT — see [LICENSE](LICENSE).

---

## Русский

### Что это

ArKrPlugFR — набор небольших инструментов для Revit, которые убирают рутину из работы над
архитектурным или конструктивным разделом: соединение и разъединение элементов, выравнивание
геометрии по сетке, сбор видов и листов, чистка типов.

Название — это и есть охват: **Ар**хитектура + **КР** (конструкции) + **Plug**in.

### Зачем этот репозиторий

Инструменты выросли из задачи убрать рутину из реальных проектов. Каждый инструмент делает
одну вещь и его можно отключить, поэтому команда может внедрить один процесс, не меняя весь
шаблон.

Здесь лежат код, тесты и документация под MIT. Из сторонних библиотек используются только
универсальные открытые (Json.NET, xUnit) через NuGet, на условиях их собственных лицензий.

### Ключевые решения

| Область | Что показывает |
| --- | --- |
| Слоистая архитектура | Правила в сборке без Revit, доступ к API — в адаптере, интерфейс — отдельно, хост только собирает всё вместе |
| Мультиверсия | Одно дерево исходников собирается против API Revit 2020, 2022 и 2024; различия изолированы в одной папке |
| Тестируемая логика | Более 590 модульных тестов идут без Revit, потому что правила не обращаются к API |
| Лента из манифестов | Лента строится по файлам `*.plugin.json`: новый инструмент не требует пересборки хоста |
| Двуязычный интерфейс | Подписи, подсказки и отчёты берутся из манифеста по культуре, с безопасным запасным вариантом |
| Аккуратность в эксплуатации | Журналы с суточной ротацией и сроком хранения, данные пользователя в его профиле, ничего не пишется в папку Revit |
| Сборка без предупреждений | Сборка обязана давать ноль предупреждений; CI это проверяет |

### Архитектура

```
ArKrPlugFR.Core      предметная область: единицы, правила геометрии, анализ связей, журнал, манифесты
        ▲            (netstandard2.0, без ссылки на Revit API)
        │
ArKrPlugFR.Revit     адаптеры: доступ к Revit API, базовый класс команды, совместимость версий
        ▲            (net48)
        │
ArKrPlugFR.Ui        оболочка WPF, палитра и стили, интероп с окном-владельцем (net48)
        ▲
        │
ArKrPlugFR.Host      точка входа в Revit: читает манифесты, строит ленту (net48)
        ▲
        │
ArKrPlugFR.Plugins.* по одному проекту на инструмент: команда и манифест (net48)
```

Правила зависимостей:

- `Core` не ссылается на Revit, поэтому его логика тестируется и переносима.
- `Revit` — единственное место, где вызывается Revit API.
- Инструмент не ссылается на другой инструмент; общее поведение поднимается в `Core`, `Revit` или `Ui`.
- У хоста нет ссылок на инструменты на этапе компиляции: они находятся во время запуска.

Подробнее: [docs/architecture.md](docs/architecture.md).

### Поддерживаемые версии Revit

Revit 2020, 2022 и 2024 из одного дерева исходников. Reference-сборки API берутся из NuGet
(`Nice3point.Revit.Api.*`), поэтому проект собирается и пакуется на машине без установленного
Revit, а CI собирает все три цели.

```
dotnet build ArKrPlugFR.sln -p:RevitVersion=2020
dotnet build ArKrPlugFR.sln -p:RevitVersion=2022   # по умолчанию
dotnet build ArKrPlugFR.sln -p:RevitVersion=2024
```

Неподдерживаемая версия валит сборку с явным сообщением, а не собирается молча против чужого API.

Там, где API действительно различается, различие изолировано в
`src/ArKrPlugFR.Revit/Compatibility/`: например, у `ElementId` в Revit 2024 изменился тип
доступа с `int` на `long`.

Три инструмента объявлены только для Revit 2022 и 2024 — `AnalyticalAlignment`,
`AnalyticalExtension` и `SheetExport` — потому что используемых ими возможностей платформы в
Revit 2020 нет. Каждый манифест перечисляет поддерживаемые версии, и на версии, которой в
манифесте нет, хост не выводит кнопку инструмента.

### Требования

- .NET SDK 8 или 9 (для тестового проекта и сборки под .NET Framework)
- targeting pack .NET Framework 4.8 (ставится вместе с Visual Studio 2022)
- Revit **не** нужен для сборки, тестов и упаковки; он нужен только чтобы запускать инструменты

### Сборка, тесты, упаковка

```powershell
# собрать всё под версию по умолчанию (2022)
dotnet build ArKrPlugFR.sln

# модульные тесты (Revit не нужен)
dotnet test tests/ArKrPlugFR.Core.Tests

# проверить, что слой WPF загружается без Revit
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/smoke-ui.ps1

# собрать папку для развёртывания под конкретную версию Revit
./scripts/pack.ps1 -RevitVersion 2024
```

`pack.ps1` собирает плоскую поставку в `artifacts/deploy/<версия>/`: хост, общие сборки и каждый
инструмент вместе с манифестом.

### Установка

Скопируйте содержимое `artifacts/deploy/<версия>/` в

```
%APPDATA%\Autodesk\Revit\Addins\<версия>\
```

Манифест надстройки задаёт сборку относительно собственной папки, поэтому абсолютных путей нет.
Запустите Revit — появится вкладка **ArKrPlugFR** с зарегистрированными инструментами.

### Как добавить инструмент

1. Создайте проект `ArKrPlugFR.Plugins.<Имя>` со ссылками на `Core`, `Revit` и `Ui`.
2. Унаследуйте команду от `PluginCommand` и реализуйте `Run(PluginContext)`.
3. Положите рядом с сборкой манифест `<Имя>.plugin.json`.
4. Соберите и упакуйте. Лента подхватит инструмент без изменений в хосте.

Схема манифеста и пример: [docs/plugin-authoring.md](docs/plugin-authoring.md).

### Инструменты

Каждый инструмент — отдельная папка в `src/`, отдельный манифест и отдельные тесты.

| Инструмент | Что делает |
| --- | --- |
| `UnjoinElements` | Убирает вырезы колонн и балок в стенах одной отменяемой операцией и показывает пары, которые разъединить не удалось. |
| `DuplicateTypeFinder` | Сравнивает типы и семейства проекта, показывает, что повторяется, и удаляет только то, у чего нет размещённых экземпляров — с повторным подсчётом экземпляров непосредственно перед удалением. |
| `SheetDuplicator` | Дублирует выбранные листы текущего проекта вместе с видами, которые на них помещены, давая каждой копии свободный номер и имя с отметкой о копии. |
| `SheetCopy` | Копирует листы текущего проекта вместе с размещёнными на них видами в другие открытые проекты, подставляя свободные номера листов; на каждый проект — своя транзакция. |
| `ViewTemplateTransfer` | Копирует виды и шаблоны видов из текущего проекта в другой открытый проект, группируя их так, что можно отметить целые ветки одним нажатием. |
| `DuplicateViews` | Создаёт множество копий видов, ведомостей и шаблонов видов текущего проекта одним действием, с количеством копий для каждой строки и необязательным шаблоном вида для копий. |
| `HoleCuttingPlugin` | Вырезает прямоугольное отверстие в стене в месте пересечения с воздуховодом, трубой, кабельным лотком или кабелепроводом, с запасом с каждой стороны; ничего не загружается и временные элементы не создаются. |
| `WallOpeningsPlugin` | Вырезает проёмы в отделочных стенах, идущих вдоль выбранных, напротив расположенных в них дверей и окон, с запоминанием параметров между сеансами. |
| `CutHolesInConcrete` | Вырезает отверстия в бетонных стенах, перекрытиях и колоннах в местах, где через них проходит оборудование и коммуникации из связанной модели; ничего не загружается в проект, связь не изменяется. |
| `RefreshAnnotations` | Пересчитывает текст марок активного вида: с выделением — только марки выделенных элементов, без него — весь вид; каждая марка остаётся на своём месте. |
| `AlignToGrids` | Подтягивает выбранные колонны точно на выбранные оси: колонна встаёт на ближайшую ось в пределах захвата или на пересечение двух осей; элементы в группах пропускаются. |
| `CountByMark` | Считает конструкции выбранного вида с одинаковой маркой и записывает число в каждый стержень с совпадающей меткой основы; стержням без совпадения ничего не пишется. |
| `ReassignRebar` | Привязывает выбранную арматуру к выбранной колонне, стене, балке или перекрытию: обычные стержни меняют основу, арматура из IFC получает марку основы, стержни в системах пропускаются с отчётом. |
| `SectionCollector` | Собирает бетонные колонны, стены, перекрытия и балки 3D-вида, группирует их по этажам, категориям и сечениям из имён типоразмеров, выделяет все элементы и копирует отчёт. |
| `AssignWorksets` | Раскладывает элементы модели по рабочим наборам по упорядоченным правилам — категория, префикс семейства или типа, значение параметра — и создаёт отсутствующие наборы там же. |
| `SplitFloors` | Разделяет выбранные полы: каждый внешний контур плоского пола становится отдельным полом со своими проёмами, на том же уровне и с тем же типом, отметкой, признаком несущей конструкции и рабочим набором. |
| `TransferParameters` | Три переноса в одном окне: одно значение на отмеченные листы, слоистая конструкция стены или перекрытия в текстовый параметр и значение помещения на окружающие его конструкции. |
| `SelectTypes` | Находит типоразмеры семейств проекта — все, размещённые в модели или видимые на текущем виде — и выделяет экземпляры отмеченных типов. |
| `SheetBrowser` | Показывает листы под папками активной схемы браузера, в порядке схемы, и копирует путь любой строки в виде «Группа → Подгруппа → Лист». |
| `CouplerMarks` | Находит ближайший стержень для каждого выбранного соединителя и записывает марку и категорию его основы в два заданных параметра соединителя. |
| `MarkStructures` | Определяет вид каждой колонны, балки, перекрытия, стены, фундамента и сваи по категории и форме и пишет в «Марку» настроенный код вида плюс суффикс материала. |
| `CompareParameters` | Ставит два элемента рядом: показывает параметры обоих, отмечает различия и переносит значения источника в приёмник — только заполняет, никогда не очищает. |
| `AreaRebarMarks` | Записывает выбранное значение каждого армирования по площади в тот же параметр стержней, которые к нему относятся, сохраняя уже заполненные значения без явного указания. |
| `ConcreteExport` | Записывает бетонные конструкции проекта или выделения в файл с разделителями: категория, семейство, тип, этаж, материалы, объём, габариты и марка. |
| `PileData` | Нумерует выбранные сваи с заданного угла и записывает отметку низа плиты, отметку низа каждой сваи и высоту срубаемой под плитой части. |
| `CageMetres` | Считает погонные метры арматурного каркаса по выбранным линиям раскладки и его параметрам — продольные по линиям, наклонные и поперечные по шагам — и записывает итог в каркас. |
| `ScheduleHeaderText` | Записывает значение в заголовок спецификации — из элементов, которые она перечисляет, или введённое в окне, — чтобы обозначение жило в одном месте, а не в двух. |
| `WallLineLoads` | Прикладывает вес выбранных стен к плитам линейными нагрузками в низу каждой стены — сила, момент, вариант нагружений и необязательный комментарий — для расчётной модели. |
| `AnalyticalVisibility` | Скрывает или показывает аналитическую модель несущих конструкций на видах модели, ничего не удаляя: аналитические элементы, нагрузки и граничные условия остаются в проекте. |
| `AnalyticalAlignment` | Приводит аналитические элементы к высоте выбранного целевого элемента, а узлы — на его линию, подтягивая за узлом концы его стержней. |
| `AnalyticalExtension` | Продлевает аналитическую модель выбранных колонн и стен вверх до ближайшего перекрытия или балки, чтобы ни один элемент расчётной схемы не обрывался в пустоте. |
| `SheetExport` | Экспортирует отмеченные листы по одному PDF-файлу на лист, с именем по листу, в выбранную папку, и сообщает о каждом созданном файле. |
| `BarSketches` | Создаёт лист на каждый выбранный стержень: вид, обрезанный по стержню, и надпись с его формой, номинальным диаметром, длиной и маркой. |
| `ScheduleColumns` | Скрывает столбцы спецификации, в которых все строки пусты или равны нулю, и возвращает скрытые столбцы с данными, чтобы спецификация следовала своему содержанию. |

### Структура репозитория

```
src/ArKrPlugFR.Core                  правила предметной области, без зависимости от Revit
src/ArKrPlugFR.Revit                 адаптеры Revit API, базовый класс команды, совместимость
src/ArKrPlugFR.Ui                    оболочка WPF, тема, интероп
src/ArKrPlugFR.Host                  точка входа в Revit и построение ленты
src/ArKrPlugFR.Plugins.UnjoinElements   отсоединение колонн и балок от стен
src/ArKrPlugFR.Plugins.DuplicateTypeFinder  поиск и удаление дубликатов типов и семейств
src/ArKrPlugFR.Plugins.SheetDuplicator   дубликация листов вместе с их видами
src/ArKrPlugFR.Plugins.SheetCopy         копирование листов этого проекта в другие открытые проекты
src/ArKrPlugFR.Plugins.ViewTemplateTransfer  копирование видов и шаблонов видов в другой открытый проект
src/ArKrPlugFR.Plugins.DuplicateViews      дубликация видов, ведомостей и шаблонов видов
src/ArKrPlugFR.Plugins.HoleCuttingPlugin   вырезание отверстия в стене под коммуникацию
src/ArKrPlugFR.Plugins.WallOpeningsPlugin  проёмы в отделочных стенах напротив дверей и окон
src/ArKrPlugFR.Plugins.CutHolesInConcrete  отверстия там, где коммуникации связи проходят сквозь бетон
src/ArKrPlugFR.Plugins.RefreshAnnotations  пересчёт текста марок активного вида
src/ArKrPlugFR.Plugins.AlignToGrids        подтяжка колонн точно на оси
src/ArKrPlugFR.Plugins.CountByMark         подсчёт конструкций по марке в стержни
src/ArKrPlugFR.Plugins.ReassignRebar       привязка арматуры к своей конструкции
src/ArKrPlugFR.Plugins.SectionCollector    сбор бетонных конструкций по этажам, категориям и сечениям
src/ArKrPlugFR.Plugins.AssignWorksets      распределение элементов по рабочим наборам по правилам
src/ArKrPlugFR.Plugins.SplitFloors         разделение пола на отдельные полы по контурам
src/ArKrPlugFR.Plugins.TransferParameters  перенос значений на листы, в текст конструкции и вокруг помещений
src/ArKrPlugFR.Plugins.SelectTypes         поиск типоразмеров семейств и выделение их экземпляров
src/ArKrPlugFR.Plugins.SheetBrowser        дерево листов браузера проекта и копирование пути
src/ArKrPlugFR.Plugins.CouplerMarks        маркировка соединителей по ближайшему стержню
src/ArKrPlugFR.Plugins.MarkStructures       маркировка конструкций по виду и материалу
src/ArKrPlugFR.Plugins.CompareParameters    сравнение двух элементов и перенос различий
src/ArKrPlugFR.Plugins.AreaRebarMarks       заполнение стержней из их армирования по площади
src/ArKrPlugFR.Plugins.ConcreteExport       экспорт бетонных конструкций таблицей данных
src/ArKrPlugFR.Plugins.PileData             нумерация свай, отметки и высота сруба
src/ArKrPlugFR.Plugins.CageMetres           расчёт погонных метров арматурного каркаса
src/ArKrPlugFR.Plugins.ScheduleHeaderText   запись значения в заголовок спецификации
src/ArKrPlugFR.Plugins.WallLineLoads        линейные нагрузки в низу выбранных стен
src/ArKrPlugFR.Plugins.AnalyticalVisibility скрытие и показ аналитической модели на видах
src/ArKrPlugFR.Plugins.AnalyticalAlignment  выравнивание аналитических элементов и узлов по цели
src/ArKrPlugFR.Plugins.AnalyticalExtension  продление аналитических колонн и стен до перекрытия или балки
src/ArKrPlugFR.Plugins.SheetExport         экспорт листов по одному PDF-файлу на лист
src/ArKrPlugFR.Plugins.BarSketches          лист-эскиз на каждый выбранный стержень
src/ArKrPlugFR.Plugins.ScheduleColumns      скрытие пустых столбцов спецификации
tests/ArKrPlugFR.Core.Tests          модульные тесты предметной области
installer/ArKrPlugFR.addin           манифест надстройки Revit
scripts/pack.ps1                     сборка и упаковка развёртывания
scripts/smoke-ui.ps1                 загрузка слоя WPF без Revit
docs/                                архитектура, руководство по инструментам, спецификации
```

### Лицензия

MIT — см. [LICENSE](LICENSE).
