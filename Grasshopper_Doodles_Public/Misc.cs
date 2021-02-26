using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grasshopper_Doodles_Public
{
    class Misc
    {

        /// <summary>
        /// This method will give you a stepsize when iterating through results for instance.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="roundToNearest"></param>
        /// <returns></returns>
        public static double AutoStep(double[] domain, out double roundToNearest)
        {

            const double TARGET = 12.0; //<-- we are aiming for around 12 steps. 10 is ideal, but less than 10 is not. That's why we went for 12.

            double range = domain[1] - domain[0];
            double digits = Math.Round(Math.Log10(range));

            double multiplier2 = Math.Pow(10, digits - 2);

            double min2 = double.MaxValue;
            double[] multipliers = new double[]
            {
                multiplier2,
                multiplier2 * 2,
                multiplier2 * 5,
                multiplier2 * 10,
                multiplier2 * 20,
                multiplier2 * 50
            };

            Console.WriteLine($"{String.Join(", ", multipliers)}");

            for (int i = 0; i < multipliers.Length; i++)
            {
                if (Math.Abs(TARGET - range / multipliers[i]) < Math.Abs(TARGET - range / min2))
                {
                    min2 = multipliers[i];
                }
            }
            roundToNearest = multiplier2 * 10;

            // DEBUG 
            //Console.WriteLine($"StepSize = {min2:0.0000} will yield (roundToNearest {roundToNearest}: {String.Join(",  ", Enumerable.Range(Convert.ToInt32(0), 1 + Convert.ToInt32((domain[1] - domain[0]) / min2)).Select(p => domain[0] + p * min2).Select(p => p.ToString("F3"))):0.0}");
            //Console.WriteLine($"StepSize = {min2:0.00} will yield: {String.Join(",  ", Enumerable.Range(Convert.ToInt32(0), 1 + Convert.ToInt32((domain[1] - domain[0]) / min2)).Select(p => domain[0] + p * min2).Select(p => p.ToString("F3"))):0.0}");

            return min2;


        }


        /// <summary>
        /// Creates a range from x to y based on the range input. if nothing is specified, then min and max of the numbers is used.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static double[] AutoDomain(string range, GH_Structure<GH_Number> numbers)
        {
            if (TryGetDomain(range, out double?[] testDomain))
            {
                return new double[] { testDomain[0].Value, testDomain[1].Value };
            }
            else
            {
                double min = double.MaxValue;
                double max = -double.MaxValue;
                for (int i = 0; i < numbers.Branches.Count; i++)
                {

                    for (int j = 0; j < numbers[i].Count; j++)
                    {
                        double num = numbers[i][j].Value;
                        if (num < min)
                            min = num;
                        if (num > max)
                            max = num;
                    }


                }
                return new double[] { min, max };
            }

        }



        /// <summary>
        /// Generates a domain based on string or double inputs. Or the GH "5 to 10" domain.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool TryGetDomain(string range, out double?[] domain)
        {
            string[] words;
            domain = new double?[] { null, null };
            bool success = Double.TryParse(range, out double max);
            if (success)
            {
                domain[1] = max;
                return true;
            }

            if (range != String.Empty)
            {
                //String[] seperator = { " " };
                words = range.Split(' ');

                if (words.Length == 3)
                {
                    string start = words[0];
                    string to = words[1];
                    string end = words[2];


                    if (to.ToLower() == "to")
                    {

                        if (Double.TryParse(start, out double domainStart) && Double.TryParse(end, out double domainEnd))
                        {
                            domain[0] = domainStart;
                            domain[1] = domainEnd;
                            return true;
                        }
                    }
                }

            }
            return false;


        }

    }
}
