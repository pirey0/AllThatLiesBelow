using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SacrificePricesParser
{
    const string PATH = "SacrificesData";

    private static Dictionary<string, int> itemIndexMap;
    private static Dictionary<string, int> rewardIndexMap;
    private static string[,] table;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ParsePrices()
    {
        DurationTracker tracker = new DurationTracker("SacrificePricesParser");


        if (CSVHelper.ResourceMissing(PATH))
            return;

        itemIndexMap = new Dictionary<string, int>();
        rewardIndexMap = new Dictionary<string, int>();

        string[] items = CSVHelper.GetRow0(PATH);
        string[] rewards = CSVHelper.GetColumn0(PATH);

        for (int i = 0; i < items.Length; i++)
            itemIndexMap.Add(items[i], i);

        for (int i = 0; i < rewards.Length; i++)
        {
            rewardIndexMap.Add(rewards[i], i);
        }

        table = CSVHelper.LoadTableAtPath(PATH);

        tracker.Stop();
    }

    public static string GetDisplayNameOf(string reward)
    {
        if (rewardIndexMap.ContainsKey(reward))
        {
            return table[1, rewardIndexMap[reward]];
        }
        else
        {
            return "UNDEFINED";
        }
    }

    public static string[] GetRewardsAvailableAtLevel(int level)
    {
        List<string> viableRewards = new List<string>();

        for (int y = 1; y < table.GetLength(1); y++)
        {
            var reward = table[0, y];
            var sminLev = table[2, y];
            var smaxLev = table[3, y];

            if (int.TryParse(sminLev, out int minLev))
            {
                if (int.TryParse(smaxLev, out int maxLev))
                {
                    if (level >= minLev && level <= maxLev)
                        viableRewards.Add(reward);
                }
                else
                {
                    Debug.LogError("PriceTableError: could not parse: " + smaxLev + " at 3/" + y);
                }
            }
            else
            {
                Debug.LogError("PriceTableError: could not parse: " + sminLev + " at 2/" + y);
            }

        }
        return viableRewards.ToArray();
    }

    public static ItemAmountPair[] GetPaymentsFor(string reward)
    {
        if (rewardIndexMap.ContainsKey(reward))
        {
            int y = rewardIndexMap[reward];
            List<ItemAmountPair> l = new List<ItemAmountPair>();

            for (int i = 4; i < table.GetLength(0); i++)
            {
                string s = table[i, y];

                if (int.TryParse(s, out int amount))
                {
                    if (System.Enum.TryParse(table[i, 0], out ItemType type))
                    {
                        l.Add(new ItemAmountPair(type, amount));
                    }
                    else
                    {
                        Debug.LogError("PriceTableError: could not parse: " + table[i, 0] + " at " + i + "/0");
                    }
                }
            }

            if (l.Count > 0)
                return l.ToArray();
        }
        else
        {
            Debug.LogError("PriceTableError: No reward present named " + reward);
        }

        return null;
    }


}
