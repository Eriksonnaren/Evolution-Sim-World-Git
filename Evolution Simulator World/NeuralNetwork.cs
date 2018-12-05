using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    
    [Serializable]
    public abstract class NeuralNetwork
    {
        public enum OutputType
        {
            Sigmoid,
            Tanh,
            Linear,
            LinearClamp
        }
        public float[] WeightMatrix;
        public int Layers;
        public int InputCount;
        public int OutputCount;
        public int NeuronCount;
        public OutputType[] outputType;
        public abstract void SetInputs(float[] Values);
        public abstract float[] GetOutputs();
        public abstract void Update();
        public abstract void Show(Graphics Graphics,Rectangle Rect);
        public void Mutate(float MutationRate,float MinValue=0,float MaxValue=0)
        {
            bool ShouldClamp = !(MinValue == 0 && MaxValue == 0);
            for (int i = 0; i < WeightMatrix.Length; i++)
            {
                float G = Gaussian();
                WeightMatrix[i] += MutationRate * G;
                if (ShouldClamp)
                {
                    WeightMatrix[i] = Clamp(WeightMatrix[i], MinValue, MaxValue);
                }
            }
        }
        public void GenerateRandom(float MinValue,float MaxValue)
        {
            for (int i = 0; i < WeightMatrix.Length; i++)
            {
                WeightMatrix[i] = Lerp(MinValue, MaxValue, (float)Form1.Rand.NextDouble());
            }
        }
        public void GenerateFromFamilyList(List<NeuralNetwork> List)
        {
            for (int i = 0; i < WeightMatrix.Length; i++)
            {
                int Parent = Form1.Rand.Next(List.Count);
                WeightMatrix[i] = List[Parent].WeightMatrix[i];
            }
        }
        public abstract void CopyFrom(NeuralNetwork N);
        public float Clamp(float x,float Min,float Max)
        {
            if (x > Max)
                return Max;
            else if (x < Min)
                return Min;
            else
                return x;
        }
        public int ILerp(int A, int B, float T)
        {
            return A + (int)((B - A) * T);
        }
        public float Lerp(float A, float B, float T)
        {
            return A + ((B - A) * T);
        }
        public Color CLerp(Color A, Color B, float T)
        {
            T = Clamp(T,0,1);
            return Color.FromArgb(ILerp(A.R, B.R, T), ILerp(A.G, B.G, T), ILerp(A.B, B.B, T));
        }
        const double tolerance = 0.95;
        readonly double scalar = 1 / Math.Log((1 + tolerance) / (1 - tolerance));
        /// <summary>
        /// Returns a random number between -1 and 1 in a gaussian distribution
        /// </summary>
        /// <returns></returns>
        public float Gaussian()
        {
            double x = Form1.Rand.NextDouble() * 2 - 1;
            return (float)(scalar * Math.Log((1 - x * tolerance) / (1 + x * tolerance)));
        }
    }
}
