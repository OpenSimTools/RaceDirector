namespace RaceDirector.Remote.Networking.Client;

public delegate void MessageHandler<in T>(T message);