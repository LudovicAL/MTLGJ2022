namespace Core
{
    public interface IState
    {
        void Tick();
        void OnEnter();
        void OnExit();
    }
}