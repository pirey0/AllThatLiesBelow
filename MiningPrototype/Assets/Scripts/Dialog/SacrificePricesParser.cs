using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SacrificePricesParser
{
    const string PATH = "SacrificesData";

    private static Dictionary<(string, string), int> pricingTable;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ParsePrices()
    {
        DurationTracker tracker = new DurationTracker("SacrificePricesParser");


        if(CSVHelper.ResourceMissing(PATH))
            return;

        pricingTable = new Dictionary<(string, string), int>();
        string[] resources = CSVHelper.GetRow0(PATH);
        string[] rewards = CSVHelper.GetColumn0(PATH);
        string[,] table = CSVHelper.LoadTableAtPath(PATH);


        for (int x = 1; x < table.GetLength(0); x++)
        {
            for (int y = 1; y < table.GetLength(1); y++)
            {
                if (int.TryParse(table[x, y], out int result))
                {
                    pricingTable.Add((resources[x], rewards[y]), result);
                }
            }
        }

        tracker.Stop();
    }

    /// <summary>
    /// Returns -1 if no price is given
    /// </summary>
    public static int GetPriceFor(string reward, string resource)
    {
        if(pricingTable.ContainsKey((resource, reward)))
        {
            return pricingTable[(resource, reward)];
        }
        return -1;
    }
}
