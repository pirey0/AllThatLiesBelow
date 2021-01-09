using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    private List<PlatformObject> platformObjects = new List<PlatformObject>();
    const bool SHOW_DEBUG = true;

    public void NotifyPlatformPlaced(Platform newPlatform)
    {
        PlatformObject adjacendPlatformObject = null;

        foreach (PlatformObject platformObject in platformObjects)
        {
            foreach (Platform platform in platformObject.platforms)
            {
                if (platform != null && platform.IsAdjacendTo(newPlatform))
                {
                    if (adjacendPlatformObject == null)
                        adjacendPlatformObject = platformObject;
                    else
                    {
                        platformObject.AddNewPlatform(newPlatform);

                        if (platformObject != adjacendPlatformObject)
                            MergePlatformObjects(platformObject, adjacendPlatformObject);

                        return;
                    }
                }
            }
        }

        if (adjacendPlatformObject != null)
        {
            adjacendPlatformObject.AddNewPlatform(newPlatform);
            return;
        }

        platformObjects.Add(new PlatformObject(newPlatform));
    }

    private void MergePlatformObjects(PlatformObject platformObject, PlatformObject adjacendPlatformObject)
    {
        PlatformObject first = platformObject.startX < adjacendPlatformObject.startX ? platformObject : adjacendPlatformObject;
        PlatformObject second = platformObject.startX > adjacendPlatformObject.startX ? platformObject : adjacendPlatformObject;

        PlatformObject merged = new PlatformObject(first.startX);
        merged.platforms.AddRange(first.platforms);
        merged.platforms.AddRange(second.platforms);

        platformObjects.Remove(first);
        platformObjects.Remove(second);

        platformObjects.Add(merged);
    }

    public void NotifyPlatformDestroyed(Platform platform)
    {
        Debug.LogWarning("destroyed platform" + platform);

        for (int p = platformObjects.Count - 1; p > 0; p--)
        {
            PlatformObject platformObject = platformObjects[p];
            if (platformObject != null && platformObject.platforms.Contains(platform))
            {
                int x = platform.transform.position.ToGridPosition().x;
                Debug.LogWarning("index: " + x);

                List<Platform> potentiallyUnstable = CheckForConnectionsLeft(platformObject, x);
                potentiallyUnstable.AddRange(CheckForConnectionsRight(platformObject, x));

                foreach (Platform unstable in potentiallyUnstable)
                {
                    platformObject.Remove(unstable);
                    unstable.UncarveDestroy();
                }

                platformObject.Remove(platform);

                if (platformObject.platforms.Count <= 0)
                    platformObjects.Remove(platformObject);
            }
        }
    }

    private static List<Platform> CheckForConnectionsLeft(PlatformObject platformObject, int x)
    {
        List<Platform> potentiallyUnstable = new List<Platform>();

        for (int i = x - 1; i >= (platformObject.startX); i--)
        {
            //Debug.LogWarning("check left x " + i + " index => " + (i - platformObject.startX));

            Platform current = platformObject.platforms[i - platformObject.startX];

            if (current != null)
            {
                potentiallyUnstable.Add(current);

                if (i == platformObject.startX && current.HasConnectionToWall())
                {
                    Debug.LogWarning("found wall at " + i + " index => " + (i - platformObject.startX));
                    potentiallyUnstable.Clear();
                    break;
                }
            }
            else
                break;
        }

        string str = "";

        foreach (var item in potentiallyUnstable)
            str += item.transform.position.ToGridPosition().x + ", ";

        Debug.LogWarning("found " + potentiallyUnstable.Count + " to the left : " + str);

        return potentiallyUnstable;
    }

    private static List<Platform> CheckForConnectionsRight(PlatformObject platformObject, int x)
    {
        List<Platform> potentiallyUnstable = new List<Platform>();

        for (int i = x + 1; i < (platformObject.startX + platformObject.platforms.Count); i++)
        {
            //Debug.LogWarning("check right x " + i + " index => " + (i - platformObject.startX));

            Platform current = platformObject.platforms[i - platformObject.startX];

            if (current != null)
            {
                potentiallyUnstable.Add(current);

                if (i == (platformObject.startX + platformObject.platforms.Count - 1) && current.HasConnectionToWall())
                {
                    Debug.LogWarning("found wall at " + i + " index => " + (i - platformObject.startX));
                    potentiallyUnstable.Clear();
                    break;
                }
            }
            else
                break;
        }

        string str = "";

        foreach (var item in potentiallyUnstable)
            str += item.transform.position.ToGridPosition().x + ", ";

        Debug.LogWarning("found " + potentiallyUnstable.Count + " to the right : " + str);

        return potentiallyUnstable;
    }

    public class PlatformObject
    {
        public int startX;
        public List<Platform> platforms = new List<Platform>();

        public PlatformObject(Platform platform)
        {
            startX = platform.transform.position.ToGridPosition().x;
            platforms.Add(platform);
        }

        public PlatformObject(int x)
        {
            startX = x;
        }

        public void Destroy()
        {
            for (int i = platforms.Count - 1; i >= 0; i--)
            {
                if (platforms[i] != null)
                {
                    platforms[i].UncarveDestroy();
                }
            }
        }

        public void AddNewPlatform(Platform newPlatform)
        {
            int x = newPlatform.transform.position.ToGridPosition().x;

            //first
            if (x < startX)
            {
                Debug.LogWarning("placed left");
                startX--;
                List<Platform> newPlatformFirst = new List<Platform>();
                newPlatformFirst.Add(newPlatform);
                newPlatformFirst.AddRange(platforms);
                platforms = newPlatformFirst;
                return;
            }

            //inbetween
            if (x < startX + platforms.Count)
            {
                platforms[x - startX] = newPlatform;
                return;
            }

            Debug.LogWarning("placed right");
            //last
            platforms.Add(newPlatform);

        }

        public void Remove(Platform unstable)
        {
            Debug.LogWarning("remove " + (platforms.IndexOf(unstable)) + " of " + platforms.Count);

            platforms[platforms.IndexOf(unstable)] = null;
            int i = 0;

            while (i >= 0 && i < platforms.Count && platforms[i] == null)
                platforms.RemoveAt(i);

            i = platforms.Count - 1;

            while (i >= 0 && i < platforms.Count && platforms[i] == null)
                platforms.RemoveAt(i--);

            if (platforms.Count > 0)
                startX = platforms[0].transform.position.ToGridPosition().x;
        }
    }

    private void OnGUI()
    {
        if (!SHOW_DEBUG)
            return;

        string str = "";
        int index = 1;

        foreach (PlatformObject platformObject in platformObjects)
        {
            str += "platform " + index + " (start: " + platformObject.startX + ") : ";

            foreach (Platform platform in platformObject.platforms)
            {
                if (platform != null)
                    str += platform.transform.position.ToGridPosition().x + ", ";
                else
                    str += "- , ";
            }

            str += "\n";

            index++;
        }

        GUILayout.Box(str);
    }
}
