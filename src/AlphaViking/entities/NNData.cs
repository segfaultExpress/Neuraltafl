using System;

namespace NeuralTaflAi
{
    /// <summary>
    /// Cool thing about Python - You can just call [pi, v] = model.predict(state)
    /// Cool thing about C# - You can't do that. Here's the alternative
    /// </summary>
    public class NNData
    {
        
        public int[] boardArray {get; set;}
        
        public double[] policy {get; set;}
        public double v {get; set;}

        public NNData(int[] boardArray, double[] policy, double v)
        {
            this.boardArray = boardArray;
            this.policy = policy;
            this.v = v;
        }
    }
}