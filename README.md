# RaceDirector

![build](https://github.com/OpenSimTools/RaceDirector/actions/workflows/ci.yaml/badge.svg)

RaceDirector is the sim racer's Swiss Army knife.

The main software implements a pluggable architecture and a default set of components for sim
racing. Plugins can be written to extend it with new functionality. In this early development
phase, some plugins come bundled with it:
 - [HUD](./docs/Plugins/HUD.md)
 - [PitCrew](./docs/Plugins/PitCrew.md)

## Install

At the moment there is no installer.

The package can be found in the "Artifacts" section of the
[latest build](https://github.com/OpenSimTools/RaceDirector/actions/workflows/ci.yaml?query=event%3Apush).

## Run

Execute `RaceDirector.exe` from the installation directory.

The software can be configured editing the `application.json` file in the same location.

## Build

.NET 6 is required to build, run tests and publish the artefacts: 

```
dotnet test
dotnet publish
```

`RaceDirector.exe` will then be found in `src\RaceDirector\bin\Debug\net6.0\publish`.
