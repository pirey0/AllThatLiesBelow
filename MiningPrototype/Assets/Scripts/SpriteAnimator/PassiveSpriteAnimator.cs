using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSpriteAnimator : MonoBehaviour
{
    [SerializeField] SpriteAnimation startingAnimation;

    [SerializeField] new SpriteRenderer renderer;

    BasicSpriteAnimator basicSA = new BasicSpriteAnimator();

    public SpriteAnimation Animation { get => basicSA.Animation; }
    public SpriteRenderer Renderer { get => renderer; }
    private void Start()
    {

        if (renderer == null)
        {
            renderer = GetComponentInChildren<SpriteRenderer>();
        }

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

    public void ActiveUpdate(float updateTime)
    {
        if (renderer == null)
            return;

        var sprite = basicSA.Update(updateTime);

        if (sprite != null)
            renderer.sprite = sprite;
    }
}
