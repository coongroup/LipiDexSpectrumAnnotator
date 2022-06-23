using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LumenWorks.Framework.IO.Csv;

namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public class LipidSpectralMatch
    {
        public int mgfIndex;
        public double rt;
        public string identificationString;
        public string identificationSpecies;
        public string precursorMz;
        public string dotProduct;
        public int purity;
        public string library;
        public bool preferredPolarity;
        public MgfSpectrum mgfSpectrum;
        public string reportedIdentificationString;
        public string reportedIdentificationSpecies;
       
        public LipidSpectralMatch(CsvReader reader)
        {
            this.mgfIndex = Convert.ToInt32(reader["MS2 ID"]);
            this.rt = Convert.ToDouble(reader["Retention Time (min)"]);
            this.identificationString = reader["Identification"].Replace(";", "");
            this.identificationSpecies = this.identificationString.Split(new string[] { "[M" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            this.precursorMz = reader["Precursor Mass"];
            this.dotProduct = reader["Dot Product"];
            this.purity = Convert.ToInt32(reader["Purity"]);
            this.library = reader["Library"];
            this.preferredPolarity = reader["Optimal Polarity"].Equals("true");

            CollapseToSpeciesLevelIds();
        }

        private void CollapseIdentificationString()
        {
            var returnString = "";

            var endAdduct = this.identificationString.Split(new string[] { "[M" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            returnString += this.reportedIdentificationSpecies + " [M" + endAdduct;

            this.reportedIdentificationString = returnString;
        }

        private void CollapseToSpeciesLevelIds()
        {
            if (!this.preferredPolarity || this.purity < 75)
            {
                var unsplitAcylChains = this.identificationSpecies.Split(' ').Last();

                var splitAcylChains = unsplitAcylChains.Split('_').ToList();

                var preSpeciesModifier = "";
                var carbons = 0;
                var dbes = 0;

                foreach (var chain in splitAcylChains)
                {
                    var thisChain = chain;

                    if (chain.Contains("d"))
                    {
                        thisChain = chain.Replace("d", "");

                        preSpeciesModifier = "d";
                    }
                    else if (chain.Contains("P-"))
                    {
                        thisChain = chain.Replace("P-", "");

                        preSpeciesModifier = "P-";
                    }
                    else if(chain.Contains("O-"))
                    {
                        thisChain = chain.Replace("O-", "");

                        preSpeciesModifier = "O-";
                    }

                    var splitChain = thisChain.Split(':');
                    carbons += Convert.ToInt32(splitChain[0]);
                    dbes += Convert.ToInt32(splitChain[1]);
                }

                var reformattedIdeentificationSpecies = "";

                reformattedIdeentificationSpecies += this.identificationSpecies.Split(' ').First();

                reformattedIdeentificationSpecies += " " + preSpeciesModifier + carbons + ":" + dbes;

                this.reportedIdentificationSpecies = reformattedIdeentificationSpecies;

                CollapseIdentificationString();
            }
        }
    }
}
