using UnityEngine;
using System.Collections.Generic;

public class WeightedRandomizer {

    int totalWeight = 0;
    List<int> Weights;

    /// <summary>
    /// Returns a weighted random index based on a given list of weights
    /// </summary>
    /// <param name="weights">Unordered list of weights</param>
    public WeightedRandomizer(List<int> weights)
    {
        this.Weights = weights;

        //Sum all weights
        this.Weights.ForEach(x => totalWeight += x);
    }
    
    /// <summary>
    /// Gets a randomized indexed based weights
    /// </summary>
    /// <returns>Index</returns>
    public int GetRandomIndex()
    {
        int ran = Random.Range(0, totalWeight);
        int weightProgression = 0;

        for (int i = 0; i < this.Weights.Count; i++)
        {
            weightProgression += this.Weights[i];
            if (ran < weightProgression)
            {
                return i;
            }
        }

        return -1;
    }
}
