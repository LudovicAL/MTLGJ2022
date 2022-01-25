using Core;

namespace HowTo
{
    public class Attack : IState
    {
        private AIController _aiController;

        public Attack(AIController aiController)
        {
            _aiController = aiController;
        }

        public void OnEnter()
        {
            //Do something once when the state change for this one
        }

        public void OnExit()
        {
            //Do something once when exiting this state
        }

        public void Tick()
        {
            //Do something every frame
        }
    }
}
