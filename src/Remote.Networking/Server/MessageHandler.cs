namespace RaceDirector.Remote.Networking.Server;

public delegate void MessageHandler<in T>(ISession session, T message);