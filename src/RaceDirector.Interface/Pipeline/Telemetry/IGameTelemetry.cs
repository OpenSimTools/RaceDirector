using System;
using System.Linq;
using RaceDirector.Pipeline.Telemetry.Physics;

namespace RaceDirector.Pipeline.Telemetry
{
    // TODO
    // - What happens in rF2 or ACC before swapping driver?
    //   - rF2 procedure https://simracing-gp.net/topic-568/how_to_driver_swap_procedure
    //   - rF2 telemetry https://youtu.be/N_S6stecqkc

    namespace V0
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Sectors are a mapped as a generic array. Oval racing doesn't have sectors, Rally usually has more than 3.
        /// 
        /// Playing has Player that is/has also a Driver, replay has CurrentDriver, menu neither.
        /// </remarks>
        public interface IGameTelemetry
        {
            GameState GameState { get; }

            bool? UsingVR { get; }

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
            /// It may contain higher resolution telemetry in some games.
            /// </remarks>
            IFocusedVehicle? FocusedVehicle { get; }

            /// <summary>
            /// Player information if available.
            /// </summary>
            /// <remarks>
            /// Usually when player's car is focused.
            /// </remarks>
            IPlayer? Player { get; }
        }

        public enum GameState
        {
            Unknown,
            Driving,
            Paused,  // R3E GamePaused - is the clock ticking? (-1 in main menu, 0 in replay, 1 in single player menu, 0 in multi player menu)
            Menu,    // R3E GameInMenus (-1 in main menu, 1 in single or multi player menu, 1 in monitor)
            Replay   // R3E GameInReplay (-1 in main menu, 1 in replay, 0 in monitor)
        }

        public interface IEvent
        {
            ITrackLayout TrackLayout { get; }

            /// <summary>
            /// Fuel burning rate. 0.0 disabled, 1.0 is normal, 2.0 double, etc.
            /// </summary>
            double FuelRate { get; } // R3E FuelUseActive (it's a multiplier!)

            //double TireRate { get; }

            //double MechanicalDamage { get; }
        }

        public interface ITrackLayout
        {
            IFraction<IDistance>[] SectorsEnd { get; } // R3E SectorStartFactors

            IDistance? Length { get => SectorsEnd.LastOrDefault()?.Total; }
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
            /// Is the pit lane open?
            /// </summary>
            /// <remarks>
            /// Some game modes disable the pit lane (e.g. R3E leaderboard).
            /// In some games pits are closed at the beginning of the race.
            /// </remarks>
            bool PitLaneOpen { get; } // R3E PitWindowStatus > 0

            /// <summary>
            /// Session duration.
            /// </summary>
            /// <remarks>
            /// Not present during unlimited time practice.
            /// </remarks>
            ISessionDuration? Length { get; } // R3E NumberOfLaps, SessionTimeDuration

            /// <summary>
            /// Elapsed time in this session
            /// </summary>
            /// <remarks>
            /// Some games, like R3E, do not expose this at all. It can be inferred from the time remaining for time-based race.
            /// </remarks>
            TimeSpan? ElapsedTime { get; }

            /// <summary>
            /// Time remaining in the session.
            /// </summary>
            /// <remarks>
            /// Menu might have a session time if it's in game menu and the session has started, as opposed to the main menu.
            /// </remarks>
            TimeSpan? TimeRemaining { get; } // R3E Player.GameSimulationTime is 0.0 during replays but SessionTimeRemaining is present!

            /// <summary>
            /// Wait time after the session is finished.
            /// </summary>
            TimeSpan? WaitTime { get; }

            IStartLights? StartLights { get; }

            ILapTime? BestLap { get; }
            
            ISectors? BestSectors { get; }

            ISessionFlags Flags { get; }
        }

        public enum SessionType
        {
            Practice,         // R3E, ACC, AC, AMS2
            Test,             // AMS2 ???
            Qualify,          // R3E, ACC, AC, AMS2
            Warmup,           // R3E, RF2 (rF2GamePhase)
            Race,             // R3E, ACC, AC, AMS2
            HotLap,           // ACC, AC
            TimeAttack,       // ACC, AC, AMS2
            Drift,            // ACC, AC
            Drag,             // ACC, AC
            HotStint,         // ACC
            HotStintSuperPole // ACC
        }

        public enum SessionPhase
        {
            Unknown = 0,
            Garage = 1,           // R3E, RF2
            GridWalk = 2,         // R3E, RF2
            Formation = 3,        // R3E, AMS2 (eSessionState), RF2
            Countdown = 4,        // R3E, RF2
            Started = 5,          // R3E, AMS2 (eRaceState), RF2
            FullCourseYellow = 6, // RF2
            Stopped = 7,          // RF2
            Over = 8              // R3E, AMS2 (eRaceState)
        }

        public interface ISessionRequirements
        {
            /// <summary>
            /// Mandatory pit window.
            /// </summary>
            Interval<IPitWindowBoundary>? PitWindow { get; } // R3E PitWindowEnd, PitWindowStart

            /// <summary>
            /// Mandatory pit stops to be performed (in the pit window if configured)
            /// </summary>
            /// <remarks>
            /// Set to 0 to disable.
            /// </remarks>
            uint MandatoryPitStops { get; }

            /// <summary>
            /// Minimum requirements for mandatory pit stops to count.
            /// </summary>
            MandatoryPitRequirements MandatoryPitRequirements { get; }

            // TODO
            //  - driver swap, max stint, ...
            //    ACC https://www.acc-wiki.info/wiki/Server_Configuration#eventRules.json
        }

        [Flags]
        public enum MandatoryPitRequirements // R3E PitStopStatus in DriverData tells if it's 2 or 4 wheels
        {
            None       = 0,
            Fuel       = 1 << 0,
            DriverSwap = 1 << 1,
            TwoTires   = 1 << 2,
            FourTires  = 1 << 3
        }

        public interface IPitWindowBoundary : IComparable<IRaceInstant> { }

        public interface ISessionDuration { }

        public static class RaceDuration {

            public record LapsDuration(uint Laps, TimeSpan? EstimatedTime) : ISessionDuration, IPitWindowBoundary
            {
                int IComparable<IRaceInstant>.CompareTo(IRaceInstant? other) => Laps.CompareTo(other?.Laps);
            }

            public record TimeDuration(TimeSpan Time, uint? EstimatedLaps) : ISessionDuration, IPitWindowBoundary
            {
                int IComparable<IRaceInstant>.CompareTo(IRaceInstant? other) => Time.CompareTo(other?.Time);
            }

            /// <summary>
            /// Laps are added after the time duration.
            /// </summary>
            public record TimePlusLapsDuration(TimeSpan Time, uint ExtraLaps, uint? EstimatedLaps) : ISessionDuration;

            // public record TimeOrLapsDuration(TimeSpan Time, uint Laps) : ISessionDuration;
        }

        // TODO shall we have an array of Off/Red/Green? Or separate game and real (historic might be different)?
        // - F1 or Indycar (standing starts) doesn't show green light at the start
        //   https://www.fia.com/sites/default/files/regulation/file/03__Recommended_light_signals.pdf
        // - Indycar and NASCAR doesn't seem to have lights for rolling starts
        // - Nothing in this world seems to use iR's crazy alternating green lights
        // 
        // iR startHidden = null, startReady = Red 0/1, startSet = Red 1/1, startGo = Green 1/1
        public interface IStartLights
        {
            LightColor Color { get; }
           
            IBoundedValue<uint> Lit { get; }
        }

        public enum LightColor
        {
            Red,
            Green
        }

        public interface ISessionFlags
        {
            TrackFlags Track { get; }
            SectorFlags[] Sectors { get; }
            LeaderFlags Leader { get; }
        }

        public enum TrackFlags
        {
            None = 0,
            Green,
            Red,
            FCY,
            SC,
            VSC
            //Code60
        }

        public enum SectorFlags
        {
            None = 0,
            Yellow
        }

        public enum LeaderFlags
        {
            None = 0,
            /// <summary>
            /// Final lap.
            /// </summary>
            White,
            /// <summary>
            /// Finished the race.
            /// </summary>
            Checkered
        }

        public interface IVehicle
        {
            uint Id { get; } // R3E DriverData[].DriverInfo.SlotId

            int ClassPerformanceIndex { get; } // R3E DriverData[].DriverInfo.ClassPerformanceIndex

            IRacingStatus RacingStatus { get; } // R3E DriverData[].FinishStatus DriverData[].PenaltyReason ACC penalty

            EngineType EngineType { get; }

            ControlType ControlType { get; }

            uint Position { get; }      // R3E DriverData[].Place or Position
            uint PositionClass { get; } // R3E DriverData[].PlaceClass or PositionClass

            /// <summary>
            /// Gap to the car in front, if any.
            /// </summary>
            /// <remarks>
            /// Always positive.
            /// </remarks>
            TimeSpan? GapAhead { get; } // R3E DriverData[].TimeDeltaAhead

            /// <summary>
            /// Gap to the car in behind, if any.
            /// </summary>
            /// <remarks>
            /// Always positive.
            /// </remarks>
            TimeSpan? GapBehind { get; } // R3E DriverData[].TimeDeltaBehind
            // TODO Have Gap.{Ahead|Behind} and ClassGap.{Ahead|Behind}?

            uint CompletedLaps { get; } // R3E DriverData[].CompletedLaps

            LapValidState LapValid { get; }

            ILapTime? CurrentLapTime { get; } // R3E DriverData[].LapTimeCurrentSelf for player

            ILapTime? PreviousLapTime { get; }
           
            ILapTime? BestLapTime { get; }

            ISectors? BestSectors { get; } // R3E BestIndividualSectorTimeSelf.Sector* DriverData[].SectorTimeBestSelf.Sector*

            IFraction<IDistance> CurrentLapDistance { get; } // R3E LapDistance + LapDistanceFraction


            /// <summary>
            /// World position.
            /// </summary>
            Vector3<IDistance> Location { get; } // R3E DriverData[].Position.*

            // R3E CarOrientation for the current vehicle only!
            // High res of Player.Orientation in monitor is player is focused
            Orientation? Orientation { get; }

            ISpeed Speed { get; }

            IDriver CurrentDriver { get; }

            // String TeamName { get; } // In ACC

            // IDriver Drivers { get; } // Only one per vehicle in R3E

            IVehiclePit Pit { get; }

            /// <summary>
            /// Flags as displayed by the game.
            /// </summary>
            /// <remarks>
            /// Some games (AMS2) show partial flags for all cars, others only for the player's car.
            /// </remarks>
            IVehicleFlags Flags { get; }

            /// <summary>
            /// Penalties for the current vehicle.
            /// </summary>
            IPenalty[] Penalties { get; }
        }

        public interface IRacingStatus
        {
            public static readonly IRacingStatus Unknown = new SimpleStatus();
            public static readonly IRacingStatus Racing = new SimpleStatus();
            public static readonly IRacingStatus Finished = new SimpleStatus();
            public static readonly IRacingStatus DNF = new SimpleStatus();
            public static readonly IRacingStatus DNQ = new SimpleStatus();
            public static readonly IRacingStatus DNS = new SimpleStatus();

            private class SimpleStatus : IRacingStatus { };

            public record DQ(DQReason Reason) : IRacingStatus;

            public enum DQReason
            {
                Unknown,
                Cutting, // R3E, ACC
                EnteringPitsUnderRed, // R3E, ACC PitEntry
                ExceededDriverStintLimit, // ACC
                ExitingPitsUnderRed, // R3E, ACC PitExit
                FailedDriverSwap, // R3E
                FalseStart, // R3E
                IgnoredBlueFlags, // R3E
                IgnoredDriveThroughPenalty, // R3E
                IgnoredDriverStint, // ACC
                IgnoredPitstopPenalty, // R3E
                IgnoredStopAndGoPenalty, // R3E
                IgnoredTimePenalty, // R3E 
                LappedTooManyTimes, // R3E
                IgnoredMandatoryPit, // R3E, ACC
                PitlaneSpeeding, // R3E, ACC
                ThreeDriveThroughsInLap, // R3E
                WrongWay, // R3E, ACC
                OffTrackBehaviour // ACC Trolling
            }
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
            String Name { get; } // R3E DriverData[].DriverInfo.Name
            // TODO ranking, ...
        }

        public interface IVehiclePit
        {
            uint StopsDone { get; }
            uint MandatoryStopsDone { get; } // R3E DriverData[].PitStopStatus for 0 or 1
            PitLanePhase? PitLanePhase { get; } // R3E PitState (player) or DriverData[].InPitlane and inference

            /// <summary>
            /// Time since entering the pit lane. When the vehicle exits, it stays
            /// available for an entire lap or until entering the pit lane again.
            /// </summary>
            /// <remarks>
            /// It can be inferred when not available (ACC, R3E in replay mode, etc.)
            /// </remarks>
            TimeSpan? PitLaneTime { get; } // R3E PitTotalDuration for player

            /// <summary>
            /// Time since stopping at the pit stall. When the vehicle leaves it, it stays
            /// available for an entire lap or until entering the pit lane again.
            /// </summary>
            /// <remarks>
            /// It can be inferred when not available (ACC, R3E in replay mode or for other drivers, etc.)
            /// </remarks>
            TimeSpan? PitStallTime { get; } // R3E PitElapsedTime for player
        }

        public enum PitLanePhase
        {
            Entered,   // Entered pit lane
            Stopped,   // Stopped at the pit stall
            Exiting    // Heading for pit exit
        }

        public interface IPenalty
        {
            PenaltyType Type { get; }
            PenaltyReason Reason { get; }
        }

        public enum PenaltyType
        {
            Unknown,
            SlowDown,
            TimeDeduction, // ACC's PostRaceTime
            DriveThrough,
            PitStop,
            StopAndGo10, // R3E?
            StopAndGo20,
            StopAndGo30,
            GivePositionBack,
            RemoveBestLaptime // ACC
        }

        public enum PenaltyReason
        {
            Unknown, // ACC TP
            Cutting, // R3E DT, SG (single, mult), SD (single, mult), ACC DT, SG10, SG20, SG30, BL
            DrivingTooSlow, // R3E DT
            FalseStart, // R3E DT, ACC DT
            IgnoredBlueFlags, // R3E DT
            IgnoredDriverStint, // ACC DT
            IgnoredMinimumPitstopDuration, // R3E TP
            IgnoredPitstopPenalty, // R3E PS
            IgnoredPitstopWindow, // R3E PS
            IgnoredSlowDown, // R3E DT
            IllegallyPassedBeforeFinish, // R3E DT
            IllegallyPassedBeforeGreen, // R3E DT
            IllegallyPassedBeforePitEntrance, // R3E DT
            PitlaneSpeeding, // R3E DT, ACC DT, SG10, SG20, SG30, BL
            ServedMandatoryPitstopLate, // R3E TP
            YellowFlagOvertake // R3E SG
        }

        public interface IFocusedVehicle : IVehicle
        {
            IInputs? Inputs { get; }
        }

        // r3e not all available during replay (only black ond checquered)
        public interface IVehicleFlags
        {
            IGreen? Green { get; }
            IBlue? Blue { get; }
            IYellow? Yellow { get; }
            // IYellowRedStriped { get; }
            IWhite? White { get; }
            IFlag? Checkered { get; }
            // IGreenWhiteCheckered GreenWhiteCheckered { get; }
            /// <summary>
            /// It can mean Disqualified (FIA, with white cross NASCAR and IndyCar) or ReturnToPits (NASCAR and IndyCar).
            /// </summary>
            /// <remarks>
            /// iRacing uses black flag for penalties, other games use the per-bend black and white.
            /// </remarks>
            IFlag? Black { get; }

            /// <summary>
            /// Per-bend black and white for unsportsmanlike conduct.
            /// </summary>
            /// <remarks>
            /// Most games use this flag for penalties.
            /// </remarks>
            IBlackWhite? BlackWhite { get; }

            public interface IGreen : IFlag
            {
                GreenReason Reason { get; }
            }

            public enum GreenReason
            {
                Unknown,
                RaceStart,
                EndOfYellowSection,
                ResumeRace
            }

            public interface IBlue : IFlag
            {
                BlueReason Reason { get; }
            }

            public enum BlueReason
            {
                Unknown,
                GiveWay,         // NASCAR, IndyCar with yellow stripe
                TrackObstruction // NASCAR road course
            }

            public interface IYellow : IFlag
            {
                YellowReason Reason { get; }
                // IDistance ClosestOnTrack { get; }
                // bool? CausedIt { get; }
            }

            public enum YellowReason
            {
                Unknown,
                SingleVehicle,
                MultipleVehicles,
                TrackObstruction // double waved yellows?
            }

            public interface IWhite : IFlag
            {
                WhiteReason Reason { get; }
            }

            public enum WhiteReason
            {
                Unknown,
                /// <summary>
                /// White flag on track: slow car ahead.
                /// </summary>
                /// <remarks>
                /// <a href="https://forum.sector3studios.com/index.php?threads/flag-rules.7614/">RaceRoom</a> uses the white flag for slow car on track.
                /// </remarks>
                SlowCarAhead,
                /// <summary>
                /// White flag on the main straight: final lap.
                /// </summary>
                /// <remarks>
                /// <a href="https://iracing.fandom.com/wiki/IRacing_Flags">iRacing</a> uses the white flag only for the final lap.
                /// </remarks>
                LastLap
            }

            public interface IBlackWhite : IFlag
            {
                BlackWhiteReason Reason { get; }
            };

            public enum BlackWhiteReason
            {
                Unknown,
                Cutting, // R3E
                IgnoredBlueFlags, // R3E
                WrongWay // R3E
            }

            public interface IFlag { }
        }

        public interface IPlayer
        {
            // R3E Player.Position
            // ...

            // R3E PitWindowStatus for Stopped and Completed

            IRawInputs RawInputs { get; } // ACC physics.gas, physics.brake, ...

            // TODO driver
            // IInputs Inputs { get; } // R3E always zero???

            IDrivingAids DrivingAids { get; }

            IVehicleSettings VehicleSettings { get; }

            IVehicleDamage VehicleDamage { get; }

            ITire[][] Tires { get; } // [[FL,FR],[RL,RR]]
            
            uint? TireSet { get; }

            IFuel Fuel { get; }

            IEngine Engine { get; }

            // Where does P2P, KERS, DRS go?

            // Physics section?
            Vector3<IDistance> CgLocation { get; }

            Orientation Orientation { get; }

            /// <summary>
            /// Acceleration relative to the Orientation.
            /// </summary>
            Vector3<IAcceleration> LocalAcceleration { get; } // R3E Player.LocalGforce.*, ACC/AC accG

            // TODO player or current vehicle?
            // this is odd because it's related to the player's car class. we could extract it in the session if we compute it outselves
            ILapTime? ClassBestLap { get; } // R3E LapTimeBestLeaderClass

            ISectors? ClassBestSectors { get; } // R3E BestIndividualSectorTimeLeaderClass

            /// <summary>
            /// Player's fastest sector times, not considering overall lap time.
            /// </summary>
            ISectors? PersonalBestSectors { get; } // R3E BestIndividualSectorTimeSelf

            /// <summary>
            /// Delta between the current lap and the player's best lap.
            /// </summary>
            TimeSpan? PersonalBestDelta { get; }   // R3E TimeDeltaBestSelf (works also in monitor but only for player's car)

            /// <summary>
            /// DRS information, if available on the vehicle.
            /// </summary>
            IActivationToggled? Drs { get; }

            /// <summary>
            /// Push-to-pass information, if available on the vehicle.
            /// </summary>
            IWaitTimeToggled? PushToPass { get; }

            PlayerPitStopStatus PitStopStatus { get; }

            IPlayerWarnings Warnings { get; }

            bool? OvertakeAllowed { get; }
            
            IPitMenu PitMenu { get; }
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
            double Steering { get; }

            IAngle SteerWheelRange { get; } // R3E SteerWheelRangeDegrees
        }

        public interface IInputs
        {
            // TODO Add nullable steering? R3E doesn't show it but RF2 and AMS2 do.

            double Throttle { get; }

            double Brake { get; }

            double Clutch { get; }
        }

        /// <summary>
        /// On-board electronic or game software assists.
        /// </summary>
        /// <remarks>
        /// Factory ABS and TC are considered driving aids. ESP could be as well for a road car.
        /// </remarks>
        public interface IDrivingAids
        {
            // R3E AidSettings.Abs (-1 = N/A, 0 = off, 1 = on, 5 = currently active)
            // rF2 mAntiLockBrakes (0 = off, 2 = high)
            IAid? Abs { get; } // Null if not available

            // R3E
            //     AidSettings.Tc (-1 = N/A, 0 = off, 1 = on, 5 = currently active)
            //     TractionControlSetting (-1, 1 - 6)
            //     TractionControlPercent (-1.0, 0.0 - 100.0)
            // rF2 mTractionControl (0 = off, 3 = high)
            ITractionControl? Tc { get; }

            // R3E AidSettings.Esp (-1 = N/A, 0 = off, 1 = on low, 2 = on medium, 3 = on high, 5 = currently active)
            IAid? Esp { get; }

            // R3E AidSettings.Countersteer (-1 = N/A, 0 = off, 1 = on, 5 = currently active)
            IAid? Countersteer { get; }

            // R3E AidSettings.Cornering (-1 = N/A, 0 = off, 1 = on, 5 = currently active)
            IAid? Cornering { get; }

            // rF2 has a lot more
        }

        public interface ITractionControl : IAid
        {
             uint? Cut { get; }
        }

        public interface IAid
        {
            /// <summary>
            /// Numeric setting.
            /// </summary>
            uint Level { get; }

            /// <summary>
            /// Aid is actively altering the driver's inputs.
            /// </summary>
            bool Active { get; }
        }

        public interface IVehicleSettings
        {
            /// <summary>
            /// ECU (Engine control unit) or EM (Engine map).
            /// </summary>
            /// <remarks>
            /// ACC comes with real ECU settings, not percentages https://www.assettocorsa.net/forum/index.php?threads/ecu-maps-implementation.54472/
            /// </remarks>
            uint? EngineMap { get; } // R3E EngineMapSetting

            uint? EngineBrakeReduction { get; } // R3E EngineBrakeSetting

            // MGU-K...
        }

        // [0,1] where 1 is OK
        public interface IVehicleDamage
        {
            double AerodynamicsPercent { get; }

            double EnginePercent { get; }

            double SuspensionPercent { get; }

            double TransmissionPercent { get; }
        }

        public interface ITire
        {
            TireCompound Compound { get; }

            IPressure Pressure { get; }

            double Dirt { get; } // [0,1] where 0 = no dirt

            double Grip { get; } // [0,1] where 0 = no grip, 1 = max grip

            double Wear { get; } // [0,1] where 1 is new

            ITemperaturesMatrix Temperatures { get; } // [[L,C,R]] not [[I,C,O]], when more layers (rF2) [Thread,...,Carcass]

            ITemperaturesSingle BrakeTemperatures { get; }
        }

        public enum TireCompound
        {
            Unknown,
            Dry,
            Wet
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
            ITemperature[][] CurrentTemperatures { get; } // TODO specify what this array means

            ITemperature OptimalTemperature { get; }

            ITemperature ColdTemperature { get; }

            ITemperature HotTemperature { get; }
        }

        // TODO define quantities (litres, gallons, etc.)
        public interface IFuel
        {
            ICapacity Max { get; } // R3E FuelCapacity

            ICapacity Left { get; } // R3E FuelLeft

            ICapacity? PerLap { get; } // R3E FuelPerLap, ACC fuelXLap, AC to be inferred
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

        public enum LapValidState
        {
            Unknown,
            Invalid,
            Valid,
            // CurrentAndNextInvalid, // Does any game expose this information?
        }

        [Flags]
        public enum PlayerPitStopStatus
        {
            None = 0,

            Requested = 1 << 0,
            Preparing = 1 << 1,

            ServingPenalty  = 1 << 3,
            SwappingDrivers = 1 << 4,
            
            Refuelling = 1 << 9,

            ChangingFrontTires = 1 << 17,
            ChangingRearTires  = 1 << 18,

            RepairingBodywork   = 1 << 27,
            RepairingFrontWing  = 1 << 28,
            RepairingRearWing   = 1 << 29,
            RepairingSuspension = 1 << 30
        }

        public interface IPlayerWarnings
        {
            IBoundedValue<uint>? IncidentPoints { get; } // R3E IncidentPoints / MaxIncidentPoints
            IBoundedValue<uint>? BlueFlagWarnings { get; } // R3E Flags.BlackAndWhite
            // TrackLimitWarnings // ACC does not expose this in telemetry
            uint GiveBackPositions { get; } // R3E YellowPositionsGained
        }

        public interface IPitMenu
        {
            PitMenuFocusedItem FocusedItem { get; }
            PitMenuSelectedItems SelectedItems { get; }
            ICapacity? FuelToAdd { get; }
            uint? StrategyTireSet { get; }
            uint? TireSet { get; }
            IPressure[][] TirePressures { get; }
        }

        public enum PitMenuFocusedItem
        {
            None        = 0,
            Unavailable = 0,

            PitStopRequest = 1,
            ServePenalty   = 3,
            DriverSwap     = 4,

            // Adjustments
            Fuel                = 9,
            FrontWingAdjustment = 10,
            RearWingAdjustment  = 11,

            // Change
            Tires       = 16,
            FrontTires  = 17,
            RearTires   = 18,
            Brakes      = 19,
            FrontBrakes = 20,
            RearBrakes  = 21,

            // Fix damage
            AllDamage        = 26,
            BodyworkDamage   = 27,
            FrontWingDamage  = 28,
            RearWingDamage   = 29,
            SuspensionDamage = 30
        }
        
        [Flags]
        public enum PitMenuSelectedItems
        {
            None        = 0,
            Unavailable = 0,

            ServePenalty = 1 << 3,
            DriverSwap   = 1 << 4,

            // Adjustments
            Fuel                = 1 << 9,
            FrontWingAdjustment = 1 << 10,
            RearWingAdjustment  = 1 << 11,

            // Change
            Tires       = FrontTires | RearTires,
            FrontTires  = 1 << 17,
            RearTires   = 1 << 18,
            Brakes      = FrontBrakes | RearBrakes,
            FrontBrakes = 1 << 20,
            RearBrakes  = 1 << 21,

            // Fix damage
            AllDamage        = BodyworkDamage | SuspensionDamage,
            BodyworkDamage   = FrontWingDamage | RearWingDamage,
            FrontWingDamage  = 1 << 28,
            RearWingDamage   = 1 << 29,
            SuspensionDamage = 1 << 30
        }
    }

    public interface IFraction<T> : IBoundedValue<T>
    {
        double Fraction { get; }
    }

    public interface IBoundedValue<T>
    {
        T Value { get; }

        T Total { get; }
    }

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
        /// <summary>
        /// It can be activated.
        /// </summary>
        bool Available { get; }

        /// <summary>
        /// Currently activated.
        /// </summary>
        bool Engaged { get; }

        /// <summary>
        /// Number of activations left.
        /// </summary>
        IBoundedValue<uint>? ActivationsLeft { get; }
    }

    public interface IWaitTimeToggled : IActivationToggled
    {
        /// <summary>
        /// Time left on the activation.
        /// </summary>
        TimeSpan EngagedTimeLeft { get; }

        /// <summary>
        /// Time left before it can be activated.
        /// </summary>
        /// <remarks>
        /// It might not be available even if this is zero. Think of P2P during qualifying.
        /// </remarks>
        TimeSpan WaitTimeLeft { get; }
    }

    public interface IRaceInstant {
        TimeSpan? Time { get; }
        uint Laps { get; }

        /// <summary>
        /// Returns if the race instant is within a certain interval.
        /// </summary>
        /// <param name="boundary"></param>
        /// <returns>If the instant is within the interval.</returns>
        /// <remarks>
        /// Interval start is included, finish excluded.
        /// </remarks>
        bool IsWithin<T>(Interval<T> boundary) where T : IComparable<IRaceInstant>;
    }
}