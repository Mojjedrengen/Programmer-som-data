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

## (i)

```fsharp
run (fromFile "assignment/CEx/ex01.c") [3];;
```

```fsharp
run (fromFile "assignment/CEx/ex02.c") [10];;
```

```fsharp
run (fromFile "assignment/CEx/ex03.c") [];;
```
