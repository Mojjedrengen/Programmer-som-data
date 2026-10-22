# Compiling and Loading the List-C Evaluator and Parser

Chapter 10 presents heap-allocation and garbage collection and some
fundamental garbage collection algorithms, and also discusses the
(more complex) algorithms used in the Java and .NET virtual machines.
Garbage collection is not specific to abstract machines, but its
adoption in mainstream programming languages since 1995 is very much
due to those virtual machines.


## Building the List-C Command Line Compiler

The compiler has been tested on **.NET 8**, **.NET 9**, and **.NET 10**.

Choose the desired .NET version in the `listc.fsproj` file by changing:

```xml
<TargetFramework>net10.0</TargetFramework>
```

to one of:

- `net8.0`
- `net9.0`
- `net10.0`

Make `ListC` the current working directory.

To build the compiler, run:

```bash
dotnet build listc.fsproj
```

or simply:

```bash
dotnet build
```

For example:

```bash
dotnet build
```

```bash
Restore complete (0.1s)
listc net10.0 succeeded (0.1s) → bin/Debug/net10.0/listc.dll

Build succeeded in 0.4s
```

The build process automatically downloads and installs the
**FsLexYacc** tools (if necessary), then generates the following files:

- `CLex.fs`
- `CLex.fsi`
- `CPar.fs`
- `CPar.fsi`

and installs the runtime library:

- `FsLexYacc.Runtime.dll`

The compiled compiler executable is placed in:

```bash
bin/Debug/net10.0/listc.dll
```

The `FsLexYacc.Runtime.dll` file is located in the same directory.

## Compiling a List-C Program

To compile a List-C program, for example `ListcEx/ex30.lc`, run:

```bash
dotnet run ListcEx/ex30.lc
```

This compiles `ex30.lc` from the `ListcEx` directory and generates the
output file:

```bash
ListcEx/ex30.out
```

Example:

```bash
dotnet run ListcEx/ex30.lc
```

```bash
List-C compiler v 2.0.0.0 of 2026-06-16
Compiling ListcEx/ex30.lc to ListcEx/ex30.out
```

## The Micro Virtual Machine

The micro virtual machine is located in the `MicroVM` directory, where
the file `README.md` explains how to build and use it.

The result is an executable named:

- `microvm` (Unix/macOS)
- `microvm.exe` (Windows)

located in the `MicroVM` directory.

## A Complete Example

The following example demonstrates compiling and executing `ex30.lc`.

1. Change to the `ListC` directory.

2. Compile the program:

```bash
dotnet run ListcEx/ex30.lc
```

```bash
List-C compiler v 2.0.0.0 of 2026-06-16
Compiling ListcEx/ex30.lc to ListcEx/ex30.out
```

3. Run the compiled program using the micro virtual machine:

```bash
../MicroVM/microvm ListcEx/ex30.out 10
```

```bash
10 9 8 7 6 5 4 3 2 1
Result value: 0
Used 0 cpu milli-seconds
Number of GC: 0
```

## List-C Example Programs

The List-C compiler supports the existing micro-C example programs
(`exXX.c`) located in the `ListcEx` directory.

Additional test programs named `exXX.lc` demonstrate allocation of
cons cells on the heap. Some of these examples require a working
garbage collector to complete successfully.

For example, `ex34.lc` continuously allocates memory and eventually
runs out of memory unless garbage collection is enabled.

Example:

```bash
dotnet run ListcEx/ex32.lc
```

```bash
List-C compiler v 2.0.0.0 of 2026-06-16
Compiling ListcEx/ex32.lc to ListcEx/ex32.out
```

```bash
../MicroVM/microvm ListcEx/ex32.out 10
```

```bash
1 2 3 4 5 6 7 8 9 10
GC[M,BS]Heap: 66667 blocks (133333 words); of which 1 free (1 words, largest 1 words); 0 orphans
Out of memory
```

## List-C Test Suite

List-C includes an automated test suite located in:

```bash
ListcEx/test.fsx
```

The test suite contains both micro-C and List-C test programs. Program
arguments and expected output are specified in the `test.fsx` file.

Run the test suite from the `ListcEx` directory:

```bash
dotnet fsi test.fsx
```

```bash
Compiling file ex01.c

list-C out: [10 9 8 7 6 5 4 3 2 1
]
10 9 8 7 6 5 4 3 2 1

Compiling file ex02.c

list-C out: [0 0 3 0
...
list-C out: [42 9999 ]
42 9999

Programs that succeed.
ex01.c: OK
ex02.c: OK
ex03.c: OK
...
ex30.lc: OK
ex31.lc: OK
ex33.lc: OK
...
ex37.lc: OK
exGC01.lc: OK
```

The test script:

- compiles and runs all example programs,
- compares each execution with the expected output,
- summarizes the results at the end,
- works across multiple platforms.

> **Note:** The test suite is relatively slow because it launches
    external system processes to compile and execute each program.


