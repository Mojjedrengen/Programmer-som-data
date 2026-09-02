TODO: Added some text

Exercises Solved: 1.1, 1.2, 1.4, 2.1, 2.2, 2.3

Touched files:

- Intro/Intro2.fs (1.1, 1.2)
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

- 2.1

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

- Intcomp\Machine.cs (1.4)
