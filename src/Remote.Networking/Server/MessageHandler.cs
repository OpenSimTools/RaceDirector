namespace RaceDirector.Remote.Networking.Server;

public delegate void MessageHandler<in TIn, TOut>(ISession<TOut> session, TIn message);