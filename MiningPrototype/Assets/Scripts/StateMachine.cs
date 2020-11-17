using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateMachineUser
{
    StateMachine GetStateMachine();
}

public class StateMachine
{
    private string name;
    private List<State> states;
    private List<Transition> globalTransitions;

    private State currentState;

    bool log;

    public event System.Action<string> Log;

    public State CurrentState { get => currentState; }
    public bool DoesLog { get => log; set => log = value; }
    public string Name { get => name; }

    public List<State> States { get => states; }

    public StateMachine(string name = "Unnamed")
    {
        this.name = name;
        states = new List<State>();
        globalTransitions = new List<Transition>();
    }

    public State AddState(string name, System.Action enter, System.Action update = null, System.Action exit = null)
    {
        State state = new State(name, enter, update, exit);
        states.Add(state);
        TryLog("Added State: " + state.ToString());

        return state;
    }



    public Transition NewGlobalTransition(System.Func<bool> condition, State state)
    {
        Transition transition = new Transition(condition, state);
        globalTransitions.Add(transition);
        TryLog("Added Global Transition: " + transition.ToString());

        return transition;
    }

    public void ForceTransitionTo(State state)
    {
        TryLog("Force To State: " + state.ToString());
        TransitionToState(state);
    }
    public void ForceTransitionTo(string stateName)
    {
        var state = states.Find((x) => x.Name == stateName);

        if (state != null)
            ForceTransitionTo(state);
    }


    public override string ToString()
    {
        string stateName = CurrentState == null ? "Null" : CurrentState.ToString();
        return $"SM_{name}_{stateName}";
    }

    public void Start()
    {
        if(currentState == null && states.Count > 0)
        {
            TryLog("Transitioning to default: " + states[0].ToString());
            TransitionToState(states[0]);
        }
    }

    public void Update()
    {
        if(currentState != null)
        {
            currentState.Update?.Invoke();

            foreach (var t in globalTransitions)
            {
                if (t.Condition != null && t.Condition())
                {
                    TryLog(currentState == null ? "" : currentState.ToString() + " -> " + t.TargetState == null ? "" : t.TargetState.ToString() + " because of " + t.Condition.Method.Name);

                    TransitionToState(t.TargetState);
                    return;
                }
            }

            foreach (var t in currentState.Transitions)
            {
                if(t.Condition != null && t.Condition())
                {
                    TryLog(currentState == null ? "" : currentState.ToString() + " -> " + t.TargetState == null? "" : t.TargetState.ToString() + " because of " + t.Condition.Method.Name);

                    TransitionToState(t.TargetState);
                    return;
                }
            }
        }
    }

    private void TransitionToState(State newState)
    {
        if(currentState != null)
        {
            currentState.Exit?.Invoke();
        }

        if(newState != null)
        {
            newState.Enter?.Invoke();
        }

        currentState = newState;
    }

    private void TryLog(string msg)
    {
        if (DoesLog)
        {
            Log?.Invoke(msg);
        }
    }


    public class State
    {
        public string Name;
        public System.Action Enter;
        public System.Action Update;
        public System.Action Exit;

        public List<Transition> Transitions;

        public State(string name, System.Action enter, System.Action update = null, System.Action exit = null)
        {
            this.Name = name;
            this.Enter = enter;
            this.Update = update;
            this.Exit = exit;

            Transitions = new List<Transition>();
        }

        public Transition AddTransition(System.Func<bool> condition, State state)
        {
            Transition transition = new Transition(condition, state);
            Transitions.Add(transition);
            return transition;
        }

        public override string ToString()
        {
            return Name + " (" + Transitions.Count + ")";
        }
    }

    public class Transition
    {
        public System.Func<bool> Condition;
        public State TargetState;

        public Transition(System.Func<bool> condition, State target)
        {
            this.Condition = condition;
            this.TargetState = target;
        }
    }
}


