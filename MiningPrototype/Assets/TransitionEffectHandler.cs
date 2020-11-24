using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FadeType {
	Circle,
	Death,
	Nightmare
}

public class TransitionEffectHandler : MonoBehaviour
{
	[SerializeField] AudioSource audioSource;

	public Material mat;
	public AnimationCurve circleFadeCurve, deathFadeCurve, nightmareFadeCurve;
	public Texture circleFadeTexture, deathFadeTexture, nightmareFadeTexture;
	public AudioClip nightmareWakeup;

	[SerializeField] bool testFadeOut;
	[SerializeField] FadeType testFadeType;

	private void Start()
	{
		FadeIn();
	}

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

	public  void FadeOut(FadeType fadeType = FadeType.Circle)
	{
		mat.SetTexture("_PatternTex", GetTextureByType(fadeType));
		StartCoroutine(FadeRoutine(fadeIn:false, GetCurveByType(fadeType)));
	}

	public void FadeIn(FadeType fadeType = FadeType.Circle)
	{

		mat.SetTexture("_PatternTex", GetTextureByType(fadeType));
		StartCoroutine(FadeRoutine(fadeIn: true, GetCurveByType(fadeType)));

		if (fadeType == FadeType.Nightmare)
		{
			audioSource.clip = nightmareWakeup;
			audioSource.Play();
		}
	}

	public IEnumerator FadeRoutine(bool fadeIn, AnimationCurve curve)
	{

		float time = fadeIn ? curve[curve.length - 1].time : curve[0].time;
		float endTime = fadeIn ? curve[0].time : curve[curve.length - 1].time;

		//Debug.Log("start" + (fadeIn?"fadeOut":"fadeIn") +  " routine : " + time + " -> " + endTime);

		while (fadeIn && time > endTime || !fadeIn && time < endTime)
		{
			time += Time.deltaTime * (fadeIn ? -1f : 1f);
			mat.SetFloat("_Cutoff", curve.Evaluate(time));
			//Debug.Log("fade update "+ curve.Evaluate(time));
			yield return null;
		}
	}

	public AnimationCurve GetCurveByType(FadeType fadeType)
	{
		switch(fadeType)
		{
			case FadeType.Death:
				return deathFadeCurve;

			case FadeType.Nightmare:
				return nightmareFadeCurve;

			default:
				return circleFadeCurve;
		}
	}

	public  Texture GetTextureByType(FadeType fadeType)
	{
		switch (fadeType)
		{
			case FadeType.Death:
				return deathFadeTexture;

			case FadeType.Nightmare:
				return nightmareFadeTexture;

			default:
				return circleFadeTexture;
		}
	}

	private void OnDestroy()
	{
		mat.SetFloat("_Cutoff", 1);
	}
}
