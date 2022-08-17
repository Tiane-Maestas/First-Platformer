using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    public class GDelegateState : GState
    {
        // Callback functions so that any general 'player controller' can create states
        // w/o having to change or add dependencies to itself or a GState.
        public delegate bool DelegatedCondition();
        public DelegatedCondition condition;

        public delegate void DelegatedAction();
        public DelegatedAction action;

        public delegate void DelegatedEnter();
        public DelegatedEnter enter;

        public delegate void DelegatedLeave();
        public DelegatedLeave leave;

        public delegate void DelegatedUpdate();
        public DelegatedUpdate update;

        public GDelegateState(DelegatedCondition condition, DelegatedAction action,
                              DelegatedEnter enter, DelegatedLeave leave,
                              DelegatedUpdate update,
                              int id, string name, List<int> allowedTransitions,
                              int priority, Animator animator, string animatorBoolName)
                              :
                              base(id, name, allowedTransitions, priority, animator,
                              animatorBoolName)
        {
            this.condition = condition;
            this.action = action;
            this.enter = enter;
            this.leave = leave;
            this.update = update;
        }

        public override void Enter()
        {
            base.Enter();
            // Saftey Check. Not every component needs to be implemented.
            if (enter == null)
            {
                return;
            }
            enter();
        }

        public override void Leave()
        {
            base.Leave();
            // Saftey Check. Not every component needs to be implemented.
            if (leave == null)
            {
                return;
            }
            leave();
        }

        public override bool Condition()
        {
            return condition();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            // Saftey Check. Not every component needs to be implemented.
            if (action == null)
            {
                return;
            }
            action();
        }

        public override void Update()
        {
            base.Update();
            // Saftey Check. Not every component needs to be implemented.
            if (update == null)
            {
                return;
            }
            update();
        }
    }
}