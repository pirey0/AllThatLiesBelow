using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class SpriteAnimation : ScriptableObject
{
    [SerializeField] bool loop;
    [SerializeField] float speedMultiplyer = 1;
    [SerializeField] List<AnimationFrame> frames;

    [Header("Auto")]
    [SerializeField] Object fileToLoadFrom;
    [SerializeField] float defaultTime = 0.25f;

    private int index = 0;
    private float timer = 0;

    public Sprite CurrentFrame { get => IsDone()? null : frames[index].Sprite; }
    public int FramesCount { get => frames.Count; }
    public bool Looping { get => loop; }

    public void AddFrame(AnimationFrame frame)
    {
        if(frames == null) { frames = new List<AnimationFrame>(); }

        frames.Add(frame);
    }

    public Sprite Next(float deltaTime)
    {
        timer += deltaTime * speedMultiplyer;
        var sp = frames[index].Sprite;

        if(frames[index].Duration < timer)
        {
            timer = 0;
            index += 1;

            if(loop && index >= frames.Count)
            {
                index = 0;
            }
        }

        return sp;
    }

    public bool IsDone()
    {
        bool done = index >= frames.Count;

        return done; 
    }

    public void Reset()
    {
        index = 0;
        timer = 0;
    }

    public void SetLoop(bool loop)
    {
        this.loop = loop;
    }

    public void LoadFromObject()
    {
    #if UNITY_EDITOR
        if (fileToLoadFrom == null)
            return;

        var path = AssetDatabase.GetAssetPath(fileToLoadFrom);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

        frames = new List<AnimationFrame>();

        for (int i = 0; i < sprites.Length; i++)
        {
            frames.Add(new AnimationFrame());
            frames[i].Sprite = sprites[i];
            frames[i].Duration = defaultTime;
        }
#endif
    }
}

[System.Serializable]
public class AnimationFrame
{
    public Sprite Sprite;
    public float Duration;

}

