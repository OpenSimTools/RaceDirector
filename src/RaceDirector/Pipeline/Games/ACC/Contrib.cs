//////////////////////////////////////////////////////////////////////////
// Based on the official documentation from Kunos
// https://www.assettocorsa.net/forum/index.php?threads/acc-shared-memory-documentation.59965/
// 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain.We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors.We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to<http://unlicense.org>
// 
//////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace RaceDirector.Pipeline.Games.ACC;

public static class Contrib
{
    public static class Constant
    {
        public const string SharedMemoryPhysicsName = "Local\\acpmf_physics";
        public const string SharedMemoryGraphicName = "Local\\acpmf_graphics";
        public const string SharedMemoryStaticName = "Local\\acpmf_static";

        public const int SmVersionMajor = 1;
        public const int SmVersionMinor = 9;

        public enum FlagType
        {
            NoFlag = 0,
            BlueFlag = 1,
            YellowFlag = 2,
            BlackFlag = 3,
            WhiteFlag = 4,
            CheckeredFlag = 5,
            PenaltyFlag = 6,
            GreenFlag = 7,
            OrangeFlag = 8
        }

        public enum PenaltyType
        {
            None = 0,
            DriveThroughCutting = 1,
            StopAndGo10Cutting = 2,
            StopAndGo20Cutting = 3,
            StopAndGo30Cutting = 4,
            DisqualifiedCutting = 5,
            RemoveBestLaptimeCutting = 6,
            DriveThroughPitSpeeding = 7,
            StopAndGo10PitSpeeding = 8,
            StopAndGo20PitSpeeding = 9,
            StopAndGo30PitSpeeding = 10,
            DisqualifiedPitSpeeding = 11,
            RemoveBestLaptimePitSpeeding = 12,
            DisqualifiedIgnoredMandatoryPit = 13,
            PostRaceTime = 14,
            DisqualifiedTrolling = 15,
            DisqualifiedPitEntry = 16,
            DisqualifiedPitExit = 17,
            DisqualifiedWrongway = 18,
            DriveThroughIgnoredDriverStint = 19,
            DisqualifiedIgnoredDriverStint = 20,
            DisqualifiedExceededDriverStintLimit = 21
        }

        public enum SessionType
        {
            Unknown = -1,
            Practice = 0,
            Qualify = 1,
            Race = 2,
            Hotlap = 3,
            Timeattack = 4,
            Drift = 5,
            Drag = 6,
            Hotstint = 7,
            HotstintSuperpole = 8
        }

        public enum Status
        {
            Off = 0,
            Replay = 1,
            Live = 2,
            Pause = 3
        }

        public enum TrackGripStatus
        {
            Green = 0,
            Fast = 1,
            Optimum = 2,
            Greasy = 3,
            Damp = 4,
            Wet = 5,
            Flooded = 6
        }

        public enum RainIntensity
        {
            NoRain = 0,
            Drizzle = 1,
            LightRain = 2,
            MediumRain = 3,
            HeavyRain = 4,
            Thunderstorm = 5
        }
    }

    public static class Data
    {
        public struct Shared {
            public SPageFilePhysics Physics;
            public SPageFileGraphic Graphic;
            public SPageFileStatic Static;
        }

        /// <summary>
        /// The following members change at each graphic step. They all refer to
        /// the player’s car.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        [Serializable]
        public struct SPageFilePhysics
        {
            public int PacketId; // Current step index
            public float Gas; // Gas pedal input value (from -0 to 1.0)
            public float Brake; // Brake pedal input value (from -0 to 1.0)
            public float Fuel; // Amount of fuel remaining in kg [Wrong: it's in liters!]
            public int Gear; // Current gear
            public int Rpm; // Engine revolutions per minute
            public float SteerAngle; // Steering input value (from -1.0 to 1.0)
            public float SpeedKmh; // Car speed in km/h
            public Coords Velocity; // Car velocity vector in global coordinates
            public Coords AccG; // Car acceleration vector in global coordinates
            public Wheels<float> WheelSlip; // Tyre slip for each tyre
            public Wheels<float> WheelLoad; // Wheel load for each tyre
            public Wheels<float> WheelPressure; // Tyre pressure
            public Wheels<float> WheelAngularSpeed; // Wheel angular speed in rad/s
            public Wheels<float> TyreWear; // Tyre wear
            public Wheels<float> TyreDirtyLevel; // Dirt accumulated on tyre surface
            public Wheels<float> TyreCoreTemp; // Tyre rubber core temperature
            public Wheels<float> CamberRAD; // Wheels camber in radians
            public Wheels<float> SuspensionTravel; // Suspension travel
            public float Drs; // DRS on
            public float Tc; // TC in action
            public float Heading; // Car yaw orientation
            public float Pitch; // Car pitch orientation
            public float Roll; // Car roll orientation
            public float CgHeight; // Centre of gravity height
            public Damage CarDamage; // Car damage
            public int NumberOfTyresOut; // Number of tyres out of track
            public int PitLimiterOn; // Pit limiter is on
            public float Abs; // ABS in action
            public float KersCharge; // Not used in ACC
            public float KersInput; // Not used in ACC
            public int AutoshifterOn; // Automatic transmission on
            public RideHeight RideHeight; // Ride height
            public float TurboBoost; // Car turbo level
            public float Ballast; // Car ballast in kg / Not implemented
            public float AirDensity; // Air density
            public float AirTemp; // Air temperature
            public float RoadTemp; // Road temperature
            public Coords LocalAngularVel; // Car angular velocity vector in local coordinates
            public float FinalFF; // Force feedback signal
            public float PerformanceMeter; // Not used in ACC
            public int EngineBrake; // Not used in ACC
            public int ErsRecoveryLevel; // Not used in ACC
            public int ErsPowerLevel; // Not used in ACC
            public int ErsHeatCharging; // Not used in ACC
            public int ErsIsCharging; // Not used in ACC
            public float KersCurrentKJ; // Not used in ACC
            public int DrsAvailable; // Not used in ACC
            public int DrsEnabled; // Not used in ACC
            public Wheels<float> BrakeTemp; // Brake discs temperatures
            public float Clutch; // Clutch pedal input value (from -0 to 1.0)
            public Wheels<float> TyreTempI; // Not shown in ACC
            public Wheels<float> TyreTempM; // Not shown in ACC
            public Wheels<float> TyreTempO; // Not shown in ACC
            public int IsAIControlled; // Car is controlled by the AI
            public Wheels<Coords> TyreContactPoint; // Tyre contact point global coordinates
            public Wheels<Coords> TyreContactNormal; // Tyre contact normal
            public Wheels<Coords> TyreContactHeading; // Tyre contact heading
            public float BrakeBias; // Front brake bias, see Appendix 4
            public Coords LocalVelocity; // Car velocity vector in local coordinates
            public int P2PActivation; // Not used in ACC
            public int P2PStatus; // Not used in ACC
            public float CurrentMaxRpm; // Maximum engine rpm
            public Wheels<float> Mz; // Not shown in ACC
            public Wheels<float> Fx; // Not shown in ACC
            public Wheels<float> Fy; // Not shown in ACC
            public Wheels<float> SlipRatio; // Tyre slip ratio in radians
            public Wheels<float> SlipAngle; // Tyre slip angle
            public int TcinAction; // TC in action
            public int AbsInAction; // ABS in action
            public Wheels<float> SuspensionDamage; // Suspensions damage levels
            public Wheels<float> TyreTemp; // Tyres core temperatures
            public float WaterTemp; // Water Temperature
            public Wheels<float> BrakePressure; // Brake pressure see Appendix 2
            public int FrontBrakeCompound; // Brake pad compund front
            public int RearBrakeCompound; // Brake pad compund rear
            public Wheels<float> PadLife; // Brake pad wear
            public Wheels<float> DiscLife; // Brake disk wear
            public int IgnitionOn; // Ignition switch set to on?
            public int StarterEngineOn; // Starter Switch set to on?
            public int IsEngineRunning; // Engine running?
            public float KerbVibration; // vibrations sent to the FFB, could be used for motion rigs
            public float SlipVibrations; // vibrations sent to the FFB, could be used for motion rigs
            public float GVibrations; // vibrations sent to the FFB, could be used for motion rigs
            public float AbsVibrations; // vibrations sent to the FFB, could be used for motion rigs
        }

        /// <summary>
        /// The following members are updated at each graphical step. They mostly
        /// refer to player’s car except for carCoordinates and carID, which refer
        /// to the cars currently on track.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        [Serializable]
        public struct SPageFileGraphic
        {
            public int PacketId; // Current step index
            public Constant.Status Status;
            public Constant.SessionType Session;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string CurrentTime; // Current lap time in wide character
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string LastTime; // Last lap time in wide character
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string BestTime; // Best lap time in wide character
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string Split; // Last split time in wide character
            public int CompletedLaps; // No of completed laps
            public int Position; // Current player position
            public int ICurrentTime; // Current lap time in milliseconds
            public int ILastTime; // Last lap time in milliseconds
            public int IBestTime; // Best lap time in milliseconds
            public float SessionTimeLeft; // Session time left
            public float DistanceTraveled; // Distance travelled in the current stint
            public int IsInPit; // Car is pitting
            public int CurrentSectorIndex; // Current track sector
            public int LastSectorTime; // Last sector time in milliseconds
            public int NumberOfLaps; // Number of completed laps
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string TyreCompound; // Tyre compound used
            public float ReplayTimeMultiplier; // Not used in ACC
            public float NormalizedCarPosition; // Car position on track spline(0.0 start to 1.0 finish)
            public int ActiveCars; // Number of cars on track
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public Coords[] CarCoordinates; // Coordinates of cars on track
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public int[] CarID; // Car IDs of cars on track
            public int PlayerCarID; // Player Car ID
            public float PenaltyTime; // Penalty time to wait
            public Constant.FlagType Flag;
            public Constant.PenaltyType Penalty;
            public int IdealLineOn; // Ideal line on
            public int IsInPitLane; // Car is in pit lane
            public float SurfaceGrip; // Ideal line friction coefficient
            public int MandatoryPitDone; // Mandatory pit is completed
            public float WindSpeed; // Wind speed in m/s
            public float WindDirection; // wind direction in radians
            public int IsSetupMenuVisible; // Car is working on setup
            public int MainDisplayIndex; // current car main display index, see Appendix 1
            public int SecondaryDisplyIndex; // current car secondary display index
            public int Tc; // Traction control level
            public int TcCut; // Traction control cut level
            public int EngineMap; // Current engine map
            public int Abs; // ABS level
            public float FuelXLap; // Average fuel consumed per lap in liters
            public int RainLights; // Rain lights on
            public int FlashingLights; // Flashing lights on
            public int LightsStage; // Current lights stage
            public float ExhaustTemperature; // Exhaust temperature
            public int WiperLV; // Current wiper stage
            public int DriverStintTotalTimeLeft; // Time the driver is allowed to drive/race(ms)
            public int DriverStintTimeLeft; // Time the driver is allowed to drive/stint(ms)
            public int RainTyres; // Are rain tyres equipped
            public int SessionIndex;
            public float UsedFuel; // Used fuel since last time refueling
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string DeltaLapTime; // Delta time in wide character
            public int IDeltaLapTime; // Delta time time in milliseconds
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string EstimatedLapTime; // Estimated lap time in wide character
            public int IEstimatedLapTime; // Estimated lap time in milliseconds
            public int IsDeltaPositive; // Delta positive(1) or negative(0)
            public int ISplit; // Last split time in milliseconds
            public int IsValidLap; // Check if Lap is valid for timing
            public float FuelEstimatedLaps; // Laps possible with current fuel level
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string TrackStatus; // Status of track
            public int MissingMandatoryPits; // Mandatory pitstops the player still has to do
            public float Clock; // Time of day in seconds
            public int DirectionLightsLeft; // Is Blinker left on
            public int DirectionLightsRight; // Is Blinker right on
            public int GlobalYellow; // Yellow Flag is out?
            public int GlobalYellow1; // Yellow Flag in Sector 1 is out?
            public int GlobalYellow2; // Yellow Flag in Sector 2 is out?
            public int GlobalYellow3; // Yellow Flag in Sector 3 is out?
            public int GlobalWhite; // White Flag is out?
            public int GlobalGreen; // Green Flag is out?
            public int GlobalChequered; // Checkered Flag is out?
            public int GlobalRed; // Red Flag is out?
            public int MfdTyreSet; // # of tyre set on the MFD
            public float MfdFuelToAdd; // How much fuel to add on the MFD
            public Wheels<float> MfdTyrePressure; // Tyre pressures on the MFD
            public Constant.TrackGripStatus TrackGripStatus;
            public Constant.RainIntensity RainIntensity;
            public Constant.RainIntensity RainIntensityIn10min;
            public Constant.RainIntensity RainIntensityIn30min;
            public int CurrentTyreSet; // Tyre Set currently in use
            public int StrategyTyreSet; // Next Tyre set per strategy
            public int gapAhead; // Distance in ms to car in front int gapBehind Distance in ms to car behind
        }

        /// <summary>
        /// The following members are initialized when the instance starts and
        /// never changes until the instance is closed.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
        [Serializable]
        public struct SPageFileStatic
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string SmVersion; // Shared memory version
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
            public string AcVersion; // Assetto Corsa version
            public int NumberOfSessions; // Number of sessions
            public int NumCars; // Number of cars
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string CarModel; // Player car model see Appendix 2
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string Track; // Track name
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string PlayerName; // Player name
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string PlayerSurname; // Player surname
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string PlayerNick; // Player nickname
            public int SectorCount; // Number of sectors
            public float MaxTorque; // Not shown in ACC
            public float MaxPower; // Not shown in ACC
            public int MaxRpm; // Maximum rpm
            public float MaxFuel; // Maximum fuel tank capacity
            public Wheels<float> SuspensionMaxTravel; // Not shown in ACC
            public Wheels<float> TyreRadius; // Not shown in ACC
            public float MaxTurboBoost; // Maximum turbo boost
            public float Deprecated1;
            public float Deprecated2;
            public int PenaltiesEnabled; // Penalties enabled
            public float AidFuelRate; // Fuel consumption rate
            public float AidTireRate; // Tyre wear rate
            public float AidMechanicalDamage; // Mechanical damage rate
            public float AllowTyreBlankets; // Not allowed in Blancpain endurance series
            public float AidStability; // Stability control used
            public int AidAutoclutch; // Auto clutch used
            public int AidAutoBlip; // Always true in ACC
            public int HasDds; // Not used in ACC
            public int HasErs; // Not used in ACC
            public int HasKers; // Not used in ACC
            public float KersMaxJ; // Not used in ACC
            public int EngineBrakeSettingsCount; // Not used in ACC
            public int ErsPowerControllerCount; // Not used in ACC
            public float TrackSplineLength; // Not used in ACC
            public char TrackConfiguration; // Not used in ACC
            public float ErsMaxJ; // Not used in ACC
            public int IsTimedRace; // Not used in ACC
            public int HasExtraLap; // Not used in ACC
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string CarSkin; // Not used in ACC
            public int ReversedGridPositions; // Not used in ACC
            public int PitWindowStart; // Pit window opening time
            public int PitWindowEnd; // Pit windows closing time
            public int IsOnline; // If is a multiplayer session
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string DryTyresName; // Name of the dry tyres
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string WetTyresName; // Name of the wet tyres
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        [Serializable]
        public struct Wheels<T>
        {
            public T FL;
            public T FR;
            public T RL;
            public T RR;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        [Serializable]
        public struct Coords
        {
            public float X;
            public float Y;
            public float Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        [Serializable]
        public struct Damage
        {
            public float Front;
            public float Rear;
            public float Left;
            public float Right;
            public float Center;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        [Serializable]
        public struct RideHeight
        {
            public float Front;
            public float Rear;
        }
    }
}
