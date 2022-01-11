# RaceDirector

At the moment the software is a replacement for RaceRoom's closed source
[Dash](https://github.com/sector3studios/webhud/blob/master/dist/dash.zip)
command.

It is a console application that will read every 15ms the memory mapped file that
RaceRoom exposes, only when the game is running. It will then expose that data on
a WebSocket (`ws://localhost:8070/r3e`). The supported fields are those required
by [RaceRoom's WebHud](https://github.com/sector3studios/webhud), and can be found
[here](docs/HUD/README.md).

To see it in action, RaceRoom can be configured to use the WebHud or a Web Browser
window can be pointed to https://sector3studios.github.io/webhud/dist/.

Please note that [OtterHUD](https://forum.sector3studios.com/index.php?threads/otterhud-a-custom-webhud-with-additional-features.13152/) (closed source) requires more fields than *currently* supported.

## Build

```
dotnet test
dotnet publish
```

## Run

```
src\RaceDirector\bin\Release\net5.0\publish\RaceDirector.exe
```

## Example

WebSocket (using a [Web browser test page](http://livepersoninc.github.io/ws-test-page/)):
```
{"VersionMajor":2,"VersionMinor":11,"GameInMenus":1,...}
{"VersionMajor":2,"VersionMinor":11,"GameInMenus":1,...}
...
```
