using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.Physics;
using RaceDirector.Pipeline.Utils;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks.Dataflow;

namespace RaceDirector.Pipeline.Games.R3E
{
    [SupportedOSPlatform("windows")]
    public class Game : IGame
    {
        public record Config(TimeSpan PollingInterval); // TODO remove when config done

        private Config _config;

        public string GameName => "R3E";

        public string[] GameProcessNames => new[] { "RRRE64", "RRRE" };

        public Game(Config config)
        {
            _config = config;
        }

        public ISourceBlock<Telemetry.V0.IGameTelemetry> CreateTelemetrySource()
        {
            var mmReader = new MemoryMappedFileReader<Contrib.Data.Shared>(Contrib.Constant.SharedMemoryName);
            return PollingSource.Create<Telemetry.V0.IGameTelemetry>(_config.PollingInterval, () => Transform(mmReader.Read()));
        }

        private static GameTelemetry Transform(Contrib.Data.Shared sharedData)
        {
            return new GameTelemetry(
                GameState(sharedData),
                sharedData.GameUsingVr > 0,
                Event(sharedData),
                Session(sharedData),
                // TODO
                new Telemetry.V0.IVehicle[0],
                null,
                null
            );
        }

        private static Telemetry.V0.GameState GameState(Contrib.Data.Shared sharedData)
        {
            if (sharedData.GameInMenus > 0)
                return Telemetry.V0.GameState.Menu;
            if (sharedData.GameInReplay > 0)
                return Telemetry.V0.GameState.Replay;
            return Telemetry.V0.GameState.Driving;
        }

        private static Telemetry.V0.IEvent? Event(Contrib.Data.Shared sharedData)
        {
            // TODO if not in session it should be null!
            var layoutLength = IDistance.FromM(sharedData.LayoutLength);
            var sectors = new IFraction<IDistance>[]
            {
                IFraction<IDistance>.Of(layoutLength, sharedData.SectorStartFactors.Sector1),
                IFraction<IDistance>.Of(layoutLength, sharedData.SectorStartFactors.Sector2),
                IFraction<IDistance>.Of(layoutLength, sharedData.SectorStartFactors.Sector3)
            };
            return new Event(
                new TrackLayout(sectors),
                sharedData.FuelUseActive >= 0 ? sharedData.FuelUseActive : 0
            );
        }

        private static Telemetry.V0.ISession? Session(Contrib.Data.Shared sharedData)
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
                ISpeed.FromKmPH(1),
                TimeSpan.FromSeconds(1),
                null,
                null,
                null
            );
        }

        private static Telemetry.V0.SessionType? SessionType(Contrib.Data.Shared sharedData) =>
            (Contrib.Constant.Session)sharedData.SessionType switch
            {
                Contrib.Constant.Session.Practice => Telemetry.V0.SessionType.Practice,
                Contrib.Constant.Session.Qualify => Telemetry.V0.SessionType.Qualify,
                Contrib.Constant.Session.Warmup => Telemetry.V0.SessionType.Warmup,
                Contrib.Constant.Session.Race => Telemetry.V0.SessionType.Race,
                _ => null
            };

        private static Telemetry.V0.SessionPhase? SessionPhase(Contrib.Data.Shared sharedData) =>
            (Contrib.Constant.SessionPhase) sharedData.SessionPhase switch
            {
            Contrib.Constant.SessionPhase.Garage => Telemetry.V0.SessionPhase.Garage,
                Contrib.Constant.SessionPhase.Gridwalk => Telemetry.V0.SessionPhase.Gridwalk,
                Contrib.Constant.SessionPhase.Formation => Telemetry.V0.SessionPhase.Formation,
                Contrib.Constant.SessionPhase.Green => Telemetry.V0.SessionPhase.Started,
                Contrib.Constant.SessionPhase.Checkered => Telemetry.V0.SessionPhase.Over,
                _ => null
            };

        private static Telemetry.V0.ISessionDuration? SessionLength(Contrib.Data.Shared sharedData)
        {
            return (Contrib.Constant.SessionLengthFormat)sharedData.SessionLengthFormat switch
            {
                Contrib.Constant.SessionLengthFormat.LapBased =>
                    new Telemetry.V0.ISessionDuration.LapsDuration(
                        Convert.ToUInt32(sharedData.NumberOfLaps),
                        null // TODO
                    ),
                Contrib.Constant.SessionLengthFormat.TimeBased =>
                    new Telemetry.V0.ISessionDuration.TimeDuration(
                        TimeSpan.FromSeconds(Convert.ToDouble(sharedData.SessionTimeDuration)),
                        null // TODO
                    ),
                Contrib.Constant.SessionLengthFormat.TimeAndLapBased =>
                    new Telemetry.V0.ISessionDuration.TimePlusLapsDuration(
                        TimeSpan.FromSeconds(Convert.ToDouble(sharedData.SessionTimeDuration)),
                        Convert.ToUInt32(sharedData.NumberOfLaps), // TODO check if this is correct
                        null // TODO
                    ),
                _ => null
            };
        }
        private static Telemetry.V0.ISessionRequirements SessionRequirements(Contrib.Data.Shared sharedData)
        {
            if (sharedData.PitWindowStart <= 0 || sharedData.PitWindowEnd <= 0)
                return new SessionRequirements(0, null);

            var window = (Contrib.Constant.SessionLengthFormat)sharedData.SessionLengthFormat switch
            {
                Contrib.Constant.SessionLengthFormat.LapBased =>
                    new Interval<Telemetry.V0.ISessionDuration>(
                        new Telemetry.V0.ISessionDuration.LapsDuration(
                            Convert.ToUInt32(sharedData.PitWindowStart),
                            null // TODO
                        ),
                        new Telemetry.V0.ISessionDuration.LapsDuration(
                            Convert.ToUInt32(sharedData.PitWindowStart),
                            null // TODO
                        )
                    ),
                _ =>
                    new Interval<Telemetry.V0.ISessionDuration>(
                        new Telemetry.V0.ISessionDuration.TimeDuration(
                            TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowStart)),
                            null // TODO
                        ),
                        new Telemetry.V0.ISessionDuration.TimeDuration(
                            TimeSpan.FromMinutes(Convert.ToDouble(sharedData.PitWindowEnd)),
                            null // TODO
                        )
                    ),
            };
            return new SessionRequirements(1, window);
        }
    }
}
