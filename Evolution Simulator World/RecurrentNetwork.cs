using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    class RecurrentNetwork : NeuralNetwork
    {
        float[] Inputs;
        float[] Neurons;
        float[] Outputs;
        public RecurrentNetwork(int InputCount, int OutputCount,int NeuronCount)
        {
            this.InputCount = InputCount;
            this.InputCount = InputCount;
            this.InputCount = InputCount;
            Inputs = new float[InputCount];
            Outputs = new float[OutputCount];
            Neurons = new float[NeuronCount];
            WeightMatrix = new float[(Inputs.Length + Neurons.Length)* (Outputs.Length + Neurons.Length)];
        }
        public void GenerateRandom(int StartingSynapsAmount = 0)
        {
            if (StartingSynapsAmount > 0)
            {
                for (int i = 0; i < StartingSynapsAmount; i++)
                {
                    WeightMatrix[Form1.Rand.Next(WeightMatrix.Length)] = 4 * (float)Form1.Rand.NextDouble() - 2;
                }
            }
            else
            {
                for (int i = 0; i < WeightMatrix.Length; i++)
                {
                    WeightMatrix[i] = 4 * (float)Form1.Rand.NextDouble() - 2;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">Start</param>
        /// <param name="j">End</param>
        /// <returns></returns>
        int ToIndex(int i, int j)
        {
            return i + j* (Inputs.Length + Neurons.Length);
        }
        public override void CopyFrom(NeuralNetwork N )
        {
            InputCount = N.InputCount;
            NeuronCount = N.NeuronCount;
            OutputCount = N.OutputCount;
            Inputs = new float[InputCount];
            Outputs = new float[OutputCount];
            Neurons = new float[NeuronCount];
            WeightMatrix = new float[(Inputs.Length + Neurons.Length)* (Outputs.Length + Neurons.Length)];
            for (int i = 0; i < WeightMatrix.Length; i++)
            {
                    WeightMatrix[i] = N.WeightMatrix[i];
            }
        }
        public override void Show(Graphics Graphics,Rectangle Rect)
        {
            Pen Pe = new Pen(Color.Black, 10);
            int Padding = 20;
            int dY = (Rect.Height-Padding*2)/(Inputs.Length-1);
            int r = 10;
            Pe.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            Graphics.DrawRectangle(Pe, Rect);
            Graphics.FillRectangle(Brushes.LightGray, Rect);

            for (int i = 0; i < Inputs.Length; i++)
            {
                Color C = CLerp(Color.Black, Color.White, Inputs[i]);
                PointF Pos = new PointF(Rect.X+Padding-r, Rect.Y + Padding+dY*i-r);
                Graphics.FillEllipse(new SolidBrush(C), Pos.X, Pos.Y, r * 2, r * 2);
                Graphics.DrawEllipse(new Pen(Color.Black, 2), Pos.X, Pos.Y, r * 2, r * 2);
            }
            for (int i = 0; i < Neurons.Length; i++)
            {
                Color C = CLerp(Color.Black, Color.White, Neurons[i]);
                PointF Pos = new PointF(Rect.X + Rect.Width / 2 - r, Rect.Y + Padding + dY * (i + 0.5f) - r);
                Graphics.FillEllipse(new SolidBrush(C), Pos.X, Pos.Y, r * 2, r * 2);
                Graphics.DrawEllipse(new Pen(Color.Black, 2), Pos.X, Pos.Y, r * 2, r * 2);
            }
            for (int i = 0; i < Outputs.Length; i++)
            {
                Color C;
                if (Outputs[i] > 0)
                    C = CLerp(Color.Black, Color.Red, Outputs[i]);
                else
                    C = CLerp(Color.Black, Color.Blue, -Outputs[i]);
                PointF Pos = new PointF(Rect.X + Rect.Width - Padding - r, Rect.Y + Padding + dY * i - r);
                Graphics.FillEllipse(new SolidBrush(C), Pos.X, Pos.Y, r * 2, r * 2);
                Graphics.DrawEllipse(new Pen(Color.Black, 2), Pos.X, Pos.Y, r * 2, r * 2);
            }
            Pe = new Pen(Color.Orange, 2);
            for (int i = 0; i < InputCount + NeuronCount; i++)
            {
                for (int j = 0; j < NeuronCount+OutputCount; j++)
                {
                    if (WeightMatrix[ToIndex(i,j)] != 0)
                    {
                        Color Col;
                        float mult = NeuronValueFromWeight(i);
                        if (WeightMatrix[ToIndex(i, j)] > 0)
                            Col = Color.FromArgb((int)(WeightMatrix[ToIndex(i, j)] * 127 * mult), Color.Red);
                        else
                            Col = Color.FromArgb(-(int)(WeightMatrix[ToIndex(i, j)] * 127 * mult), Color.Blue);
                        Pe.Color = Col;
                        PointF Start = NeuronPosIn(i);
                        PointF End = NeuronPosOut(j);
                        Graphics.DrawLine(Pe, Start, End);
                    }
                }
            }
            PointF NeuronPosIn(int id)
            {
                if (id < Inputs.Length)
                {
                    return new PointF(Rect.X + Padding + r, Rect.Y + Padding + dY * id);
                }
                else
                {
                    return new PointF(Rect.X + Rect.Width/2 + r, Rect.Y + Padding + dY * (id-Inputs.Length + 0.5f));
                }
            }
            PointF NeuronPosOut(int id)
            {
                if (id < Outputs.Length)
                {
                    return new PointF(Rect.X +Rect.Width- Padding - r, Rect.Y + Padding + dY * id);
                }
                else
                {
                    return new PointF(Rect.X + Rect.Width / 2 - r, Rect.Y + Padding + dY * (id - Outputs.Length + 0.5f));
                }
            }
        }
        public override void SetInputs(float[] Inputs)
        {
            this.Inputs = Inputs;
        }
        public override float[] GetOutputs()
        {
            return Outputs;
        }
        public override void Update()
        {
            for (int i = 0; i < Neurons.Length; i++)
            {
                float Sum = 0;
                for (int j = 0; j < WeightMatrix.Length; j++)
                {
                    float mult = 0;
                    if (j < Inputs.Length)
                        mult = Inputs[j];
                    else
                        mult = Neurons[j - Inputs.Length];
                    Sum += WeightMatrix[ToIndex(j,i + Outputs.Length)] * mult;
                }
                Neurons[i] = ActivSig(Sum);
            }
            //output
            for (int i = 0; i < Outputs.Length; i++)
            {
                float Sum = 0;
                for (int j = 0; j < WeightMatrix.Length; j++)
                {
                    float mult = 0;
                    if (j < Inputs.Length)
                        mult = Inputs[j];
                    else
                        mult = Neurons[j - Inputs.Length];
                    Sum += WeightMatrix[ToIndex(i,j)] * mult;
                }
                Outputs[i] = Clamp(Sum,-1,1);
            }
        }
        float ActivSig(float x)
        {
            return 1 / (1 + (float)Exp(-4 * x));
        }
        float ActivBump(float x)
        {
            return 1 - (float)Exp(-4 * x * x);
        }
        public static double Exp(double val)
        {
            long tmp = (long)(1512775 * val + 1072632447);
            return BitConverter.Int64BitsToDouble(tmp << 32);
        }

        float NeuronValueFromWeight(int i)
        {
            if (i < Inputs.Length)
            {
                return Inputs[i];
            }
            else
            {
                return Neurons[i - Inputs.Length];
            }
        }
    }
}
