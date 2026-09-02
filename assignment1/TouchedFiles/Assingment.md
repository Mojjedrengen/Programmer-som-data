TODO: Added some text

Exercises Solved: 1.1, 1.2, 1.4, 2.1, 2.2, 2.3

Touched files:

- Intro/Intro2.fs (1.1, 1.2)
- Intcomp\Machine.cs (1.4)
- Intcomp\Intcomp1.fs (2.1, 2.2, 2.3)


- 1.1

Added an extra pattern maching inside to mach on the operator inside of prim 
Also added the if statement evaluation
follwing both 1.1 (i) and (iii)

Added some test cases
Following 1.1 (ii)

- 1.2

Made the aexpr which is like the expr that was give.
But the prim have ben pulled out to there own enum variables. 
These are add, mul, sub
following 1.2 (i)

Made test expression following 1.2 (ii)

Made the fmt function which converts the aexpr to a string
following 1.2 (iii)

Made the simplify function following 1.2 (iv) which simplify the given aexpr

Made the symbolic diffrent function, symdiff. 
This takes a var, as a string, and then diffrentiates the function based on it. 
Following 1.2 (v)

- 1.4

Implementation made in C# fulfilling the criteria of assignment 1.4, by being a object oriented implementation of assignment 1.1 and 1.2.
Abstract class Expr implemented by CSTI and VAR
Consisting of functions ToString(),Eval & Simplify
Abstract class BinOp implementing Expr and is implemented by operations: Add, Mul & Sub
Furthermore expanding the classes to have constructors with two expressions.
All ToString() override, eval contain a evaluation off the two expressions, with respect to the cohering operation.
Simplify made with switch cases as a match, works good everywhere else than in the comparison found under subtraction.
Cant compare the two classes in a good way, so had to ToString() them first. Could possibly have used records instead of classes or Icomparable.

- 2.1

Our "Let" is now a expression with the definition
Let of (string * expr) list * expr

Containing a tuple consisting of (list of tuples (bindings list) ,  and the body expression)

Allowing for us to run through a list of expressions before evaluating the final/body expression
Before: Let consisted of a single binding, needing multiple bindings required nesting multiple lets
After: List of multiple bindings

Implemented by, tail recursive function with accumulator
Started by passing the bindings list and the current environment to the helper function
Looping through all bindings and passing the binding onto the environment immediately (accumulator)
Finally returning the accumulator consisting of the entire environment, that then gets run through eval a last time (executing body expression)

- 2.2

Revised freevars to work for the newly updated expr language
Due to the expr language Let constructor change, freevars had to be
updated before being able to be compiled.

this meant modifying the function to handle multiple sequential let bindings.

Via a helper recursive function, the let match recursively unions
all free variables taking into account all the bound variables before the current erhs

the names bound by the let are excluded from the ebody free variables as well.

- 2.3

Updated tcomp to work with the updated expr language

uses a recursive accumulator helper function to stack
all variables in the list of bindings into the environment
to compile each right hand side against the environment built so far,
and at the end to run the ebody against the accumulated environment.

instead of a list tcomp creates nested TLet's

