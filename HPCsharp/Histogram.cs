﻿// TODO: Add histogramming of arrays of other data types, with byte and ushort counts
// TODO: Add histogramming of 2-D and jagged arrays of variety of data types, with byte and ushort counts
// TODO: This may pay off big, and us being able to find the optimal number of bits for Radix Sort to minimize the number of passes or recursion levels.
//       Counting array should fit into L1-cache and possibly as large as fitting into L2-cache, since these are separate for each core in Intel CPUs, whereas L3-cache is
//       shared between all cores.
// TODO: Pull out the Histogram/Counting algorithm from LSD Radix Sort where multiple components are being counted in one pass, generalize it and parallelize it.
// TODO: Simplify example benchmarks by passing in the two sorting functions to be compared. This will reduce complexity a lot! Couldn't do it since Linq Sort are not
//       really functions, but are extension methods. How do you pass those in
// TODO: Consider for 64-bit Histogram and 9-bits/component implementing 9/10-bit components, where most are 9-bit, but the last one is 10-bit, to save one pass.
// TODO: Figure out which way is faster for byte component Histogram (one byte version): // ?? Which way is faster. Need to look at assembly language listing too
// TODO: Switch from mask-shift to shift-mask which makes the mask be the same for all bytes, which may make it faster than union. Try also casting to a byte instead of masking. Look at assembly to see
//       which is better. Time to see which is faster.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static int[] Histogram(this byte[] inArray)
        {
            const int numberOfBins = 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }

        public static int[] Histogram(this sbyte[] inArray)
        {
            const int numberOfBins = 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[(int)inArray[currIndex] + 128]++;

            return counts;
        }

        public static int[] Histogram(this ushort[] inArray)
        {
            const int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }

        public static int[] Histogram(this short[] inArray)
        {
            const int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[(int)inArray[currIndex] + 32768]++;

            return counts;
        }

        public static int[] Histogram(this uint[] inArray, int numberOfBits)
        {
            if (numberOfBits > 31)
                throw new ArgumentOutOfRangeException("numberOfBits must be <= 31");

            int numberOfBins = 1 << numberOfBits;
            int[] counts = new int[numberOfBins];
            uint mask = (uint)(numberOfBins - 1);

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[mask & inArray[currIndex]]++;

            return counts;
        }

        public static uint[][] HistogramByteComponents(uint[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            var union  = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                count[0][ value &       0xff       ]++;
                count[1][(value &     0xff00) >>  8]++;
                count[2][(value &   0xff0000) >> 16]++;
                count[3][(value & 0xff000000) >> 24]++;
            }
#endif
            return count;
        }

        public static uint[][][] HistogramByteComponentsAcrossWorkQuantas(uint[] inArray, uint workQuanta)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint numberOfQuantas = (inArray.Length % workQuanta) == 0 ? (uint)(inArray.Length / workQuanta) : (uint)(inArray.Length / workQuanta + 1);
            //Console.WriteLine("Histogram: inArray.Length = {0}, workQuanta = {1}, numberOfQuantas = {2}", inArray.Length, workQuanta, numberOfQuantas);

            uint[][][] count = new uint[numberOfQuantas][][];          // count for each parallel work item
            for (int i = 0; i < numberOfQuantas; i++)
            {
                count[i] = new uint[numberOfDigits][];
                for (int d = 0; d < numberOfDigits; d++)
                    count[i][d] = new uint[numberOfBins];
            }

            uint numberOfFullQuantas = (uint)(inArray.Length / workQuanta);
            int currIndex = 0;
            var union = new UInt32ByteUnion();
            uint q = 0;
            for (; q < numberOfFullQuantas; q++)
            {
                for (uint j = 0; j < workQuanta; j++)
                {
                    union.integer = inArray[currIndex++];
                    count[q][0][union.byte0]++;
                    count[q][1][union.byte1]++;
                    count[q][2][union.byte2]++;
                    count[q][3][union.byte3]++;
                }
            }
            // Last work quanta may be a partial one, whenever array length doesn't divide evenly by work quanta
            for (; currIndex < inArray.Length;)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[currIndex++];
                count[q][0][union.byte0]++;
                count[q][1][union.byte1]++;
                count[q][2][union.byte2]++;
                count[q][3][union.byte3]++;
            }

            //for (int d = 0; d < numberOfDigits; d++)
            //    for (q = 0; q < numberOfQuantas; q++)
            //    {
            //        Console.WriteLine("h: q = {0}   d = {1}", q, d);
            //        for (uint b = 0; b < numberOfBins; b++)
            //            Console.Write("{0} ", count[q][d][b]);
            //        Console.WriteLine();
            //    }

            return count;
        }

        public static uint[][] HistogramByteComponents<T>(T[] inArray, Int32 l, Int32 r, Func<T, UInt32> getKey)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(UInt32);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            var union = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = getKey(inArray[current]);
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = getKey(inArray[current]);
                count[0][(byte)value        ]++;
                count[1][(byte)(value >>  8)]++;
                count[2][(byte)(value >> 16)]++;
                count[3][(byte)(value >> 24)]++;
            }
#endif
            return count;
        }

        public static uint[][] HistogramByteComponents<T>(T[] inArray, Int32 l, Int32 r, Func<T, UInt64> getKey)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(UInt64);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            var union = new UInt64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = getKey(inArray[current]);
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
                count[4][union.byte4]++;
                count[5][union.byte5]++;
                count[6][union.byte6]++;
                count[7][union.byte7]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                count[0][ value &       0xff       ]++;
                count[1][(value &     0xff00) >>  8]++;
                count[2][(value &   0xff0000) >> 16]++;
                count[3][(value & 0xff000000) >> 24]++;
            }
#endif
            return count;
        }

        public static Tuple<uint[][], UInt32[]> HistogramByteComponentsAndKeyArray<T>(T[] inArray, Int32 l, Int32 r, Func<T, UInt32> getKey)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(UInt32);
            var inKeys = new UInt32[inArray.Length];
            var count  = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            var union = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = getKey(inArray[current]);
                inKeys[current] = union.integer;
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                count[0][ value &       0xff       ]++;
                count[1][(value &     0xff00) >>  8]++;
                count[2][(value &   0xff0000) >> 16]++;
                count[3][(value & 0xff000000) >> 24]++;
            }
#endif
            return new Tuple<uint[][], UInt32[]>(count, inKeys);
        }

        public static Tuple<uint[][], UInt64[]> HistogramByteComponentsAndKeyArray<T>(T[] inArray, Int32 l, Int32 r, Func<T, UInt64> getKey)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(UInt32);
            var inKeys = new UInt64[inArray.Length];
            var count  = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            var union = new UInt64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = getKey(inArray[current]);
                inKeys[current] = union.integer;
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
                count[4][union.byte4]++;
                count[5][union.byte5]++;
                count[6][union.byte6]++;
                count[7][union.byte7]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                count[0][ value &       0xff       ]++;
                count[1][(value &     0xff00) >>  8]++;
                count[2][(value &   0xff0000) >> 16]++;
                count[3][(value & 0xff000000) >> 24]++;
            }
#endif
            return new Tuple<uint[][], UInt64[]>(count, inKeys);
        }
        // The idea of 1-D array is that the individual digit counts (256 per digit in case of 8-bit digits) don't interfere with each other in L1 cache
        // whereas with jagged array they may depending on how each row happens to be allocated on the heap
        public static uint[] HistogramByteComponents1D(uint[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[] count = new uint[numberOfDigits * numberOfBins];

            var union = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)
            {
                union.integer = inArray[current];
                count[      union.byte0]++;
                count[256 + union.byte1]++;
                count[512 + union.byte2]++;
                count[768 + union.byte3]++;
            }
            return count;
        }

        public static uint[][] HistogramByteComponents(ulong[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];

            var union  = new UInt64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
                count[4][union.byte4]++;
                count[5][union.byte5]++;
                count[6][union.byte6]++;
                count[7][union.byte7]++;
            }
            return count;
        }

        public static uint[][] HistogramByteComponents(long[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];

            var union = new Int64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
                count[4][union.byte4]++;
                count[5][union.byte5]++;
                count[6][union.byte6]++;
                count[7][((ulong)inArray[current] >> 56) ^ 128]++;
            }
            return count;
        }

        public static Tuple<uint[][], int> HistogramByteComponentsAndStatistics(long[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
            int numElementsPreSorted = 0;

            var union = new Int64ByteUnion();

            int current = l;
            if (current <= r)
            {
                union.integer = inArray[current];
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
                count[4][union.byte4]++;
                count[5][union.byte5]++;
                count[6][union.byte6]++;
                count[7][((ulong)inArray[current] >> 56) ^ 128]++;
                current++;

                numElementsPreSorted++;     // initial single array element is considered sorted, since there is only a single element
            }

            for (; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count[0][union.byte0]++;
                count[1][union.byte1]++;
                count[2][union.byte2]++;
                count[3][union.byte3]++;
                count[4][union.byte4]++;
                count[5][union.byte5]++;
                count[6][union.byte6]++;
                count[7][((ulong)inArray[current] >> 56) ^ 128]++;

                // TODO: It should be possible to take the if/branch out, possibly by using SIMD/SSE or by separating the > from the ==, where ? can be done with a subtraction
                //       and equal with XOR followed by a subtraction from all 1's
                if (inArray[current] >= inArray[current - 1])
                    numElementsPreSorted++;
            }
            return new Tuple<uint[][], int>(count, numElementsPreSorted);
        }

        public static int[] HistogramOneByteComponent(ulong[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            //const ulong byteMask = numberOfBins - 1;
            int[] count = new int[numberOfBins];

            for (int current = l; current <= r; current++)
            {
                //count[(inArray[current] >> shiftRightAmount) & byteMask]++;
                count[(byte)(inArray[current] >> shiftRightAmount)]++;          // ?? Which way is faster. Need to look at assembly language listing too
            }

            return count;
        }

        public static int[] HistogramOneByteComponent(long[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != 56)
            {
                for (int current = l; current <= r; current++)
                {
                    count[(byte)(inArray[current] >> shiftRightAmount)]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    count[(byte)(inArray[current] >> shiftRightAmount) ^ 128]++;
                }
            }

            return count;
        }

        public static int[] HistogramOneByteComponent(int[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != 24)
            {
                for (int current = l; current <= r; current++)
                {
                    count[(byte)(inArray[current] >> shiftRightAmount)]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    count[(byte)(inArray[current] >> shiftRightAmount) ^ 128]++;
                }
            }

            return count;
        }
        public static int[] HistogramOneByteComponent(float[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];
            var f2i = default(FloatUInt32Union);

            if (shiftRightAmount != 24)
            {
                for (int current = l; current <= r; current++)
                {
                    uint digit;
                    f2i.floatValue = inArray[current];
                    if ((f2i.uinteger & 0x80000000U) == 0)
                        digit = f2i.uinteger >> shiftRightAmount;                   // positive values => don't flip anything
                    else
                        digit = (f2i.uinteger ^ 0xFFFFFFFFU) >> shiftRightAmount;   // negative values => flip the whole value

                    count[(byte)digit]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    uint digit;
                    f2i.floatValue = inArray[current];
                    if ((f2i.uinteger & 0x80000000U) == 0)
                        digit = (f2i.uinteger >> shiftRightAmount) ^ 128;               // positive values => flip just the sign bit
                    else
                        digit = (f2i.uinteger ^ 0xFFFFFFFFU) >> shiftRightAmount;       // negative values => flip the whole value including the sign bit

                    count[(byte)digit]++;
                }
            }

            return count;
        }

        public static int[] HistogramOneByteComponent(double[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];
            var d2i = default(DoubleUInt64Union);

            if (shiftRightAmount != 56)
            {
                for (int current = l; current <= r; current++)
                {
                    ulong digit;
                    d2i.doubleValue = inArray[current];
                    if ((d2i.ulongInteger & 0x8000000000000000) == 0)
                        digit = d2i.ulongInteger >> shiftRightAmount;                           // positive values => don't flip anything
                    else
                        digit = (d2i.ulongInteger ^ 0xFFFFFFFFFFFFFFFF) >> shiftRightAmount;    // negative values => flip the whole value

                    count[(byte)digit]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    ulong digit;
                    d2i.doubleValue = inArray[current];
                    if ((d2i.ulongInteger & 0x8000000000000000) == 0)
                        digit = (d2i.ulongInteger >> shiftRightAmount) ^ 128;                       // positive values => flip just the sign bit
                    else
                        digit = (d2i.ulongInteger ^ 0xFFFFFFFFFFFFFFFF) >> shiftRightAmount;        // negative values => flip the whole value including the sign bit

                    count[(byte)digit]++;
                }
            }

            return count;
        }

        public static int[] HistogramNbitComponents(long[] inArray, Int32 l, Int32 r, int shiftRightAmount, int numberOfBitPerComponent)
        {
            const int NumBitsInLong = sizeof(long) * 8;
            ulong numberOfBins      = 1UL << numberOfBitPerComponent;
            ulong halfOfNumBins     = numberOfBins / 2;
            ulong bitMask           = numberOfBins - 1;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != (NumBitsInLong - numberOfBitPerComponent))
            {
                for (int current = l; current <= r; current++)
                {
                    count[((ulong)inArray[current] >> shiftRightAmount) & bitMask]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    count[((ulong)inArray[current] >> shiftRightAmount) ^ halfOfNumBins]++;
                }
            }

            return count;
        }


        public static int[] HistogramByteComponentsUsingUnion(ulong[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];
            int whichByte = shiftRightAmount / 8;

            var union = new UInt64ByteUnion();

            switch (whichByte)
            {
                case 0:
                    for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    {
                        union.integer = inArray[current];
                        count[union.byte0]++;
                    }
                    break;
                case 1:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte1]++;
                    }
                    break;
                case 2:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte2]++;
                    }
                    break;
                case 3:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte3]++;
                    }
                    break;
                case 4:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte4]++;
                    }
                    break;
                case 5:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte5]++;
                    }
                    break;
                case 6:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte6]++;
                    }
                    break;
                case 7:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte7]++;
                    }
                    break;
            }
            return count;
        }

        public static int[] HistogramByteComponentsUsingUnion(long[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];
            int whichByte = shiftRightAmount / 8;

            var union = new Int64ByteUnion();

            switch (whichByte)
            {
                case 0:
                    for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    {
                        union.integer = inArray[current];
                        count[union.byte0]++;
                    }
                    break;
                case 1:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte1]++;
                    }
                    break;
                case 2:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte2]++;
                    }
                    break;
                case 3:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte3]++;
                    }
                    break;
                case 4:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte4]++;
                    }
                    break;
                case 5:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte5]++;
                    }
                    break;
                case 6:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte6]++;
                    }
                    break;
                case 7:
                    for (int current = l; current <= r; current++)
                    {
                        count[((ulong)inArray[current] >> shiftRightAmount) ^ 128]++;
                    }
                    break;
            }
            return count;
        }

        public static int[] Histogram9bitComponents(float[] inArray, Int32 l, Int32 r, uint bitMask, int shiftRightAmount)
        {
            const int numberOfBins = 512;
            //const uint bitMask = numberOfBins - 1;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != 23)
            {
                for (int current = l; current <= r; current++)
                    count[((uint)inArray[current] & bitMask) >> shiftRightAmount]++;
            }
            else
            {
                for (int current = l; current <= r; current++)
                    count[((uint)inArray[current] >> shiftRightAmount) ^ 256]++;
            }

            return count;
        }

        public static int[] Histogram12bitComponents(double[] inArray, Int32 l, Int32 r, ulong bitMask, int shiftRightAmount)
        {
            const int numberOfBins = 4096;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != 52)
            {
                for (int current = l; current <= r; current++)
                {
                    var currValue = BitConverter.ToUInt64(BitConverter.GetBytes(inArray[current]), 0);
                    count[(currValue & bitMask) >> shiftRightAmount]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    var currValue = BitConverter.ToUInt64(BitConverter.GetBytes(inArray[current]), 0);
                    count[(currValue >> shiftRightAmount) ^ 2048]++;
                }
            }

            return count;
        }

        public static uint[][] HistogramNBitsPerComponents(uint[] inArray, Int32 l, Int32 r, int bitsPerComponent)
        {
            int numberOfBins = 1 << bitsPerComponent;
            int numberOfDigits = (sizeof(uint) * 8 + bitsPerComponent - 1) / bitsPerComponent;  // round up
            //Console.WriteLine("HistogramNBitsPerComponents: NumberOfDigits = {0}", numberOfDigits);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];
            if (bitsPerComponent == 8)
            {
                var union = new UInt32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][union.byte3]++;
                }
            }
            else if (bitsPerComponent == 9)
            {
                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft[0][value  &      0x1ff       ]++;
                    countLeft[1][(value &    0x3fe00) >>  9]++;
                    countLeft[2][(value &  0x7fc0000) >> 18]++;
                    countLeft[3][(value & 0xf8000000) >> 27]++;
                }
            }
            else if (bitsPerComponent == 10)
            {
                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft[0][ value &      0x3ff       ]++;
                    countLeft[1][(value &    0xffc00) >> 10]++;
                    countLeft[2][(value & 0x3ff00000) >> 20]++;
                    countLeft[3][(value & 0xc0000000) >> 30]++;
                }
            }
            else if (bitsPerComponent == 11)
            {
                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft[0][ value &      0x7ff       ]++;
                    countLeft[1][(value &   0x3ff800) >> 11]++;
                    countLeft[2][(value & 0xffc00000) >> 22]++;
                }
            }
            else if (bitsPerComponent == 12)
            {
                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft[0][ value &      0xfff       ]++;
                    countLeft[1][(value &   0xfff000) >> 12]++;
                    countLeft[2][(value & 0xff000000) >> 24]++;
                }
            }
            else if (bitsPerComponent == 13)
            {
                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft[0][ value &     0x1fff       ]++;
                    countLeft[1][(value &  0x3ffe000) >> 13]++;
                    countLeft[2][(value & 0xfc000000) >> 26]++;
                }
            }
            else if (bitsPerComponent == 16)
            {
                var union = new UInt32UShortUnion();
                for (int current = l; current <= r; current++)
                {
                    union.integer = inArray[current];
                    countLeft[0][union.ushort0]++;
                    countLeft[1][union.ushort1]++;
                }
            }
            else
            {
                uint componentMask = (uint)numberOfBins - 1;
                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    for (int i = 0; i < numberOfDigits; i++)
                    {
                        countLeft[i][value & componentMask]++;
                        componentMask <<= bitsPerComponent;
                    }
                }
            }
            return countLeft;
        }

        public static uint[][] HistogramNBitsPerComponents(ulong[] inArray, Int32 l, Int32 r, int bitsPerComponent)
        {
            int numberOfBins = 1 << bitsPerComponent;
            int numberOfDigits = (sizeof(uint) * 8 + bitsPerComponent - 1) / bitsPerComponent;  // round up
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];
            if (bitsPerComponent == 8)
            {
                var union = new UInt64ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][union.byte3]++;
                    countLeft[4][union.byte4]++;
                    countLeft[5][union.byte5]++;
                    countLeft[6][union.byte6]++;
                    countLeft[7][union.byte7]++;
                }
            }
            else if (bitsPerComponent == 9)
            {
                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft[0][ value &              0x1ff       ]++;
                    countLeft[1][(value &            0x3fe00) >>  9]++;
                    countLeft[2][(value &          0x7fc0000) >> 18]++;
                    countLeft[3][(value &        0xff8000000) >> 27]++;
                    countLeft[4][(value &     0x1ff000000000) >> 36]++;
                    countLeft[5][(value &   0x3fe00000000000) >> 45]++;
                    countLeft[6][(value & 0x7fc0000000000000) >> 54]++;
                    countLeft[7][(value & 0x8000000000000000) >> 63]++;
                }
            }
            else if (bitsPerComponent == 10)    // useful for 64-bit
            {
                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft[0][ value &              0x3ff       ]++;
                    countLeft[1][(value &            0xffc00) >> 10]++;
                    countLeft[2][(value &         0x3ff00000) >> 20]++;
                    countLeft[3][(value &       0xffc0000000) >> 30]++;
                    countLeft[4][(value &    0x3ff0000000000) >> 40]++;
                    countLeft[5][(value &  0xffc000000000000) >> 50]++;
                    countLeft[6][(value & 0xf000000000000000) >> 60]++;
                }
            }
            else if (bitsPerComponent == 11)
            {
                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft[0][ value &              0x7ff       ]++;
                    countLeft[1][(value &           0x3ff800) >> 11]++;
                    countLeft[2][(value &        0x1ffc00000) >> 22]++;
                    countLeft[3][(value &      0xffe00000000) >> 33]++;
                    countLeft[4][(value &   0x7ff00000000000) >> 44]++;
                    countLeft[5][(value & 0xff80000000000000) >> 55]++;
                }
            }
            else if (bitsPerComponent == 12)
            {
                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft[0][ value &              0xfff       ]++;
                    countLeft[1][(value &           0xfff000) >> 12]++;
                    countLeft[2][(value &        0xfff000000) >> 24]++;
                    countLeft[3][(value &     0xfff000000000) >> 36]++;
                    countLeft[4][(value &  0xfff000000000000) >> 48]++;
                    countLeft[5][(value & 0xf000000000000000) >> 60]++;
                }
            }
            else if (bitsPerComponent == 13)
            {
                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft[0][ value &             0x1fff       ]++;
                    countLeft[1][(value &          0x3ffe000) >> 13]++;
                    countLeft[2][(value &       0x7ffc000000) >> 26]++;
                    countLeft[3][(value &    0xfff8000000000) >> 39]++;
                    countLeft[4][(value & 0xfff0000000000000) >> 52]++;
                }
            }
            else if (bitsPerComponent == 16)
            {
                var union = new UInt64UShortUnion();
                for (int current = l; current <= r; current++)
                {
                    union.integer = inArray[current];
                    countLeft[0][union.ushort0]++;
                    countLeft[1][union.ushort1]++;
                    countLeft[2][union.ushort0]++;
                    countLeft[3][union.ushort1]++;
                }
            }
            else
            {
                uint componentMask = (uint)numberOfBins - 1;
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    ulong value = inArray[current];
                    for (int i = 0; i < numberOfDigits; i++)
                    {
                        countLeft[i][value & componentMask]++;
                        componentMask <<= bitsPerComponent;
                    }
                }
            }
            return countLeft;
        }
    }
}
