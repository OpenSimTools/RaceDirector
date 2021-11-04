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
                GameState: GameState(sharedData),
                UsingVR: sharedData.GameUsingVr > 0,
                Event: Event(sharedData),
                Session: Session(sharedData),
                Vehicles: new Vehicle[0],
                FocusedVehicle: null,
                Player: Player(sharedData)
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
                Track: track,
                FuelRate: sharedData.FuelUseActive >= 0 ? sharedData.FuelUseActive : 0
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
            return new TrackLayout(
                SectorsEnd: sectors
            );
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
                Type: maybeSessionType.Value,
                Phase: maybeSessionPhase.Value,
                Length: maybeSessionLength,
                Requirements: sessionRequirements,
                PitSpeedLimit: ISpeed.FromMPS(sharedData.SessionPitSpeedLimit),
                ElapsedTime: TimeSpan.FromSeconds(1),
                StartLights: StartLights(sharedData.StartLights),
                BestLap: null,
                BestSectors: null
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
                        Time: TimeSpan.FromMinutes(minutes),
                        ExtraLaps: Convert.ToUInt32(laps),
                        EstimatedLaps: null
                    );
                else
                    return new Pipeline.Telemetry.V0.RaceDuration.LapsDuration
                    (
                        Laps: Convert.ToUInt32(laps),
                        EstimatedTime: null
                    );
            }
            else
            {
                if (minutes >= 0)
                    return new Pipeline.Telemetry.V0.RaceDuration.TimeDuration
                    (
                        Time: TimeSpan.FromMinutes(minutes),
                        EstimatedLaps: null
                    );
                else
                    return null;
            }
        }

        private static SessionRequirements SessionRequirements(Contrib.Data.Shared sharedData)
        {
            if (sharedData.PitWindowStart <= 0 || sharedData.PitWindowEnd <= 0)
                return new SessionRequirements(
                    MandatoryPitStops: 0,
                    MandatoryPitRequirements: 0,
                    PitWindow: null
                );

            var window = (Contrib.Constant.SessionLengthFormat)sharedData.SessionLengthFormat switch
            {
                Contrib.Constant.SessionLengthFormat.LapBased =>
                    new Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>(
                        new Pipeline.Telemetry.V0.RaceDuration.LapsDuration(
                            Laps: Convert.ToUInt32(sharedData.PitWindowStart),
                            EstimatedTime: null
                        ),
                        new Pipeline.Telemetry.V0.RaceDuration.LapsDuration(
                            Laps: Convert.ToUInt32(sharedData.PitWindowStart),
                            EstimatedTime: null
                        )
                    ),
                _ =>
                    new Interval<Pipeline.Telemetry.V0.IPitWindowBoundary>(
                        new Pipeline.Telemetry.V0.RaceDuration.TimeDuration(
                            Time: TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowStart)),
                            EstimatedLaps: null
                        ),
                        new Pipeline.Telemetry.V0.RaceDuration.TimeDuration(
                            Time: TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowEnd)),
                            EstimatedLaps: null
                        )
                    ),
            };
            return new SessionRequirements(
                MandatoryPitStops: 1,
                MandatoryPitRequirements: 0,
                PitWindow: window
            );
        }

        private static StartLights? StartLights(Int32 startLights)
        {
            if (startLights < 0)
                return null;
            if (startLights > MaxLights)
                return new StartLights(
                    Colour: Pipeline.Telemetry.V0.LightColour.Green,
                    Lit: new BoundedValue<UInt32>(MaxLights, MaxLights)
                );
            return new StartLights(
                Colour: Pipeline.Telemetry.V0.LightColour.Red,
                Lit: new BoundedValue<UInt32>((UInt32)startLights, MaxLights)
            );
        }

        private static Player? Player(Contrib.Data.Shared sharedData)
        {
            if (sharedData.Player.GameSimulationTicks <= 0)
                return null;
            return new Player
            (
                RawInputs: new RawInputs
                (
                    Steering: 0.0,
                    Throttle: 0.0,
                    Brake: 0.0,
                    Clutch: 0.0,
                    SteerWheelRange: IAngle.FromDeg(0.0)
                ),
                DrivingAids: new DrivingAids
                (
                    Abs: null,
                    Tc: null,
                    Esp: null,
                    Countersteer: null,
                    Cornering: null 
                ),
                VehicleSettings: new VehicleSettings
                (
                    EngineMap: null,
                    EngineBrakeReduction: null
                ),
                VehicleDamage: new VehicleDamage
                (
                    AerodynamicsPercent: 0.0,
                    EnginePercent: 0.0,
                    SuspensionPercent: 0.0,
                    TransmissionPercent: 0.0
                ),
                Tyres: new Tyre[0][],
                Fuel: new Fuel
                (
                    Max: 0.0,
                    Left: 0.0,
                    PerLap: null
                ),
                Engine: new Engine
                (
                    Speed: IAngularSpeed.FromRevPS(0.0),
                    UpshiftSpeed: IAngularSpeed.FromRevPS(0.0),
                    MaxSpeed: IAngularSpeed.FromRevPS(0.0)
                ),
                CgLocation: new Vector3<IDistance>
                (
                    X: IDistance.FromM(sharedData.Player.Position.X),
                    Y: IDistance.FromM(sharedData.Player.Position.Y),
                    Z: IDistance.FromM(sharedData.Player.Position.Z)
                ),
                Orientation: new Orientation
                (
                    Yaw: IAngle.FromDeg(0.0),
                    Pitch: IAngle.FromDeg(0.0),
                    Roll: IAngle.FromDeg(0.0)
                ),
                LocalAcceleration: new Vector3<IAcceleration>
                (
                    X: IAcceleration.FromMPS2(sharedData.Player.LocalAcceleration.X),
                    Y: IAcceleration.FromMPS2(sharedData.Player.LocalAcceleration.Y),
                    Z: IAcceleration.FromMPS2(sharedData.Player.LocalAcceleration.Z)
                ),
                ClassBestLap: null,
                ClassBestSectors: null,
                PersonalBestSectors: null,
                PersonalBestDelta: null,
                Drs: null,
                PushToPass: null,
                PitStop: 0,
                GameFlags: 0
            );
        }
    }
}
