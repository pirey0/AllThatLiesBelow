using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCollectDisplayElement : MonoBehaviour
{
    [SerializeField] float duration;
    [SerializeField] Text text;
    private void Start()
    {
        StartCoroutine(FadeOut());
    }

    public void SetText(string newText)
    {
        text.text = newText;
    }

    private IEnumerator FadeOut()
    {
        float t = 0;

        while (t < duration)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1 - t / duration);
            yield return null;
            t += Time.deltaTime;
        }
        Destroy(gameObject);
    }
}
