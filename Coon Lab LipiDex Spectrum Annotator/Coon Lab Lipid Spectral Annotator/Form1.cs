using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace Coon_Lab_LipiDex_Spectrum_Annotator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            MgfSpectrum.PopulateLipiDexLibraries();
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var testWorkingDirectory = @"P:\LRS_2020_endosomes\endo_spectrumsearcherIDs_0p005_MS1_tolerance\";

            // parse MGF collections
            // first, get all MGFs from the working directory
            var spectralFiles = Directory.GetFiles(testWorkingDirectory);

            // read in associated spectra file from LipiDex Peakfinder. This file will be the base of how we retrieve spectra.
            // the associated spectra file links MS2 spectra to chromatographic peaks. 
            var associatedSpectra = ParseAssociatedSpectra(testWorkingDirectory + @"Associated Spectra\Associated_Spectra.csv");

            // now that we have all associated spectra loaded, map the individual spectral match files to get the MGF index
            var spectralMatchFiles = Directory.GetFiles(testWorkingDirectory + @"Spectrum Searcher", "*.csv");

            foreach (var file in spectralMatchFiles)
            {
                // parse associated filename key from the Spectrum Searcher filenames
                var filename = Path.GetFileName(file).Split(new string[] { "_Results" }, StringSplitOptions.RemoveEmptyEntries)[0];

                var reader = new CsvReader(new StreamReader(file), true);

                // Associate spectral matches with the associated features
                AssociateSpectralMatches(reader, associatedSpectra[filename]);
            }

            // now link the mgf spectra with the spectral matches
            var mgfFiles = Directory.GetFiles(testWorkingDirectory + @"MGFs", "*.mgf");

            foreach (var file in mgfFiles)
            {
                var mgfCollection = AssociateMgfEntries(file, associatedSpectra);

                var extractedAssociatedSpectra = ExtractAnnotatedTandemMs(file, associatedSpectra);

                PlotSpectra(extractedAssociatedSpectra, testWorkingDirectory, Path.GetFileNameWithoutExtension(file));
            }
        }

        

        private List<PlottableSpectrum> ExtractAnnotatedTandemMs(string file, Dictionary<string, Dictionary<string, List<AssociatedSpectrum>>> associatedSpectra)
        {
            var returnPlottableSpectra = new List<PlottableSpectrum>();
            var testPath = Path.GetFileNameWithoutExtension(file);
            var thisFileSet = associatedSpectra[Path.GetFileNameWithoutExtension(file)];

            var identifiedLipidsUnadductedKeys = thisFileSet.Keys;

            foreach (var speciesKey in identifiedLipidsUnadductedKeys)
            {
                var lipidSpecies = thisFileSet[speciesKey];

                foreach (var chromaFeature in lipidSpecies)
                {
                    foreach (var spectralMatch in chromaFeature.associatedSpectralMatches)
                    {
                        var plottableSpectrum = new PlottableSpectrum();

                        if (spectralMatch.identificationString.Equals("PC 10:0_18:1 [M+H]+") || spectralMatch.identificationString.Equals("PC 28:1 [M+H]+"))
                        {
                            var t = "";
                        }
                        plottableSpectrum.chromaPeakRT = chromaFeature.retentionTime;
                        plottableSpectrum.dotProduct = Convert.ToDouble(spectralMatch.dotProduct);
                        plottableSpectrum.isolatedPrecursorMz = Convert.ToDouble(spectralMatch.precursorMz);

                        if (string.IsNullOrEmpty(spectralMatch.reportedIdentificationString))
                        {
                            plottableSpectrum.lipidId = spectralMatch.identificationString;
                        }
                        else
                        {
                            plottableSpectrum.lipidId = spectralMatch.reportedIdentificationString;
                        }

                        
                        plottableSpectrum.rawfile = Path.GetFileNameWithoutExtension(file);
                        plottableSpectrum.rawfileScanNumber = spectralMatch.mgfSpectrum.originalScanNumber;
                        plottableSpectrum.mgfIndex = spectralMatch.mgfIndex;
                        plottableSpectrum.spectralPurity = Convert.ToDouble(spectralMatch.purity);
                        plottableSpectrum.spectrumData = spectralMatch.mgfSpectrum;
                        plottableSpectrum.chromaPeakMz = chromaFeature.precursorMz;

                        returnPlottableSpectra.Add(plottableSpectrum);
                    }
                }
            }

            return returnPlottableSpectra;
        }

        private MgfCollection AssociateMgfEntries(string mgfFilePath, Dictionary<string, Dictionary<string, List<AssociatedSpectrum>>> associatedSpectra)
        {
            // open a connection to each MGF and save spectral data
            var mgfCollection = new MgfCollection(mgfFilePath);

            // convert mgf to shared sample name key for dictionary lookup
            var mgfFileKey = Path.GetFileNameWithoutExtension(mgfFilePath);

            var targetFileAssociatedSpectraSet = associatedSpectra[mgfFileKey];

            foreach (var associatedIdSet in targetFileAssociatedSpectraSet.Values)
            {
                foreach (var associatedId in associatedIdSet)
                {
                    foreach (var spectralMatch in associatedId.associatedSpectralMatches)
                    {
                        spectralMatch.mgfSpectrum = mgfCollection.spectra[spectralMatch.mgfIndex];

                        spectralMatch.mgfSpectrum.Annotate(spectralMatch);
                    }
                }
            }

            return mgfCollection;
        }

        private void AssociateSpectralMatches(CsvReader reader, Dictionary<string, List<AssociatedSpectrum>> associatedSpectra)
        {
            while (reader.ReadNextRecord())
            {
                var lipidSpectralMatch = new LipidSpectralMatch(reader);

                // if dictionary does not contain key, pause here and figure out wtf is happening
                if (!associatedSpectra.ContainsKey(lipidSpectralMatch.identificationSpecies))
                {
                    continue;
                }

                var targetIdentification = associatedSpectra[lipidSpectralMatch.identificationSpecies];

                var smallestRtDifference = Double.MaxValue;
                AssociatedSpectrum bestAssociation = null;

                foreach (var associatedSpectrum in targetIdentification)
                {
                    var thisRtDifference = Math.Abs(lipidSpectralMatch.rt - associatedSpectrum.retentionTime);

                    if (thisRtDifference < smallestRtDifference)
                    {
                        smallestRtDifference = thisRtDifference;
                        bestAssociation = associatedSpectrum;
                    }
                }

                // standard LipiDex Filters
                if (smallestRtDifference <= 0.25)
                {
                    bestAssociation.associatedSpectralMatches.Add(lipidSpectralMatch);
                }
            }
        }

        private Dictionary<string, Dictionary<string, List<AssociatedSpectrum>>> ParseAssociatedSpectra(string filePath)
        {
            // read in associated spectra file from LipiDex Peakfinder. This file will be the base of how we retrieve spectra.
            // the associated spectra file links MS2 spectra to chromatographic peaks. 
            var associatedSpectra = new Dictionary<string, Dictionary<string, List<AssociatedSpectrum>>>();

            var reader = new CsvReader(new StreamReader(filePath), true);

            while (reader.ReadNextRecord())
            {
                if (reader["Associated"].Equals("Associated"))
                {
                    
                    var formattedSampleName = reader["Sample"].Split(new string[] { ".raw" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (!associatedSpectra.ContainsKey(formattedSampleName))
                    {
                        associatedSpectra.Add(formattedSampleName, new Dictionary<string, List<AssociatedSpectrum>>());
                    }

                    if (!associatedSpectra[formattedSampleName].ContainsKey(reader["Name"]))
                    {
                        associatedSpectra[formattedSampleName].Add(reader["Name"], new List<AssociatedSpectrum>());
                    }

                    associatedSpectra[formattedSampleName][reader["Name"]].Add(new AssociatedSpectrum(reader));
                }
            }

            return associatedSpectra;
        }

        private void PlotSpectra(List<PlottableSpectrum> spectraCollection, string wd, string fileName)
        {
            // if output PDF directory is not created, create it now
            var basePdfDir = wd + @"Annotated Spectra\";
            if (!Directory.Exists(basePdfDir))
            {
                Directory.CreateDirectory(basePdfDir);
            }

            var fileSpecificDir = basePdfDir + fileName + "\\";
            // check if file-specific directory is created
            if (!Directory.Exists(fileSpecificDir))
            {
                Directory.CreateDirectory(fileSpecificDir);
            }

            foreach (var spectrum in spectraCollection)
            {
                // don't plot anything with a dot prod < 500
                if (spectrum.dotProduct < 500)
                {
                    continue;
                }

                // try plotting each spectrum...
                // do the plotting
                var myModel = new PlotModel
                {
                    //Title = "Lipid Identification | MGF Index | Other Info"
                    Title = TitleBuilder(spectrum)
                    //Title = CompiledTitleBuilder(spectrum)
                };

                

                // calculate dynamic coord bounds. Round to lower/upper hundred for xMin and xMax 
                var xMin = Math.Floor(spectrum.spectrumData.mzPeaks.First() / 100d) * 100;
                var xMax = Math.Ceiling(spectrum.spectrumData.mzPeaks.Last() / 100d) * 100;

                // set x-axis title and bounds
                myModel.Axes.Add(new LinearAxis()
                {
                    Minimum = xMin,
                    Maximum = xMax,
                    Position = AxisPosition.Bottom,
                    Title = "m/z"
                });

                // set y-axis title and bounds
                myModel.Axes.Add(new LinearAxis()
                {
                    Minimum = 0,
                    Maximum = 125,
                    MajorStep = 10,
                    Position = AxisPosition.Left,
                    Title = "Relative Intensity", 
                });

                var scanHeaderAnnotation = new TextAnnotation
                {
                    Text = FormatAnnotationHeader(spectrum),
                    TextPosition = new DataPoint(xMin + 10, 120),
                    StrokeThickness = 0,
                    TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left
                };
                myModel.Annotations.Add(scanHeaderAnnotation);

                for (var i = 0; i < spectrum.spectrumData.mzPeaks.Count(); i++)
                {
                    // add peaks for each spectral feature
                    var xCord = spectrum.spectrumData.mzPeaks[i];
                    var yCord = spectrum.spectrumData.relativeIntensities[i];

                    var lineSeries = new LineSeries();

                    lineSeries.Points.Add(new DataPoint(xCord, 0));
                    lineSeries.Points.Add(new DataPoint(xCord, yCord));

                    lineSeries.Color = spectrum.spectrumData.lineColors[i];
                    lineSeries.LineStyle = LineStyle.Solid;
                    lineSeries.StrokeThickness = spectrum.spectrumData.lineStrokes[i];

                    myModel.Series.Add(lineSeries);

                    // add labels if they exist
                    if (!string.IsNullOrWhiteSpace(spectrum.spectrumData.labels[i]))
                    {
                        // label string
                        var textAnnotation = new TextAnnotation
                        {
                            Text = spectrum.spectrumData.labels[i],
                            TextPosition = new DataPoint(xCord, yCord + 4),
                            StrokeThickness = 0, 
                        };
                        myModel.Annotations.Add(textAnnotation);

                        textAnnotation = new TextAnnotation
                        {
                            Text = xCord.ToString("n4"),
                            TextPosition = new DataPoint(xCord, yCord + 0),
                            StrokeThickness = 0
                        };
                        myModel.Annotations.Add(textAnnotation);
                    }
                }

                this.plotView1.Model = myModel;
                this.plotView1.Refresh();
                ExportSpectrumPDF(this.plotView1.Model, fileSpecificDir, spectrum);
            }
        }

        private void ExportSpectrumPDF(PlotModel plotModel, string fileDirectory, PlottableSpectrum spectrum)
        {
            var outputPath = string.Format("{0}{1}_{2}mz_{3}mins_Scan{4}.pdf", fileDirectory, spectrum.lipidId.Replace(':', '_'),
                spectrum.chromaPeakMz, spectrum.chromaPeakRT, spectrum.rawfileScanNumber);

            var fileStream = File.Create(outputPath);

            var pdfExporter = new PdfExporter { Width = 1252, Height = 541 };
            pdfExporter.Export(plotModel, fileStream);

            fileStream.Close();
            fileStream.Dispose();
        }

        private string TitleBuilder(PlottableSpectrum spectrum)
        {
            return string.Format("{0} | Ret. Time (min): {1} | m/z: {2}", spectrum.lipidId, spectrum.chromaPeakRT, spectrum.isolatedPrecursorMz);
        }

        private string FormatAnnotationHeader(PlottableSpectrum spectrum)
        {
            return string.Format("{0}.raw, Scan Number: {1}, Isolation m/z: {2}, Dot Product: {3}", 
                spectrum.rawfile, spectrum.rawfileScanNumber, spectrum.isolatedPrecursorMz, spectrum.dotProduct);
        }

        private string CompiledTitleBuilder(PlottableSpectrum spectrum)
        {
            return string.Format("{0}, Scan {1} | {2} | Ret. Time (min): {3} | m/z: {4}",  spectrum.lipidId, spectrum.chromaPeakRT, spectrum.isolatedPrecursorMz);
        }
    }
}
