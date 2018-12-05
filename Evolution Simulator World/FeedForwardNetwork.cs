using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    public class FeedForwardNetwork : NeuralNetwork
    {
        float[][] Neurons;
        public FeedForwardNetwork(int InputCount, int OutputCount, int NeuronCount, int HiddenLayers,OutputType DefaultOutputType)
        {
            this.InputCount = InputCount;
            this.OutputCount = OutputCount;
            this.NeuronCount = NeuronCount;
            Layers = HiddenLayers + 2;
            outputType = new OutputType[OutputCount];
            for (int i = 0; i < OutputCount; i++)
            {
                outputType[i] = DefaultOutputType;
            }
            Neurons = new float[Layers][];
            Neurons[0] = new float[InputCount];
            for (int i = 1; i < Layers-1; i++)
            {
                Neurons[i] = new float[NeuronCount];
            }
            Neurons[Layers-1] = new float[OutputCount];
            WeightMatrix = new float[(InputCount+OutputCount+(Layers-3)*NeuronCount) * NeuronCount];
        }
        public FeedForwardNetwork(FeedForwardNetwork OldNetwork)
        {
            InputCount = OldNetwork.InputCount;
            OutputCount = OldNetwork.OutputCount;
            NeuronCount = OldNetwork.NeuronCount;
            Layers = OldNetwork.Layers;
            outputType = new OutputType[OldNetwork.outputType.Length];
            OldNetwork.outputType.CopyTo(outputType,0);
            Neurons = new float[Layers][];
            WeightMatrix = new float[OldNetwork.WeightMatrix.Length];
            for (int i = 0; i < Neurons.Length; i++)
            {
                Neurons[i] = new float[OldNetwork.Neurons[i].Length];
            }
            for (int i = 0; i < WeightMatrix.Length; i++)
            {
                WeightMatrix[i] = OldNetwork.WeightMatrix[i];
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">Layer</param>
        /// <param name="j">Start</param>
        /// <param name="k">End</param>
        /// <returns></returns>
        public int ToIndex(int i,int j,int k)
        {
            if (i == 0)//input to neuron
            {
                return j + k * InputCount;
            }
            else//neuron to neuron or neuron to out
            {
                return (i-1 + InputCount) *NeuronCount + j + k * NeuronCount;
            }
        }
        public override void CopyFrom(NeuralNetwork N)
        {
            InputCount = N.InputCount;
            OutputCount = N.OutputCount;
            NeuronCount = N.NeuronCount;
            Layers = N.Layers;
            outputType = new OutputType[N.outputType.Length];
            for (int i = 0; i < N.outputType.Length; i++)
            {
                outputType[i] = N.outputType[i];
            }
            N.outputType.CopyTo(outputType, 0);
            Neurons = new float[Layers][];
            WeightMatrix = new float[N.WeightMatrix.Length];
            for (int i = 0; i < Neurons.Length; i++)
            {
                Neurons[i] = new float[NeuronCount];
            }
            for (int i = 0; i < WeightMatrix.Length; i++)
            {
                WeightMatrix[i] = N.WeightMatrix[i];
            }
        }
        public override void Show(Graphics Graphics, Rectangle Rect)
        {
            Font F = new Font("Arial Black",7);
            StringFormat Sf = new StringFormat();
            Sf.Alignment = StringAlignment.Center;
            Pen Pe = new Pen(Color.Black, 10);
            int Padding = 20;
            int dx = (Rect.Width - Padding * 2) / (Neurons.Length-1);
            int[] dy = new int[Neurons.Length];
            for (int i = 0; i < Neurons.Length; i++)
            {
                dy[i]= (Rect.Height - Padding * 2) / (Neurons[i].Length-1);
            }
            int r = 10;
            Pe.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            Graphics.DrawRectangle(Pe, Rect);
            Graphics.FillRectangle(Brushes.LightGray, Rect);
            Pe = new Pen(Color.Orange, 2);
            for (int i = 0; i < Layers-1; i++)
            {
                for (int j = 0; j < GetNeuronAmount(i); j++)
                {
                    for (int k = 0; k < GetNeuronAmount(i+1); k++)
                    {
                        if (WeightMatrix[ToIndex(i,j,k)] != 0)
                        {
                            Color Col;
                            float mult = Draw.DrawTrueWeights?1:Neurons[i][j]/2;
                            if (WeightMatrix[ToIndex(i, j, k)] > 0)
                                Col = Color.FromArgb((int)(Clamp(WeightMatrix[ToIndex(i, j, k)] * mult, 0, 1) * 255), Color.Red);
                            else
                                Col = Color.FromArgb((int)(Clamp(-WeightMatrix[ToIndex(i, j, k)] * mult, 0, 1) * 255), Color.Blue);
                            Pe.Color = Col;
                            PointF Start = NeuronPos(i,j);
                            PointF End = NeuronPos(i+1,k);
                            Graphics.DrawLine(Pe, Start, End);
                        }
                    }
                    
                }
            }
            for (int i = 0; i < Neurons.Length; i++)
            {
                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    Color C;
                    if(i==Neurons.Length-1)
                    {
                        if (Neurons[i][j] > 0)
                            C = CLerp(Color.Black, Color.Red, Neurons[i][j]);
                        else
                            C = CLerp(Color.Black, Color.Blue, -Neurons[i][j]);
                    }
                    else
                    {
                        C = CLerp(Color.Black, Color.White, Neurons[i][j]);
                    }
                    PointF Pos1 = NeuronPos(i,j);
                    PointF Pos2 = new PointF(Pos1.X - r, Pos1.Y - r);
                    
                    Graphics.FillEllipse(new SolidBrush(C), Pos2.X, Pos2.Y, r * 2, r * 2);
                    Graphics.DrawEllipse(new Pen(Color.Black, 2), Pos2.X, Pos2.Y, r * 2, r * 2);
                    Graphics.DrawString(Neurons[i][j].ToString("0.0"), F, Brushes.Orange, Pos1.X, Pos1.Y-F.Height/2, Sf);
                }
            }
            
            PointF NeuronPos(int i, int j)
            {
                return new PointF(Rect.X+Padding+i*dx, Rect.Y + Padding +j*dy[i]);
            }
        }
        int GetNeuronAmount(int Layer)
        {
            if (Layer == 0)
                return InputCount;
            else if (Layer == Layers - 1)
                return OutputCount;
            else
                return NeuronCount;
        }
        public override void SetInputs(float[] Inputs)
        {
            for (int i = 0; i < Inputs.Length; i++)
            {
                Neurons[0][i] = Inputs[i];
            }
            
        }
        public override float[] GetOutputs()
        {
            return Neurons[Neurons.Length-1];
        }
        public override void Update()
        {
            for (int i = 1; i < Neurons.Length; i++)
            {
                for (int j = 0; j < Neurons[i].Length; j++)
                    Neurons[i][j] = 0;
            }
            for (int i = 0; i < Neurons.Length-1; i++)
            {
                Neurons[i][Neurons[i].Length - 1] = 1;
            }

            for (int i = 0; i < Neurons.Length-1; i++)
            {
                if(i>0)
                    for (int j = 0; j < Neurons[i].Length-1; j++)
                    {
                        //Neurons[i][j] = ActivSig(Neurons[i][j]);
                        if (Neurons[i][j] < 0)
                            Neurons[i][j] = 0;
                        //else
                            //Neurons[i][j] = 0f;
                    }
                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    for (int k = 0; k < Neurons[i+1].Length-1; k++)
                    {
                        Neurons[i+1][k] += Neurons[i][j] * WeightMatrix[ToIndex(i, j, k)];
                    }
                }
            }
            //output
            for (int i = 0; i < OutputCount; i++)
            {
                switch (outputType[i])
                {
                    case OutputType.Sigmoid:
                        Neurons[Neurons.Length - 1][i] = ActivSig(Neurons[Neurons.Length - 1][i]);
                        break;
                    case OutputType.Tanh:
                        Neurons[Neurons.Length - 1][i] = 2*ActivSig(Neurons[Neurons.Length - 1][i])-1;
                        break;
                    case OutputType.Linear:
                        break;
                    case OutputType.LinearClamp:
                        Neurons[Neurons.Length - 1][i] = Clamp(Neurons[Neurons.Length - 1][i],-1,1);
                        break;
                    default:
                        break;
                }
                
            }
        }
        float ActivSig(float x)
        {
            return 1 / (1 + (float)Exp(-x));
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

        
    }
}
