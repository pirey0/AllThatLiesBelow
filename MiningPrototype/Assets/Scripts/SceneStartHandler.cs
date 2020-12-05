using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneStartHandler : StateListenerBehaviour
{

    [Zenject.Inject] TransitionEffectHandler transitionEffectHandler;

    private void Start()
    {
        gameState.ChangeStateTo(GameState.State.Entry);
        transitionEffectHandler.SetToBlack();
    }

    protected override void OnStartAfterLoad()
    {
        transitionEffectHandler.FadeIn(FadeType.Circle);
    }
}
