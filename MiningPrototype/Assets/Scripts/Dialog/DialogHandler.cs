using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogHandler : Singleton<DialogHandler>
{
    
    public IDialogSubsection GetPaymentProcess()
    {
        Statement current = new Statement("and how will you pay?");
        Statement success = new Statement("in the morning you will receive your reward");
        Statement failure = new Statement("I will await..");

        current.Attach
            (new Choice(
                new ChoicePair("Rocks", new Statement("Does it look like i need more rocks?").Attach(current)),
                new ChoicePair("Copper", new Statement("20 Copper will suffice").Attach(new AwaitPayment().SucceedsIn(success).FailsIn(failure))),
                new ChoicePair("Gold", new Statement("5 Gold is the price").Attach(new AwaitPayment().SucceedsIn(success).FailsIn(failure))
                )));

        return current;
    }

}
