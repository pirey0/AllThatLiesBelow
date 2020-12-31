using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageEffectHandler : MonoBehaviour
{
    [SerializeField] AnimationCurve OpacityOverTimeCurve;
    [SerializeField] Image damageImage;
    float damageTime;

    public void TakeDamage(float intensity = 0.5f)
    {
        float maxDuration = OpacityOverTimeCurve.keys[OpacityOverTimeCurve.length - 1].time;
        damageTime = Mathf.Min(damageTime + intensity * maxDuration, maxDuration);
        StopAllCoroutines();
        StartCoroutine(DamageDecay());
    }

    IEnumerator DamageDecay ()
    {
        while(damageTime > 0)
        {
            damageImage.color = new Color(1, 1, 1, OpacityOverTimeCurve.Evaluate(damageTime));
            damageTime -= Time.deltaTime;
            yield return null;
        }

        damageImage.color = new Color(0,0,0,0);
    }
}
