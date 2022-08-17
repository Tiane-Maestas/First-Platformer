using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nebula
{
    public class GState
    {
        // Identifiers for the state machine and human's.
        public int id { get; private set; }
        public string name { get; private set; }

        // Allows for toggling of animations to play.
        private string animatorBoolName;
        private Animator animator;

        // The time when the 'enter' method was last called.
        public float startTime { get; private set; }

        // A list of 'id' to specify what transitions are allowed.
        public List<int> allowedTransitions { get; private set; }

        // In the case that multiple 'transitions' are allowed choose the one with
        // the highest priority.
        public int priority { get; private set; }

        public GState(int id, string name, List<int> allowedTransitions, int priority,
                      Animator animator, string animatorBoolName)
        {
            this.id = id;
            this.name = name;
            this.allowedTransitions = allowedTransitions;
            this.priority = priority;
            this.animator = animator;
            this.animatorBoolName = animatorBoolName;
        }

        public virtual void Enter()
        {
            startTime = Time.time;
            animator.SetBool(animatorBoolName, true);
            // ToDo: Implement the logic that is performed once when entering a state
        }

        public virtual void Leave()
        {
            animator.SetBool(animatorBoolName, false);
            // ToDo: Implement the logic that is performed once when leaving a state
        }

        public virtual bool Condition()
        {
            // ToDo: Implement what condition is required to be in this state.
            return false;
        }

        public virtual void FixedUpdate()
        {
            // ToDo: Implement the action that this state performs
        }

        public virtual void Update()
        {
            // ToDo: Implement logic that this state needs to be checked every frame.
        }
    }
}
