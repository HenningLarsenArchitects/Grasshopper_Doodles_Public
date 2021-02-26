using Grasshopper.GUI.Gradient;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grasshopper_Doodles_Public
{
    class GradientParser
    {
        // Source primarily from : https://discourse.mcneel.com/t/get-values-from-a-gradient-component/108532/6

        readonly Grasshopper.GUI.Gradient.GH_Gradient Gradient = new Grasshopper.GUI.Gradient.GH_Gradient();
        public double? Max { get; set; } = null;
        public double? Min { get; set; } = null;
        public Color AboveMax { get; set; }
        public Color BelowMin { get; set; }
        public bool Cap { get; set; }
        public bool Reverse { get; set; } = false;

        public GradientParser(GH_GradientControl gradientControl = null)
        {
            if (gradientControl != null)
            {
                Gradient = gradientControl.Gradient;

            }
            else
            {
                Gradient = GH_Gradient.Heat();

            }

            List<double> gripsParameters;
            List<Color> gripsColourLeft;
            List<Color> gripsColourRight;



            try
            {

                //this.Params.Input[0].Sources[0].Sources[0];




                var param = (GH_PersistentParam<GH_Number>)gradientControl.Params.Input[2];
                if (param.VolatileData.IsEmpty)
                {
                    param.PersistentData.Append(new GH_Number(1.0));

                    param.ExpireSolution(true);
                    return;
                }

                bool isLinear = Gradient.Linear;
                bool isLocked = Gradient.Locked;
                int gripCount = Gradient.GripCount;

                var parameters = new List<double>();
                var colourLeft = new List<Color>();
                var colourRight = new List<Color>();

                for (var i = 0; i < Gradient.GripCount; i++)
                {
                    parameters.Add(Gradient[i].Parameter);
                    colourLeft.Add(Gradient[i].ColourLeft);
                    colourRight.Add(Gradient[i].ColourRight);
                }
                gripsParameters = parameters;
                gripsColourLeft = colourLeft;
                gripsColourRight = colourRight;

            }
            catch
            {
            }

        }

        /// <summary>
        /// Use this method to get N colors
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Color[] GetDefaultColors(int count = 50)
        {

            Color[] colors = new Color[count < 2 ? 2 : count];

            for (int i = 0; i < count; i++)
            {
                colors[i] = Gradient.ColourAt(i / (count - 1));
            }

            return colors;

        }

        public Color[] GetColors(IList<double> data)
        {
            //Rhino.RhinoApp.WriteLine($"{Min} to {Max}.. .and data is from {data.Min()} to {data.Max()}");
            //if (Cap)
            //    Rhino.RhinoApp.WriteLine("CAPPED");
            //else
            //    Rhino.RhinoApp.WriteLine("UNCAPPED");
            Color[] colors = new Color[data.Count];

            if (!Min.HasValue || !Max.HasValue)
                throw new Exception("Min or Max wasnt set for the GradientParser. Please do that before using me");
            //int k = 1;
            if (Reverse)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i] >= Min.Value && data[i] <= Max.Value)
                        colors[i] = Gradient.ColourAt(1 - (data[i] - Min.Value) / (Max.Value - Min.Value));
                    else if (data[i] < Min)
                    {
                        //Rhino.RhinoApp.WriteLine($"data {data[i]} is below {Min}");
                        if (Cap)
                            colors[i] = BelowMin;
                        else
                            colors[i] = Gradient.ColourAt(0);
                    }

                    else
                    {
                        //Rhino.RhinoApp.WriteLine($"data {data[i]} is above {Max}");
                        if (Cap)
                            colors[i] = AboveMax;
                        else
                            colors[i] = Gradient.ColourAt(1);

                    }

                }
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (!Cap || (Cap && (data[i] >= Min.Value && data[i] <= Max.Value)))
                        colors[i] = Gradient.ColourAt((data[i] - Min.Value) / (Max.Value - Min.Value));
                    else if (data[i] < Min)
                    {
                        //Rhino.RhinoApp.WriteLine($"data {data[i]} is below {Min}");
                        if (Cap)
                            colors[i] = BelowMin;
                        else
                            colors[i] = Gradient.ColourAt(0);
                    }

                    else
                    {
                        //Rhino.RhinoApp.WriteLine($"data {data[i]} is above {Max}");
                        if (Cap)
                            colors[i] = AboveMax;
                        else
                            colors[i] = Gradient.ColourAt(1);

                    }
                }
            }


            return colors;

        }

    }
}
