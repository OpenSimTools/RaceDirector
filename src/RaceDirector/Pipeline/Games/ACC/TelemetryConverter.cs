using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
using System;

namespace RaceDirector.Pipeline.Games.ACC
{
    internal class TelemetryConverter
    {
        internal GameTelemetry Transform(Contrib.Data.SPageFilePhysics physicsData, Contrib.Data.SPageFileGraphic graphicData, Contrib.Data.SPageFileStatic staticData)
        {
            if (staticData.SmVersionMajor != Contrib.Constant.SmVersionMajor)
                throw new ArgumentException("Incompatible major version");
            return new GameTelemetry(
                GameState: GameState(physicsData, graphicData, staticData),
                UsingVR: null,
                Event: Event(physicsData, graphicData, staticData),
                Session: Session(physicsData, graphicData, staticData),
                Vehicles: Vehicles(physicsData, graphicData, staticData),
                FocusedVehicle: FocusedVehicle(physicsData, graphicData, staticData),
                Player: Player(physicsData, graphicData, staticData)
            );
        }

        private Player? Player(Contrib.Data.SPageFilePhysics physicsData, Contrib.Data.SPageFileGraphic graphicData, Contrib.Data.SPageFileStatic staticData)
        {
            throw new NotImplementedException();
        }

        private Vehicle? FocusedVehicle(Contrib.Data.SPageFilePhysics physicsData, Contrib.Data.SPageFileGraphic graphicData, Contrib.Data.SPageFileStatic staticData)
        {
            throw new NotImplementedException();
        }

        private Vehicle[] Vehicles(Contrib.Data.SPageFilePhysics physicsData, Contrib.Data.SPageFileGraphic graphicData, Contrib.Data.SPageFileStatic staticData)
        {
            throw new NotImplementedException();
        }

        private Session? Session(Contrib.Data.SPageFilePhysics physicsData, Contrib.Data.SPageFileGraphic graphicData, Contrib.Data.SPageFileStatic staticData)
        {
            throw new NotImplementedException();
        }

        private Event? Event(Contrib.Data.SPageFilePhysics physicsData, Contrib.Data.SPageFileGraphic graphicData, Contrib.Data.SPageFileStatic staticData)
        {
            throw new NotImplementedException();
        }

        private GameState GameState(Contrib.Data.SPageFilePhysics physicsData, Contrib.Data.SPageFileGraphic graphicData, Contrib.Data.SPageFileStatic staticData)
        {
            throw new NotImplementedException();
        }
    }
}
