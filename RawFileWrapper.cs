using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoRawFileParser
{
    public class RawFileWrapper : IDisposable
    {
        public IRawDataPlus RawFile { get; set; }
        public int FirstScanNumber { get; set; }
        public int LastScanNumber { get; set; }
        public ParseInput ParseInput { get; set; }

        public RawFileWrapper(IRawDataPlus rawFile, int firstScanNumber, int lastScanNumber, ParseInput parseInput)
        {
            RawFile = rawFile;
            FirstScanNumber = firstScanNumber;
            LastScanNumber = lastScanNumber;
            ParseInput = parseInput;
        }

        public void Dispose()
        {
            if (RawFile != null && RawFile.IsOpen)
            {
                RawFile.Dispose();
            }
        }
    }
}