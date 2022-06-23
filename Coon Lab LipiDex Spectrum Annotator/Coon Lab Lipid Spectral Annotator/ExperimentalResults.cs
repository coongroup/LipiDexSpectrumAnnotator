using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public class ExperimentalResults
    {

        public Dictionary<string, List<AssociatedSpectrum>> associatedSpectra;
        public Dictionary<string, List<LipidSpectralMatch>> lipidSpectralMatches;
        public Dictionary<int, MgfSpectrum> spectra;

        //public List<MappedData> mappedSpectraForAnnotation;
    }
}
