# RaceDirector

![build](https://github.com/OpenSimTools/RaceDirector/actions/workflows/ci.yaml/badge.svg)

At the moment the software is a replacement for RaceRoom's closed source
[Dash](https://github.com/sector3studios/webhud/blob/master/dist/dash.zip) command. The major
difference is that it can export telemetry from other games (see [supported games](docs/Games.md)).

It is a console application that every 15ms will read the memory mapped file that RaceRoom exposes,
only when the game is running. It will then expose that data on a WebSocket. The supported fields
are those required by [RaceRoom's WebHud](https://github.com/sector3studios/webhud), and can be found
[here](docs/Plugins/HUD/README.md).

Please note that
[OtterHUD](https://forum.sector3studios.com/index.php?threads/otterhud-a-custom-webhud-with-additional-features.13152/)
(closed source) requires more fields than *currently* supported.

## Build

At the moment there is no installer, so the software needs to be built: 

```
dotnet test
dotnet publish
```

`RaceDirector.exe` will then be found in `src\RaceDirector\bin\Debug\net6.0\publish`.

## Run

Since RaceRoom's WebHud requires a specific older minor version of the memory mapped files,
it will be necessary to override the default one using a command line argument or editing
`application.conf` in the same directory.

```
.\RaceDirector.exe --RaceDirector.Plugin.HUD.Plugin:DashboardServer:R3EDash:MinorVersion=8
```

To see it in action, RaceRoom can be configured to use the WebHud or a Web Browser window can be
pointed to https://sector3studios.github.io/webhud/dist/.

Alternatively, the telemetry can be seen connecting to the WebSocket port
(`ws://localhost:8070/r3e`) using a client or a
[Web browser test page](http://livepersoninc.github.io/ws-test-page/)):
```
{"VersionMajor":2,"VersionMinor":8,"GameInMenus":1,...}
{"VersionMajor":2,"VersionMinor":8,"GameInMenus":1,...}
...
```
