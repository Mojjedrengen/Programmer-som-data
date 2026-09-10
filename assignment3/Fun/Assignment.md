Markdown file for the assignment 4.2
TODO: Move the file contents to the generic file

## 4.1

The files FunLex.fs, FunLex.fsi, FunPar.fs & FunPar.fsi has been generated

## 4.2

For setup send this to the F# interactive

```fsharp
#r @"bin/Debug/net10.0/FsLexYacc.Runtime.dll"
#load "Absyn.fs"
#load "FunPar.fs"
#load "FunLex.fs"
#load "Parse.fs"
#load "Fun.fs"
#load "ParseAndRun.fs"

open ParseAndRun
```

```fsharp
let sum1000 = fromString "let sum n = if n = 0 then n else n + sum(n-1) in sum 1000 end";;
run sum1000;;
```

```fsharp
let power8 = fromString "let power8 x = let aux n = if n = 0 then 1 else x * aux (n - 1) in aux 8 end in power8 3 end";;
run power8;;
```

```fsharp
let e1 = fromString "let sum m = if m = 11 then n else let powaux n = let pow x = if n = 0 then 1 else x * pow (n - 1) in pow 8 end in sum (m + 1) + powaux n end in 0 end";;

```
