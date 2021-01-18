using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    private List<PlatformObject> platformObjects = new List<PlatformObject>();
    const bool SHOW_DEBUG = false;

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

    internal void CheckForAttachmentToWall(Platform platform)
    {
        PlatformObject platformObject = GetPlatformObjectByPlatform(platform);

        if (platformObject != null && !platformObject.IsBeeingDestroyed)
        {
            platformObject.RemoveEmptyAtStart();
            platformObject.RemoveEmptyAtEnd();

            if (!platformObject.platforms[0].HasConnectionToWall() && !platformObject.platforms[platformObject.platforms.Count-1].HasConnectionToWall())
                platformObject.Destroy();
        }
    }

    private PlatformObject GetPlatformObjectByPlatform(Platform platform)
    {
        for (int p = platformObjects.Count - 1; p >= 0; p--)
        {
            if (platformObjects[p] != null && platformObjects[p].platforms.Contains(platform))
            {
                return platformObjects[p];
            }
        }

        return null;
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
        PlatformObject platformObject = GetPlatformObjectByPlatform(platform);

        if (platformObject != null && platformObject.platforms.Contains(platform))
        {
            int x = platform.transform.position.ToGridPosition().x;

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

    private static List<Platform> CheckForConnectionsLeft(PlatformObject platformObject, int x)
    {
        List<Platform> potentiallyUnstable = new List<Platform>();

        for (int i = x - 1; i >= (platformObject.startX); i--)
        {
            Platform current = platformObject.platforms[i - platformObject.startX];

            if (current != null)
            {
                potentiallyUnstable.Add(current);

                if (i == platformObject.startX && current.HasConnectionToWall())
                {
                    potentiallyUnstable.Clear();
                    break;
                }
            }
            else
                break;
        }

        return potentiallyUnstable;
    }

    private static List<Platform> CheckForConnectionsRight(PlatformObject platformObject, int x)
    {
        List<Platform> potentiallyUnstable = new List<Platform>();

        for (int i = x + 1; i < (platformObject.startX + platformObject.platforms.Count); i++)
        {
            Platform current = platformObject.platforms[i - platformObject.startX];

            if (current != null)
            {
                potentiallyUnstable.Add(current);

                if (i == (platformObject.startX + platformObject.platforms.Count - 1) && current.HasConnectionToWall())
                {
                    potentiallyUnstable.Clear();
                    break;
                }
            }
            else
                break;
        }

        return potentiallyUnstable;
    }

    public class PlatformObject
    {
        public int startX;
        public List<Platform> platforms = new List<Platform>();

        public bool IsBeeingDestroyed = false;

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
            IsBeeingDestroyed = true;

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

            //last
            platforms.Add(newPlatform);

        }

        public void Remove(Platform unstable)
        {
            platforms[platforms.IndexOf(unstable)] = null;


            RemoveEmptyAtStart();
            RemoveEmptyAtEnd();

            if (platforms.Count > 0)
                startX = platforms[0].transform.position.ToGridPosition().x;
        }

        public void RemoveEmptyAtEnd()
        {
            int i = platforms.Count - 1;

            while (i >= 0 && i < platforms.Count && platforms[i] == null)
                platforms.RemoveAt(i--);
        }

        public void RemoveEmptyAtStart()
        {
            int i = 0;

            while (i >= 0 && i < platforms.Count && platforms[i] == null)
                platforms.RemoveAt(i);
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
