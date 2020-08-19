using System;
using System.IO;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoRawFileParser.Writer;

namespace ThermoRawFileParser
{
    public static class RawFileParser
    {
        /*private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);*/

        /// <summary>
        /// Process and extract the RAW file(s). 
        /// </summary>
        /// <param name="parseInput">the parse input object</param>
        public static void Parse(ParseInput parseInput)
        {
            // Input raw folder mode
            if (parseInput.RawDirectoryPath != null)
            {
                //Log.Info("Started analyzing folder " + parseInput.RawDirectoryPath);
                Console.WriteLine("Started analyzing folder " + parseInput.RawDirectoryPath);

                var rawFilesPath =
                    Directory.EnumerateFiles(parseInput.RawDirectoryPath);
                if (Directory.GetFiles(parseInput.RawDirectoryPath, "*", SearchOption.TopDirectoryOnly).Length == 0)
                {
                    //Log.Debug("No raw files found in folder");
                    throw new RawFileParserException("No raw files found in folder!");
                }

                foreach (var filePath in rawFilesPath)
                {
                    parseInput.RawFilePath = filePath;
                    //Log.Info("Started parsing " + parseInput.RawFilePath);
                    try
                    {
                        ProcessFile(parseInput);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        /*Log.Error(!ex.Message.IsNullOrEmpty()
                            ? ex.Message
                            : "Attempting to write to an unauthorized location.");*/
                    }
                    catch (Exception ex)
                    {
                        if (ex is RawFileParserException)
                        {
                            //Log.Error(ex.Message);
                        }
                        else
                        {
                            //Log.Error("An unexpected error occured while parsing file:" + parseInput.RawFilePath);
                            //Log.Error(ex.ToString());
                        }
                    }
                }
            }
            // Input raw file mode
            else
            {
                //Log.Info("Started parsing " + parseInput.RawFilePath);

                ProcessFile(parseInput);
            }
        }

        public static RawFileWrapper InitRawFile(ParseInput parseInput)
        {
            // Create the IRawDataPlus object for accessing the RAW file
            IRawDataPlus rawFile;
            rawFile = RawFileReaderFactory.ReadFile(parseInput.RawFilePath);

            if (!rawFile.IsOpen)
            {
                throw new RawFileParserException("Unable to access the RAW file using the native Thermo library.");
            }

            // Check for any errors in the RAW file
            if (rawFile.IsError)
            {
                throw new RawFileParserException($"Error opening ({rawFile.FileError}) - {parseInput.RawFilePath}");
            }

            // Check if the RAW file is being acquired
            if (rawFile.InAcquisition)
            {
                throw new RawFileParserException("RAW file still being acquired - " + parseInput.RawFilePath);
            }

            // Get the number of instruments (controllers) present in the RAW file and set the 
            // selected instrument to the MS instrument, first instance of it
            rawFile.SelectInstrument(Device.MS, 1);

            // Get the first and last scan from the RAW file
            var firstScanNumber = rawFile.RunHeaderEx.FirstSpectrum;
            var lastScanNumber = rawFile.RunHeaderEx.LastSpectrum;
            //Console.WriteLine("Last scan number is " + lastScanNumber);

            return new RawFileWrapper(rawFile, firstScanNumber, lastScanNumber, parseInput);
        }

        /// <summary>
        /// Process and extract the given RAW file. 
        /// </summary>
        /// <param name="parseInput">the parse input object</param>
        private static void ProcessFile(ParseInput parseInput)
        {
            var rawFileWrapper = InitRawFile(parseInput);
            ProcessFile(rawFileWrapper.RawFile, rawFileWrapper.FirstScanNumber, rawFileWrapper.LastScanNumber, parseInput);
            rawFileWrapper.Dispose();
        }

        private static void ProcessFile(IRawDataPlus rawFile, int firstScanNumber, int lastScanNumber, ParseInput parseInput)
        {

            if (parseInput.MetadataFormat != MetadataFormat.NONE)
            {
                MetadataWriter metadataWriter;
                if (parseInput.MetadataOutputFile != null)
                {
                    metadataWriter = new MetadataWriter(null, parseInput.MetadataOutputFile);
                }
                else
                {
                    metadataWriter = new MetadataWriter(parseInput.OutputDirectory,
                        parseInput.RawFileNameWithoutExtension);
                }

                switch (parseInput.MetadataFormat)
                {
                    case MetadataFormat.JSON:
                        metadataWriter.WriteJsonMetada(rawFile, firstScanNumber, lastScanNumber);
                        break;
                    case MetadataFormat.TXT:
                        metadataWriter.WriteMetadata(rawFile, firstScanNumber, lastScanNumber);
                        break;
                }
            }

            if (parseInput.OutputFormat != OutputFormat.NONE)
            {
                SpectrumWriter spectrumWriter;
                switch (parseInput.OutputFormat)
                {
                    case OutputFormat.MGF:
                        spectrumWriter = new MgfSpectrumWriter(rawFile, parseInput);
                        spectrumWriter.Write(firstScanNumber, lastScanNumber);
                        break;
                    case OutputFormat.MzML:
                    case OutputFormat.IndexMzML:
                        spectrumWriter = new MzMlSpectrumWriter(rawFile, parseInput);
                        spectrumWriter.Write(firstScanNumber, lastScanNumber);
                        break;
                    case OutputFormat.Parquet:
                        spectrumWriter = new ParquetSpectrumWriter(rawFile, parseInput);
                        spectrumWriter.Write(firstScanNumber, lastScanNumber);
                        break;
                }
            }

            //Log.Info("Finished parsing " + parseInput.RawFilePath);
            Console.WriteLine("Finished parsing " + parseInput.RawFilePath);
        }
    }
}