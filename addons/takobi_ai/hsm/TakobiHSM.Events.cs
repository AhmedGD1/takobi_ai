using System;
using System.Collections.Generic;
using FSMEventName = Godot.StringName;

namespace TakobiAI.HSM;

public partial class TakobiHSM<T>
{
    private readonly Queue<FSMEventName> pendingEvents = new();

    #region Event Trigger

    /// <summary>
    /// Enqueues an event to be processed on the next <see cref="Tick"/>.<br/>
    /// Events are matched against transitions that have a matching <see cref="TakobiTransition{T}.OnEvent"/> name.
    /// Only the first matching, unblocked transition fires per event — subsequent matches are ignored.
    /// </summary>
    /// <param name="eventName">The name of the event to trigger.</param>
    public void TriggerEvent(FSMEventName eventName) => pendingEvents.Enqueue(eventName);

    #endregion

    #region Events Evaluation

    private void ProcessEvents()
    {
        while (pendingEvents.Count > 0)
        {
            var eventName = pendingEvents.Dequeue();
            var span = transitionCache.AsSpan(0, transitionCacheCount);

            foreach (ref readonly var transition in span)
            {                
                if (transition.EventName != eventName) continue;
                if (ShouldSkipTransition(transition)) continue;

                OnTransitionSelected(transition);
                return; // one transition per event
            }
        }
    }
    
    #endregion
}

