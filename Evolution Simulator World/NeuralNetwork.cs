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
        public abstract void SetInputs(float[] Values);
        public abstract float[] GetOutputs();
        public abstract void Update();
        public abstract void Show(Graphics Graphics,Rectangle Rect);
        public abstract void Mutate();
        public abstract NeuralNetwork Copy();
        public float Clamp(float x)
        {
            if (x > 1)
                return 1;
            else if (x < -1)
                return -1;
            else
                return x;
        }
        public float Clamp01(float x)
        {
            if (x > 1)
                return 1;
            else if (x < 0)
                return 0;
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
            return Color.FromArgb(ILerp(A.R, B.R, T), ILerp(A.G, B.G, T), ILerp(A.B, B.B, T));
        }
    }
}
