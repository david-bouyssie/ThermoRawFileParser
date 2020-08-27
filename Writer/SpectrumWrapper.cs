using System;
using ThermoRawFileParser.Writer.MzML;

namespace ThermoRawFileParser.Writer
{
    // See: https://social.msdn.microsoft.com/Forums/en-US/11fae50a-21bf-4c73-b03c-0fa5fd0ee7eb/converting-double-to-intptr-and-then-to-byte-array?forum=csharpgeneral
    public class SpectrumWrapper
    {
        public SpectrumType Spectrum { get; set; }
        public double[] Masses { get; set; }
        public double[] Intensities { get; set; }

        //private MemoryMappedFile mmf;
        //private MemoryMappedViewAccessor mmv;

        public SpectrumWrapper(SpectrumType spectrum, double[] masses, double[] intensities)
        {
            Spectrum = spectrum;
            Masses = masses;
            Intensities = intensities;
        }

        /*public void Dispose()
        {
            if (mmv != null)
                mmv.Dispose();

            if (mmf != null)
                mmf.Dispose();
        }*/

        public int getPeaksCount()
        {
            return Masses == null ? 0 : Masses.Length;
        }

        public double GetMzValue(int pos)
        {
            return Masses == null ? -1.0 : Masses[pos];
        }

        public double GetIntensityValue(int pos)
        {
            return Masses == null ? -1.0 : Intensities[pos];
        }

        //public void InitStore(string location)
        //{
        //    mmf = MemoryMappedFile.CreateNew(location, int.MaxValue);
        //    mmv = mmf.CreateViewAccessor(0, 32); // size
        //}

        /*public void StoreContent()
        {

            //writer.WriteArray<byte>(0, bytes, 0, bytes.Length);
            if (mmv == null) Console.WriteLine("mmv is null");
            else
            {
                mmv.Write(0, Masses[0]);
                mmv.Write(8, Masses[1]);

                Console.WriteLine(mmv.ReadDouble(0));
            }
        }*/

        public void CopyMzListToPointer(long ptrAddress)
        {
            if (Masses == null) return;

            unsafe
            {
                IntPtr ptrAddressAsInt = new IntPtr(ptrAddress);
                double* mzPtr = (double*) ptrAddressAsInt.ToPointer();

                int peaksCount = Masses.Length;

                int peakIdx = 0;
                
                while (peakIdx < peaksCount)
                {
                    //*mzPtr++ = Masses[peakIdx];
                    mzPtr[peakIdx] = Masses[peakIdx];
                    peakIdx++;
                }
            }
        }

        public void CopyIntensityListToPointer(long ptrAddress)
        {
            if (Intensities == null) return;

            unsafe
            {
                IntPtr ptrAddressAsInt = new IntPtr(ptrAddress);
                double* intensityPtr = (double*)ptrAddressAsInt.ToPointer();

                int peaksCount = Intensities.Length;

                int peakIdx = 0;

                while (peakIdx < peaksCount)
                {
                    intensityPtr[peakIdx] = Masses[peakIdx];
                    peakIdx++;
                }
            }
        }

        public void CopyDataToPointers(long mzPtrAddress, long intensityPtrAddress)
        {
            if (Masses == null || Intensities == null) return;

            unsafe
            {
                IntPtr mzPtrAddressAsInt = new IntPtr(mzPtrAddress);
                IntPtr intensityPtrAddressAsInt = new IntPtr(intensityPtrAddress);

                double* mzPtr = (double*)mzPtrAddressAsInt.ToPointer();
                double* intensityPtr = (double*)intensityPtrAddressAsInt.ToPointer();

                int peaksCount = Masses.Length;

                int peakIdx = 0;

                while (peakIdx < peaksCount)
                {
                    mzPtr[peakIdx] = Masses[peakIdx];
                    intensityPtr[peakIdx] = Intensities[peakIdx];
                    peakIdx++;
                }
            }
        }

    }
}