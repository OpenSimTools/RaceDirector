# PitCrew Plugin

The PitCrew plugin allows telemetry to be sent to a server, where other team mates can see it.
It provides functionality similar to [RST PitManager](https://racingsimtools.com/add-ons)
([video](https://youtu.be/HaRj2sznYLA)).

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

## Server

A separate package can be downloaded from the same location as RaceDirector and run in a similar
way. The default port is 8042.

Once the server is running, telemetry pushed by RaceDirector will be broadcasted to all
connected clients. Currently there is no UI, so a WebSocket client or a
[Web browser test page](http://livepersoninc.github.io/ws-test-page/) is required to access it
from the same URL where the client is pushing telemetry to.

It should look like this:
```
{"Fuel":{"Left":12.34}}
{"Fuel":{"Left":12.33}}
...
```