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
    [SerializeField] EdgeCollider2D gatingInCave;
    [SerializeField] bool skipIntroInEditor;

    [Zenject.Inject] PlayerStateMachine player;
    [Zenject.Inject] OverworldEffectHandler effectHandler;
    [Zenject.Inject] ItemPlacingHandler placingHandler;
    [Zenject.Inject] InventoryManager inventoryManager;

    bool placedTorch;

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
        placedTorch = false;
        effectHandler.SetNight();
        effectHandler.SetSnowAmount(3);
        player.InCinematicMode = true;
        player.CinematicSlowWalk = true;
        player.CinematicHorizontal = horizontalSpeed;
        gatingInCave.enabled = false;

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
        gatingInCave.enabled = true;
        placingHandler.Placed += OnIntroPlaced;

        while (!placedTorch)
            yield return null;

        placingHandler.Placed -= OnIntroPlaced;
        player.InCinematicMode = true;
        player.CinematicHorizontal = horizontalSpeed;
        gatingInCave.enabled = false;

        yield return new WaitForSeconds(5);
        player.CinematicVertical = 1;
        yield return new WaitForSeconds(3f);
        player.CinematicVertical = 0;
        player.CinematicSlowWalk = true;
        yield return new WaitForSeconds(5f);
        player.InCinematicMode = false;
        player.CinematicSlowWalk = false;
    }

    private void OnIntroPlaced(ItemType obj)
    {
        if (obj == ItemType.Torch)
        {
            placedTorch = true;
        }
    }
}

[System.Serializable]
public class IntroTextElement
{
    public string text;
    public float duration;
}
