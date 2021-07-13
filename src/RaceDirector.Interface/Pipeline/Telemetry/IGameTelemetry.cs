using System;
using RaceDirector.Pipeline.Telemetry.Physics;

namespace RaceDirector.Pipeline.Telemetry
{
    // Notes:
    // - Sectors are a generic array. Oval racing doesn't have sectors, rally has more than 3.
    // - Playing has Player that is/has also a Driver, replay has CurrentDriver, menu neither.
    //   Menu might have a session time if it's in game menu vs main menu.
    // - Sector times are often -1!

    // TODO
    // - What happens in rF2 or ACC before swapping driver?
    //   - rF2 procedure https://simracing-gp.net/topic-568/how_to_driver_swap_procedure
    //   - rF2 telemetry https://youtu.be/N_S6stecqkc

    namespace V0
    {
        public interface IGameTelemetry
        {
            GameState GameState { get; }

            Boolean UsingVR { get; }

            /// <summary>
            /// Race event information.
            /// </summary>
            /// <remarks>
            /// Unavailable in the main menu.
            /// </remarks>
            IEvent? Event { get; }

            /// <summary>
            /// Session information.
            /// </summary>
            /// <remarks>
            /// Unavailable in the main manu.
            /// </remarks>
            ISession? Session { get; }

            IVehicle[] Vehicles { get; }

            /// <summary>
            /// Vehicle currently focused.
            /// </summary>
            /// <remarks>
            /// Usually available when driving, in monitor or replay.
            /// </remarks>
            IVehicle? CurrentVehicle { get; }

            /// <summary>
            /// Player information if available.
            /// </summary>
            /// <remarks>
            /// Usually when player's car is focused.
            /// </remarks>
            IPlayer? Player { get; }
        }

        // TODO TODO TODO TODO TODO TODO TODO Don't use enums! Can't extend them!
        // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types
        public enum GameState
        {
            Driving,
            //Paused,  // R3E GamePaused - is the clock ticking? (-1 in main menu, 0 in replay, 1 in single player menu, 0 in multi player menu)
            Menu,    // R3E GameInMenus (-1 in main menu, 1 in single or multi player menu, 1 in monitor)
            Replay   // R3E GameInReplay (-1 in main menu, 1 in replay, 0 in monitor)
        }

        public interface IEvent
        {
            ITrackLayout Track { get; }

            /// <summary>
            /// Fuel burning rate. 0.0 disabled, 1.0 is normal, 2.0 double, etc.
            /// </summary>
            Double FuelRate { get; } // R3E FuelUseActive (it's a multiplier!)

            //Double TireRate { get; }

            //Double MechanicalDamage { get; }
        }

        public interface ITrackLayout
        {
            IFraction<IDistance>[] SectorsEnd { get; } // R3E SectorStartFactors

            IDistance Length()
            {
                return SectorsEnd[^1].Value;
            }
        }

        public interface ISession
        {
            SessionType Type { get; }

            SessionPhase Phase { get; }

            ISessionRequirements Requirements { get; }

            /// <summary>
            /// Pit speed limit for this session.
            /// </summary>
            /// <remarks>
            /// Different sessions could have different speed limits.
            /// </remarks>
            ISpeed PitSpeedLimit { get; }

            /// <summary>
            /// Session duration.
            /// </summary>
            /// <remarks>
            /// Not present during unlimited time practice.
            /// </remarks>
            ISessionDuration? Length { get; } // R3E NumberOfLaps, SessionTimeDuration

            /// <summary>
            /// Time since the session started.
            /// </summary>
            TimeSpan ElapsedTime { get; } // R3E Player.GameSimulationTime is 0.0 during replays but SessionTimeRemaining is present!

            IStartLights? StartLights { get; }

            ILapTime? BestLap { get; }
            
            ISectors? BestSectors { get; }
        }

        public enum SessionType
        {
            Practice,         // R3E, ACC, AC, AMS2
            Test,             // AMS2 ???
            Qualify,          // R3E, ACC, AC, AMS2
            Warmup,           // R3E, RF2 (rF2GamePhase)
            Race,             // R3E, ACC, AC, AMS2
            Hotlap,           // ACC, AC
            TimeAttack,       // ACC, AC, AMS2
            Drift,            // ACC, AC
            Drag,             // ACC, AC
            HotStint,         // ACC
            HotStintSuperPole // ACC
        }

        public enum SessionPhase
        {
            Garage,           // R3E, RF2
            Gridwalk,         // R3E, RF2
            Formation,        // R3E, AMS2 (eSessionState), RF2
            Countdown,        // R3E, RF2
            Started,          // R3E, AMS2 (eRaceState), RF2
            FullCourseYellow, // RF2
            Stopped,          // RF2
            Over              // R3E, AMS2 (eRaceState)
        }

        public interface ISessionRequirements
        {
            UInt32 MandatoryPitStops { get; } // 0 to disable

            /// <summary>
            /// Mandatory pit window.
            /// </summary>
            Interval<ISessionDuration>? PitWindow { get; } // R3E PitWindowEnd, PitWindowStart

            // TODO
            //  - driver swap, max stint, tyre change, refuel, ... https://www.acc-wiki.info/wiki/Server_Configuration#eventRules.json
            //  - cleared or not goes in the player or driver?
        }

        public interface ISessionDuration
        {
            public record LapsDuration(UInt32 Laps, TimeSpan? EstimatedTime) : ISessionDuration;
            
            public record TimeDuration(TimeSpan Time, UInt32? EstimatedLaps) : ISessionDuration; // might have an extra lap or not?
            
            /// <summary>
            /// Laps are added after the time duration.
            /// </summary>
            public record TimePlusLapsDuration(TimeSpan Time, UInt32 ExtraLaps, UInt32? EstimatedLaps) : ISessionDuration;

            // public record TimeOrLapsDuration(TimeSpan Time, UInt32 Laps) : IRaceDuration;
        }

        // TODO shall we have an array of Off/Red/Green? Or like for flags separate game and real (historic might be different)?
        // - F1 or Indycar (standing starts) doesn't show green light at the start
        //   https://www.fia.com/sites/default/files/regulation/file/03__Recommended_light_signals.pdf
        // - Indycar and NASCAR doesn't seem to have lights for rolling starts
        // - Nothing in this world seems to use iR's crazy alternating green lights
        // 
        // iR startHidden = null, startReady = Red 0/1, startSet = Red 1/1, startGo = Green 1/1
        public interface IStartLights
        {
            LightColour Colour { get; }
           
            IBoundedValue<UInt32> Lit { get; }
        }

        public enum LightColour
        {
            Red,
            Green
        }

        public interface IVehicle
        {
            UInt32 Id { get; } // R3E DriverData[].DriverInfo.SlotId

            String DriverName { get; } // R3E DriverData[].DriverInfo.Name

            Int32 ClassPerformanceIndex { get; } // R3E DriverData[].DriverInfo.ClassPerformanceIndex

            EngineType EngineType { get; }

            ControlType ControlType { get; }

            UInt32 PositionClass { get; } // R3E DriverData[].PlaceClass or PositionClass

            TimeSpan GapAhead { get; } // R3E DriverData[].TimeDeltaAhead
            
            TimeSpan GapBehind { get; } // R3E DriverData[].TimeDeltaBehind
            // TODO Have Gap.{Ahead|Behind} and ClassGap.{Ahead|Behind}?

            ISectors? BestSectors { get; } // R3E BestIndividualSectorTimeSelf.Sector* DriverData[].SectorTimeBestSelf.Sector*

            UInt32 CompletedLaps { get; } // R3E DriverData[].CompletedLaps

            Boolean CurrentLapValid { get; }

            ILapTime? CurrentLapTime { get; } // R3E DriverData[].LapTimeCurrentSelf for player

            ILapTime? PreviousLapTime { get; }
           
            ILapTime? PersonalBestLapTime { get; }

            IFraction<IDistance> CurrentLapDistance { get; } // R3E LapDistance + LapDistanceFraction


            /// <summary>
            /// World position.
            /// </summary>
            Vector3<IDistance> Location { get; } // R3E DriverData[].Position.*

            ISpeed Speed { get; }

            IDriver CurrentDriver { get; }

            // String TeamName { get; } // In ACC

            // IDriver Drivers { get; } // Only one per vehicle in R3E

            ICounter MandatoryPitStops { get; } // R3E DriverData[].PitStopStatus for 0 or 1

            IVehiclePit Pit { get; }
        }

        public enum EngineType
        {
            Unknown,
            Combustion,
            Electric,
            Hybrid
        }

        public enum ControlType
        {
            LocalPlayer,
            RemotePlayer,
            AI,
            Replay // ...or ghost
        };

        public interface IDriver
        {
            String Name { get; }
            // TODO ranking, ...
        }

        public interface IVehiclePit
        {
            Boolean InPitLane { get; } // R3E DriverData[].InPitlane

            /// <summary>
            /// Time since entering the pit lane. Available for an entire lap after pitting or until entering the pit lane again.
            /// </summary>
            /// <remarks>
            /// Not available in replay mode.
            /// </remarks>
            TimeSpan? PitLaneTime { get; } // R3E PitTotalDuration for player

            Boolean? InPitStall { get; } // R3E PitState for player

            TimeSpan? PitStallTime { get; } // R3E PitElapsedTime for player
        }

        public interface IPlayer
        {
            // R3E Player.Position
            // ...

            // R3E PitWindowStatus for Stopped and Completed

            IRawInputs RawInputs { get; }

            // TODO driver
            // IInputs Inputs { get; } // R3E always zero???

            IDrivingAids DrivingAids { get; }

            IVehicleSettings VehicleSettings { get; }

            IVehicleDamage VehicleDamage { get; }

            ITyre[][] Tyres { get; } // [[FL,FR],[RL,RR]]

            IFuel Fuel { get; }

            IEngine Engine { get; }

            // Where does P2P, KERS, DRS go?

            // Physics section?
            Vector3<IDistance> CgLocation { get; }

            Orientation Orientation { get; }

            Vector3<IAcceleration> Acceleration { get; } // R3E Player.LocalGforce.*, ACC/AC accG

            // TODO player or current vehicle?
            // this is odd because it's related to the player's car class. we could extract it in the session if we compute it outselves
            ILapTime? ClassBestLap { get; }        // R3E LapTimeBestLeaderClass

            ISectors? ClassBestSectors { get; } // R3E BestIndividualSectorTimeLeaderClass

            ISectors? PersonalBestSectors { get; } // R3E BestIndividualSectorTimeSelf

            TimeSpan? PersonalBestDelta { get; }   // R3E TimeDeltaBestSelf

            /// <summary>
            /// DRS information, if available on the vehicle.
            /// </summary>
            IActivationToggled? Drs { get; }

            /// <summary>
            /// Push-to-pass information, if available on the vehicle.
            /// </summary>
            IWaitTimeToggled? PushToPass { get; }

            IPlayerPit? Pit { get; }

            // TODO: Replace with something more realistic (see https://en.wikipedia.org/wiki/Racing_flags#Summary)
            //   but still keeping the original flag information if one is shown.
            //   iRacing uses the white flag for the final lap https://iracing.fandom.com/wiki/IRacing_Flags
            //   RaceRoom uses the white flag for slow car on track https://forum.sector3studios.com/index.php?threads/flag-rules.7614/
            // Missing information:
            //  - full course and sector yellows
            //  - yellow distance
            //  - caused yellow?
            Flags GameFlags { get; }

            // TODO: Replace along with flags. Missing information:
            //  - how much slow down
            //  - stop and go times
            //  - how much time deduction
            Penalties Penalties { get; }
        }

        // FIXME
        //public enum PitWindowStatus // TODO open/closed are for session, stopped/completed are for player... should really be about requirements and not just pit stops
        //{
        //    Closed,   // Pit stops are enabled, but you're not allowed to perform one right now
        //    Open,     // Allowed to perform a pit stop now
        //    Stopped,  // Currently performing the pit stop changes (changing driver, etc.)
        //    Completed // After the current mandatory pitstop have been completed
        //}

        public interface IRawInputs : IInputs
        {
            IAngle SteerWheelRange { get; } // R3E SteerWheelRangeDegrees
        }

        public interface IInputs
        {
            Double Steering { get; }

            Double Throttle { get; }

            Double Brake { get; }

            Double Clutch { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Factory ABS and TC are considered driving aids. ESP could be as well for a road car.
        /// </remarks>
        public interface IDrivingAids
        {
            // TODO add a class for raw, etc.?
            UInt32? AbsLevelRaw { get; }           // R3E AidSettings.Abs (-1 = N/A, 0 = off, 1 = on)

            Double? AbsLevelPercent { get; }       // Normalised to [0,1]

            Boolean AbsActive { get; }             // R3E AidSettings.Abs (5 = currently active)

            UInt32? TcLevelRaw { get; }            // R3E AidSettings.Tc (-1 = N/A, 0 = off, 1 = on) + TractionControlSetting (1 = 100%, 2 = 80%, ... 6 = 0%)

            Double? TcLevelPercent { get; }        // Normalised to [0,1]

            Boolean TcActive { get; }              // R3E AidSettings.Tc (5 = currently active)

            UInt32? EspLevelRaw { get; }           // R3E AidSettings.Esp (-1 = N/A, 0 = off, 1 = on low, 2 = on medium, 3 = on high)

            Double? EspLevelPercent { get; }       // Normalised to [0,1]

            Boolean EspActive { get; }             // R3E AidSettings.Esp (5 = currently active)

            Boolean? CountersteerEnabled { get; }  // R3E AidSettings.Countersteer (-1 = N/A, 0 = off, 1 = on)

            Boolean CountersteerActive { get; }    // R3E AidSettings.Countersteer (5 = currently active)

            Boolean? CorneringEnabled { get; }     // R3E AidSettings.Cornering (-1 = N/A, 0 = off, 1 = on)

            Boolean CorneringActive { get; }       // R3E AidSettings.Cornering (5 = currently active)
        }

        public interface IVehicleSettings
        {
            // TODO add a class for raw, etc.?
            UInt32? EngineMapRaw { get; }       // R3E EngineMapSetting

            Double? EngineMapPercent { get; }   // Normalised to [0,1] if we can

            UInt32? EngineBrakeRaw { get; }     // R3E EngineBrakeSetting

            Double? EngineBrakePercent { get; } // Normalised to [0,1] if we can

            // MGU-K...
        }

        // [0,1] where 1 is OK
        public interface IVehicleDamage
        {
            Double AerodynamicsPercent { get; }

            Double EnginePercent { get; }

            Double SuspensionPercent { get; }

            Double TransmissionPercent { get; }
        }

        public interface ITyre
        {
            // Compound
            // Pressure
            Double Dirt { get; } // [0,1] where 0 = no dirt

            Double Grip { get; } // [0,1] where 0 = no grip, 1 = max grip

            Double Wear { get; } // [0,1] where 1 is new

            ITemperaturesMatrix Temperatures { get; } // [[L,C,R]] not [[I,C,O]], when more layers (rF2) [Thread,...,Carcass]

            ITemperaturesSingle BrakeTemperatures { get; }
        }

        public interface ITemperaturesSingle
        {
            ITemperature CurrentTemperature { get; }

            ITemperature OptimalTemperature { get; }

            ITemperature ColdTemperature { get; }

            ITemperature HotTemperature { get; }
        }

        public interface ITemperaturesMatrix
        {
            ITemperature[][] CurrentTemperatures { get; }

            ITemperature OptimalTemperature { get; }

            ITemperature ColdTemperature { get; }

            ITemperature HotTemperature { get; }
        }

        public interface IFuel
        {
            Double Max { get; } // R3E FuelCapacity

            Double Left { get; } // R3E FuelLeft

            Double? PerLap { get; } // R3E FuelPerLap, ACC fuelXLap, AC to be inferred
        }

        public interface IEngine
        {
            /// <summary>
            /// Current rotational speed.
            /// </summary>
            IAngularSpeed Speed { get; } // R3E EngineRps

            /// <summary>
            /// Optimal rotational speed for upshifts.
            /// </summary>
            IAngularSpeed UpshiftSpeed { get; } // R3E UpshiftRps

            /// <summary>
            /// Maximum rotational speed.
            /// </summary>
            IAngularSpeed MaxSpeed { get; } // R3E MaxEngineRps
        }

        public interface IPlayerPit
        {
            PitState State { get; }

            PitAction Action { get; }

            TimeSpan DurationTotal { get; }

            TimeSpan DurationLeft { get; }
        }

        public enum PitState
        {
            Requested,      // Requested stop
            EnteredPitlane, // Entered pitlane heading for pitspot
            Pitspot,        // Stopped at pitspot
            ExitingPitlane  // Exiting pitspot heading for pit exit
        }

        [Flags]
        public enum PitAction
        {
            Preparing = 1 << 0,
            ServingPenalty = 1 << 1,
            DriverChange = 1 << 2,
            Refuelling = 1 << 3,
            ChangeFrontTyres = 1 << 4,
            ChangeRearTyres = 1 << 5,
            RepairFrontWing = 1 << 6,
            RepairRearWing = 1 << 7,
            RepairSuspension = 1 << 8
        }

        [Flags]
        public enum Flags // r3e not all available during replay (only black ond checquered)
        {
            Black = 1 << 1,
            BlackAndWhite = 1 << 2,
            Blue = 1 << 3,
            Checkered = 1 << 4,
            Green = 1 << 5,
            White = 1 << 6,
            Yellow = 1 << 7
        }

        [Flags]
        public enum Penalties
        {
            DriveThrough = 1 << 1,
            PitStop = 1 << 2, // ?
            SlowDown = 1 << 3,
            StopAndGo = 1 << 4,
            TimeDeduction = 1 << 5
        }
    }

    public interface ICounter
    {
        UInt32 Total { get; }

        UInt32 Left { get; }

        UInt32 Done { get; }
    }

    public interface IFraction<T> : IBoundedValue<T>
    {
        Double Fraction { get; }

        static IFraction<IDistance> Of(IDistance Total, Double Fraction) => new DistanceFraction(Total, Fraction);

        private record DistanceFraction(IDistance Total, Double Fraction) : IFraction<IDistance>
        {
            private Lazy<IDistance> _LazyValue = new Lazy<IDistance>(() => Total * Fraction);

            public IDistance Value => _LazyValue.Value;
        }
    }

    public interface IBoundedValue<T>
    {
        T Value { get; }

        T Total { get; }
    }

    public record BoundedValue<T>(T Total, T Value) : IBoundedValue<T>;

    public record Interval<T>(T Start, T Finish);

    public interface ILapTime
    {
        TimeSpan Overall { get; }

        ISectors Sectors { get; }
    }


    public interface ISectors
    {
        // Should probably keep track of this myself because games expose one or the other
        TimeSpan[] Individual { get; }

        TimeSpan[] Cumulative { get; }
    }

    public interface IActivationToggled
    {
        Boolean Available { get; }

        Boolean Engaged { get; }

        UInt32 ActivationsLeft { get; }
    }

    public interface IWaitTimeToggled : IActivationToggled
    {
        TimeSpan EngagedTimeLeft { get; }

        TimeSpan WaitTimeLeft { get; }
    }
}
