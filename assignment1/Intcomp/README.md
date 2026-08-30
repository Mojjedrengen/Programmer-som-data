# Very simple interpretation and translation to intermediate code

Chapter 2 introduces the distinction between interpreters and
compilers, and demonstrates some concepts of compilation, using the
simple expression language as an example.  Some concepts of
interpretation are illustrated also, using a stack machine as an
example.

Folder `Intcomp` contains the files used in Chapter 2.


## A. Opening the Postscript files depends on operating system.

On Linux and MacOS, when Ghostscript and display are available:

```bash
gs prog.ps
```

will compute and show the result in a separate window.

Otherwise, use Ghostscript (install it if necessary) to convert
the Postscript file to PDF:

```bash
gs -sDEVICE=pdfwrite -dNOPAUSE -dBATCH -sOutputFile=prog.pdf prog.ps
```

and then open the `prog.pdf` file with a PDF viewer.

Otherwise, on MacOS the free PDF reader Skim can convert Postscript to
PDF and open the file: `Skim > File > Open > select .ps` file.

On Windows, one can install Ghostscript and GSview, then open
the Postscript file with `GSview > File > Open > select .ps` file.

The approaches above work for the other Postscript files `prog.ps` and
`sierpinski.eps` also.


## B. To use the simple interpreters and compilers in Intcomp1.fs

```bash
dotnet fsi Intcomp1.fs
```

```fsharp
open Intcomp1;;
```

```fsharp
eval e1 [];;
run e1;;
```

```fsharp
closedin e1 [];;
closed1 e1;;
```

```fsharp
subst e6 [("z", CstI 17)];;
subst e7s0 [("z", CstI 100)];;
```

```fsharp
freevars e1;;
closed2 e1;;
freevars e7s0;;
closed2 e7s0;;
```

```fsharp
tcomp e1 [];;
teval (tcomp e1 []) [];;
eval e1 [];;
teval (tcomp e1 []) [] = (eval e1 []);;
```

```fsharp
reval [RCstI 10; RCstI 17; RDup; RMul; RAdd] [];;
rcomp (Prim("+", Prim("*", CstI 2, CstI 3), CstI 4));;
```

```fsharp
eval e0 [];;
rcomp e0;;
reval (rcomp e0) [];;
```

```fsharp
eval e1 [];;
scomp e1 [];;
seval (scomp e1 []) [];;

#q;;
```

## C. Compiling the simple byte code machine

```bash
javac Machine.java
```

To run the two build in instruction sequences `rpn1` and `rpn2` in the
`Main` method

```bash
java Machine 
34
2217
```