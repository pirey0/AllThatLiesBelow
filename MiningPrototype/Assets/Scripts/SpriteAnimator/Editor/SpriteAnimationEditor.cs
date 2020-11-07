using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SpriteAnimation))]
public class SpriteAnimationEditor : Editor
{
    BasicSpriteAnimator previewAnimator = new BasicSpriteAnimator();
    bool preview = true;
    double timeStamp;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        if(GUILayout.Button("LoadFromObject"))
        {
            (serializedObject.targetObject as SpriteAnimation).LoadFromObject();
        }

        preview = EditorGUILayout.Toggle("Preview: ", preview);

        if(preview)
        {
            if(GUILayout.Button("Update Preview"))
            {
                previewAnimator = new BasicSpriteAnimator();
                previewAnimator.Play(serializedObject.targetObject as SpriteAnimation, resetSame: true);
            }
            else
            {
                previewAnimator.Play(serializedObject.targetObject as SpriteAnimation, resetSame: false);
            }

            float deltaTime = (float)(EditorApplication.timeSinceStartup - timeStamp);
            var sp = previewAnimator.Update(deltaTime);
            Texture texture;

            if(sp != null)
            {
                texture = GenerateTextureFromSprite(sp);
            }
            else
            {
                texture = new Texture2D(1, 1);
            }

            var controlRect = EditorGUILayout.GetControlRect(false, 300);
            Vector2 position = controlRect.position;
            Vector2 scale = new Vector2(256, 256);
            GUI.DrawTexture(new Rect(position, scale), texture, ScaleMode.ScaleToFit);

            timeStamp = EditorApplication.timeSinceStartup;


            Repaint();
        }
    }

    Texture2D GenerateTextureFromSprite(Sprite aSprite)
    {
        var rect = aSprite.rect;
        var tex = new Texture2D((int)rect.width, (int)rect.height,TextureFormat.RGBA32,false, false);
        tex.filterMode = FilterMode.Point;
        var data = aSprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        tex.SetPixels(data);
        tex.Apply(true);
        return tex;
    }
}
