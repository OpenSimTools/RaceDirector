# RaceDirector HUD

The HUD plugin implements support for several ways of exposing telemetry to applications and dashboards.

## RaceRoom Dash Protocol

It is an alternative to [RaceRoom's Dash](https://github.com/sector3studios/webhud/blob/master/dist/dash.zip), exposing a WebSocket with telemetry information (on `ws://localhost:8070/r3e`). The advantage is that it works with all simulators supported by RaceDirector, and not only RaceRoom.

| Field | Supported |
| --- | --- |
| VersionMajor | ✓ |
| VersionMinor | ✓ |
| AllDriversOffset | |
| DriverDataSize | |
| GamePaused | ✓ |
| GameInMenus | ✓ |
| GameInReplay | ✓ |
| GameUsingVr | ✓ |
| GameUnused1 | ✗ |
| Player.GameSimulationTicks | |
| Player.GameSimulationTime | |
| Player.Position.X | ✓ |
| Player.Position.Y | ✓ |
| Player.Position.Z | ✓ |
| Player.Velocity.X | |
| Player.Velocity.Y | |
| Player.Velocity.Z | |
| Player.LocalVelocity.X | |
| Player.LocalVelocity.Y | |
| Player.LocalVelocity.Z | |
| Player.Acceleration.X | |
| Player.Acceleration.Y | |
| Player.Acceleration.Z | |
| Player.LocalAcceleration.X | |
| Player.LocalAcceleration.Y | |
| Player.LocalAcceleration.Z | |
| Player.Orientation.X | |
| Player.Orientation.Y | |
| Player.Orientation.Z | |
| Player.Rotation.X | |
| Player.Rotation.Y | |
| Player.Rotation.Z | |
| Player.AngularAcceleration.X | |
| Player.AngularAcceleration.Y | |
| Player.AngularAcceleration.Z | |
| Player.AngularVelocity.X | |
| Player.AngularVelocity.Y | |
| Player.AngularVelocity.Z | |
| Player.LocalAngularVelocity.X | |
| Player.LocalAngularVelocity.Y | |
| Player.LocalAngularVelocity.Z | |
| Player.LocalGforce.X | ✓ |
| Player.LocalGforce.Y | ✓ |
| Player.LocalGforce.Z | ✓ |
| Player.SteeringForce | |
| Player.SteeringForcePercentage | |
| Player.EngineTorque | |
| Player.CurrentDownforce | |
| Player.Voltage | |
| Player.ErsLevel | |
| Player.PowerMguH | |
| Player.PowerMguK | |
| Player.TorqueMguK | |
| Player.SuspensionDeflection.FrontLeft | |
| Player.SuspensionDeflection.FrontRight | |
| Player.SuspensionDeflection.RearLeft | |
| Player.SuspensionDeflection.RearRight | |
| Player.SuspensionVelocity.FrontLeft | |
| Player.SuspensionVelocity.FrontRight | |
| Player.SuspensionVelocity.RearLeft | |
| Player.SuspensionVelocity.RearRight | |
| Player.Camber.FrontLeft | |
| Player.Camber.FrontRight | |
| Player.Camber.RearLeft | |
| Player.Camber.RearRight | |
| Player.RideHeight.FrontLeft | |
| Player.RideHeight.FrontRight | |
| Player.RideHeight.RearLeft | |
| Player.RideHeight.RearRight | |
| Player.FrontWingHeight | |
| Player.FrontRollAngle | |
| Player.RearRollAngle | |
| Player.thirdSpringSuspensionDeflectionFront | |
| Player.thirdSpringSuspensionVelocityFront | |
| Player.thirdSpringSuspensionDeflectionRear | |
| Player.thirdSpringSuspensionVelocityRear | |
| Player.Unused1 | ✗ |
| TrackName | |
| LayoutName | |
| TrackId | |
| LayoutId | |
| LayoutLength | ✓ |
| SectorStartFactors.Sector1 | ✓ |
| SectorStartFactors.Sector2 | ✓ |
| SectorStartFactors.Sector3 | ✓ |
| RaceSessionLaps.Race1 | |
| RaceSessionLaps.Race2 | |
| RaceSessionLaps.Race3 | |
| RaceSessionMinutes.Race1 | |
| RaceSessionMinutes.Race2 | |
| RaceSessionMinutes.Race3 | |
| EventIndex | |
| SessionType | ✓ |
| SessionIteration | |
| SessionLengthFormat | |
| SessionPitSpeedLimit | ✓ |
| SessionPhase | ✓ |
| StartLights | ✓ |
| TireWearActive | |
| FuelUseActive | ✓ |
| NumberOfLaps | ✓ |
| SessionTimeDuration | |
| SessionTimeRemaining | ✓ |
| MaxIncidentPoints | |
| EventUnused2 | ✗ |
| PitWindowStatus | ✓ |
| PitWindowStart | ✓ |
| PitWindowEnd | ✓ |
| InPitlane | ✓ |
| PitMenuSelection | |
| PitMenuState.Preset | |
| PitMenuState.Penalty | |
| PitMenuState.Driverchange | |
| PitMenuState.Fuel | |
| PitMenuState.FrontTires | |
| PitMenuState.RearTires | |
| PitMenuState.FrontWing | |
| PitMenuState.RearWing | |
| PitMenuState.Suspension | |
| PitMenuState.ButtonTop | |
| PitMenuState.ButtonBottom | |
| PitState | ✓ |
| PitTotalDuration | ✓ |
| PitElapsedTime | ✓ |
| PitAction | ✓ |
| NumPitstopsPerformed | |
| PitMinDurationTotal | |
| PitMinDurationLeft | |
| Flags.Yellow | ✓ |
| Flags.YellowCausedIt | |
| Flags.YellowOvertake | |
| Flags.YellowPositionsGained | |
| Flags.SectorYellow.Sector1 | |
| Flags.SectorYellow.Sector2 | |
| Flags.SectorYellow.Sector3 | |
| Flags.ClosestYellowDistanceIntoTrack | |
| Flags.Blue | ✓ |
| Flags.Black | ✓ |
| Flags.Green | ✓ |
| Flags.Checkered | ✓ |
| Flags.White | ✓ |
| Flags.BlackAndWhite | ✓ |
| Position | |
| PositionClass | ✓ |
| FinishStatus | |
| CutTrackWarnings | |
| Penalties.DriveThrough | ✓ |
| Penalties.StopAndGo | ✓ |
| Penalties.PitStop | ✓ |
| Penalties.TimeDeduction | ✓ |
| Penalties.SlowDown | ✓ |
| NumPenalties | |
| CompletedLaps | ✓ |
| CurrentLapValid | ✓ |
| TrackSector | |
| LapDistance | ✓ |
| LapDistanceFraction | ✓ |
| LapTimeBestLeader | |
| LapTimeBestLeaderClass | |
| SectorTimesSessionBestLap.Sector1 | |
| SectorTimesSessionBestLap.Sector2 | |
| SectorTimesSessionBestLap.Sector3 | |
| LapTimeBestSelf | ✓ |
| SectorTimesBestSelf.Sector1 | ✓ |
| SectorTimesBestSelf.Sector2 | ✓ |
| SectorTimesBestSelf.Sector3 | ✓ |
| LapTimePreviousSelf | |
| SectorTimesPreviousSelf.Sector1 | |
| SectorTimesPreviousSelf.Sector2 | |
| SectorTimesPreviousSelf.Sector3 | |
| LapTimeCurrentSelf | ✓ |
| SectorTimesCurrentSelf.Sector1 | ✓ |
| SectorTimesCurrentSelf.Sector2 | ✓ |
| SectorTimesCurrentSelf.Sector3 | ✓ |
| LapTimeDeltaLeader | |
| LapTimeDeltaLeaderClass | |
| TimeDeltaFront | |
| TimeDeltaBehind | |
| TimeDeltaBestSelf | ✓ |
| BestIndividualSectorTimeSelf.Sector1 | ✓ |
| BestIndividualSectorTimeSelf.Sector2 | ✓ |
| BestIndividualSectorTimeSelf.Sector3 | ✓ |
| BestIndividualSectorTimeLeader.Sector1 | |
| BestIndividualSectorTimeLeader.Sector2 | |
| BestIndividualSectorTimeLeader.Sector3 | |
| BestIndividualSectorTimeLeaderClass.Sector1 | ✓ |
| BestIndividualSectorTimeLeaderClass.Sector2 | ✓ |
| BestIndividualSectorTimeLeaderClass.Sector3 | ✓ |
| IncidentPoints | |
| ScoreUnused1 | ✗ |
| ScoreUnused2 | ✗ |
| ScoreUnused3 | ✗ |
| VehicleInfo.Name | |
| VehicleInfo.CarNumber | |
| VehicleInfo.ClassId | |
| VehicleInfo.ModelId | |
| VehicleInfo.TeamId | |
| VehicleInfo.LiveryId | |
| VehicleInfo.ManufacturerId | |
| VehicleInfo.UserId | |
| VehicleInfo.SlotId | ✓ |
| VehicleInfo.ClassPerformanceIndex | ✓ |
| VehicleInfo.EngineType | ✓ |
| VehicleInfo.Unused1 | ✗ |
| VehicleInfo.Unused2 | ✗ |
| PlayerName | ✓ |
| ControlType | ✓ |
| CarSpeed | ✓ |
| EngineRps | ✓ |
| MaxEngineRps | ✓ |
| UpshiftRps | ✓ |
| Gear | |
| NumGears | |
| CarCgLocation.X | ✓ |
| CarCgLocation.Y | ✓ |
| CarCgLocation.Z | ✓ |
| CarOrientation.Pitch | ✓ |
| CarOrientation.Yaw | ✓ |
| CarOrientation.Roll | ✓ |
| LocalAcceleration.X | |
| LocalAcceleration.Y | |
| LocalAcceleration.Z | |
| TotalMass | |
| FuelLeft | ✓ |
| FuelCapacity | ✓ |
| FuelPerLap | ✓ |
| EngineWaterTemp | |
| EngineOilTemp | |
| FuelPressure | |
| EngineOilPressure | |
| TurboPressure | |
| Throttle | ✓ |
| ThrottleRaw | ✓ |
| Brake | ✓ |
| BrakeRaw | ✓ |
| Clutch | ✓ |
| ClutchRaw | ✓ |
| SteerInputRaw | ✓ |
| SteerLockDegrees | |
| SteerWheelRangeDegrees | ✓ |
| AidSettings.Abs | ✓ |
| AidSettings.Tc | ✓ |
| AidSettings.Esp | ✓ |
| AidSettings.Countersteer | ✓ |
| AidSettings.Cornering | ✓ |
| Drs.Equipped | ✓ |
| Drs.Available | ✓ |
| Drs.NumActivationsLeft | ✓ |
| Drs.Engaged | ✓ |
| PitLimiter | |
| PushToPass.Available | ✓ |
| PushToPass.Engaged | ✓ |
| PushToPass.AmountLeft | ✓ |
| PushToPass.EngagedTimeLeft | ✓ |
| PushToPass.WaitTimeLeft | ✓ |
| BrakeBias | |
| DrsNumActivationsTotal | |
| PtPNumActivationsTotal | |
| VehicleUnused1 | ✗ |
| VehicleUnused2 | ✗ |
| TireType | |
| TireRps.FrontLeft | |
| TireRps.FrontRight | |
| TireRps.RearLeft | |
| TireRps.RearRight | |
| TireSpeed.FrontLeft | |
| TireSpeed.FrontRight | |
| TireSpeed.RearLeft | |
| TireSpeed.RearRight | |
| TireGrip.FrontLeft | ✓ |
| TireGrip.FrontRight | ✓ |
| TireGrip.RearLeft | ✓ |
| TireGrip.RearRight | ✓ |
| TireWear.FrontLeft | ✓ |
| TireWear.FrontRight | ✓ |
| TireWear.RearLeft | ✓ |
| TireWear.RearRight | ✓ |
| TireFlatspot.FrontLeft | |
| TireFlatspot.FrontRight | |
| TireFlatspot.RearLeft | |
| TireFlatspot.RearRight | |
| TirePressure.FrontLeft | |
| TirePressure.FrontRight | |
| TirePressure.RearLeft | |
| TirePressure.RearRight | |
| TireDirt.FrontLeft | ✓ |
| TireDirt.FrontRight | ✓ |
| TireDirt.RearLeft | ✓ |
| TireDirt.RearRight | ✓ |
| TireTemp.FrontLeft.CurrentTemp.Left | ✓ |
| TireTemp.FrontLeft.CurrentTemp.Center | ✓ |
| TireTemp.FrontLeft.CurrentTemp.Right | ✓ |
| TireTemp.FrontLeft.OptimalTemp | ✓ |
| TireTemp.FrontLeft.ColdTemp | ✓ |
| TireTemp.FrontLeft.HotTemp | ✓ |
| TireTemp.FrontRight.CurrentTemp.Left | ✓ |
| TireTemp.FrontRight.CurrentTemp.Center | ✓ |
| TireTemp.FrontRight.CurrentTemp.Right | ✓ |
| TireTemp.FrontRight.OptimalTemp | ✓ |
| TireTemp.FrontRight.ColdTemp | ✓ |
| TireTemp.FrontRight.HotTemp | ✓ |
| TireTemp.RearLeft.CurrentTemp.Left | ✓ |
| TireTemp.RearLeft.CurrentTemp.Center | ✓ |
| TireTemp.RearLeft.CurrentTemp.Right | ✓ |
| TireTemp.RearLeft.OptimalTemp | ✓ |
| TireTemp.RearLeft.ColdTemp | ✓ |
| TireTemp.RearLeft.HotTemp | ✓ |
| TireTemp.RearRight.CurrentTemp.Left | ✓ |
| TireTemp.RearRight.CurrentTemp.Center | ✓ |
| TireTemp.RearRight.CurrentTemp.Right | ✓ |
| TireTemp.RearRight.OptimalTemp | ✓ |
| TireTemp.RearRight.ColdTemp | ✓ |
| TireTemp.RearRight.HotTemp | ✓ |
| TireTypeFront | |
| TireTypeRear | |
| TireSubtypeFront | |
| TireSubtypeRear | |
| BrakeTemp.FrontLeft.CurrentTemp | ✓ |
| BrakeTemp.FrontLeft.OptimalTemp | ✓ |
| BrakeTemp.FrontLeft.ColdTemp | ✓ |
| BrakeTemp.FrontLeft.HotTemp | ✓ |
| BrakeTemp.FrontRight.CurrentTemp | ✓ |
| BrakeTemp.FrontRight.OptimalTemp | ✓ |
| BrakeTemp.FrontRight.ColdTemp | ✓ |
| BrakeTemp.FrontRight.HotTemp | ✓ |
| BrakeTemp.RearLeft.CurrentTemp | ✓ |
| BrakeTemp.RearLeft.OptimalTemp | ✓ |
| BrakeTemp.RearLeft.ColdTemp | ✓ |
| BrakeTemp.RearLeft.HotTemp | ✓ |
| BrakeTemp.RearRight.CurrentTemp | ✓ |
| BrakeTemp.RearRight.OptimalTemp | ✓ |
| BrakeTemp.RearRight.ColdTemp | ✓ |
| BrakeTemp.RearRight.HotTemp | ✓ |
| BrakePressure.FrontLeft | |
| BrakePressure.FrontRight | |
| BrakePressure.RearLeft | |
| BrakePressure.RearRight | |
| TractionControlSetting | |
| EngineMapSetting | |
| EngineBrakeSetting | |
| TireUnused1 | ✗ |
| TireUnused2.FrontLeft | ✗ |
| TireUnused2.FrontRight | ✗ |
| TireUnused2.RearLeft | ✗ |
| TireUnused2.RearRight | ✗ |
| TireLoad.FrontLeft | |
| TireLoad.FrontRight | |
| TireLoad.RearLeft | |
| TireLoad.RearRight | |
| CarDamage.Engine | ✓ |
| CarDamage.Transmission | ✓ |
| CarDamage.Aerodynamics | ✓ |
| CarDamage.Suspension | ✓ |
| CarDamage.Unused1 | ✗ |
| CarDamage.Unused2 | ✗ |
| NumCars | |
| DriverData[].DriverInfo.Name | ✓ |
| DriverData[].DriverInfo.CarNumber | |
| DriverData[].DriverInfo.ClassId | |
| DriverData[].DriverInfo.ModelId | |
| DriverData[].DriverInfo.TeamId | |
| DriverData[].DriverInfo.LiveryId | |
| DriverData[].DriverInfo.ManufacturerId | |
| DriverData[].DriverInfo.UserId | |
| DriverData[].DriverInfo.SlotId | ✓ |
| DriverData[].DriverInfo.ClassPerformanceIndex | ✓ |
| DriverData[].DriverInfo.EngineType | |
| DriverData[].DriverInfo.Unused1 | ✗ |
| DriverData[].DriverInfo.Unused2 | ✗ |
| DriverData[].FinishStatus | |
| DriverData[].Place | |
| DriverData[].PlaceClass | ✓ |
| DriverData[].LapDistance | ✓ |
| DriverData[].Position.X | ✓ |
| DriverData[].Position.Y | ✓ |
| DriverData[].Position.Z | ✓ |
| DriverData[].TrackSector | |
| DriverData[].CompletedLaps | ✓ |
| DriverData[].CurrentLapValid | |
| DriverData[].LapTimeCurrentSelf | |
| DriverData[].SectorTimeCurrentSelf.Sector1 | |
| DriverData[].SectorTimeCurrentSelf.Sector2 | |
| DriverData[].SectorTimeCurrentSelf.Sector3 | |
| DriverData[].SectorTimePreviousSelf.Sector1 | |
| DriverData[].SectorTimePreviousSelf.Sector2 | |
| DriverData[].SectorTimePreviousSelf.Sector3 | |
| DriverData[].SectorTimeBestSelf.Sector1 | ✓ |
| DriverData[].SectorTimeBestSelf.Sector2 | ✓ |
| DriverData[].SectorTimeBestSelf.Sector3 | ✓ |
| DriverData[].TimeDeltaFront | ✓ |
| DriverData[].TimeDeltaBehind | ✓ |
| DriverData[].PitStopStatus | |
| DriverData[].InPitlane | |
| DriverData[].NumPitstops | |
| DriverData[].Penalties.DriveThrough | |
| DriverData[].Penalties.StopAndGo | |
| DriverData[].Penalties.PitStop | |
| DriverData[].Penalties.TimeDeduction | |
| DriverData[].Penalties.SlowDown | |
| DriverData[].CarSpeed | |
| DriverData[].TireTypeFront | |
| DriverData[].TireTypeRear | |
| DriverData[].TireSubtypeFront | |
| DriverData[].TireSubtypeRear | |
| DriverData[].BasePenaltyWeight | |
| DriverData[].AidPenaltyWeight | |
| DriverData[].DrsState | |
| DriverData[].PtpState | |
| DriverData[].PenaltyType | |
| DriverData[].PenaltyReason | |
| DriverData[].Unused1 | ✗ |
| DriverData[].Unused2 | ✗ |
| DriverData[].Unused3 | ✗ |
| DriverData[].Unused4 | ✗ |
