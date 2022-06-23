using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public class AssociatedSpectrum
    {
        public string lipidName;
        public double precursorMz;
        public double retentionTime;
        public string sampleName;
        public double dotProduct;
        public double purity;
        public List<LipidSpectralMatch> associatedSpectralMatches;

        public AssociatedSpectrum(CsvReader reader)
        {
            this.sampleName = reader["Sample"].Split(new string[] { ".raw" }, StringSplitOptions.RemoveEmptyEntries)[0];
            this.lipidName = reader["Name"];
            this.precursorMz = Convert.ToDouble(reader["Precursor"]);
            this.retentionTime = Convert.ToDouble(reader["Retention"]);
            this.dotProduct = Convert.ToDouble(reader["DotProduct"]);
            this.purity = Convert.ToDouble(reader["Purity"]);
            this.associatedSpectralMatches = new List<LipidSpectralMatch>();
        }
    }
}
