# Compiling and Loading the micro-C Evaluator and Parser

In an earlier chapter we presented a simple stack-based abstract
machine for the evaluation of expressions with variables and variable
bindings.  In chapter 8 we extend it to an abstract machine that can
execute programs compiled from our imperative programming language
micro-C.  We also write a compiler from micro-C to this abstract
machine.

Chapter 12 shows that thinking in continuations is beneficial also
when compiling micro-C to stack machine code. Generating stack machine
code back-wards may seem silly, but it enables the compiler to inspect
the code that will consume the result of the code being generated
right now.  This permits the compiler to perform many optimizations
(code improvement) easily.


## Generate and Compile the Lexer and Parser for the micro-C Language

Build the parser project:

```text
dotnet build parse.fsproj
```

This automatically downloads and installs the **FsLexYacc** tools (if
necessary), and generates:

- `CLex.fs`
- `CLex.fsi`
- `CPar.fs`
- `CPar.fsi`

and installs the runtime library:

- `FsLexYacc.Runtime.dll`

These generated files are used by the interpreter and compiler
described below.

### Load the lexer, parser, and interpreter in F# Interactive

```text
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll \
    Absyn.fs CPar.fs CLex.fs Parse.fs \
    Interp.fs ParseAndRun.fs
```

Then execute:

```fsharp
open ParseAndRun;;
fromFile "CEx/ex01.c";;
```

```fsharp
run (fromFile "CEx/ex01.c") [17];;
```

```fsharp
run (fromFile "CEx/ex05.c") [4];;
```

```fsharp
run (fromFile "CEx/ex11.c") [8];;
#q;;
```

## Load the Lexer, Parser, and Compiler in F# Interactive

Start F# Interactive with the compiler components:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll \
    Absyn.fs CPar.fs CLex.fs Parse.fs \
    Machine.fs Comp.fs ParseAndComp.fs
```

Then execute:

```fsharp
open ParseAndComp;;
compileToFile (fromFile "CEx/ex11.c") "CEx/ex11.out";;
```

```fsharp
compile "CEx/ex11";;
#q;;
```

### Run the Compiled Program Using the Java Stack Machine

Compile the Java implementation of the stack machine:

```bash
javac Machine.java
```

Execute the compiled micro-C program:

```bash
java Machine CEx/ex11.out 8
```

### Run the Compiled Program Using the C Stack Machine

#### Building the micro virtual machine

The main source file is `machine.c` with two utility files
`utils_unix.c` and `utils_win.c` depending on platform.

####  Mac x86 and Mx (ARM) platform with MacOS

`gcc` is `clang` by default and both work.

```bash
clang -Wall machine.c -o machine
```

or

```bash
gcc -Wall machine.c -o machine
```

### On x86 platform with Linux

Both `gcc` and `clang` should work:

```text
clang -Wall machine.c -o machine
```

or

```bash
gcc -Wall machine.c -o machine
```

#### On x86 platform with Windows

We recommend using `clang` as compiler. See [Platform
Dependencies](../../README.md) on how to install across platforms.

To compile the bytecode machine:

```bash
clang --target=x86_64-pc-windows-msvc -Wall machine.c -o machine.exe
```

## Simple test of micro-VM

The file `prog0` prints an infinite number of numbers on terminal
   starting with commandline input:

```bash
./machine prog0 10
```

```bash
10 11 12 13 14 15 16 ...
```

The file `prog1` loops 20 million times

```bash
./machine prog1          
```

```bash
Result value: 0
...
```

Execute the compiled target program `ex11.out`:

```bash
./machine CEx/ex11.out 8
```

## Load the Backwards (Continuation-Based) micro-C Compiler

Start F# Interactive with the continuation-based compiler:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll \
    Absyn.fs CPar.fs CLex.fs Parse.fs \
    Machine.fs Contcomp.fs ParseAndComp.fs
```

Then execute:

```fsharp
open ParseAndComp;;
compileToFile (fromFile "CEx/ex11.c") "CEx/ex11.out";;
```

```fsharp
compile "CEx/ex11";;
#q;;
```

Run the generated code using the Java stack machine:

```bash
javac Machine.java
```

```bash
java Machine CEx/ex11.out 8
```

## Build the Backwards micro-C Command-Line Compiler

Build the command-line compiler:

```bash
dotnet build microcc.fsproj
```

Compile a micro-C program:

```bash
dotnet run --project microcc.fsproj CEx/ex11.c 
```

Run the generated code using the Java stack machine:

```bash
javac Machine.java
```

```bash
java Machine CEx/ex11.out 8
```

## Build the Forwards micro-C Command-Line Compiler

The project file `microcc.fsproj` can also be used to compile the
forwards micro-C command-line compiler. Simply include `Comp.fs`
instead of `Contcomp.fs`:

```xml
...
  <ItemGroup>
    <Compile Include="Absyn.fs" />
    <Compile Include="CPar.fs" />
    <Compile Include="CLex.fs" />
    <Compile Include="Parse.fs" />
    <Compile Include="Machine.fs" />
    <Compile Include="Comp.fs" />
    <!-- <Compile Include="Contcomp.fs" /> -->
    <Compile Include="MicroCC.fs" />    
  </ItemGroup>
...
```

You can repeat the build and run steps shown in previous section.

## Micro-C Test Suite

Micro-C includes an automated test suite located in:

```bash
CEx/test.fsx
```

The test suite contains micro-C test programs. Program
arguments and expected output are specified in the `test.fsx` file.

Run the test suite from the `CEx` directory:

```bash
dotnet fsi test.fsx
```

```bash
Compiling file ex01.c

10 9 8 7 6 5 4 3 2 1 

10 9 8 7 6 5 4 3 2 1 

Used 0 cpu milli-seconds

10 9 8 7 6 5 4 3 2 1 

Used 0.003 seconds

Compiling file ex02.c
...
Programs that succeed.
ex01.c: OK
ex01.c: OK
ex01.c: OK
ex02.c: OK
ex02.c: OK
ex02.c: OK
...
ex28.c: OK
```

The test script:

- compiles and runs all example programs,
- compares each execution with the expected output,
- summarizes the results at the end,
- works across multiple platforms.
- tests with three virtual machines: `machine.c`, `Machine.java` and
  `Micro-VM/microvm.c`.

> **Note:** The test suite is relatively slow because it launches
    external system processes to compile and execute each program.

The test suite works for both the forward and backwards compiler. You
need to build the compiler before applying the test script.

