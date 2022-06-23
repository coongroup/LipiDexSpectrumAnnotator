using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public class PlottableSpectrum
    {
        // main plot title ID
        public string lipidId;
        public double chromaPeakRT;
        public double chromaPeakMz;

        // "scan header" labels
        public bool preferredPolarity;
        public string rawfile;
        public double isolatedPrecursorMz;
        public double dotProduct;
        public double spectralPurity;
        public int mgfIndex;
        public int rawfileScanNumber;

        // spectral data
        public MgfSpectrum spectrumData;

        public PlottableSpectrum() { }
    }
}
