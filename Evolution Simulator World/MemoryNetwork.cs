using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    class MemoryNetwork : NeuralNetwork
    {
        float[] Inputs;
        float[] Neurons;
        float[] NeuronMemory;
        float[] Outputs;
        public MemoryNetwork(int InputCount, int OutputCount, int NeuronCount, int StartingSynapsAmount = 0)
        {
            this.InputCount = InputCount;
            this.NeuronCount = NeuronCount;
            this.OutputCount = OutputCount;
            Inputs = new float[InputCount];
            Outputs = new float[OutputCount];
            Neurons = new float[NeuronCount];
            NeuronMemory = new float[NeuronCount];
            WeightMatrix = new float[(Inputs.Length + Neurons.Length) * (Outputs.Length + Neurons.Length*4)];
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
            return i + j * (Inputs.Length + Neurons.Length);
        }
        public override void CopyFrom(NeuralNetwork N)
        {
            InputCount = N.InputCount;
            NeuronCount = N.NeuronCount;
            OutputCount = N.OutputCount;
            Inputs = new float[InputCount];
            Outputs = new float[OutputCount];
            Neurons = new float[NeuronCount];
            WeightMatrix = new float[(Inputs.Length + Neurons.Length) * (Outputs.Length + Neurons.Length*4)];
            for (int i = 0; i < WeightMatrix.Length; i++)
            {
                WeightMatrix[i] = N.WeightMatrix[i];
            }
        }
        public override void Show(Graphics Graphics, Rectangle Rect)
        {
            Pen Pe = new Pen(Color.Black, 10);
            int Padding = 20;
            int dY = (Rect.Height - Padding * 2) / (Inputs.Length - 1);
            int r = 10;
            Pe.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            Graphics.DrawRectangle(Pe, Rect);
            Graphics.FillRectangle(Brushes.LightGray, Rect);

            for (int i = 0; i < Inputs.Length; i++)
            {
                Color C = CLerp(Color.Black, Color.White, Inputs[i]);
                PointF Pos = new PointF(Rect.X + Padding - r, Rect.Y + Padding + dY * i - r);
                Graphics.FillEllipse(new SolidBrush(C), Pos.X, Pos.Y, r * 2, r * 2);
                Graphics.DrawEllipse(new Pen(Color.Black, 2), Pos.X, Pos.Y, r * 2, r * 2);
            }
            for (int i = 0; i < Neurons.Length; i++)
            {
                Color C = CLerp(Color.Black, Color.White, Neurons[i]);
                PointF Pos = new PointF(Rect.X + Rect.Width / 2, Rect.Y + Padding + dY * (i*2));
                Graphics.FillRectangle(new SolidBrush(C), Pos.X-r, Pos.Y-r, r * 2, dY*2-r);

                float F = Clamp(NeuronMemory[i],-1,1);
                if (F > 0)
                    C = CLerp(Color.Black, Color.Red, F);
                else
                    C = CLerp(Color.Black, Color.Blue, -F);
                Graphics.FillEllipse(new SolidBrush(C), Pos.X - r/2, Pos.Y + dY - r*2, r, r);

                Graphics.DrawRectangle(new Pen(Color.Black, 2), Pos.X - r, Pos.Y - r, r * 2, dY * 2 - r);
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
                for (int j = 0; j < NeuronCount*4 + OutputCount; j++)
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
                    return new PointF(Rect.X + Rect.Width / 2 + r, Rect.Y + Padding + dY * (id - Inputs.Length+0.25f)*2-r);
                }
            }
            PointF NeuronPosOut(int id)
            {
                if (id < Outputs.Length)
                {
                    return new PointF(Rect.X + Rect.Width - Padding - r, Rect.Y + Padding + dY * id-Outputs.Length);
                }
                else
                {
                    return new PointF(Rect.X + Rect.Width / 2 - r, Rect.Y + Padding + dY * (float)(id - Outputs.Length)/2-r);
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
                float[] Sum = new float[4];//0=Value,1=Remember,2=Forget,3=Output
                for (int j = 0; j < (Inputs.Length + Neurons.Length); j++)
                {
                    float mult = 0;
                    if (j < Inputs.Length)
                        mult = Inputs[j];
                    else
                        mult = Neurons[j - Inputs.Length];
                    for (int k = 0; k < 4; k++)
                    {
                        Sum[k] += WeightMatrix[ToIndex(j,i * 4 + k + Outputs.Length)] * mult;
                    }
                }
                float I = ActivSig(Sum[1]);
                float F = Clamp(ActivTanh(Sum[2]),0,1);
                float O = ActivSig(Sum[3]);

                NeuronMemory[i] = (1 - F) * NeuronMemory[i] + I * ActivTanh(Sum[0]);

                Neurons[i] = O*ActivSig(NeuronMemory[i]);
            }
            //output
            for (int i = 0; i < Outputs.Length; i++)
            {
                float Sum = 0;
                for (int j = 0; j < (Inputs.Length + Neurons.Length); j++)
                {
                    float mult = 0;
                    if (j < Inputs.Length)
                        mult = Inputs[j];
                    else
                        mult = Neurons[j - Inputs.Length];
                    Sum += WeightMatrix[ToIndex(j,i)] * mult;
                }
                Outputs[i] = Clamp(Sum,-1,1);
            }
        }
        float ActivSig(float x)
        {
            x = Clamp(x,-0.5f,0.5f);
            return 1 / (1 + (float)Exp(-4 * x));
        }
        float ActivTanh(float x)
        {
            x = Clamp(x, -0.5f, 0.5f);
            float E = (float)Exp(-2 * x);
            return (1-E) / (1 + E);
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
