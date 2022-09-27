# DeviceIO Plugin

The DeviceIO plugin allows RaceDirector to generate keyboard input in game.

## Configuration

For this to work, it has to be configured to send the right key presses for the desired game
action (see [documentation](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys)
on how to define special keys, sequences and combinations in `KeyMappings`).

Games are particularly susceptible to timing between key presses. This plugin comes
preconfigured with safe values for most hardware. On most modern systems the `WaitBetweenKeys`
configuration can be decreased to values around 5ms (see below).

```json
{
  "RaceDirector.DeviceIO.Plugin": {
    "Enabled": true,
    "KeyMappings": {
      "PitMenuOpen": "p",
      "PitMenuUp": "{UP}",
      "PitMenuDown": "{DOWN}",
      "PitMenuLeft": "{LEFT}",
      "PitMenuRight": "{RIGHT}",
      "PitMenuSelect": ""
    },
    "WaitBetweenKeys": "00:00:00.015"
  }
}
```
