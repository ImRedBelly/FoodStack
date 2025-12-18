namespace GameLoop.States
{
    public interface IGameState
    {
        void Init();
        void Deinit();
    }

    // public abstract class GameStateBase : IGameState
    // {
    //     void IGameState.Init()
    //     {
    //         Init();
    //     }
    //
    //     void IGameState.Deinit()
    //     {
    //         Deinit();
    //     }
    //
    //     protected abstract void Init();
    //     protected abstract void Deinit();
    // }
}