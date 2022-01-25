using UnityEngine;
using Core;
using System;

namespace HowTo
{
    public class AIController : MonoBehaviour
    {
        public GameObject _target;
        public StateMachine _stateMachine;

        private void Awake()
        {
            _stateMachine = new StateMachine();

            //Instantiate states
            var standby = new Standby(this);
            var moveToTarget = new MoveToTarget(this);
            var attack = new Attack(this);

            //Set from->to state transition condition
            At(standby, moveToTarget, HasTarget());
            At(moveToTarget, attack, CloseToTarget());

            //Set anystate->to state transition condition
            _stateMachine.AddAnyTransition(standby, HasNoTarget());

            //Set starting state
            _stateMachine.SetState(standby);


            void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
            Func<bool> HasTarget() => () => _target != null;
            Func<bool> CloseToTarget() => () => _target != null && Vector3.Distance(this.transform.position, _target.transform.position) <= 0.5f;
            Func<bool> HasNoTarget() => () => _target == null;
        }

        private void Update()
        {
            _stateMachine.Tick();
        }
    }
}

