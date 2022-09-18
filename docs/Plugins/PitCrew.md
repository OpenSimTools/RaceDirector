# PitCrew Plugin

The PitCrew plugin provides remote pit stop control similar to what is offered by
[RST PitManager](https://racingsimtools.com/add-ons) ([video](https://youtu.be/HaRj2sznYLA))
or [ACC Live](https://accdrive.com/). Telemetry is sent to a server, where other team mates
can see it. Incoming pit strategy requests are sent to the game.

## Configuration

This plugin is disabled by default, and it needs to be enabled before use, after configuring
the server endpoint. This can be done in the dedicated section of `application.conf`:

```json
{
  "RaceDirector.PitCrew.Plugin": {
    "Enabled": true,
    "ServerUrl": "ws://myserver:8042/"
  }
}
```

It can also be overridden via the command line:

```
--RaceDirector.PitCrew.Plugin:Enabled=true --RaceDirector.PitCrew.Plugin:ServerUrl=ws://myserver:8042/
```

PitCrew relies on the [DeviceIO plugin](DeviceIO.md) to control the pit menu. Make sure that it
is configured correctly.

## Server

A separate package can be downloaded from the same location as RaceDirector and run in a similar
way. The default port is 8042.

Once the server is running, telemetry pushed by RaceDirector will be broadcasted to all
connected clients. Currently there is no UI, so a WebSocket client or a
[Web browser test page](http://livepersoninc.github.io/ws-test-page/) is required to access it
from the same URL where the client is pushing telemetry to.

The received telemetry messages look like this:
```json lines
{
  "Telemetry": {
    "FuelLeftL": 12.34,
    "TirePressuresKpa": {
      "FrontLeft": 186.16,
      "FrontRight": 184.78,
      "RearLeft": 187.54,
      "RearRight": 186.16
    },
    "PitMenu": {
      "FuelToAddL":56
    }
  }
}
...
```

The same connection can be used to set the pit strategy in game by sending a message like this:
```json
{
  "PitStrategyRequest": {
    "FuelToAddL": 42,
    "TireSet": 1,
    "FrontTires": {
      "LeftPressureKpa": 182.13,
      "RightPressureKpa": 180.75
    },
    "RearTires": {
      "LeftPressureKpa": 183.51,
      "RightPressureKpa": 182.13
    }
  }
}
```

Note:
 - Fields or combinations not supported by the game will be ignored. Depending on game or racing series, it might not be possible to change only tyres on one axle.
 - If `FuelToAddL` is not present or 0, the car will not be refuelled.
 - If tire `TireSet` is the current one, tires will not be changed.
 - If `FrontTires` or `RearTires` are not present, tires on that axle will not be changed.