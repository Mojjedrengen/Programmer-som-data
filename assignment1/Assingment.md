TODO: Added some text

Exercises Solved: 1.1, 1.2, 1.4, 2.1, 2.2, 2.3

Touched files:

- Intro/Intro2.fs (1.1, 1.2)
- Intcomp\Intcomp1.fs (2.1, 2.2, 2.3)

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
