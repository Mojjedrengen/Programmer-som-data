# Exercise 8.5

The first step of this exercise was to extend the abstract syntaxt with the following:
```fsharp
 Cond of expr * expr * expr
 ```
following the order of e1, e2, e3, where:
e1 ? e2 : e3

Next was to extend the lexer specification with the following tokens:
```fsharp
  | '?'             { CON1 }
  | ':'             { CON2 }
```
Disregard my amazing naming sense.

Then we had to extend the parser spec as well to include the newly added tokens, and to extend the ExprNotAccess type with the following:
```fsharp
  | Expr CON1 Expr CON2 Expr            { Cond($1, $3, $5)     }
```
Where we would match it to our newly created abstract syntax.

Then at last we had to extend the compile to be able to create instructions with Cond in mind.
```fsharp
| Cond(e1, e2, e3) ->
      let labelfalse = newLabel()
      let labeltrue = newLabel()
      cExpr e1 varEnv funEnv
      @ [IFZERO labelfalse]
      @ cExpr e2 varEnv funEnv
      @ [GOTO labeltrue]
      @ [Label labelfalse]
      @ cExpr e3 varEnv funEnv
      @ [Label labeltrue]
```
The trick here is to use the labels as our if and then jumps.
we first get the instructions of the condition e1 and if that is false then we jump over e2 straight to e3.
if it is true then we just walk on through to e2 and afterwards jump over e3.

# Exercise 8.6

This exercise was quite similar but a bit trickier than 8.5, first we extended the abstract syntax with a new statement:
```fsharp
 | Switch of expr * (int * stmt) list
```
where expr is the expression in Switch (expression) and the list is a list of the cases and their respective blocks.

We then, like 8.5, added the keyword "switch" in the lexer to parse to the token SWITCH amd declared it in the parser spec.

We then extended the StmtM type with the following:
```fsharp
| SWITCH LPAR Expr RPAR InStms        { Switch($3, $5)       }
```
where expr is the switch (expr) and InStms is a custom type created for the list of cases/statement blocks:
```fsharp
InStms:
    /* empty */                         { []       }
  | InStms1                             { $1       }
;

InStms1:
    CSTINT COMMA Block                           { [$1,$3] }
  | CSTINT COMMA Block COMMA InStms1             { ($1,$3) :: $5 }
;
```
A switch can either have no (empty) cases, or it can have one or more cases, where each case is an int followed by its statement block.
```fsharp
We then had to extend the compiler once more:

    | Switch (e1, ls) ->
      let case = cExpr e1 varEnv funEnv
      let labelend = newLabel()
      let rec helper (ls' : list<int * stmt>) =
        match ls' with
        | [] -> []
        | (n, blk) :: rest ->
          let label1 = newLabel()
          [DUP] @ [CSTI n] @ [EQ] @ [IFZERO label1] @ cStmt blk varEnv funEnv @ [GOTO labelend] @ [Label label1] @ helper rest

      let append = helper ls 
      case @ append @ [Label labelend] @ [INCSP -1]
```

Here we first got the instructions of the expression in switch (expr) as that was going to be our condition for entering a specific case (could be named better sorry).
we then created a single end label every single case block added at the end to skip any other cases afterwards as only one case would be matched.
With the help of a recursive helper function, we then created instructions for first duplicating the expr from earlier as to avoid any side effects of the expression, meaning we had to
preprend it to the first spot in the instructions list before going through every case.
we then appended the case integer, checked if it matched, and if it did then go on, otherwise skip the case.
An important addition to the instruction list is the [INCSP -1] instruction, where we removed the leftover result from the case list instruction as it was never directly used in any EQ instruction, meaning it was left over on the stack at the end at had to be removed.