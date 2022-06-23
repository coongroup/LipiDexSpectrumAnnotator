using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using OxyPlot;

namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public class MgfSpectrum
    {
        public static Dictionary<string, LipidexLibrary> spectralLibraries = new Dictionary<string, LipidexLibrary>();

        public int spectrumIndex;
        public int originalScanNumber;
        public List<double> mzPeaks;
        public List<double> rawIntensities;
        public List<double> relativeIntensities;
        public List<string> labels;
        public List<OxyColor> lineColors;
        public List<double> lineStrokes;

        public MgfSpectrum(StreamReader reader, int spectrumNumber)
        {
            this.mzPeaks = new List<double>();
            this.rawIntensities = new List<double>();
            this.relativeIntensities = new List<double>();
            this.labels = new List<string>();
            this.lineColors = new List<OxyColor>();
            this.lineStrokes = new List<double>();

            while (true)
            {
                if (reader.EndOfStream)
                {
                    return;
                }

                var line = reader.ReadLine();

                if (line.Contains("TITLE"))
                {
                    var splitString = line.Split(new string[] { " scan=" }, StringSplitOptions.RemoveEmptyEntries);

                    this.originalScanNumber = Convert.ToInt32(splitString[1].Replace("\"", ""));
                }
                if (line.Equals("END IONS"))
                {
                    if (mzPeaks.Count > 0)
                    {
                        CalculateRelativeIntensities();
                    }
                    
                    return;
                }

                if (Char.IsDigit(line[0]))
                {
                    var splitLine = line.Split(' ');

                    this.mzPeaks.Add(Convert.ToDouble(splitLine[0]));
                    this.rawIntensities.Add(Convert.ToDouble(splitLine[1]));
                    this.labels.Add(string.Empty);

                    //default all line colors to dark gray and stroke thickness 1
                    this.lineColors.Add(OxyColors.DarkGray);
                    this.lineStrokes.Add(1);
                }
            }
        }

        private void CalculateRelativeIntensities()
        {
            var maxIntensity = this.rawIntensities.Max();

            foreach (var intensity in rawIntensities)
            {
                var relativeIntensity = (intensity / maxIntensity) * 100;

                this.relativeIntensities.Add(relativeIntensity);
            }
        }

        public void Annotate(LipidSpectralMatch match)
        {
            // get relevant libary entry from the lipid spectral match
            var targetLibraryEntry = MgfSpectrum.spectralLibraries[match.library].libraryEntries[match.identificationString];

            for (var i = 0; i < this.mzPeaks.Count; i++)
            {
                var outlabel = string.Empty;

                if (targetLibraryEntry.Matches(this.mzPeaks[i], out outlabel)) 
                {
                    this.labels[i] = outlabel;
                    this.lineColors[i] = OxyColors.DarkRed;
                    this.lineStrokes[i] = 2.25;
                }
                else
                {
                    this.labels[i] = outlabel;
                }
            }
            var t = "";
        }

        public static void PopulateLipiDexLibraries()
        {
            spectralLibraries = new Dictionary<string, LipidexLibrary>();

            var files = Directory.GetFiles(@"P:\LRS_2020_endosomes\endo_spectrumsearcherIDs_LRS12022021\LipiDex Msp Files\", "*.msp");

            foreach (var file in files)
            {
                spectralLibraries.Add(Path.GetFileName(file), new LipidexLibrary(file));
            }
        }


    }
}
