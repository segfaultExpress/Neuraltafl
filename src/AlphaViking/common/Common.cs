using System;


namespace NeuralTaflAi
{
    public static class IEnumerableExtensions {
    
        // I swear to god 90% of this project has been translating python one-liners
        // into massive, sprawling C# functions. Anyway, here's np.random.choice.

        public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, double> weightSelector) {
            double totalWeight = sequence.Sum(weightSelector);
            // The weight we are after...
            double itemWeightIndex =  (double)new Random().NextDouble() * totalWeight;
            double currentWeightIndex = 0;

            foreach(var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) }) {
                currentWeightIndex += item.Weight;
                
                // If we've hit or passed the weight we are after for this item then it's the one we want....
                if(currentWeightIndex >= itemWeightIndex)
                    return item.Value;
                
            }
            
            return default(T);
            
        }
    }
}