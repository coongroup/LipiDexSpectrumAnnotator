using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public class MspEntry
    {
        public string standardName;
        public double precursorMz;
        public List<double> fragmentMz;
        public List<string> fragmentLable;

        public MspEntry(StreamReader reader, string standardName)
        {
            this.standardName = standardName;
            this.fragmentMz = new List<double>();
            this.fragmentLable = new List<string>();

            while (true)
            {
                // break out of constructor if end of file
                if (reader.EndOfStream)
                {
                    return;
                }

                var line = reader.ReadLine();

                // reached end of msp entry. leave constructor
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                // line contains precursor m/z value
                if (line.Contains("MW:"))
                {
                    this.precursorMz = Convert.ToDouble(line.Split(new string[] { "MW: " }, StringSplitOptions.RemoveEmptyEntries)[0].Trim());
                }

                // line contains library spectral data
                if (Char.IsDigit(line[0]))
                {
                    var splitLine = line.Split(' ');

                    this.fragmentMz.Add(Convert.ToDouble(splitLine[0]));

                    // some msp entries have spaces, breaking parsing logic. 
                    var recombinedLabel = splitLine[2];

                    for (var i = 3; i < splitLine.Length; i++)
                    {
                        recombinedLabel += "_" + splitLine[i];
                    }

                    // remove quotes... don't wannem
                    recombinedLabel = recombinedLabel.Replace("\"", "");

                    // if first two chars are -_, remove them (not informative
                    if (recombinedLabel[0] == '-' && recombinedLabel[1] == '_')
                    {
                        recombinedLabel = recombinedLabel.Replace("-_", "");
                    }
                    this.fragmentLable.Add(recombinedLabel);
                }
            }
        }

        public bool Matches(double targetMz, out string outLabel, double massTolerance = 0.02)
        {
            outLabel = string.Empty;

            for (var i = 0; i < fragmentMz.Count(); i++)
            {
                var libraryMzMin = fragmentMz[i] - massTolerance; 
                var libraryMzMax = fragmentMz[i] + massTolerance;
                
                if (libraryMzMin <= targetMz && targetMz <= libraryMzMax)
                {
                    outLabel = fragmentLable[i];
                    return true;
                }
            }

            return false;
        }
    }
}
