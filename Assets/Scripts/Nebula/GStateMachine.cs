using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Nebula
{
    public class GStateMachine
    {
        List<GState> states;

        GState currentState;
        GState idleState;

        // Allows outside sources to lock state transitions
        public bool transitionLock = false;

        // Lock state transitions if a state was recently changed. This ensures that every state
        // will perform at least one action in a fixed update call.
        private bool stateRecentlyChanged = false;

        public GStateMachine()
        {
            states = new List<GState>();
        }

        public void AddState(GState newState)
        {
            states.Add(newState);
        }

        public void SetIdleState(GState newState)
        {
            this.AddState(newState);
            this.idleState = newState;
            currentState = newState;
            currentState.Enter();
        }

        public void PerformStateAction()
        {
            currentState.FixedUpdate();
            stateRecentlyChanged = false;
        }

        public int UpdateState()
        {
            currentState.Update();

            // In case a we want to lock the states from changing.
            if (transitionLock || stateRecentlyChanged)
            {
                return currentState.id;
            }

            // Handle Transitions
            // Only check the states that are allowed transitions from the current state.
            foreach (int stateId in currentState.allowedTransitions)
            {
                GState queryState = states[stateId];
                if (queryState.Condition())
                {
                    if (!currentState.Condition())
                    {
                        ChangeStateTo(queryState);
                    }
                    else if (queryState.priority > currentState.priority)
                    {
                        // Only use priorities if both state conditions are true.
                        ChangeStateTo(queryState);
                    }
                }
            }

            // Finally, if no transition state condition is met and the current state condition
            // isn't met set the current state to the idle state.
            // (Make this gradually go back using a graph at somepoint)
            if (!currentState.Condition())
            {
                ChangeStateTo(idleState); // More of a fail safe currently.
            }

            return currentState.id;
        }

        public void ChangeStateTo(GState newState)
        {
            currentState.Leave();
            currentState = newState;
            currentState.Enter();
            stateRecentlyChanged = true;
        }
    }
}
