namespace FantasyKingdom.Core;

public interface IHandler
{
    public Task Handle(params object[] args);
}