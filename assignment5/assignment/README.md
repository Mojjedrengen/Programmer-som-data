# 7.1

I have created the Lex and parser files. And have test run the programs

# 7.2

To run the files first

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll \
    Absyn.fs CPar.fs CLex.fs Parse.fs \
    Interp.fs ParseAndRun.fs
```

Then execute:

```fsharp
open ParseAndRun;;
```

Down bellow are the commands you can send to the fs interactive.
The paths might be different depending on where you invoke fs interactive.

## (i)

This program does not use for loops but instead while loops.
The argument should be between 1-4. Though it is not enforced.
Calling the program with a higher or lower argument will cause undefined behaviour.

```fsharp
let ex01 = fromFile "CEx/ex01.c";;
```

```fsharp
run ex01 [3];;
```

## (ii)

This program prints each number squared number on a new line.
If given an input number that is higher that 20 it is clamped to 20.

```fsharp
let ex02 = fromFile "CEx/ex02.c";;
```

```fsharp
run ex02 [10];;
```

## (iii)

In the main file it uses the example from the book, trying to find the histogram
of the seven numbers 1 2 1 1 1 2 0. And then calling histogram(7, arr, 3, freq)
To run different examples edit the main function before calling histogram and then run the program.

```fsharp
let ex03 = fromFile "CEx/ex03.c";;
```

```fsharp
run ex03 [];;
```
