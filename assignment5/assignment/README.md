### Exercise 6.4

### Exercise 6.5

### Exercise 7.1

### Exercise 7.2

### Exercise 7.3

Added Keyword to CLex.fsl for 
```    | "for"     -> FOR ```

Then in CPar.fsy added FOR as token, in the end of ```%token CHAR ELSE IF INT NULL PRINT PRINTLN RETURN VOID WHILE FOR```

Next we need to make it understand how to handle for loops.
Looking at the exercise we get the following
```pwsh
for(e1;e2;e3)

Is equal to the block
{
    e1;
    while(e2){
    stmt;
    e3
    }
}

Where every block is equal to a {} encapsulation.
    IF LPAR Expr RPAR StmtM ELSE StmtU  { If($3, $5, $7)       }
  | IF LPAR Expr RPAR Stmt              { If($3, $5, Block []) }
  from the book:
    if (expr1) { if (expr2) stmt1 else stmt2 }
    
  Another thing is that a block is of
  | Block of stmtordec list          (* Block: grouping and scope   *)
  Where 
  stmtordec =
  | Dec of typ * string              (* Local variable declaration  *)
  | Stmt of stmt                     (* A statement                 *)

```
Looking at While's statement definition/parsing rule we get an idea on how to construct it.

```pwsh
| WHILE LPAR Expr RPAR StmtM          { While($3, $5)        }
    1   2     3    4    5               While of Expr*stmt
What we need is for(e1;e2;e3)
From CLex we find 
  | ';'             { SEMI }
  | '('             { LPAR }
  | ')'             { RPAR }

Combining that with the definition of a block we cant just directly map to the expr, since the block doesnt support that. 
Therefore we have to create and Expr of type stmt from each mapping instead.

From that we get the following 
| FOR LPAR Expr SEMI Expr SEMI Expr RPAR StmtM { Block([Expr($3;While($5,Block[$9;Expr($7)]))])}
   1    2    3    4    5   6     7    8    9 
And for StmtU
| FOR LPAR Expr SEMI Expr SEMI Expr RPAR StmtU { Block([Expr($3;While($5,Block[$9;Expr($7)]))])}
   1    2    3    4    5   6     7    8    9 
```

By defining the parsing of for loops from elements that are already supported within the interpreter.
We dont have to adjust anything inside the intepreter. Resulting in less comparisons in runtime which is good, critical if the book lies. 





