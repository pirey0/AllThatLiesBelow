using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogTest2
{
    public class Statement : IDialogSubsection
    {
        string message;
        IDialogSubsection connection;
        public Statement(string message)
        {
            this.message = message;
        }

        public void Enter()
        {
            //Send message 
        }

        public IDialogSubsection GetConnectedSubsection()
        {
            return connection;
        }

        public bool TryExit(int parameter)
        {
            return true;
        }

        public IDialogSubsection Attach(IDialogSubsection subsection)
        {
            connection = subsection;
            return this;
        }
    }

    public class Choice : IDialogSubsection
    {
        List<ChoicePair> choices;
        IDialogSubsection connection;

        public Choice(params ChoicePair[] choicesParams)
        {
            choices = new List<ChoicePair>(choicesParams);
        }

        public void AddChoice(string option, IDialogSubsection subsection)
        {
            choices.Add(new ChoicePair(option, subsection));
        }

        public void Enter()
        {
            //Send choice
        }

        public IDialogSubsection GetConnectedSubsection()
        {
            return connection;
        }

        public bool TryExit(int parameter)
        {
            if (parameter >= 0 && parameter < choices.Count)
            {
                connection = choices[parameter].Subsection;
                return true;
            }
            return false;
        }
    }

    public class AwaitPayment : IDialogSubsection
    {
        IDialogSubsection connection;
        IDialogSubsection successful;
        IDialogSubsection failure;
        public void Enter()
        {
            //Notify payment await
        }

        public IDialogSubsection GetConnectedSubsection()
        {
            return connection;
        }

        public bool TryExit(int parameter)
        {

            return true;
        }

        public AwaitPayment SucceedsIn(IDialogSubsection subsection)
        {
            successful = subsection;
            return this;
        }

        public AwaitPayment FailsIn(IDialogSubsection subsection)
        {
            failure = subsection;
            return this;
        }
    }


    public class ChoicePair
    {
        public string Option;
        public IDialogSubsection Subsection;

        public ChoicePair(string option, IDialogSubsection subsection)
        {
            Option = option;
            Subsection = subsection;
        }
    }

    public interface IDialogSubsection
    {
        void Enter();

        bool TryExit(int parameter);
        IDialogSubsection GetConnectedSubsection();
    }
}