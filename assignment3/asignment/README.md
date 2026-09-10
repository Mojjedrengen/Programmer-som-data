#### Touched files: Parse.fs (3.6); Absyn.fs, ExprLex.fsl, ExprPar.fsy (3.7)

### 3.5:

Build and ran the required commands as found in Expr/Readme.md

### 3.6:

Added -
let compString (s : string) : sinstr list =
s |> fromString |> (fun a -> Expr.scomp a [])
To Parse.fs.

First string to expr by fromstring and then expr to sinstrlist

FromString handles expr to string, the scomp in Expr handles from expr to sinstr list

Works by taking string, piping it into fromstring and piping its result (a expr) into scomp found in Expr class.

### 3.7:

If of expr expr expr into absyn.

Then added keywords for if then else in ExprLex

and Added
| IF Expr THEN Expr ELSE Expr { If(2, 4, 6) }  
to ExprPar.

### 4.1

The files FunLex.fs, FunLex.fsi, FunPar.fs & FunPar.fsi has been generated

### 4.2

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

The examples are written below.
To use the examples just run them in F# interactive

#### 1

This example computes the sum of \[1..1000\]

```fsharp
let sum1000 = fromString "let sum n = if n = 0 then n else n + sum(n-1) in sum 1000 end";;
run sum1000;;
```

#### 2

This example computes the 3 raised to the power of 8, i.e. 3^8

```fsharp
let power8 = fromString "let power8 x = let aux n = if n = 0 then 1 else x * aux (n - 1) in aux 8 end in power8 3 end";;
run power8;;
```

#### 3

This one computes 3^0 + 3^1 + ... + 3^10 + 3^11.
It uses recursive functions and is based on the one above

```fsharp
let e3 = fromString "
let x = 3 in
    let ms = 11 in
        let pow ns =
            let aux n =
                if n = 0 then
                    1
                else
                    x * aux (n - 1)
                in
                    aux ns
                end
            in
        let sum m =
            if m < 0 then
                0
            else
                pow m + sum (m - 1)
            in
                sum ms
            end
        end
    end
end
";;

run e3;;
```

#### 4

This computes 1^8 + 2^8 + ... + 10^8.
This is almost the same as \#3 with a few lines changed

```fsharp
let e4 = fromString "
let n = 8 in
    let ms = 10 in
        let pow x =
            let aux n =
                if n = 0 then
                    1
                else
                    x * aux (n - 1)
                in
                    aux n
                end
            in
        let sum m =
            if m < 0 then
                0
            else
                pow m + sum (m - 1)
            in
                sum ms
            end
        end
    end
end
";;

run e4;;
```
