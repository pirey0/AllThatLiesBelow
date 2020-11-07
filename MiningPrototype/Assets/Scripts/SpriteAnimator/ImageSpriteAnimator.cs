using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ImageSpriteAnimator : MonoBehaviour
{
    [SerializeField] new Image renderer;
    [SerializeField] SpriteAnimation startingAnimation;

    BasicSpriteAnimator basicSA = new BasicSpriteAnimator();

    public SpriteAnimation Animation { get => basicSA.Animation; }

    private void Start()
    {
        if (startingAnimation != null)
        {
            Play(startingAnimation);
        }
    }

    public void Play(SpriteAnimation ani, bool resetSame = true)
    {
        basicSA.Play(ani, resetSame);
    }

    public bool IsDone()
    {
        return basicSA.IsDone();
    }

    private void Update()
    {
        if (renderer == null)
            return;

        var sprite = basicSA.Update(Time.deltaTime);

        if (sprite != null)
            renderer.sprite = sprite;
    }
}