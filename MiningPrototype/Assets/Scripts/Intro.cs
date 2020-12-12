using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : StateListenerBehaviour
{
    public List<IntroTextElement> texts = new List<IntroTextElement>();
    [SerializeField] DialogElementVisualization textPrefab;
    [SerializeField] Canvas textCanvas;
    [SerializeField] RectTransform textSpawnPosition;
    [SerializeField] float horizontalSpeed;
    [SerializeField] float delayBeforeTorchRequest;
    [SerializeField] bool skipIntroInEditor;

    [Zenject.Inject] PlayerStateMachine player;
    [Zenject.Inject] OverworldEffectHandler effectHandler;
    [Zenject.Inject] ItemPlacingHandler placingHandler;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] CameraPanner cameraPanner;

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
            StartCoroutine(IntroCoroutine());
    }

    IEnumerator IntroCoroutine()
    {
        inventoryManager.PlayerCollects(ItemType.Torch, 1);
        inventoryManager.PlayerCollects(ItemType.Ladder, 1);
        placedTorch = false;
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
            if (!string.IsNullOrEmpty(texts[i].text))
            {
                DialogElementVisualization text = Instantiate(textPrefab, textSpawnPosition.position, Quaternion.identity, textCanvas.transform);
                text.Init(null, texts[i].text, texts[i].duration);
            }
            yield return new WaitForSeconds(texts[i].duration);
        }

        //Torch section
        yield return new WaitForSeconds(delayBeforeTorchRequest);
        DialogElementVisualization torchText = Instantiate(textPrefab, textSpawnPosition.position, Quaternion.identity, textCanvas.transform);
        torchText.Init(null, "Its so dark in here. I should place a torch.", 5);
        effectHandler.SetSnowAmount(1);
        player.InCinematicMode = false;
        player.CinematicSlowWalk = false;
        placingHandler.Placed += OnIntroPlaced;
        cameraPanner.ExitCinematicMode();

        while (!placedTorch)
            yield return null;

        yield return new WaitForSeconds(2);
        torchText = Instantiate(textPrefab, textSpawnPosition.position, Quaternion.identity, textCanvas.transform);
        torchText.Init(null, "I can use a ladder to get out of here.", 5);

        while (!placedLadder)
            yield return null;
        placingHandler.Placed -= OnIntroPlaced;

        torchText = Instantiate(textPrefab, textSpawnPosition.position, Quaternion.identity, textCanvas.transform);
        torchText.Init(null, "It's not far now...", 5);
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
