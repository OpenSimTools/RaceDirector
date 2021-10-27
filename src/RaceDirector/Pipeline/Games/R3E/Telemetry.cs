using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using System;

namespace RaceDirector.Pipeline.Games.R3E
{
    static class Telemetry
    {
        private static readonly UInt32 MaxLights = 5;

        public static GameTelemetry Transform(Contrib.Data.Shared sharedData)
        {
            // TODO check major version
            return new GameTelemetry(
                GameState(sharedData),
                sharedData.GameUsingVr > 0,
                Event(sharedData),
                Session(sharedData),
                new Vehicle[0],
                null,
                Player(sharedData)
            );
        }

        private static Pipeline.Telemetry.V0.GameState GameState(Contrib.Data.Shared sharedData)
        {
            if (sharedData.GameInMenus > 0)
                return Pipeline.Telemetry.V0.GameState.Menu;
            if (sharedData.GameInReplay > 0)
                return Pipeline.Telemetry.V0.GameState.Replay;
            return Pipeline.Telemetry.V0.GameState.Driving;
        }

        private static Event? Event(Contrib.Data.Shared sharedData)
        {
            var track = Track(sharedData);
            if (track == null)
                return null;
            return new Event(
                track,
                sharedData.FuelUseActive >= 0 ? sharedData.FuelUseActive : 0
            );
        }

        private static TrackLayout? Track(Contrib.Data.Shared sharedData)
        {
            if (sharedData.LayoutLength < 0)
                return null;
            var layoutLength = IDistance.FromM(sharedData.LayoutLength);
            var sectors = new []
            {
                DistanceFraction.Of(layoutLength, sharedData.SectorStartFactors.Sector1),
                DistanceFraction.Of(layoutLength, sharedData.SectorStartFactors.Sector2),
                DistanceFraction.Of(layoutLength, sharedData.SectorStartFactors.Sector3)
            };
            return new TrackLayout(sectors);
        }

        private static Session? Session(Contrib.Data.Shared sharedData)
        {
            var maybeSessionType = SessionType(sharedData);
            var maybeSessionPhase = SessionPhase(sharedData);
            if (maybeSessionType is null || maybeSessionPhase is null)
                return null;
            var maybeSessionLength = SessionLength(sharedData);
            var sessionRequirements = SessionRequirements(sharedData);
            //(
            //    (sharedData.PitWindowStart > 0 && sharedData.PitWindowEnd > 0) ? 1 : 0

            //);

            return new Session
            (
                maybeSessionType.Value,
                maybeSessionPhase.Value,
                maybeSessionLength,
                sessionRequirements,
                // TODO
                ISpeed.FromMPS(sharedData.SessionPitSpeedLimit),
                TimeSpan.FromSeconds(1),
                StartLights(sharedData.StartLights),
                null,
                null
            );
        }

        private static Pipeline.Telemetry.V0.SessionType? SessionType(Contrib.Data.Shared sharedData) =>
            (Contrib.Constant.Session)sharedData.SessionType switch
            {
                Contrib.Constant.Session.Practice => Pipeline.Telemetry.V0.SessionType.Practice,
                Contrib.Constant.Session.Qualify => Pipeline.Telemetry.V0.SessionType.Qualify,
                Contrib.Constant.Session.Warmup => Pipeline.Telemetry.V0.SessionType.Warmup,
                Contrib.Constant.Session.Race => Pipeline.Telemetry.V0.SessionType.Race,
                _ => null
            };

        private static Pipeline.Telemetry.V0.SessionPhase? SessionPhase(Contrib.Data.Shared sharedData) =>
            (Contrib.Constant.SessionPhase)sharedData.SessionPhase switch
            {
                Contrib.Constant.SessionPhase.Garage => Pipeline.Telemetry.V0.SessionPhase.Garage,
                Contrib.Constant.SessionPhase.Gridwalk => Pipeline.Telemetry.V0.SessionPhase.Gridwalk,
                Contrib.Constant.SessionPhase.Formation => Pipeline.Telemetry.V0.SessionPhase.Formation,
                Contrib.Constant.SessionPhase.Green => Pipeline.Telemetry.V0.SessionPhase.Started,
                Contrib.Constant.SessionPhase.Checkered => Pipeline.Telemetry.V0.SessionPhase.Over,
                _ => null
            };

        private static Pipeline.Telemetry.V0.ISessionDuration? SessionLength(Contrib.Data.Shared sharedData)
        {
            // TODO Create Event.SessionsLength and get the SessionIteration-1 element from it
            var (laps, minutes) = sharedData.SessionIteration switch
            {
                1 => (sharedData.RaceSessionLaps.Race1, sharedData.RaceSessionMinutes.Race1),
                2 => (sharedData.RaceSessionLaps.Race2, sharedData.RaceSessionMinutes.Race2),
                3 => (sharedData.RaceSessionLaps.Race3, sharedData.RaceSessionMinutes.Race3),
                _ => (-1, -1)
            };
            if (laps >= 0)
            {
                if (minutes >= 0)
                    return new Pipeline.Telemetry.V0.RaceDuration.TimePlusLapsDuration
                    (
                        TimeSpan.FromMinutes(minutes),
                        Convert.ToUInt32(laps),
                        null // TODO
                    );
                else
                    return new Pipeline.Telemetry.V0.RaceDuration.LapsDuration
                    (
                        Convert.ToUInt32(laps),
                        null // TODO
                    );
            }
            else
            {
                if (minutes >= 0)
                    return new Pipeline.Telemetry.V0.RaceDuration.TimeDuration
                    (
                        TimeSpan.FromMinutes(minutes),
                        null // TODO
                    );
                else
                    return null;
            }
        }

        private static SessionRequirements SessionRequirements(Contrib.Data.Shared sharedData)
        {
            if (sharedData.PitWindowStart <= 0 || sharedData.PitWindowEnd <= 0)
                return new SessionRequirements(0, 0, null);

            var window = (Contrib.Constant.SessionLengthFormat)sharedData.SessionLengthFormat switch
            {
                Contrib.Constant.SessionLengthFormat.LapBased =>
                    new Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>(
                        new Pipeline.Telemetry.V0.RaceDuration.LapsDuration(
                            Convert.ToUInt32(sharedData.PitWindowStart),
                            null // TODO
                        ),
                        new Pipeline.Telemetry.V0.RaceDuration.LapsDuration(
                            Convert.ToUInt32(sharedData.PitWindowStart),
                            null // TODO
                        )
                    ),
                _ =>
                    new Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>(
                        new Pipeline.Telemetry.V0.RaceDuration.TimeDuration(
                            TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowStart)),
                            null // TODO
                        ),
                        new Pipeline.Telemetry.V0.RaceDuration.TimeDuration(
                            TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowEnd)),
                            null // TODO
                        )
                    ),
            };
            return new SessionRequirements(1, 0, window);
        }

        private static StartLights? StartLights(Int32 startLights)
        {
            if (startLights < 0)
                return null;
            if (startLights > MaxLights)
                return new StartLights(Pipeline.Telemetry.V0.LightColour.Green, new BoundedValue<UInt32>(MaxLights, MaxLights));
            return new StartLights(Pipeline.Telemetry.V0.LightColour.Red, new BoundedValue<UInt32>((UInt32)startLights, MaxLights));
        }

        private static Player? Player(Contrib.Data.Shared sharedData)
        {
            if (sharedData.Player.GameSimulationTicks <= 0)
                return null;
            return new Player
            (
                new RawInputs
                (
                    0.0, // TODO
                    0.0, // TODO
                    0.0, // TODO
                    0.0, // TODO
                    IAngle.FromDeg(0.0) // TODO
                ),
                new DrivingAids
                (
                    null, // TODO
                    null, // TODO
                    null, // TODO
                    null, // TODO
                    null  // TODO
                ),
                new VehicleSettings
                (
                    null, // TODO
                    null // TODO
                ),
                new VehicleDamage
                (
                    0.0, // TODO
                    0.0, // TODO
                    0.0, // TODO
                    0.0 // TODO
                ),
                new Tyre[0][],
                new Fuel
                (
                    0.0, // TODO
                    0.0, // TODO
                    null // TODO
                ),
                new Engine
                (
                    IAngularSpeed.FromRevPS(0.0), // TODO
                    IAngularSpeed.FromRevPS(0.0), // TODO
                    IAngularSpeed.FromRevPS(0.0) // TODO
                ),
                new Vector3<IDistance> // TODO is it meters?
                (
                    IDistance.FromM(sharedData.Player.Position.X),
                    IDistance.FromM(sharedData.Player.Position.Y),
                    IDistance.FromM(sharedData.Player.Position.Z)
                ),
                new Orientation
                (
                    IAngle.FromDeg(0.0), // TODO
                    IAngle.FromDeg(0.0), // TODO
                    IAngle.FromDeg(0.0) // TODO
                ),
                new Vector3<IAcceleration>
                (
                    IAcceleration.FromMPS2(sharedData.Player.LocalAcceleration.X),
                    IAcceleration.FromMPS2(sharedData.Player.LocalAcceleration.Y),
                    IAcceleration.FromMPS2(sharedData.Player.LocalAcceleration.Z)
                ),
                null, // TODO
                null, // TODO
                null, // TODO
                null, // TODO
                null, // TODO
                null, // TODO
                0, // TODO
                0 // TODO
            );
        }
    }
}
