# Compiling and using the Micro Virtual Machine

## Building the micro virtual machine

  The virtual machine is in directory `MicroVM`.

  The main source file is `microvm.c`.

  There are two supporting files `utils_unix.c` and `utils_win.c`
  depending on compiling for Unix or Windows based operating system.

  To compile go to directory `MicroVM` and do below depending on
  platform

###  Mac x86 and Mx (ARM) platform with MacOS

`gcc` is `clang` by default and both work.

```bash
clang -Wall microvm.c -o microvm
```

or

```bash
gcc -Wall microvm.c -o microvm
```

### On x86 platform with Linux

Both `gcc` and `clang` should work:

```bash
clang -Wall microvm.c -o microvm
```

or

```bash
gcc -Wall microvm.c -o microvm
```

### On x86 platform with Windows

We recommend using `clang` as compiler.

There are two dependencies that much be installed:

1. [MSVC toolchain](https://visualstudio.microsoft.com/visual-cpp-build-tools/)

Minimum choose "Visual Studio Build Tools".

2. `clang` for Windows x86, [llvm](https://releases.llvm.org)

  Tested with image: `LLVM-22.1.0-win64.exe`

  To compile:

```bash
clang --target=x86_64-pc-windows-msvc -Wall microvm.c -o microvm.exe
```

### On ARM platform with Widnows

Not yet tested on ARM with Windows.

## Simple test of micro-VM

The file `prog0` prints an infinite number of numbers on terminal
   starting with commandline input:

```bash
./microvm prog0 10
```

```bash
10 11 12 13 14 15 16 ...
```

The file `prog1` loops 20 million times

```bash
./microvm prog1          
```

```bash
Result value: 0 
Used 207 cpu milli-seconds
Number of GC: 0
```

## Test the micro virtual abstract machine

The compiled micro-Java n-queens program, `microJ/JavaEx/ex55.java`,
is included in the directory as file `ex55.out`.

Test the virtual abstract machine by running below examples

1. With no command line arguments

```bash
./microvm
```

```bash
micro virtual machine for 64 bit architecture, running on MacOS
Compiled with clang version 17 on May 14 2026 at 12:14:11
Usage: microvm [-trace] [-silent] <programfile> <arg1> ...

You need to provide program to run, an *.out file.
```

2. With `ex55.out` as command line argument

```bash
./microvm ex55.out 
```

```bash
Program expects 1 argument(s), but got 0.
Used 0 cpu milli-seconds
Number of GC: 0
```

The program ends asking for 1 command line argument, that is,
the size of the board on which to place queens.

3. With command line 5

```bash
./microvm ex55.out 5
```

```bash
1 3 5 2 4 
1 4 2 5 3 
2 4 1 3 5 
2 5 3 1 4 
3 1 4 2 5 
3 5 2 4 1 
4 1 3 5 2 
4 2 5 3 1 
5 2 4 1 3 
5 3 1 4 2 

10 

Result value: #52567225064 
Used 6 cpu milli-seconds
Number of GC: 0
```

There are 10 solutions.

The "Result value" is the top stack value when programs ends
execution, bytecode STOP.

## The micro virtual machine supports below command line arguments

1. `-silent`

Will execute bytecode program in silence mode.

```bash
./microvm -silent ex55.out 5
```

```bash
1 3 5 2 4 
1 4 2 5 3 
2 4 1 3 5 
2 5 3 1 4 
3 1 4 2 5 
3 5 2 4 1 
4 1 3 5 2 
4 2 5 3 1 
5 2 4 1 3 
5 3 1 4 2 

10 
```

2. `-trace`
   
Will enable stack tracing and output stack after each bytecode has
been executed.

```bash
./microvm -trace ex55.out 4
```

```bash
[ ]{0:PUSHLAB 774}
[ 774 ]{2:HEAPALLOC 1}
[ 774 #49949966336 ]{4:HEAPCOPY 1}
[ #49949966336 ]{6:LDARGS}
[ #49949966336 4 ]{8:VCALL 2 0}
[ 11 -999 #49949966336 4 ]{775:CSTI 0}
[ 11 -999 #49949966336 4 0 ]{777:GETBP}
[ 11 -999 #49949966336 4 0 2 ]{778:CSTI 2}
...
[ 11 -999 #34972106752 5 10 0 6 ... ]{2030:INCSP -1}
[ 11 -999 #34972106752 5 10 0 6 ... ]{2032:RET 10}
[ #34972119784 ]{11:STOP}

Result value: #34972119784 
Used 2058 cpu milli-seconds
Number of GC: 0
```

It is massive output and is practical usefull for small programs.

## The micro virtual machine does not come with garbage collection.

But it is prepared for a mark and sweep collector as covered in
Chapter 10 on Garbage Collection.

The relevant functions are

- `void collect(word s[], word sp)`: initiates collection.
- `void markPhase(word s[], word sp)`: marks live objects
- `void sweepPhase()`: sweeps the heap memory address space and
   build new freelist.
   
