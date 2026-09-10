#### Touched files: Parse.fs (3.6); Absyn.fs, ExprLex.fsl, ExprPar.fsy (3.7)

### 3.5:
Build and ran the required commands as found in Expr/Readme.md

### 3.6: 
Added - 
let compString (s : string) : sinstr list  =
s |> fromString |> (fun a -> Expr.scomp a [])
To Parse.fs.

First string to expr by fromstring and then expr to sinstrlist

FromString handles expr to string, the scomp in Expr handles from expr to sinstr list

Works by taking string, piping it into fromstring and piping its result (a expr) into scomp found in Expr class.

### 3.7: 
If of expr expr expr into absyn. 

Then added keywords for if then else in ExprLex

and Added
| IF Expr THEN Expr ELSE Expr         { If(2, 4, 6)   }        
to ExprPar. 