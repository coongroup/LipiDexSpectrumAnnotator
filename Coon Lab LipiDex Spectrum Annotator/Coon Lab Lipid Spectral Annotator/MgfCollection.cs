using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public class MgfCollection
    {
        public string file;
        public Dictionary<int, MgfSpectrum> spectra = new Dictionary<int, MgfSpectrum>();

        public MgfCollection(string file)
        {
            var reader = new StreamReader(file);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line.Equals("BEGIN IONS"))
                {
                    spectra.Add(spectra.Count(), new MgfSpectrum(reader, spectra.Count()));
                }
            }
        }
    }
}
