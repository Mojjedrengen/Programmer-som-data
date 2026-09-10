# Compiling and loading the micro-ML evaluator and parser

Chapter 4 presents the functional language micro-ML, a small subset
of ML or F#.  A functional programming language is one in which the
evaluation of expressions and function calls is the primary means of
computation. A pure functional language is one in which expressions
cannot have side effects, such as changing the value of variables, or
printing to the console. The micro-ML language is first-order, which
means that functions cannot be used as values.  The next chapter
presents a higher-order functional language, in which functions can be
used as values as in ML and F#.

The items A, B, and C below concern the first-order micro-ML language.

Chapter 5 extends micro-ML to a higher-order functional language in
which a function may be used as a value, just like an integer or a
boolean.  That is, the value of a variable may be a function, and a
function may take a function as argument and may return a function as
a result.

Items D and E concern evaluation of the higher-order micro-ML
language.  

Chapter 6 discusses polymorphic types and type inference in F# and
other ML-family languages, as well parametric polymorphism in Java and
C#, often called generic types and methods.  We present the type
system both through declarative type rules and through a concrete
implementation of polymorphic type inference.

Items F and G concern type inference of the higher-order micro-ML
language.  Item H concerns the generic linked list example.


## A. Loading the micro-ML evaluator, with abstract syntax only

Load the evaluator into F# interactive:

```bash
dotnet fsi Absyn.fs Fun.fs
```

Run example programs: 

```fsharp
open Absyn;;
open Fun;;
let res = run (Prim("+", CstI 5, CstI 7));;
```

```fsharp
#q;;
```

## B. Build and compile the lexer and parser

The project ```parse.fsproj``` is used to build the lexer and parser
specifications:

```bash
dotnet build parse.fsproj
```

This will automatically download and install the `fslex` and `fsyacc`
tools, if necessary, and use them to generate files `FunLex.fs` and
`FunLex.fsi` for the lexer and `FunPar.fs` and `FunPar.fsi` for the
parser, and also install the `FsLexYacc.Runtime.dll` file.  These
files are used below.

Load the generated lexer and parser and exercise them in F#
interactive:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll Absyn.fs FunPar.fs FunLex.fs Parse.fs   
```

```fsharp
open Parse;;	 
let e1 = fromString "5+7";;
```

```fsharp
let e2 = fromString "let y = 7 in y + 2 end";;
```

```fsharp
let e3 = fromString "let f x = x + 7 in f 2 end";;
```

```fsharp
#q;;
```

## C. Build the lexer, parser and first-order evaluator

Generating and compiling the lexer, parser and first-order evaluator,
and loading them together:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll Absyn.fs FunPar.fs FunLex.fs Parse.fs Fun.fs ParseAndRun.fs
```

```fsharp
open ParseAndRun;;
run (fromString "5+7");;
```

```fsharp
run (fromString "let y = 7 in y + 2 end");;
```

```fsharp
run (fromString "let f x = x + 7 in f 2 end");;
```

```fsharp
#q;;
```

## D. Build the lexer, parser and higher-order evaluator

Loading the evaluator for a higher-order functional language (same
abstract syntax as the first-order language):

```bash
dotnet fsi Absyn.fs HigherFun.fs
```

```fsharp
open HigherFun;;
eval ex1 [];;
```

```fsharp
open Absyn;;
run (Letfun ("twice", "f",
             Letfun ("g", "x", Call (Var "f", Call (Var "f", Var "x")), Var "g"),
                     Letfun ("mul3", "z", Prim ("*", Var "z", CstI 3),
                             Call (Call (Var "twice",Var "mul3"),CstI 2))));;
```

```fsharp
#q;;
```

The above abstract syntax term corresponds to the concrete syntax term
shown in point E below.


## E. Using the lexer, parser and higher-order evaluator together:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll Absyn.fs FunPar.fs FunLex.fs Parse.fs HigherFun.fs ParseAndRunHigher.fs
```

```fsharp
open ParseAndRunHigher;;
run (fromString @"let twice f = let g x = f(f(x)) in g end 
                  in let mul3 z = z*3 in twice mul3 2 end end");;
```

```fsharp
#q;;
```

## F. Using the lexer, parser and polymorphic type inference together:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll Absyn.fs FunPar.fs FunLex.fs Parse.fs TypeInference.fs ParseAndType.fs
```

```fsharp
open ParseAndType;;
inferType (fromString "let f x = 1 in f 7 + f false end");;
```

```fsharp
#q;;
```

## G. Type variable explosion

To see the number of type variables explode, load
```slowTypeInference.fsx``` to let F#'s type inference work on it:

```bash
dotnet fsi
```

```fsharp
#load "slowTypeInference.fsx";;
```

```fsharp
#q;;
```

Same example with our own type inference:

```bash
dotnet fsi -r bin/Debug/net10.0/FsLexYacc.Runtime.dll Absyn.fs FunPar.fs FunLex.fs Parse.fs TypeInference.fs ParseAndType.fs
```

```fsharp
open ParseAndType;;
slowTypeInferenceExample();;
```

```fsharp
#q;;
```

## H. Compiling and executing the LinkedList example

```bash
javac LinkedList.java
```

```bash
java TestLinkedList
```
