# PitCrew Plugin

The PitCrew plugin provides remote pit stop control similar to what is offered by
[RST PitManager](https://racingsimtools.com/add-ons) ([video](https://youtu.be/HaRj2sznYLA))
or [ACC Live](https://accdrive.com/). Telemetry is sent to a server, where other team mates
can see it. Incoming pit strategy requests are sent to the game.

## Client

### Configuration

This plugin is disabled by default, and it needs to be enabled before use, after configuring
the server endpoint. This can be done in the dedicated section of `application.conf`:

```json
{
  "RaceDirector.PitCrew.Plugin": {
    "Enabled": true,
    "ServerUrl": "ws://myserver:8042/",
    "TelemetryThrottling": "00:00:01.000",
    "MaxMenuNavigationWait": "00:00:00.300"
  }
}
```

It can also be overridden via the command line:

```
--RaceDirector.PitCrew.Plugin:Enabled=true --RaceDirector.PitCrew.Plugin:ServerUrl=ws://myserver:8042/
```

The only necessary configuration is the `ServerUrl`. Other parameters have sensible defaults:
- `TelemetryThrottling` is how frequently the client will send telemetry to the server.
- `MaxMenuNavigationWait` is needed because some games don't expose the selected item in the pit
   menu and we need to use some heuristics to guess where we are. This parameter determines the
   maximum time that we'll wait for a change before assuming that nothing observable happened.
   It is important for this value to be large enough to see some telemetry data. By default it
   is set to 300ms, with `PollingInterval` and `WaitBetweenKeys` both 15ms.

PitCrew relies on the [DeviceIO plugin](DeviceIO.md) to control the pit menu. Make sure that it
is configured correctly.

## Server

A separate package can be downloaded from the same location as RaceDirector and run in a similar
way. Once the server is running, pushed telemetry and pit strategies are broadcasted to all
connected clients.

The Web interface can be accessed:
 - From the `/ui` directory on the server itself (e.g. `http://myserver:8042/ui/`). It will try
   to connect automatically to the same server, unless otherwise specified. 
 - On the [PitCrew section](https://opensimtools.github.io/RaceDirector/PitCrew/) of
   RaceDirector's Web pages. Note that the server will have to support encrypted connections
   to use this method (configuration is out of scope for this guide).

Auto-connect can also be triggered using the `connect` parameter (e.g.
`https://opensimtools.github.io/RaceDirector/PitCrew/?connect=wss://myserver:8042/`).

### Configuration

The default port is 8042, and it can be changed in the `appsettings.json` configuration file.
Serving the UI can also be disabled if that is not required.

```json
{
    "Port": 8042,
    "ServeUI": true
}
```

## Internals

WebSocket messages can be seen connecting to the same URL as the client.

The received telemetry messages look like this:

```json
{
  "Telemetry": {
    "FuelLeftL": 12.34,
    "TireSet": 1,
    "FrontTires": {
      "Compound": "Dry",
      "Left": {
        "PressureKpa": 186.16,
        "Wear": 0.72
      },
      "Right": {
        "PressureKpa": 184.78,
        "Wear": 0.68
      }
    },
    "RearTires": {
      "Compound": "Dry",
      "Left": {
        "PressureKpa": 187.54,
        "Wear": 0.71
      },
      "Right": {
        "PressureKpa": 186.16,
        "Wear": 0.70
      }
    },
    "PitMenu": {
      "FuelToAddL": 56,
      "TireSet": 2,
      "FrontTires": {
        "Compound": "Dry",
        "LeftPressureKpa": 182.13,
        "RightPressureKpa": 180.75
      },
      "RearTires": {
        "Compound": "Dry",
        "LeftPressureKpa": 183.51,
        "RightPressureKpa": 182.13
      }
    }
  }
}
```

The same connection can be used to set the pit strategy in game by sending a message like this:
```json
{
  "PitStrategyRequest": {
    "FuelToAddL": 42,
    "TireSet": 3,
    "FrontTires": {
      "Compound": "Dry",
      "LeftPressureKpa": 182.13,
      "RightPressureKpa": 180.75
    },
    "RearTires": {
      "Compound": "Dry",
      "LeftPressureKpa": 183.51,
      "RightPressureKpa": 182.13
    }
  }
}
```

 - Depending on game or racing series, it might not be possible to change only tyres on one axle,
   have different compounds, etc. Fields or combinations not supported by the game will fail to
   configure the pit menu.
 - If `FuelToAddL` is not present or 0, the car will not be refuelled.
 - If `Tires` is not present, tires will not be changed.
   - If `Set` is not present, the default will be used.
   - If `Front` or `Rear` is present, tires on that axle will be changed.
     - If `Compound` is not present, it will use the current one.
     - If `LeftPressureKpa` or `RightPressureKpa` are not present, it will try to use the default
       ones and might produce unexpected behaviours in some games.
