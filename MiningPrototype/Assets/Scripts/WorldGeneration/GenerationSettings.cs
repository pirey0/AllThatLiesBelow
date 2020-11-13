using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "GenerationSettings")]
public class GenerationSettings : ScriptableObject
{

    public bool SeedIsRandom;

    public int Seed;

    public int Size;

    [Range(0, 1)]
    public float InitialAliveChance;

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

    public GameObject PhysicalTilePrefab;
}
