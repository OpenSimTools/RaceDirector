using RaceDirector.Pipeline.Games.R3E.Contrib.Data;
using RaceDirector.Plugin.HUD.Utils;
using System;
using System.IO;
using System.Text.Json;

namespace E2ETest
{
    /// <summary>
    /// Writer to allow us to selectively output only supported fields for comparison.
    /// </summary>
    public static class SharedDataJsonWriter
    {
        public static byte[] ToJson(Shared shared)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions()))
                {
                    WriteSharedData(writer, shared);
                }
                return stream.ToArray();
            }
        }

        private static void WriteSharedData(Utf8JsonWriter w, Shared shared)
        {
            w.WriteObject(_ =>
            {
                w.WriteFormattedNumber("VersionMajor", shared.VersionMajor);
                w.WriteFormattedNumber("VersionMinor", shared.VersionMinor);

                // AllDriversOffset
                // DriverDataSize
                // GamePaused

                w.WriteFormattedNumber("GameInMenus", shared.GameInMenus);
                w.WriteFormattedNumber("GameInReplay", shared.GameInReplay);
                w.WriteFormattedNumber("GameUsingVr", shared.GameUsingVr);

                w.WriteObject("Player", _ =>
                {
                    var player = shared.Player;

                    // Player.GameSimulationTicks
                    // Player.GameSimulationTime

                    w.WriteCoordinates("Position", player.Position);

                    // Player.Velocity.X
                    // Player.Velocity.Y
                    // Player.Velocity.Z
                    // Player.LocalVelocity.X
                    // Player.LocalVelocity.Y
                    // Player.LocalVelocity.Z
                    // Player.Acceleration.X
                    // Player.Acceleration.Y
                    // Player.Acceleration.Z

                    w.WriteCoordinates("LocalAcceleration", player.LocalAcceleration);

                    // Player.Orientation.X
                    // Player.Orientation.Y
                    // Player.Orientation.Z
                    // Player.Rotation.X
                    // Player.Rotation.Y
                    // Player.Rotation.Z
                    // Player.AngularAcceleration.X
                    // Player.AngularAcceleration.Y
                    // Player.AngularAcceleration.Z
                    // Player.AngularVelocity.X
                    // Player.AngularVelocity.Y
                    // Player.AngularVelocity.Z
                    // Player.LocalAngularVelocity.X
                    // Player.LocalAngularVelocity.Y
                    // Player.LocalAngularVelocity.Z

                    w.WriteCoordinates("LocalGforce", player.LocalGforce);

                    // Player.SteeringForce
                    // Player.SteeringForcePercentage
                    // Player.EngineTorque
                    // Player.CurrentDownforce
                    // Player.Voltage
                    // Player.ErsLevel
                    // Player.PowerMguH
                    // Player.PowerMguK
                    // Player.TorqueMguK
                    // Player.SuspensionDeflection.FrontLeft
                    // Player.SuspensionDeflection.FrontRight
                    // Player.SuspensionDeflection.RearLeft
                    // Player.SuspensionDeflection.RearRight
                    // Player.SuspensionVelocity.FrontLeft
                    // Player.SuspensionVelocity.FrontRight
                    // Player.SuspensionVelocity.RearLeft
                    // Player.SuspensionVelocity.RearRight
                    // Player.Camber.FrontLeft
                    // Player.Camber.FrontRight
                    // Player.Camber.RearLeft
                    // Player.Camber.RearRight
                    // Player.RideHeight.FrontLeft
                    // Player.RideHeight.FrontRight
                    // Player.RideHeight.RearLeft
                    // Player.RideHeight.RearRight
                    // Player.FrontWingHeight
                    // Player.FrontRollAngle
                    // Player.RearRollAngle
                    // Player.thirdSpringSuspensionDeflectionFront
                    // Player.thirdSpringSuspensionVelocityFront
                    // Player.thirdSpringSuspensionDeflectionRear
                    // Player.thirdSpringSuspensionVelocityRear
                });

                // TrackName
                // LayoutName
                // TrackId
                // LayoutId

                w.WriteFormattedNumber("LayoutLength", shared.LayoutLength);
                w.WriteSectors("SectorStartFactors", shared.SectorStartFactors);

                // RaceSessionLaps.Race1
                // RaceSessionLaps.Race2
                // RaceSessionLaps.Race3
                // RaceSessionMinutes.Race1
                // RaceSessionMinutes.Race2
                // RaceSessionMinutes.Race3
                // EventIndex

                w.WriteFormattedNumber("SessionType", (int)shared.SessionType);

                // SessionIteration
                // SessionLengthFormat

                w.WriteFormattedNumber("SessionLengthFormat", (int)shared.SessionLengthFormat);
                w.WriteFormattedNumber("SessionPitSpeedLimit", shared.SessionPitSpeedLimit);
                w.WriteFormattedNumber("SessionPhase", (int)shared.SessionPhase);
                w.WriteFormattedNumber("StartLights", shared.StartLights);

                // TireWearActive

                w.WriteFormattedNumber("FuelUseActive", shared.FuelUseActive);
                w.WriteFormattedNumber("NumberOfLaps", shared.NumberOfLaps);
                w.WriteFormattedNumber("SessionTimeDuration", shared.SessionTimeDuration);
                w.WriteFormattedNumber("SessionTimeRemaining", shared.SessionTimeRemaining);

                // MaxIncidentPoints

                w.WriteFormattedNumber("PitWindowStatus", (int)shared.PitWindowStatus);
                w.WriteFormattedNumber("PitWindowStart", shared.PitWindowStart);
                w.WriteFormattedNumber("PitWindowEnd", shared.PitWindowEnd);
                w.WriteFormattedNumber("InPitLane", shared.InPitlane);

                // PitMenuSelection
                // PitMenuState.Preset
                // PitMenuState.Penalty
                // PitMenuState.Driverchange
                // PitMenuState.Fuel
                // PitMenuState.FrontTires
                // PitMenuState.RearTires
                // PitMenuState.FrontWing
                // PitMenuState.RearWing
                // PitMenuState.Suspension
                // PitMenuState.ButtonTop
                // PitMenuState.ButtonBottom

                w.WriteFormattedNumber("PitState", shared.PitState);
                w.WriteFormattedNumber("PitTotalDuration", shared.PitTotalDuration);
                w.WriteFormattedNumber("PitElapsedTime", shared.PitElapsedTime);
                w.WriteFormattedNumber("PitAction", shared.PitAction);

                // NumPitstopsPerformed
                // PitMinDurationTotal
                // PitMinDurationLeft

                w.WriteObject("Flags", _ =>
                {
                    var flags = shared.Flags;

                    w.WriteFormattedNumber("Yellow", flags.Yellow);

                    // Flags.YellowCausedIt

                    w.WriteFormattedNumber("YellowOvertake", flags.YellowOvertake);
                    w.WriteFormattedNumber("YellowPositionsGained", flags.YellowPositionsGained);

                    // Flags.SectorYellow.Sector1
                    // Flags.SectorYellow.Sector2
                    // Flags.SectorYellow.Sector3
                    // Flags.ClosestYellowDistanceIntoTrack

                    w.WriteFormattedNumber("Blue", flags.Blue);
                    w.WriteFormattedNumber("Black", flags.Black);
                    w.WriteFormattedNumber("Green", flags.Green);
                    w.WriteFormattedNumber("Checkered", flags.Checkered);
                    w.WriteFormattedNumber("White", flags.White);
                    w.WriteFormattedNumber("BlackAndWhite", flags.BlackAndWhite);
                });

                // Position

                w.WriteFormattedNumber("PositionClass", shared.PositionClass);

                // FinishStatus
                // CutTrackWarnings

                w.WriteObject("Penalties", _ =>
                {
                    var penalties = shared.Penalties;

                    w.WriteFormattedNumber("DriveThrough", penalties.DriveThrough);
                    w.WriteFormattedNumber("StopAndGo", penalties.StopAndGo);
                    w.WriteFormattedNumber("PitStop", penalties.PitStop);
                    w.WriteFormattedNumber("TimeDeduction", penalties.TimeDeduction);
                    w.WriteFormattedNumber("SlowDown", penalties.SlowDown);
                });

                // NumPenalties

                w.WriteFormattedNumber("CompletedLaps", shared.CompletedLaps);
                w.WriteFormattedNumber("CurrentLapValid", shared.CurrentLapValid);

                // TrackSector

                w.WriteFormattedNumber("LapDistance", shared.LapDistance);
                w.WriteFormattedNumber("LapDistanceFraction", shared.LapDistanceFraction);

                // LapTimeBestLeader
                // LapTimeBestLeaderClass
                // SectorTimesSessionBestLap.Sector1
                // SectorTimesSessionBestLap.Sector2
                // SectorTimesSessionBestLap.Sector3

                w.WriteFormattedNumber("LapTimeBestSelf", shared.LapTimeBestSelf);
                w.WriteSectors("SectorTimesBestSelf", shared.SectorTimesBestSelf);

                // LapTimePreviousSelf
                // SectorTimesPreviousSelf.Sector1
                // SectorTimesPreviousSelf.Sector2
                // SectorTimesPreviousSelf.Sector3

                w.WriteFormattedNumber("LapTimeCurrentSelf", shared.LapTimeCurrentSelf);
                w.WriteSectors("SectorTimesCurrentSelf", shared.SectorTimesCurrentSelf);

                // LapTimeDeltaLeader
                // LapTimeDeltaLeaderClass
                // TimeDeltaFront => Player or -1.0
                // TimeDeltaBehind => Player or -1.0

                w.WriteFormattedNumber("TimeDeltaBestSelf", shared.TimeDeltaBestSelf);

                w.WriteSectors("BestIndividualSectorTimeSelf", shared.BestIndividualSectorTimeSelf);

                // BestIndividualSectorTimeLeader.Sector1
                // BestIndividualSectorTimeLeader.Sector2
                // BestIndividualSectorTimeLeader.Sector3

                w.WriteSectors("BestIndividualSectorTimeLeaderClass", shared.BestIndividualSectorTimeLeaderClass);

                // IncidentPoints

                w.WriteObject("VehicleInfo", _ =>
                {
                    var vehicleInfo = shared.VehicleInfo;

                    // VehicleInfo.Name
                    // VehicleInfo.CarNumber
                    // VehicleInfo.ClassId
                    // VehicleInfo.ModelId
                    // VehicleInfo.TeamId
                    // VehicleInfo.LiveryId
                    // VehicleInfo.ManufacturerId
                    // VehicleInfo.UserId

                    w.WriteFormattedNumber("SlotId", vehicleInfo.SlotId);
                    w.WriteFormattedNumber("ClassPerformanceIndex", vehicleInfo.ClassPerformanceIndex);
                    w.WriteFormattedNumber("EngineType", (int)vehicleInfo.EngineType);
                });

                w.WriteString("PlayerName", ToBase64(shared.PlayerName));
                w.WriteFormattedNumber("ControlType", (int)shared.ControlType);
                w.WriteFormattedNumber("CarSpeed", shared.CarSpeed);
                w.WriteFormattedNumber("EngineRps", shared.EngineRps);
                w.WriteFormattedNumber("MaxEngineRps", shared.MaxEngineRps);
                w.WriteFormattedNumber("UpshiftRps", shared.UpshiftRps);

                // Gear
                // NumGears

                w.WriteCoordinates("CarCgLocation", shared.CarCgLocation);
                w.WriteOrientationPYR("CarOrientation", shared.CarOrientation);

                // LocalAcceleration.X
                // LocalAcceleration.Y
                // LocalAcceleration.Z
                // TotalMass

                w.WriteFormattedNumber("FuelLeft", shared.FuelLeft);
                w.WriteFormattedNumber("FuelCapacity", shared.FuelCapacity);
                w.WriteFormattedNumber("FuelPerLap", shared.FuelPerLap);

                // EngineWaterTemp
                // EngineOilTemp
                // FuelPressure
                // EngineOilPressure
                // TurboPressure

                w.WriteFormattedNumber("Throttle", shared.Throttle);
                w.WriteFormattedNumber("ThrottleRaw", shared.ThrottleRaw);
                w.WriteFormattedNumber("Brake", shared.Brake);
                w.WriteFormattedNumber("BrakeRaw", shared.BrakeRaw);
                w.WriteFormattedNumber("Clutch", shared.Clutch);
                w.WriteFormattedNumber("ClutchRaw", shared.ClutchRaw);
                w.WriteFormattedNumber("SteerInputRaw", shared.SteerInputRaw);

                // SteerLockDegrees

                w.WriteFormattedNumber("SteerWheelRangeDegrees", shared.SteerWheelRangeDegrees);

                w.WriteObject("AidSettings", _ =>
                {
                    var aidSettings = shared.AidSettings;

                    w.WriteFormattedNumber("Abs", aidSettings.Abs);
                    w.WriteFormattedNumber("Tc", aidSettings.Tc);
                    w.WriteFormattedNumber("Esp", aidSettings.Esp);
                    w.WriteFormattedNumber("Countersteer", aidSettings.Countersteer);
                    w.WriteFormattedNumber("Cornering", aidSettings.Cornering);
                });

                w.WriteObject("Drs", _ =>
                {
                    var drs = shared.Drs;

                    w.WriteFormattedNumber("Equipped", drs.Equipped);
                    w.WriteFormattedNumber("Available", drs.Available);
                    w.WriteFormattedNumber("NumActivationsLeft", drs.NumActivationsLeft);
                    w.WriteFormattedNumber("Engaged", drs.Engaged);
                });

                // PitLimiter

                w.WriteObject("PushToPass", _ =>
                {
                    var pushToPass = shared.PushToPass;

                    w.WriteFormattedNumber("Available", pushToPass.Available);
                    w.WriteFormattedNumber("Engaged", pushToPass.Engaged);
                    w.WriteFormattedNumber("AmountLeft", pushToPass.AmountLeft);
                    w.WriteFormattedNumber("EngagedTimeLeft", pushToPass.EngagedTimeLeft);
                    w.WriteFormattedNumber("WaitTimeLeft", pushToPass.WaitTimeLeft);
                });

                // BrakeBias

                w.WriteFormattedNumber("DrsNumActivationsTotal", shared.DrsNumActivationsTotal);
                w.WriteFormattedNumber("PtPNumActivationsTotal", shared.PtpNumActivationsTotal);

                // TireType
                // TireRps.FrontLeft
                // TireRps.FrontRight
                // TireRps.RearLeft
                // TireRps.RearRight
                // TireSpeed.FrontLeft
                // TireSpeed.FrontRight
                // TireSpeed.RearLeft
                // TireSpeed.RearRight

                w.WriteTireData("TireGrip", shared.TireGrip);

                w.WriteTireData("TireWear", shared.TireWear);

                // TireFlatspot.FrontLeft
                // TireFlatspot.FrontRight
                // TireFlatspot.RearLeft
                // TireFlatspot.RearRight
                // TirePressure.FrontLeft
                // TirePressure.FrontRight
                // TirePressure.RearLeft
                // TirePressure.RearRight

                w.WriteTireData("TireDirt", shared.TireDirt);

                w.WriteTireData("TireTemp", shared.TireTemp);

                // TireTypeFront
                // TireTypeRear
                // TireSubtypeFront
                // TireSubtypeRear

                w.WriteTireData("BrakeTemp", shared.BrakeTemp);
 
                // BrakePressure.FrontLeft
                // BrakePressure.FrontRight
                // BrakePressure.RearLeft
                // BrakePressure.RearRight
                // TractionControlSetting
                // EngineMapSetting
                // EngineBrakeSetting
                // TireLoad.FrontLeft
                // TireLoad.FrontRight
                // TireLoad.RearLeft
                // TireLoad.RearRight

                w.WriteObject("CarDamage", _ =>
                {
                    var carDamage = shared.CarDamage;

                    w.WriteFormattedNumber("Engine", carDamage.Engine);
                    w.WriteFormattedNumber("Transmission", carDamage.Transmission);
                    w.WriteFormattedNumber("Aerodynamics", carDamage.Aerodynamics);
                    w.WriteFormattedNumber("Suspension", carDamage.Suspension);
                });

                // NumCars

                w.WriteArray("DriverData", _ =>
                {
                    foreach (var driverData in shared.DriverData)
                    {
                        // Note: Sometimes there's a lot of "null" vehicles after NumCars, but we won't render them.
                        if (driverData.DriverInfo.SlotId == -1)
                            continue;

                        w.WriteObject(_ =>
                        {
                            w.WriteObject("DriverInfo", _ =>
                            {
                                var driverInfo = driverData.DriverInfo;

                                w.WriteString("Name", ToBase64(driverInfo.Name));

                                // DriverData[].DriverInfo.CarNumber
                                // DriverData[].DriverInfo.ClassId
                                // DriverData[].DriverInfo.ModelId
                                // DriverData[].DriverInfo.TeamId
                                // DriverData[].DriverInfo.LiveryId
                                // DriverData[].DriverInfo.ManufacturerId
                                // DriverData[].DriverInfo.UserId

                                w.WriteFormattedNumber("SlotId", driverInfo.SlotId);
                                w.WriteFormattedNumber("ClassPerformanceIndex", driverInfo.ClassPerformanceIndex);

                                // DriverData[].DriverInfo.EngineType
                            });

                            // DriverData[].FinishStatus
                            // DriverData[].Place

                            w.WriteFormattedNumber("PlaceClass", driverData.PlaceClass);
                            w.WriteFormattedNumber("LapDistance", driverData.LapDistance);
                            w.WriteCoordinates("Position", driverData.Position);

                            // DriverData[].TrackSector

                            w.WriteFormattedNumber("CompletedLaps", driverData.CompletedLaps);

                            // DriverData[].CurrentLapValid
                            // DriverData[].LapTimeCurrentSelf
                            // DriverData[].SectorTimeCurrentSelf.Sector1
                            // DriverData[].SectorTimeCurrentSelf.Sector2
                            // DriverData[].SectorTimeCurrentSelf.Sector3
                            // DriverData[].SectorTimePreviousSelf.Sector1
                            // DriverData[].SectorTimePreviousSelf.Sector2
                            // DriverData[].SectorTimePreviousSelf.Sector3

                            w.WriteSectors("SectorTimeBestSelf", driverData.SectorTimeBestSelf);
                            w.WriteFormattedNumber("TimeDeltaFront", driverData.TimeDeltaFront);
                            w.WriteFormattedNumber("TimeDeltaBehind", driverData.TimeDeltaBehind);

                            // DriverData[].PitStopStatus
                            // DriverData[].InPitlane
                            // DriverData[].NumPitstops
                            // DriverData[].Penalties.DriveThrough
                            // DriverData[].Penalties.StopAndGo
                            // DriverData[].Penalties.PitStop
                            // DriverData[].Penalties.TimeDeduction
                            // DriverData[].Penalties.SlowDown
                            // DriverData[].CarSpeed
                            // DriverData[].TireTypeFront
                            // DriverData[].TireTypeRear
                            // DriverData[].TireSubtypeFront
                            // DriverData[].TireSubtypeRear
                            // DriverData[].BasePenaltyWeight
                            // DriverData[].AidPenaltyWeight
                            // DriverData[].DrsState
                            // DriverData[].PtpState
                            // DriverData[].PenaltyType
                            // DriverData[].PenaltyReason
                        });
                    }
                });
            });
        }

        private static String ToBase64(byte[] value)
        {
            var nullIndex = Array.IndexOf<byte>(value, 0);
            return Convert.ToBase64String(value, 0, nullIndex + 1);
        }

        private static void WriteCoordinates(this Utf8JsonWriter writer, String propertyName, Vector3<Single> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("X", v.X);
                writer.WriteFormattedNumber("Y", v.Y);
                writer.WriteFormattedNumber("Z", v.Z);
            });
        }

        private static void WriteCoordinates(this Utf8JsonWriter writer, String propertyName, Vector3<Double> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("X", v.X);
                writer.WriteFormattedNumber("Y", v.Y);
                writer.WriteFormattedNumber("Z", v.Z);
            });
        }

        private static void WriteSectors(this Utf8JsonWriter writer, String propertyName, SectorStarts<Single> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("Sector1", v.Sector1);
                writer.WriteFormattedNumber("Sector2", v.Sector2);
                writer.WriteFormattedNumber("Sector3", v.Sector3);
            });
        }

        private static void WriteSectors(this Utf8JsonWriter writer, String propertyName, Sectors<Single> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("Sector1", v.Sector1);
                writer.WriteFormattedNumber("Sector2", v.Sector2);
                writer.WriteFormattedNumber("Sector3", v.Sector3);
            });
        }

        private static void WriteSectors(this Utf8JsonWriter writer, String propertyName, Sectors<Int32> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("Sector1", v.Sector1);
                writer.WriteFormattedNumber("Sector2", v.Sector2);
                writer.WriteFormattedNumber("Sector3", v.Sector3);
            });
        }

        private static void WriteOrientationPYR(this Utf8JsonWriter writer, String propertyName, Orientation<Single> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("Pitch", v.Pitch);
                writer.WriteFormattedNumber("Yaw", v.Yaw);
                writer.WriteFormattedNumber("Roll", v.Roll);
            });
        }

        private static void WriteTireData(this Utf8JsonWriter writer, String propertyName, TireData<Single> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("FrontLeft", v.FrontLeft);
                writer.WriteFormattedNumber("FrontRight", v.FrontRight);
                writer.WriteFormattedNumber("RearLeft", v.RearLeft);
                writer.WriteFormattedNumber("RearRight", v.RearRight);
            });
        }
        
        private static void WriteTireData(this Utf8JsonWriter writer, String propertyName, TireData<Int32> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("FrontLeft", v.FrontLeft);
                writer.WriteFormattedNumber("FrontRight", v.FrontRight);
                writer.WriteFormattedNumber("RearLeft", v.RearLeft);
                writer.WriteFormattedNumber("RearRight", v.RearRight);
            });
        }

        private static void WriteTireData(this Utf8JsonWriter writer, String propertyName, TireData<TireTempInformation> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteTireTempInformation("FrontLeft", v.FrontLeft);
                writer.WriteTireTempInformation("FrontRight", v.FrontRight);
                writer.WriteTireTempInformation("RearLeft", v.RearLeft);
                writer.WriteTireTempInformation("RearRight", v.RearRight);
            });
        }

        private static void WriteTireTempInformation(this Utf8JsonWriter writer, String propertyName, TireTempInformation v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteObject("CurrentTemp", _ =>
                {
                    writer.WriteFormattedNumber("Left", v.CurrentTemp.Left);
                    writer.WriteFormattedNumber("Center", v.CurrentTemp.Center);
                    writer.WriteFormattedNumber("Right", v.CurrentTemp.Right);
                });
                writer.WriteFormattedNumber("OptimalTemp", v.OptimalTemp);
                writer.WriteFormattedNumber("ColdTemp", v.ColdTemp);
                writer.WriteFormattedNumber("HotTemp", v.HotTemp);
            });
        }

        private static void WriteTireData(this Utf8JsonWriter writer, String propertyName, TireData<BrakeTemp> v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteBrakeTemp("FrontLeft", v.FrontLeft);
                writer.WriteBrakeTemp("FrontRight", v.FrontRight);
                writer.WriteBrakeTemp("RearLeft", v.RearLeft);
                writer.WriteBrakeTemp("RearRight", v.RearRight);
            });
        }

        private static void WriteBrakeTemp(this Utf8JsonWriter writer, String propertyName, BrakeTemp v)
        {
            writer.WriteObject(propertyName, _ =>
            {
                writer.WriteFormattedNumber("CurrentTemp", v.CurrentTemp);
                writer.WriteFormattedNumber("OptimalTemp", v.OptimalTemp);
                writer.WriteFormattedNumber("ColdTemp", v.ColdTemp);
                writer.WriteFormattedNumber("HotTemp", v.HotTemp);
            });
        }

        private static void WriteFormattedNumber(this Utf8JsonWriter writer, String propertyName, Int32 v) =>
            writer.WriteNumber(propertyName, v);

        private static void WriteFormattedNumber(this Utf8JsonWriter writer, String propertyName, Double v) =>
            writer.WriteNumber(propertyName, v, RaceDirector.Plugin.HUD.Pipeline.R3EDashTransformer.DecimalDigits);
    }
}
