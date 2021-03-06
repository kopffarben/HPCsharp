If you like HPCsharp, give it a star or donate to help us keep more good stuff coming.\
Give us feedback and let us know where else could use additional performance.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)

# High Performance Computing in C# (HPCsharp)

High performance cross-platform C# generic algorithms. Community driven to raise C# performance. Familiar interfaces,
similar to standard C# algorithms and Linq. Free, open source, on nuget.org

*Algorithm*|*\**|*\*\**|*SSE*|*Multi-Core*|*Array*|*List*|*Details*
--- | --- | --- | --- | :---: | :---: | --- | :--
[Add](#Add) | 2 | 14 | :heavy_check_mark: |:heavy_check_mark: | :heavy_check_mark: | | Adds two arrays element-wise
[Binary Search](#Binary-Search) | 1 | 2 | | | :heavy_check_mark: | :heavy_check_mark: | Generic IComparer\<T\>
[Block Swap](#Block-Swap) | 4 | 5 | | | :heavy_check_mark: | | Generic
[Parallel Copy](#Parallel-Copy) | 1 | 11 | | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | List.ToArray() and Array.Copy() parallel generic
[Counting Sort](#Counting-Sort) | 3 | 14 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | byte, ushort, sbyte, short arrays. Ludicrous speed!
[Divide-and-Conquer](#Divide-and-Conquer) | 2 | 4 | | :heavy_check_mark: | :heavy_check_mark: | | Generic serial and parallel abstraction
Fill | 4 | 10 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Numeric arrays
Histogram | 14 | 35 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Byte, N-bit components of numeric arrays
[Insertion Sort](#Insertion-Sort) | 1 | 2 | | | :heavy_check_mark: | :heavy_check_mark: | Generic IComparer\<T\>
[Max, Min](#Min-and-Max) | 2 | 12 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | Generic IComparer\<T\>
[Mean Absolute Deviation](#Mean-Absolute-Deviation) | 3 | 6 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | float[] and double[]
[Merge](#Merge) | 2 | 18 | | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | Generic IComparer\<T\>
Multi-way Merge | 1 | | | | :heavy_check_mark: | |
[Merge Sort](#Merge-Sort) | 2 | 25 | | :heavy_check_mark: | :heavy_check_mark: | | Generic, Stable or not, whole or partial
Priority Queue | 2 | 15 | | | :heavy_check_mark: | | 
[Radix Sort (LSD)](#LSD-Radix-Sort) | 6 | 40 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Numeric arrays, user defined types, Stable
Radix Sort (MSD) | 4 | 24| :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Numeric arrays, user defined types, In-place
Sequence Equal | 2 | 19 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | 
[Standard Deviation](#Standard-Deviation) | 7 | 12 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Avoids arithmetic overflow exception
[Sum](#Better-Sum-in-Many-Ways) | 7 | 214 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Numeric arrays. [Better in many ways](https://duvanenko.tech.blog/2019/04/23/better-sum-in-c/)
Swap | 4 | 4 | | | :heavy_check_mark: | | Generic swap variations
[Zero Array Detect](#Zero-Array-Detect) | 3 | 13 | :heavy_check_mark: | | :heavy_check_mark: | | Detect if byte array is all zeroes 

\* Number of different algorithms\
\*\* Number of functions for this algorithm\
\*\*\* Coming soon

## Examples
Usage examples are provided in the HPCsharpExamples folder, which has a VisualStudio 2017 solution. Build and run it to see performance gains on your machine.
To get the maximum performance make sure to target x64 processor architecture for the Release build in VisualStudio, increasing performance by as much as 50%.

## Benchmarking
The first time you call a function that is implemented using SIMD/SSE instructions, C# just-in-time (JIT) compiler takes the time to compile and optimize
that function, which results in much slower performance. On the second use of the function and on subsequent uses, the SIMD/SSE function will run at
its full performance. Keep this behavior of the C# JIT compiler in mind as you use or benchmark HPCsharp functions.

## Better Sum in Many Ways
HPCsharp improves .Sum() of numeric arrays in the following ways:
- Adds support for the missing signed integer data types: sbyte and short
- Adds support for all unsigned integer data types: byte, ushort, uint, and ulong
- Simplified use: no arithmetic overflow exceptions to deal with, for all integer data types
- SIMD/SSE implementations for all integer and floating-point data types, to boost performance several times per processor core, as well as multi-core to use all the cores
- Adds support for BigInteger: single-core and multi-core
- New checked SIMD/SSE addition in C#, unsigned and signed, for much higher performance
- Extended precision ulong[] and long[] summation for a full precision to a Decimal and BigInteger result, using integer computation only: SIMD/SSE, single-core and multi-core
- Reduced error from ***O***(eN) downto ***O***(elgN) for float and double arrays by performing pair-wise summation
- Reduced error further down to ***O***(e) by implementing Kahan summation for float and double arrays, with slight
performance reduction, implemented in SIMD/SSE and multi-core
- GigaAdds/sec performance for all processor native data types

The table below compares performance (in GigaAdds/second) of Linq.AsParallel().Sum() and HPCsharp.SumSsePar() - both use multi-core (6 or 14 of them), with HPCsharp also using SIMD/SSE data parallel instructions on each core to gain additional performance:

*Library*|*sbyte*|*byte*|*short*|*ushort*|*int*|*uint*|*long*|*ulong*| Details
--- | --- | --- | --- | --- | --- | --- | --- | --- | ---
array.Sum() | n/a | n/a | n/a | n/a |1.5\*|n/a|1.7\*|n/a| using 6 cores
array.Sum(v => (long)v) |0.72|0.76|0.75|0.76|0.7|0.7| | | using 6 cores
array.Sum(v => (decimal)v) | | | | | |0.35|0.31|0.29| using 6 cores
Parallel.ForEach((long)v)  | | | | |7.4| | | | using 6 cores
HPC# (6-core) |33|31|16|16|8|8|3.7|4| using 2 cores
HPC# (14-core) |63|63|16|22|14|14|3.2|7.1|

\* arithmetic overflow exception is possible\
n/a not available

*Library*|*float*|*floatToDouble*|*double*|*decimal*|*BigInteger*
--- | --- | --- | --- | --- | ---
array.Sum() |1.8| |2.1|0.38|0.016\*\*
array.Sum(v => (double)v) | |0.66| | |
HPC# |8.3|7.9|4.2|0.5|0.075
HPC# pair-wise \* |8.3|7.9|4.2| |
HPC# Kahan |6.7|5.9|3.6| |

\*\* Linq doesn't implement BigInteger.Sum(), used .Aggregate() instead, which doesn't speed-up with .AsParallel()\
\* HPCsharp implements pair-wise floating-point parallel (multi-core) by default, since it uses divide-and-conquer algorithm for multi-core implementation.

All HPCsharp integer summations (unsigned and signed) including long[] and ulong[] arrays, do not throw overflow exceptions, while producing a perfectly accurate result. This simplifies usage, while providing high performance.

HPCsharp ulong[] array summation implements a full accuracy algorithm using integer only arithmetic to provide maximum performance. It detects and deals with arithmetic overflow internally, without using exceptions, using integer only computation. HPCsharp also uses SIMD/SSE data parallel instructions to get maximum performance out of each core, and uses multi-core to run even faster.

For more details, see several blogs on various aspects:
- [Better C# .Sum() in Many Ways](https://duvanenko.tech.blog/2019/04/23/better-sum-in-c/ "Better C# .Sum() in Many Ways")
- [Better C# .Sum() in More Ways](https://duvanenko.tech.blog/2019/09/06/better-c-sum-in-more-ways/ "Better C# .Sum() in More Ways")
- [Faster Checked Addition in C#](https://duvanenko.tech.blog/2019/07/20/checked-data-parallel-arithmetic-in-c/ "Faster Checked Addition in C#")
- [Faster Checked Addition in C# (Part 2)](https://duvanenko.tech.blog/2019/09/23/faster-checked-addition-in-c-part-2/ "Faster Checked Addition in C# (Part 2)")
- [Checked SIMD/SSE Addition in C#](https://duvanenko.tech.blog/2019/07/20/checked-data-parallel-arithmetic-in-c/ "Checked SIMD/SSE Addition in C#")
- [Video of Checked SIMD/SSE/multi-core Addition in C#](https://www.youtube.com/watch?v=hNqE1Ghwbv4 "Checked SIMD/SSE/multi-core Addition in C#")

## Standard Deviation
Accelerated and safer implementation of standard deviation for integer type arrays, float and double arrays. Accelerated by using multi-core and SSE data parallel instructions. Avoids arithmetic overflow exceptions for integer data types, using the same methods as HPCsharp's .Sum(). The following benchmarks ran on 6-core i7-9750H processor:

*Library*|*intToLong*|*longToDecimal*|*ulongToDecimal*|*float*|*floatToDouble*|*double*
--- | --- | --- | --- | --- | --- | ---
Linq |0.33|0.21|0.2|0.48|0.47|0.48
HPC# |3.3|1.9|2.0|4.0|3.8|2.0

The above benchmarks of Linq code were implemented in the following way:
```
intArray.Average(v => (long)v);     // intToLong
or
longArray.Average(v => (decimal)v); // longToDecimal
```
to ensure that no arithmetic overflow exception is possible, to make a fair comparison to HPCsharp implementations.

The following benchmarks ran on 14-core Xeon W-2175 processor:

*Library*|*intToLong*|*longToDecimal*|*ulongToDecimal*|*float*|*floatToDouble*|*double*
--- | --- | --- | --- | --- | --- | --- 
Linq |0.44|0.29|0.26|0.6|0.5|0.5
HPC# |4.9|2.2|3.6|6.5|5.9|3.7

https://duvanenko.tech.blog/2020/03/22/parallel-standard-deviation/

## Mean Absolute Deviation
Another useful measure of variability within a dataset is Mean Absolute Deviation. It is related to
standard deviation, using absolute value of the difference between the average value of the data set and-Conquer
each data value, eliminating warping of the data.

https://duvanenko.tech.blog/2020/03/22/how-standard-deviation-measures-warped-data/

## Divide-and-Conquer
Provides parallel and serial generic functions, which support multi-core and single-core divide-and-conquer algorithm.
Two versions are provided: single data type and two types.

For more details, see blog:
- [Divide-and-Conquer](https://duvanenko.tech.blog/2019/12/25/parallel-divide-and-conquer-abstraction-in-c/ "Divide-and-Conquer")

## Counting Sort

*Algorithm*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaBytes/sec*|*Data Type*
--- | --- | --- | --- | --- | --- | ---
Counting Sort|Random|27-56X|156-343X|39-70X|846|byte
Counting Sort|Presorted|26-56X|168-344X|38-66X|864|byte
Counting Sort|Constant|30-56X|165-321X|34-70X|847|byte

Counting Sort above is a linear time ***O***(N) algorithm, sorting an array of byte, sbyte, short or ushort data types.
In-place and not-in-place version have been implementated.
The above benchmark is on a single core! Multi-core sorts even faster, at GigaElements/second ludicrous speed.

## LSD Radix Sort

LSD Radix Sort is a linear time ***O***(N), stable sorting algorithm.
 
*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*|*Data Type*
--- | --- | --- | --- | --- | --- | --- | ---
Radix Sort|Array, List|Random|5X-8X|14X-35X|4X-9X|82|UInt32
Radix Sort|Array, List|Presorted|0.3X-0.6X|3X-5X|1X-3X|48|UInt32
Radix Sort|Array, List|Constant|1.3X-1.8X|5X-8X|2X-3X|50|UInt32

LSD Radix Sort runs on a single core, whereas Linq.AsParallel ran on all the cores.
Only slower when sorting presorted Array or List, but faster for random and constant distributions,
even faster than parallel Linq.OrderBy.AsParallel.

Radix Sort has been extended to sort user defined classes based on a UInt32 or UInt64 key within the class. Radix Sort is currently using only a single core.

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Radix Sort|Array|Random|1X-4X|3X-5X|1X-2X|User defined class
Radix Sort|List|Random|2X-4X|3X-5X|1X-2X|User defined class
Radix Sort|Array|Presorted|1.2X-1.7X|0.9X-2.5X|0.9X-1.4X|User defined class
Radix Sort|List|Presorted|1.0X-1.2X|1.7X-2.1X|0.7X-1.1X|User defined class
Radix Sort|Array|Constant|3X-4X|4X-5X|2X-3X|User defined class
Radix Sort|List|Constant|2X-4X|3X-4X|1.5X-2X|User defined class

Only slightly slower than Array.Sort and List.Sort for presorted distribution, but faster for all other distributions. Uses a single core and is stable.
Faster than Linq.OrderBy and Linq.OrderBy.AsParallel

## Merge Sort

Parallel Merge Sort uses multiple CPU cores to accelerate performance. On a quad-core laptop, performance is:

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|3X|12X|5X|25
Parallel Merge Sort|Array|Presorted|2X|22X|13X|110
Parallel Merge Sort|Array|Constant|2X|15X|9X|74

Parallel Merge Sort is not stable, just like Array.Sort. Faster than Array.Sort and List.Sort across all distributions.
Substantially faster than Linq.OrderBy and Linq.OrderBy.AsParallel

**_28-core (56-threads) AWS c5.18xlarge_**

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|5X-14X|19X-90X|7X-47X|
Parallel Merge Sort|Array|Presorted|1X-6X|5X-60X|16X-122X|
Parallel Merge Sort|Array|Constant|TBD|TBD|9X-44X|

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*
--- | --- | --- | --- | --- | --- | ---
Merge Sort (stable)|Array|Random|0.6X|2.5X|1X|5
Merge Sort (stable)|Array|Presorted|0.3X|3X|2X|17
Merge Sort (stable)|Array|Constant|0.5X|3X|2X|15

Merge Sort is ***O***(NlgN), never ***O***(N<sup>2</sup>), generic, stable, and runs on a single CPU core. Faster than Linq.OrderBy and Linq.OrderBy.AsParallel.

## Merge
***O***(N) linear-time generic merge algorithms for arrays and list containers. Merges two pre-sorted arrays or lists,
of any data type that defines IComparer<T>.
Two not-in-place algorithms: comparison at the heads, and divide-and-conquer.
Parallel Merge algorithm, using divide-and-conquer, merges two presorted collections using multiple cores.
Used by Parallel Merge Sort. See example solution for working code samples.

## Insertion Sort
Insertion Sort, which is ***O***(N<sup>2</sup>), and useful for fast in-place sorting of very small collections, due to its cache-friendliness.
Generic implemenation for Array and List containers. Used by Parallel Merge Sort and MSD Radix Sort for the base case.

## Add
Two algorithms for adding two arrays together:
- c\[] = a\[] + b\[]
- a\[] += b\[]

The second algorithm is about 70% faster than the first. So far, both algorithms are implemented for int[] only, but other data types
can be easily added. Both algorithms are implemented in scalar, data-parallel SIMD/SSE on a single core, and multi-core. Both run up to
the memory bandwidth limit.

## Binary Search
Generic implementation of the binary search algorithm, for Array and List containers. Used by the scalar and parallel divide-and-conquer
Merge algorithms.

## Min and Max
*Algorithm*|*Collection*|*vs Linq*|*Parallel vs Linq*
--- | --- | --- | ---
SequenceEqual|Array, List|4X faster|up to 11X faster
Min|Array|14-26X faster|4-7X faster
Max|Array|1.5X faster

.Min() is implemented using SIMD/SSE instructions to run at 4 GigaInts/sec on a single core, and over 5 GigaInts/sec on quad-core.

## Block Swap
Three scalar algorithms for in-place swapping two neighboring sub-regions of an array, which do not have to be of equal size:
- Reversal
- Gries and Mills
- Juggle Bentley

See an article for more details (http://www.drdobbs.com/parallel/benchmarking-block-swapping-algorithms/232900395)

Also, several generic version of two element swap.

## Zero Array Detect
Detects whether a byte array is zero in every byte. Runs at 17 GBytes/sec on a quad-core laptop, with two memory channels, using a single core.
Provides short-circuit, early exit when a non-zero value is detected while scanning the array. Provides scalar, SSE, scalar-unrolled, SSE-unrolled,
scalar unrolled multi-core, and SSE unrolled multi-core implementations. Unrolled refers to the loop being unrolled a few times to gain
additional performance.

On dual memory channel CPUs, SSE-unrolled is the fastest, using a single core, saturating system memory bandwidth.
For systems with more memory channels, SSE unrolled multi-core will most likely have the highest performance.

## Parallel Copy
Converting a List to an Array is a common operation:
```
var listSource = new List<int> { 5, 7, 16, 3 };

int[] arrayDestination1 = listSource.ToArray();	    // C# standard conversion
int[] arrayDestination2 = listSource.ToArrayPar();  // HPCsharp parallel/multi-core/faster conversion
```
The following table shows performance (in GigaInt/sec) for copy functions:

*Method*|*ToArray()*|*ToArray().AsParallel()*|*ToArrayPar()*|*Paged-in*|*Description*
--- | --- | --- | --- | --- | ---
Parallel ToArray|0.4|0.09|1.2|  No | Returns new Array

```
var listSource = new List<int> { 5, 7, 16, 3 };
int[] arrayDestination = new int[4];

listSource.CopyTo(arrayDestination);	 // C# standard List to Array copy
listSource.CopyToPar(arrayDestination);  // HPCsharp parallel/multi-core/faster copy
```
The following table shows performance (in GigaInt/sec) for copy functions:

*Method*|*CopyTo()*|*CopyTo().AsParallel()*|*CopyToPar()*|*Paged-in*|*Description*
--- | --- | --- | --- | --- | ---
Parallel CopyTo|0.4| |1.3|  No | Copies to new Array
Parallel CopyTo|2.4| |2.9| Yes | Copies to existing Array

HPCsharp provides parallel (multi-core) versions of List.ToArray() and List.CopyTo() functions,
with exactly the same interfaces. Parallel Array.ToArray() and Array.CopyTo() are also available.
These parallel functions are 3 times faster when the destination is new array - i.e. allocated but never touched - a common use case
shown in the first source code case above.
When a destination array has been used before and has been paged into system memory, these parallel functions are 10-20% faster.
These parallel copy functions provide a generic interface, handling any data type.

For more details, seee blog https://duvanenko.tech.blog/2019/08/19/faster-copying-in-c/

## Naming Conventions
HPCsharp follows a few simple naming conventions:
- SSE functions append "Sse" to the function name
- multi-core functions append "Par" to the function name
- if the function name clashes with C# Linq name, then "Hpc" is appended to the function name

## Blogs and Videos
For details on the motivation see blog:
https://duvanenko.tech.blog/2018/03/03/high-performance-c/

For more performance discussion see blog:
https://duvanenko.tech.blog/2018/05/23/faster-sorting-in-c/

HPCsharp presentation at the Indianapolis .NET Consortium, March 2019 on https://youtu.be/IRNW4VGevvQ

HPCsharp lighning talk at the Indianapolis .NET Consortium, October 2019 on - https://www.youtube.com/watch?v=hNqE1Ghwbv4


## Website for Feature Votes
Visit us at https://foostate.com/ and let us know what other high performance algorithms are important to you, and you'd like to see in this NuGet package.

## Encouragement
If you like it, then help us keep more good stuff like this coming. Let us know other algorithms that could use acceleration.


[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)
