# Games Support

## Telemetry

| Field                                                 | R3E     | ACC         |
|-------------------------------------------------------|---------|-------------|
| GameState                                             | ✓       | ✓ [^approx] |
| UsingVR                                               | ✓       | ✗           |
| Event.Track.SectorsEnd                                | ✓       | ✗           |
| Event.FuelRate                                        | ✓       | ✓           |
| Session.Type                                          | ✓       | ✓           |
| Session.Phase                                         | ✓       | ✗           |
| Session.Length                                        | ✓       |             |
| Session.Requirements.MandatoryPitStops                | ✓       |             |
| Session.Requirements.MandatoryPitRequirements         |         |             |
| Session.Requirements.PitWindow                        | ✓       |             |
| Session.PitSpeedLimit                                 | ✓       |             |
| Session.PitLaneOpen                                   | ✓       |             |
| Session.ElapsedTime                                   | ✓       |             |
| Session.TimeRemaining                                 | ✓       |             |
| Session.WaitTime                                      | ✓       |             |
| Session.StartLights.Colour                            | ✓       |             |
| Session.StartLights.Lit                               | ✓       |             |
| Session.BestLap                                       |         |             |
| Session.BestSectors.Individual                        |         |             |
| Session.BestSectors.Cumulative                        |         |             |
| Session.Flags.Track                                   |         |             |
| Session.Flags.Sectors                                 | ✓       |             |
| Session.Flags.Leader                                  |         |             |
| Vehicles[].Id                                         | ✓       |             |
| Vehicles[].ClassPerformanceIndex                      | ✓       |             |
| Vehicles[].RacingStatus                               | ✓       |             |
| Vehicles[].EngineType                                 |         |             |
| Vehicles[].ControlType                                |         |             |
| Vehicles[].Position                                   |         |             |
| Vehicles[].PositionClass                              | ✓       |             |
| Vehicles[].GapAhead                                   | ✓       |             |
| Vehicles[].GapBehind                                  | ✓       |             |
| Vehicles[].CompletedLaps                              | ✓       |             |
| Vehicles[].LapValid                                   | ✓       |             |
| Vehicles[].CurrentLapTime.Overall                     |         |             |
| Vehicles[].CurrentLapTime.Sectors.Individual          |         |             |
| Vehicles[].CurrentLapTime.Sectors.Cumulative          |         |             |
| Vehicles[].PreviousLapTime                            |         |             |
| Vehicles[].BestLapTime.Overall                        |         |             |
| Vehicles[].BestLapTime.Sectors.Individual             |         |             |
| Vehicles[].BestLapTime.Sectors.Cumulative             |         |             |
| Vehicles[].BestSectors                                |         |             |
| Vehicles[].CurrentLapDistance                         | ✓       |             |
| Vehicles[].Location                                   | ✓       |             |
| Vehicles[].Orientation                                |         |             |
| Vehicles[].Speed                                      |         |             |
| Vehicles[].CurrentDriver.Name                         | Partial |             |
| Vehicles[].Pit.StopsDone                              | ✓       |             |
| Vehicles[].Pit.MandatoryStopsDone                     | ✓       |             |
| Vehicles[].Pit.PitLanePhase                           |         |             |
| Vehicles[].Pit.PitLaneTime                            |         |             |
| Vehicles[].Pit.PitStallTime                           |         |             |
| Vehicles[].Penalties                                  |         |             |
| Vehicles[].Flags.Green                                | ✗       |             |
| Vehicles[].Flags.Blue                                 | ✗       |             |
| Vehicles[].Flags.Yellow                               | ✗       |             |
| Vehicles[].Flags.White                                | ✗       |             |
| Vehicles[].Flags.Chequered                            | ✗       |             |
| Vehicles[].Flags.Black                                | ✗       |             |
| Vehicles[].Flags.BlackWhite                           | ✗       |             |
| FocusedVehicle.Id                                     | ✓       |             |
| FocusedVehicle.ClassPerformanceIndex                  | ✓       |             |
| FocusedVehicle.RacingStatus                           | ✓       |             |
| FocusedVehicle.EngineType                             | ✓       |             |
| FocusedVehicle.ControlType                            | ✓       |             |
| FocusedVehicle.Position                               |         |             |
| FocusedVehicle.PositionClass                          | ✓       |             |
| FocusedVehicle.GapAhead                               |         |             |
| FocusedVehicle.GapBehind                              |         |             |
| FocusedVehicle.CompletedLaps                          | ✓       |             |
| FocusedVehicle.LapValid                               | ✓       |             |
| FocusedVehicle.CurrentLapTime.Overall                 | ✓       |             |
| FocusedVehicle.CurrentLapTime.Sectors.Individual      |
| FocusedVehicle.CurrentLapTime.Sectors.Cumulative      | ✓       |             |
| FocusedVehicle.PreviousLapTime                        |         |             |
| FocusedVehicle.BestLapTime.Overall                    | ✓       |             |
| FocusedVehicle.BestLapTime.Sectors.Individual         |         |             |
| FocusedVehicle.BestLapTime.Sectors.Cumulative         | ✓       |             |
| FocusedVehicle.BestSectors                            |         |             |
| FocusedVehicle.CurrentLapDistance                     | ✓       |             |
| FocusedVehicle.Location                               | ✓       |             |
| FocusedVehicle.Orientation                            | ✓       |             |
| FocusedVehicle.Speed                                  | ✓       |             |
| FocusedVehicle.CurrentDriver.Name                     | ✓       |             |
| FocusedVehicle.Pit.StopsDone                          | ✓       |             |
| FocusedVehicle.Pit.MandatoryStopsDone                 | ✓       |             |
| FocusedVehicle.Pit.PitLanePhase                       | ✓       |             |
| FocusedVehicle.Pit.PitLaneTime                        | ✓       |             |
| FocusedVehicle.Pit.PitStallTime                       | ✓       |             |
| FocusedVehicle.Penalties                              | ✓       |             |
| FocusedVehicle.Inputs.Throttle                        | ✓       |             |
| FocusedVehicle.Inputs.Brake                           | ✓       |             |
| FocusedVehicle.Inputs.Clutch                          | ✓       |             |
| FocusedVehicle.Flags.Green                            | ✓       |             |
| FocusedVehicle.Flags.Blue                             | ✓       |             |
| FocusedVehicle.Flags.Yellow                           | ✓       |             |
| FocusedVehicle.Flags.White                            | ✓       |             |
| FocusedVehicle.Flags.Chequered                        | ✓       |             |
| FocusedVehicle.Flags.Black                            | ✓       |             |
| FocusedVehicle.Flags.BlackWhite                       | ✓       |             |
| Player.RawInputs.Throttle                             | ✓       |             |
| Player.RawInputs.Brake                                | ✓       |             |
| Player.RawInputs.Clutch                               | ✓       |             |
| Player.RawInputs.Steering                             | ✓       |             |
| Player.RawInputs.SteerWheelRange                      | ✓       |             |
| Player.DrivingAids.Abs                                | ✓       |             |
| Player.DrivingAids.Tc                                 | ✓       |             |
| Player.DrivingAids.Esp                                | ✓       |             |
| Player.DrivingAids.Countersteer                       | ✓       |             |
| Player.DrivingAids.Cornering                          | ✓       |             |
| Player.VehicleSettings.EngineMap                      |         |             |
| Player.VehicleSettings.EngineBrakeReduction           |         |             |
| Player.VehicleDamage.AerodynamicsPercent              | ✓       |             |
| Player.VehicleDamage.EnginePercent                    | ✓       |             |
| Player.VehicleDamage.SuspensionPercent                | ✓       |             |
| Player.VehicleDamage.TransmissionPercent              | ✓       |             |
| Player.Tyres[][].Pressure                             | ✓       | ✓           |
| Player.Tyres[][].Dirt                                 | ✓       | ✓           |
| Player.Tyres[][].Grip                                 | ✓       |             |
| Player.Tyres[][].Wear                                 | ✓       | ✓           |
| Player.Tyres[][].Temperatures.CurrentTemperatures[][] | ✓       | ✓           |
| Player.Tyres[][].Temperatures.OptimalTemperature      | ✓       |             |
| Player.Tyres[][].Temperatures.ColdTemperature         | ✓       |             |
| Player.Tyres[][].Temperatures.HotTemperature          | ✓       |             |
| Player.Tyres[][].BrakeTemperatures.CurrentTemperature | ✓       | ✓           |
| Player.Tyres[][].BrakeTemperatures.OptimalTemperature | ✓       |             |
| Player.Tyres[][].BrakeTemperatures.ColdTemperature    | ✓       |             |
| Player.Tyres[][].BrakeTemperatures.HotTemperature     | ✓       |             |
| Player.TireSet                                        | ✗       | ✓           |
| Player.Fuel.Max                                       | ✓       |             |
| Player.Fuel.Left                                      | ✓       | ✓           |
| Player.Fuel.PerLap                                    | ✓       |             |
| Player.Engine.Speed                                   | ✓       |             |
| Player.Engine.UpshiftSpeed                            | ✓       |             |
| Player.Engine.MaxSpeed                                | ✓       |             |
| Player.CgLocation                                     | ✓       |             |
| Player.Orientation                                    |         |             |
| Player.LocalAcceleration                              | ✓       |             |
| Player.ClassBestLap                                   |         |             |
| Player.ClassBestSectors.Individual                    | ✓       |             |
| Player.ClassBestSectors.Cumulative                    |         |             |
| Player.PersonalBestSectors.Individual                 | ✓       |             |
| Player.PersonalBestSectors.Cumulative                 |         |             |
| Player.PersonalBestDelta                              | ✓       |             |
| Player.Drs.Available                                  | ✓       |             |
| Player.Drs.Engaged                                    | ✓       |             |
| Player.Drs.ActivationsLeft.Value                      | ✓       |             |
| Player.Drs.ActivationsLeft.Total                      | ✓       |             |
| Player.PushToPass.Available                           | ✓       |             |
| Player.PushToPass.Engaged                             | ✓       |             |
| Player.PushToPass.ActivationsLeft.Value               | ✓       |             |
| Player.PushToPass.ActivationsLeft.Total               | ✓       |             |
| Player.PushToPass.EngagedTimeLeft                     | ✓       |             |
| Player.PushToPass.WaitTimeLeft                        | ✓       |             |
| Player.PitMenu.FocusedItem                            | ✓       | ✗           |
| Player.PitMenu.FuelToAdd                              | ✗       | ✓           |
| Player.PitMenu.SelectedItems                          | ✓       | ✗           |
| Player.PitStopStatus                                  | ✓       |             |
| Player.Warnings.IncidentPoints                        |         |             |
| Player.Warnings.BlueFlagWarnings                      | ✓       |             |
| Player.Warnings.GiveBackPositions                     | ✓       |             |
| Player.OvertakeAllowed                                | ✓       |             |
| Player.PitMenu.FocusedItem                            | ✓       | ✗           |
| Player.PitMenu.SelectedItems                          | ✓       | ✗           |
| Player.PitMenu.FuelToAdd                              | ✗       | ✓           |
| Player.PitMenu.StrategyTireSet                        | ✗       | ✓           |
| Player.PitMenu.TireSet                                | ✗       | ✓           |
| Player.PitMenu.TirePressures                          | ✗       | ✓           |

[^approx]: Approximated