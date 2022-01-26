using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
using System;

namespace RaceDirector.Pipeline.Games.ACC
{
    internal class TelemetryConverter
    {

        internal GameTelemetry Transform(ref Contrib.Data.Shared shared)
        {
            if (shared.Static.SmVersionMajor != Contrib.Constant.SmVersionMajor)
                throw new ArgumentException("Incompatible major version");
            return new GameTelemetry(
                GameState: GameState(ref shared),
                UsingVR: null,
                Event: Event(ref shared),
                Session: Session(ref shared),
                Vehicles: Vehicles(ref shared),
                FocusedVehicle: FocusedVehicle(ref shared),
                Player: Player(ref shared)
            );
        }

        private GameState GameState(ref Contrib.Data.Shared shared)
        {
            return shared.Graphic.Status switch
            {
                Contrib.Constant.Status.Off => Telemetry.V0.GameState.Replay, // recorded replay
                Contrib.Constant.Status.Replay => Telemetry.V0.GameState.Replay, // in-game replay
                Contrib.Constant.Status.Live when shared.Static.AidMechanicalDamage < 0 => Telemetry.V0.GameState.Menu, // in-game menu
                Contrib.Constant.Status.Live => Telemetry.V0.GameState.Driving,
                Contrib.Constant.Status.Pause => Telemetry.V0.GameState.Paused, // single player game paused
                _ => throw new ArgumentException("Unknown game state")
            };
        }

        private Event? Event(ref Contrib.Data.Shared shared)
        {
            return null;
        }

        private Session? Session(ref Contrib.Data.Shared shared)
        {
            return null;
        }

        private Vehicle[] Vehicles(ref Contrib.Data.Shared shared)
        {
            return Array.Empty<Vehicle>();
        }

        private Vehicle? FocusedVehicle(ref Contrib.Data.Shared shared)
        {
            return null;
        }

        private Player? Player(ref Contrib.Data.Shared shared)
        {
            return null;
        }
    }
}
