using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Intro : StateListenerBehaviour
{
    public List<IntroTextElement> texts = new List<IntroTextElement>();
    [SerializeField] float horizontalSpeed;
    [SerializeField] float delayBeforeTorchRequest;
    [SerializeField] bool skipIntroInEditor;
    [SerializeField] Light2D introLight;

    [Zenject.Inject] PlayerStateMachine player;
    [Zenject.Inject] OverworldEffectHandler effectHandler;
    [Zenject.Inject] ItemPlacingHandler placingHandler;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] PlayerInventoryOpener playerInventoryOpener;
    [Zenject.Inject] CameraPanner cameraPanner;
    [Zenject.Inject] PlayerStatementsHandler playerStatements;

    bool placedTorch;
    bool placedLadder;

    protected override void OnNewGame()
    {
        var start = LocationIndicator.Find(IndicatorType.PlayerStart);
        if (start != null)
        {
            player.transform.position = start.transform.position;
        }

        if (!Application.isEditor || !skipIntroInEditor)
        {
            StartCoroutine(IntroCoroutine());
        }
        else
        {
            player.CanDig = true;
        }
    }

    IEnumerator IntroCoroutine()
    {
        playerInventoryOpener.Hide();
        inventoryManager.PlayerCollects(ItemType.Torch, 1);
        inventoryManager.PlayerCollects(ItemType.Ladder, 1);
        placedTorch = false;
        introLight.intensity = 0.8f;
        effectHandler.SetNight();
        effectHandler.SetSnowAmount(3);
        player.CanDig = false;
        player.InCinematicMode = true;
        player.CinematicSlowWalk = true;
        player.CinematicHorizontal = horizontalSpeed;
        cameraPanner.EnterCinematicMode();

        for (int i = 0; i < texts.Count; i++)
        {
            yield return new WaitForSeconds(1);
            playerStatements.Say(texts[i].text, texts[i].duration);
            yield return new WaitForSeconds(texts[i].duration);
        }

        //Torch section
        yield return new WaitForSeconds(delayBeforeTorchRequest-4);
        introLight.intensity = 0.3f;
        yield return new WaitForSeconds(4);

        playerStatements.Say("Its so dark in here. I should place a torch.", 5);
        effectHandler.SetSnowAmount(1);
        player.InCinematicMode = false;
        player.CinematicSlowWalk = false;
        placingHandler.Placed += OnIntroPlaced;
        cameraPanner.ExitCinematicMode();

        yield return new WaitForSeconds(1);

        playerInventoryOpener.StartBlinking();
        playerInventoryOpener.Show();

        while (!placedTorch)
            yield return null;

        introLight.intensity = 0.8f;

        yield return new WaitForSeconds(1);
        playerStatements.Say("I can use a ladder to get out of here.", 5);

        while (!placedLadder)
            yield return null;
        placingHandler.Placed -= OnIntroPlaced;

        playerStatements.Say("It's not far now...", 5);
        player.CanDig = true;
    }

    private void OnIntroPlaced(ItemType obj)
    {
        if (obj == ItemType.Torch)
        {
            placedTorch = true;
        }

        if(obj == ItemType.Ladder)
        {
            placedLadder = true;
        }
    }
}

[System.Serializable]
public class IntroTextElement
{
    public string text;
    public float duration;
}
