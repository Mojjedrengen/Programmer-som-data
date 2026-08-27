# Introductory examples for F# and very simple abstract syntax

Chapter 1 introduces the approach taken and the plan followed in this
book. We show how to represent arithmetic expressions and other
program fragments as data structures in F# as well as in Java, and how
to compute with such program fragments.  We also introduce various
basic concepts of programming languages.

Folder `Intro` contains the files used in Chapter 1.

Appendix A introduces those parts of the F# programming language that
are used in this book. The textbook by Hansen and Rischel gives a
proper introduction to functional programming with and to many more
aspects of F# than we use here.  The F# programming language belongs
to the ML family, which includes Standard ML and OCaml, where F#
resembles the latter most.  All of these languages are strict, mostly
functional, and statically typed with parametric polymorphic type
inference.  Microsofts .NET platform provides a high-performance
implementation of F# on Linux, MacOS and Windows. Relevant file is
`Intro/Appendix.fs`.

## A. Crash Course

File `Appendix.fs` contains all examples from PLC Appendix A: F# Crash
Course.  There is no point in loading all of it into F# Interactive
(fsi) in one go.  Instead, start fsi in a Command Prompt and copy
example code from `Appendix.fs` to fsi.

```bash
dotnet fsi
<copy example code>
<copy example code>
...
#q;;
```

## B. File `Intro1.fs`

File `Intro1.fs` contains abstract syntax (type `expr`) for very simple
expressions without variables, and a corresponding `eval` function.

```bash
dotnet fsi Intro1.fs
open Intro1;;
<experiments>
<experiments>
...
#q;;
```

## C. File `Intro2.fs`

File `Intro2.fs` contains abstract syntax (type `expr`) for very simple
expressions with variables, and a corresponding eval function.

```bash
dotnet fsi Intro2.fs
open Intro2;;
<experiments>
<experiments>
...
#q;;
```

## D. File `SimpleExpr.java`

File `SimpleExpr.java` contains a Java version of abstract syntax
(abstract class `Expr` with subclasses `CstI`, `Var` and `Prim`) for very
simple expressions with variables, with an `eval` method.  This closely
corresponds to the `expr` type and `eval` function in F# file `Intro2.fs`.

To compile and execute it, do:

```bash
javac SimpleExpr.java 
java SimpleExpr 
```
