using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FadeType {
	Circle,
	Death,
	Nightmare
}

public class TransitionEffectHandler : Singleton<TransitionEffectHandler>
{
	[SerializeField] AudioSource audioSource;

	public Material mat;
	public AnimationCurve circleFadeCurve, deathFadeCurve, nightmareFadeCurve;
	public Texture circleFadeTexture, deathFadeTexture, nightmareFadeTexture;
	public AudioClip nightmareWakeup;

	[SerializeField] bool testFadeOut;
	[SerializeField] FadeType testFadeType;

	[Button]
	public void FadeTest()
	{
		if (testFadeOut)
			FadeOut(testFadeType);
		else
			FadeIn(testFadeType);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, mat);
	}

	public static void FadeOut(FadeType fadeType = FadeType.Circle)
	{
		if (Instance == null)
		{
			Debug.LogError("Add Transition Effect Handler to Camera");
			return;
		}

		Instance.mat.SetTexture("_PatternTex", GetTextureByType(fadeType));
		Instance.StartCoroutine(FadeRoutine(fadeIn:false, GetCurveByType(fadeType)));
	}

	public static void FadeIn(FadeType fadeType = FadeType.Circle)
	{
		if (Instance == null)
		{
			Debug.LogError("Add Transition Effect Handler to Camera");
			return;
		}

		Instance.mat.SetTexture("_PatternTex", GetTextureByType(fadeType));
		Instance.StartCoroutine(FadeRoutine(fadeIn: true, GetCurveByType(fadeType)));

		if (fadeType == FadeType.Nightmare)
		{
			Instance.audioSource.clip = Instance.nightmareWakeup;
			Instance.audioSource.Play();
		}
	}

	public static IEnumerator FadeRoutine(bool fadeIn, AnimationCurve curve)
	{

		float time = fadeIn ? curve[curve.length - 1].time : curve[0].time;
		float endTime = fadeIn ? curve[0].time : curve[curve.length - 1].time;

		Debug.Log("start" + (fadeIn?"fadeOut":"fadeIn") +  " routine : " + time + " -> " + endTime);

		while (fadeIn && time > endTime || !fadeIn && time < endTime)
		{
			time += Time.deltaTime * (fadeIn ? -1f : 1f);
			Instance.mat.SetFloat("_Cutoff", curve.Evaluate(time));
			Debug.Log("fade update "+ curve.Evaluate(time));
			yield return null;
		}
	}

	public static AnimationCurve GetCurveByType(FadeType fadeType)
	{
		switch(fadeType)
		{
			case FadeType.Death:
				return Instance.deathFadeCurve;

			case FadeType.Nightmare:
				return Instance.nightmareFadeCurve;

			default:
				return Instance.circleFadeCurve;
		}
	}

	public static Texture GetTextureByType(FadeType fadeType)
	{
		switch (fadeType)
		{
			case FadeType.Death:
				return Instance.deathFadeTexture;

			case FadeType.Nightmare:
				return Instance.nightmareFadeTexture;

			default:
				return Instance.circleFadeTexture;
		}
	}
}
