using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    private List<PlatformObject> platformObjects = new List<PlatformObject>();
    const bool SHOW_DEBUG = false;

    public void NotifyPlatformPlaced(Platform newPlatform)
    {
        foreach (PlatformObject platformObject in platformObjects)
        {
            foreach (Platform platform in platformObject.platforms)
            {
                if (platform != null && platform.IsAdjacendTo(newPlatform))
                {
                    platformObject.AddNewPlatform(newPlatform);
                    return;
                }
            }
        }

        platformObjects.Add(new PlatformObject(newPlatform));
    }

    public void NotifyPlatformDestroyed(Platform platform)
    {
        for (int p = platformObjects.Count - 1; p > 0; p--)
        {
            PlatformObject platformObject = platformObjects[p];
            if (platformObject != null && platformObject.platforms.Contains(platform))
            {
                int x = platform.transform.position.ToGridPosition().x;

                List<Platform> potentiallyUnstable = new List<Platform>();

                //left
                CheckForConnectionsLeft(platformObject, x, potentiallyUnstable);

                foreach (Platform unstable in potentiallyUnstable)
                {
                    platformObject.platforms.Remove(unstable);
                    unstable.UncarveDestroy();
                }

                potentiallyUnstable.Clear();

                //right
                CheckForConnectionsRight(platformObject, x, potentiallyUnstable);

                foreach (Platform unstable in potentiallyUnstable)
                {
                    platformObject.platforms.Remove(unstable);
                    unstable.UncarveDestroy();
                }

                platformObject.platforms.Remove(platform);

                if (platformObject.platforms.Count <= 0)
                    platformObjects.Remove(platformObject);
            }
        }
    }

    private static void CheckForConnectionsLeft(PlatformObject platformObject, int x, List<Platform> potentiallyUnstable)
    {
        for (int i = x - 1; i >= (platformObject.startX); i--)
        {
            //Debug.LogWarning("check left x " + i + " index => " + (i - platformObject.startX));

            Platform current = platformObject.platforms[i - platformObject.startX];

            if (current != null)
            {
                potentiallyUnstable.Add(current);

                if (i == platformObject.startX && current.HasConnectionToWall())
                {
                    //Debug.LogWarning("found wall at " + i + " index => " + (i - platformObject.startX));
                    potentiallyUnstable.Clear();
                    break;
                }

            }
        }
    }

    private static void CheckForConnectionsRight(PlatformObject platformObject, int x, List<Platform> potentiallyUnstable)
    {
        for (int i = x + 1; i < (platformObject.startX + platformObject.platforms.Count); i++)
        {
            //Debug.LogWarning("check right x " + i + " index => " + (i - platformObject.startX));

            Platform current = platformObject.platforms[i - platformObject.startX];

            if (current != null)
            {
                potentiallyUnstable.Add(current);

                if (i == (platformObject.startX + platformObject.platforms.Count - 1) && current.HasConnectionToWall())
                {
                    //Debug.LogWarning("found wall at " + i + " index => " + (i - platformObject.startX));
                    potentiallyUnstable.Clear();
                    break;
                }

            }
        }
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
    }

    private void OnGUI()
    {
        if (!SHOW_DEBUG)
            return;

        string str = "";
        int index = 1;

        foreach (PlatformObject platformObject in platformObjects)
        {
            str += "platform " + index + " : ";

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
