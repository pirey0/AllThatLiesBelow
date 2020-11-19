using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "GenerationSettings")]
public class GenerationSettings : ScriptableObject
{

    public bool SeedIsRandom;

    public int Seed;

    public int SizeX;
    public int SizeY;

    public AnimationCurve InitialAliveCurve;

    [Range(0, 9)]
    public int DeathLimit;

    [Range(0, 9)]
    public int BirthLimit;

    [Range(0, 10)]
    public int AutomataSteps;

    public AnimationCurve HeightMultiplyer;
    public int SnowStartHeight;

    public OrePass[] OrePasses;
    public RockPass[] RockPasses;

    public int UnstableThreshhold = 40;
    public int CollapseThreshhold = 40;

    public int CrumbleEmitAmount;
    public GameObject PhysicalTilePrefab;

    public int StabilityPropagationDistance = 5;
}
