# RaceDirector

Currently the software is a console application that starts monitoring the
memory mapped file that RaceRoom exposes whenever the game is running. It
will read the Simulation Time from it twice per second, expose it on a WebSocket like
[RaceRoom's Dash](https://github.com/sector3studios/webhud/blob/master/dist/dash.zip)
does (`ws://localhost:8070/r3e`) and print it on console.

## Example

WebSocket (using a [Web browser test page](http://livepersoninc.github.io/ws-test-page/)):
```
{"Player":{"GameSimulationTime":0}}
...
{"Player":{"GameSimulationTime":0}}
{"Player":{"GameSimulationTime":0.1625}}
{"Player":{"GameSimulationTime":0.665}}
{"Player":{"GameSimulationTime":1.1575}}
{"Player":{"GameSimulationTime":1.6525}}
{"Player":{"GameSimulationTime":2.16}}
{"Player":{"GameSimulationTime":2.6525}}
{"Player":{"GameSimulationTime":3.1625}}
```

Console:
```
Starting pipeline
> 0
...
> 0
> 0.1625
> 0.665
> 1.1575
> 1.6525
> 2.16
> 2.6525
> 3.1625
```

## Build

```
dotnet test
dotnet publish
```

## Run

```
src\RaceDirector\bin\Debug\net5.0\publish\RaceDirector.exe
```
