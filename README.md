# RaceDirector

Currently the software is a console application that starts monitoring the
memory mapped file that RaceRoom exposes whenever the game is running. It
will read and print the Simulation Time from it twice per second.

Example:
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
