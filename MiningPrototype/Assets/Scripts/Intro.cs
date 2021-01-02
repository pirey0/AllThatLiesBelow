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
    [Zenject.Inject] ProgressionHandler progressionHandler;
    [Zenject.Inject] ItemPlacingHandler placingHandler;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] PlayerInventoryOpener playerInventoryOpener;
    [Zenject.Inject] CameraPanner cameraPanner;
    [Zenject.Inject] PlayerStatementsHandler playerStatements;
    [Zenject.Inject] CursorHandler cursorHandler;

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
            progressionHandler.StartNextDay();
            cursorHandler.Show();
            foreach (BlockingLog obj in GameObject.FindObjectsOfType<BlockingLog>())
            {
                Destroy(obj.gameObject);
            }
            var m = LocationIndicator.Find(IndicatorType.InFrontOfMine);
            if (m != null)
                player.transform.position = m.transform.position;
        }
    }

    IEnumerator IntroCoroutine()
    {
        playerInventoryOpener.Hide();
        cursorHandler.Hide();
        inventoryManager.PlayerCollects(ItemType.Torch, 1);
        inventoryManager.PlayerCollects(ItemType.Rope, 1);
        placedTorch = false;
        introLight.intensity = 0.8f;
        effectHandler.SetNight();
        effectHandler.SetSnowAmount(3);
        effectHandler.OnMirrorSideChanged(RuntimeProceduralMap.MirrorState.Right);
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
        yield return new WaitForSeconds(delayBeforeTorchRequest-2);
        introLight.intensity = 0.3f;
        yield return new WaitForSeconds(2);

        playerStatements.Say("Its so dark in here. I should place a torch.", 5);
        cursorHandler.Show();
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
        playerStatements.Say("I can attach a rope to the ceiling to climb out of here.", 5);

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
