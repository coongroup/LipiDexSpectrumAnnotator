using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public class LipidexLibrary
    {
        public string name;
        public Dictionary<string, MspEntry> libraryEntries;

        public LipidexLibrary(string mspFile)
        {
            this.name = Path.GetFileName(mspFile);
            this.libraryEntries = new Dictionary<string, MspEntry>();

            var reader = new StreamReader(mspFile);
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line.Contains("Name:"))
                {
                    var lipidName = line.Split(new string[] { "Name:" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().Replace(";", "");
                    
                    if (libraryEntries.ContainsKey(lipidName))
                    {
                        continue;
                    }

                    libraryEntries.Add(lipidName, new MspEntry(reader, lipidName));
                }
            }
        }
    }
}
