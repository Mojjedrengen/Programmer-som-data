# Compiling and loading the expression evaluator and parser

Chapter 3 shows how to get from the usual textual representation of
programs to the abstract syntax, or tree-based, representation used in
programs that manipulate programs, such as interpreters and compilers.
We use regular expressions to describe local structure, that is, small
things such as names, constants, and operators.  We use context free
grammars to describe global structure, that is, expressions and
statements, the proper nesting of parentheses within parentheses, and
(in Java) of methods within classes, etc.


## A. Generate and compile the lexer and parser for the expression language

```bash
dotnet build parse.fsproj
```

This will automatically download and install the `fslex` and `fsyacc`
tools, if necessary, and use them to generate files `ExprLex.fs` and
`ExprLex.fsi` for the lexer and `ExprPar.fs` and `ExprPar.fsi` for the
parser, and also install the `FsLexYacc.Runtime.dll` file.  These
files are used below.

Load the generated lexer and parser and exercise them in F#
interactive:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll \
       Absyn.fs ExprPar.fs ExprLex.fs Parse.fs
```

```bash
open Parse;;
fromString "2 + 3 * 4";;
```

```bash
#q;;
```

## B. Combine lexer, parser, interpreter and compiler

Loading the lexer and parser, the interpreter eval, the compiler scomp
and the simple stack machine seval, and experimenting with them:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll Absyn.fs ExprPar.fs ExprLex.fs Parse.fs Expr.fs
```

```fsharp
open Parse;;
open Expr;;
run (fromString "2 + 3 * 4");;
```

```fsharp
eval (fromString "2 + x * 4") [("x", 3)];;
```

```fsharp
eval (fromString "let x = 1+2 in 2 + x * 4 end") [];;
```

```fsharp
let code1 = scomp (fromString "2 + 3 * 4") [];;
seval code1 [];;
```

```fsharp
let code2 = scomp (fromString "2 + x * 4") [Bound "x"];;
seval code2 [3];;
```

```fsharp
let code3 = scomp (fromString "let x = 1+2 in 2 + x * 4 end") [];;
seval code3 [];;
```

```fsharp
#q;;
```
