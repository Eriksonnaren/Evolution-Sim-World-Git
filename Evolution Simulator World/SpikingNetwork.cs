using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulator_World
{
    [Serializable]
    class SpikingNetwork : NeuralNetwork
    {
        enum SynapsDir
        {
            Forward,
            Backward
        }
        [Serializable]
        struct Synaps
        {
            public Synaps(int Id, SynapsDir Dir)
            {
                this.Id = Id;
                this.Dir = Dir;
                Weight = -0.5f +(0.75f+0.5f)*(float)Form1.Rand.NextDouble();
                Time = Form1.Rand.Next(2, 15);
            }
            public int Id;
            public SynapsDir Dir;
            public float Weight;
            public int Time;
        }
        float[] Inputs;
        float[] Outputs;
        Neuron[][] Neurons;
        float NeuronDecay = 0.05f;
        [Serializable]
        struct Neuron
        {
            public Neuron(int timer_max)
            {
                this.timer_max = timer_max;
                Pulses = new List<int>();
                Synapses = new List<Synaps>();
                timer = 0;
                value = 0;
            }
            public float value;
            public int timer;
            public int timer_max;
            public List<int> Pulses;
            public List<Synaps> Synapses;
            public void Add(float Val)
            {
                if (timer == 0)
                    value += Val;
            }
        }
        const int PulseMaxLength= 15;
        public SpikingNetwork(int InputCount, int OutputCount, int Layers, int NeuronAmount,int SynapsAmount)
        {
            Inputs = new float[InputCount];
            Outputs = new float[OutputCount];
            Neurons = new Neuron[Layers + 2][];
            Neurons[0] = new Neuron[InputCount];
            Neurons[Layers+1] = new Neuron[OutputCount];
            for (int i = 0; i < Layers; i++)
            {
                Neurons[i + 1] = new Neuron[NeuronAmount];
            }
            for (int i = 0; i < Layers+2; i++)
            {
                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    Neurons[i][j]=new Neuron(Form1.Rand.Next(3,7));
                    if(i<Neurons.Length-1)
                        for (int k = 0; k < Neurons[i+1].Length; k++)
                        {
                            if (Form1.Rand.NextDouble() > 0.1)
                            {
                                Synaps S = new Synaps(k, SynapsDir.Forward);
                                Neurons[i][j].Synapses.Add(S);
                            }
                        }
                    if (i > 0)
                        for (int k = 0; k < Neurons[i - 1].Length; k++)
                        {
                            if (Form1.Rand.NextDouble() > 0.01)
                            {
                                Synaps S = new Synaps(k, SynapsDir.Backward);
                                Neurons[i][j].Synapses.Add(S);
                            }
                        }
                }
            }
            /*for (int i = 0; i < SynapsAmount; i++)
            {
                int Layer = Form1.Rand.Next(Neurons.Length);
                SynapsDir Dir = (Form1.Rand.NextDouble()>0.25||Layer==0)&& Layer !=Layers+1? SynapsDir.Forward : SynapsDir.Backward;
                if(Dir== SynapsDir.Forward)
                {
                    Synaps S = new Synaps(Form1.Rand.Next(Neurons[Layer + 1].Length), Dir, Lerp(-0.2f, 0.75f, (float)Form1.Rand.NextDouble()), Form1.Rand.Next(2, 5));
                    Neurons[Layer][Form1.Rand.Next(Neurons[Layer].Length)].Synapses.Add(S);
                }else
                {
                    Synaps S = new Synaps(Form1.Rand.Next(Neurons[Layer - 1].Length), Dir, Lerp(-0.2f, 0.75f, (float)Form1.Rand.NextDouble()), Form1.Rand.Next(2, 5));
                    Neurons[Layer][Form1.Rand.Next(Neurons[Layer].Length)].Synapses.Add(S);
                }
            }*/
        }
        public override void CopyFrom(NeuralNetwork N)
        {
            SpikingNetwork N2 = N as SpikingNetwork;
            OutputCount = N.OutputCount;
            Layers = N.Layers;
            NeuronCount = N.NeuronCount;
            InputCount = N.InputCount;
            Outputs = new float[OutputCount];
            Inputs = new float[InputCount];
            Neurons = new Neuron[Layers][];
            for (int i = 0; i < Layers; i++)
            {
                Neurons[i] = new Neuron[N2.Neurons[i].Length];
            }
            for (int i = 0; i < Layers; i++)
            {
                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    Neurons[i][j] = new Neuron(N2.Neurons[i][j].timer_max);
                    foreach (var S in N2.Neurons[i][j].Synapses)
                    {
                        Neurons[i][j].Synapses.Add(S);
                    }
                }
            }
        }
        public override void Show(Graphics Graphics, Rectangle Rect)
        {
            Pen Pe = new Pen(Color.Black, 10);
            int Padding = 20;
            int dY = (Rect.Height - Padding * 2) / (Inputs.Length - 1);
            int dX = (Rect.Width - Padding * 2) / (Neurons.Length - 1);
            int r = 10;
            Pe.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            Graphics.DrawRectangle(Pe, Rect);
            Graphics.FillRectangle(Brushes.LightGray, Rect);
            for (int i = 0; i < Neurons.Length; i++)
            {
                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    Color C = CLerp(Color.Black, Color.White, Clamp(Neurons[i][j].value,0,1));
                    PointF Pos = new PointF(Rect.X + Padding + dX * i - r, Rect.Y + Padding + dY * j - r);
                    Graphics.FillEllipse(new SolidBrush(C), Pos.X, Pos.Y, r * 2, r * 2);
                    Graphics.DrawEllipse(new Pen(Color.Black, 2), Pos.X, Pos.Y, r * 2, r * 2);
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
            for (int i = 0; i < Outputs.Length; i++)
            {
                Outputs[i] *= 0.99f;
            }
            for (int i = 0; i < Neurons[0].Length; i++)
            {
                Neurons[0][i].Add(Inputs[i]*0.2f);
            }
            for (int i = 0; i < Neurons.Length; i++)
            {
                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    if (Neurons[i][j].timer > 0)
                    {
                        Neurons[i][j].timer--;
                        Neurons[i][j].value = 0;
                    }
                    else
                    {
                        for (int k = 0; k < Neurons[i][j].Pulses.Count; k++)
                        {
                            Neurons[i][j].Pulses[k]++;
                            if (Neurons[i][j].Pulses[k] > PulseMaxLength)
                                Neurons[i][j].Pulses.RemoveAt(k);
                            else
                            foreach (Synaps S in Neurons[i][j].Synapses)
                            {
                                if (S.Time == Neurons[i][j].Pulses[k])
                                {
                                    if (S.Dir == SynapsDir.Forward)
                                        Fire(i + 1, S.Id,S.Weight);
                                    else
                                        Fire(i - 1, S.Id, S.Weight);
                                }
                            }
                        }
                        if (Neurons[i][j].value >= 1)
                        {
                            Neurons[i][j].Pulses.Add(0);
                            Neurons[i][j].timer = Neurons[i][j].timer_max;
                        }
                        Neurons[i][j].value -= NeuronDecay* Neurons[i][j].value;
                    }
                }
            }
        }
        void Fire(int i,int j,float Value)
        {
            Neurons[i][j].Add(Value);
            if(i==Neurons.Length-1)
            {
                Outputs[j] = Clamp(Outputs[j]+Value,0,1);
            }
        }
        public new void Mutate(float MutationRate, float MinValue = 0, float MaxValue = 0)
        {
            int Layer = Form1.Rand.Next(Neurons.Length - 1);
            int Source = Form1.Rand.Next(Neurons[Layer].Length);
            int Target = Form1.Rand.Next(Neurons[Layer + 1].Length);
            SynapsDir Dir;
            if(Form1.Rand.NextDouble() < 0.5)
            {
                Dir = SynapsDir.Forward;
            }else
            {
                Dir = SynapsDir.Backward;
                int temp = Source;
                Source = Target;
                Target = temp;
            }
            int index = SynapsExist(Layer, Source, Target, Dir);
            if (index != -1)
            {
                Neurons[Layer][Source].Synapses.RemoveAt(index);
                if(Form1.Rand.NextDouble() < 0.6)
                {
                    Neurons[Layer][Source].Synapses.Add(new Synaps(Target,Dir));
                }
            }else
            {
                Neurons[Layer][Source].Synapses.Add(new Synaps(Target,Dir));
            }
        }
        int SynapsExist(int Layer,int Source,int Target,SynapsDir Dir)
        {
            for (int i = 0; i < Neurons[Layer][Source].Synapses.Count; i++)
            {
                Synaps S = Neurons[Layer][Source].Synapses[i];
                if (S.Dir == Dir && S.Id == Target)
                    return i;
            }
            return -1;
        }
    }
}

