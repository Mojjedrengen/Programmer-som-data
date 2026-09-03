module Intro.Intcomp.Intcomp1

(* Programming language concepts, 2026-02-20 *)

(* Evaluation, checking, and compilation of object language expressions *)
(* Stack machines for expression evaluation                             *) 

(* Object language expressions with variable bindings and nested scope *)

type expr = 
  | CstI of int
  | Var of string
  | Let of (string * expr) list * expr
  | Prim of string * expr * expr;;

(* Some closed expressions: *)

// let e0 = Prim("+", CstI 17, Prim("+", CstI 5, CstI 7));;
// let e1 = Let("z", CstI 17, Prim("+", Var "z", Var "z"));;
//
// let e2 = Let("z", CstI 17, 
//              Prim("+", Let("z", CstI 22, Prim("*", CstI 100, Var "z")),
//                        Var "z"));;
//
// let e3 = Let("z", Prim("-", CstI 5, CstI 4), 
//              Prim("*", CstI 100, Var "z"));;
//
// let e4 = Prim("+", Prim("+", CstI 20, Let("z", CstI 17, 
//                                           Prim("+", Var "z", CstI 2))),
//                    CstI 30);;
//
// let e5 = Prim("*", CstI 2, Let("x", CstI 3, Prim("+", Var "x", CstI 4)));;
//
// let e6 = Let("z", Var "x", Prim("+", Var "z", Var "x"))
// let e7 = Let("z", CstI 3, Let("y", Prim("+", Var "z", CstI 1), Prim("+", Var "z", Var "y")))
// let e8 = Let("z", Let("x", CstI 4, Prim("+", Var "x", CstI 5)), Prim("*", Var "z", CstI 2))n
// let e9 = Let("z", CstI 3, Let("y", Prim("+", Var "z", CstI 1), Prim("+", Var "x", Var "y")))
// let e10 = Let("z", Prim("+", Let("x", CstI 4, Prim("+", Var "x", CstI 5)), Var "x"), Prim("*", Var "z", CstI 2))

(* ---------------------------------------------------------------------- *)

(* Evaluation of expressions with variables and bindings *)

let rec lookup env x =
    match env with 
    | []        -> failwith (x + " not found")
    | (y, v)::r -> if x=y then v else lookup r x;;


(*
Exercise 2.1 
Revise the eval interpreter from Intcomp1.fs to work for the expr language
extended with multiple sequential let-bindings.

Let is now a expression with the definition
Let of (string * expr) list * expr

Containing a tuple consisting of (list of tuples (bindings list) ,  and the body expression)

Allowing for us to run through a list of expressions before evaluating the final/body expression
Before: Let consisted of a single binding, needing multiple bindings required nesting multiple lets
After: List of multiple bindings 

Implemented by, tail recursive function with accumulator
Started by passing the bindings list and the current environment to the helper function
Looping through all bindings and passing the binding onto the environment immediately (accumulator)
Finally returning the accumulator consisting of the entire environment, that then gets run through eval a last time (executing body expression)
*)
let rec eval e (env : (string * int) list) : int =
    match e with
    | CstI i            -> i
    | Var x             -> lookup env x 
    | Let (varLst, ebody) ->
        let rec letHelper lst acc =
            match lst with
            | [] -> acc
            | (var , exs) :: xs -> 
              let xval = eval exs acc
              letHelper xs ((var, xval)::acc)
        eval ebody (letHelper varLst env)
    | Prim("+", e1, e2) -> eval e1 env + eval e2 env
    | Prim("*", e1, e2) -> eval e1 env * eval e2 env
    | Prim("-", e1, e2) -> eval e1 env - eval e2 env
    | Prim _            -> failwith "unknown primitive";;

let run e = eval e [];;
let res = List.map run [e1;e2;e3;e4;e5;e7]  (* e6 has free variables *)


(* ---------------------------------------------------------------------- *)

(* Closedness *)

// let mem x vs = List.exists (fun y -> x=y) vs;;

let rec mem x vs = 
    match vs with
    | []      -> false
    | v :: vr -> x=v || mem x vr;;

(* Checking whether an expression is closed.  The vs is 
   a list of the bound variables.  *)

let rec closedin (e : expr) (vs : string list) : bool =
    match e with
    | CstI i -> true
    | Var x  -> List.exists (fun y -> x=y) vs
    | Let(x, erhs, ebody) -> 
      let vs1 = x :: vs 
      closedin erhs vs && closedin ebody vs1
    | Prim(ope, e1, e2) -> closedin e1 vs && closedin e2 vs;;

(* An expression is closed if it is closed in the empty environment *)

let closed1 e = closedin e [];;
let _ = List.map closed1 [e1;e2;e3;e4;e5;e6;e7;e8;e9;e10]

(* ---------------------------------------------------------------------- *)

(* Substitution of expressions for variables *)

(* This version of lookup returns a Var(x) expression if there is no
   pair (x,e) in the list env --- instead of failing with exception: *)

let rec lookOrSelf env x =
    match env with 
    | []        -> Var x
    | (y, e)::r -> if x=y then e else lookOrSelf r x;;

(* Remove (x, _) from env: *)

let rec remove env x =
    match env with 
    | []        -> []
    | (y, e)::r -> if x=y then r else (y, e) :: remove r x;;

(* Naive substitution, may capture free variables: *)

let rec nsubst (e : expr) (env : (string * expr) list) : expr =
    match e with
    | CstI i -> e
    | Var x  -> lookOrSelf env x
    | Let(x, erhs, ebody) ->
      let newenv = remove env x
      Let(x, nsubst erhs env, nsubst ebody newenv)
    | Prim(ope, e1, e2) -> Prim(ope, nsubst e1 env, nsubst e2 env)

(* Some expressions with free variables: *)

let e6s0 = Prim("+", Var "y", Var "z");;

let e6s1 = nsubst e6s0 [("z", CstI 17)];;

let e6s2 = nsubst e6s0 [("z", Prim("-", CstI 5, CstI 4))];;

let e6s3 = nsubst e6s0 [("z", Prim("+", Var "z", Var "z"))];;

// Shows that only z outside the Let gets substituted:
let e7s0 = Prim("+", Let("z", CstI 22, Prim("*", CstI 5, Var "z")),
                   Var "z");;

let e7s1 = nsubst e7s0 [("z", CstI 100)];;

// Shows that only the z in the Let rhs gets substituted
let e8s0 = Let("z", Prim("*", CstI 22, Var "z"), Prim("*", CstI 5, Var "z"));;

let e8s1 = nsubst e8s0 [("z", CstI 100)];;

// Shows (wrong) capture of free variable z under the let:
let e9s0 = Let("z", CstI 22, Prim("*", Var "y", Var "z"));;

let e9s1 = nsubst e9s0 [("y", Var "z")];;

// 
let e9s2 = nsubst e9s0 [("z", Prim("-", CstI 5, CstI 4))];;

let newVar : string -> string = 
    let n = ref 0
    let varMaker s = (n.Value <- n.Value + 1; s + string (n.Value))
    varMaker

(* Correct, capture-avoiding substitution *)

let rec subst (e : expr) (env : (string * expr) list) : expr =
    match e with
    | CstI i -> e
    | Var x  -> lookOrSelf env x
    | Let(x, erhs, ebody) ->
      let newx = newVar x
      let newenv = (x, Var newx) :: remove env x
      Let(newx, subst erhs env, subst ebody newenv)
    | Prim(ope, e1, e2) -> Prim(ope, subst e1 env, subst e2 env)

let e6s1a = subst e6 [("z", CstI 17)];;

let e6s2a = subst e6 [("z", Prim("-", CstI 5, CstI 4))];;

let e6s3a = subst e6 [("z", Prim("+", Var "z", Var "z"))];;


// Shows renaming of bound variable z (to z1)
let e7s1a = subst e7s0 [("z", CstI 100)];;

// Shows renaming of bound variable z (to z2)
let e8s1a = subst e8s0 [("z", CstI 100)];;

// Shows renaming of bound variable z (to z3), avoiding capture of free z
let e9s1a = subst e9s0 [("y", Var "z")];;

(* ---------------------------------------------------------------------- *)

(* Free variables *)

(* Operations on sets, represented as lists.  Simple but inefficient;
   one could use binary trees, hashtables or splaytrees for
   efficiency.  *)

(* union(xs, ys) is the set of all elements in xs or ys, without duplicates *)

let rec union (xs, ys) = 
    match xs with 
    | []    -> ys
    | x::xr -> if mem x ys then union(xr, ys)
               else x :: union(xr, ys);;

(* minus xs ys  is the set of all elements in xs but not in ys *)

let rec minus (xs, ys) = 
    match xs with 
    | []    -> []
    | x::xr -> if mem x ys then minus(xr, ys)
               else x :: minus (xr, ys);;

(* Find all variables that occur free in expression e *)

(*
NOTE: Modified

Exercise 2.2
Revise freevars to work for the newly updated expr language

freevars returns the free variables of an expression

the updated version now takes into account sequential let bindings

Via a helper recursive function, the let match recursively unions
all free variables taking into account all the bound variables before the current erhs

the names bound by the let are excluded from the ebody free variables as well.
*)

let rec freevars e : string list =
    match e with
    | CstI i -> []
    | Var x  -> [x]
    | Let(bindings, ebody) ->
        let rec free bs (bound : string list) =
            match bs with
            | [] -> []
            | (x, erhs) :: rest ->
                let currentlyfree = minus (freevars erhs, bound)
                union (currentlyfree, free rest (x :: bound))
        union (free bindings [], minus (freevars ebody, List.map fst bindings))
    | Prim(ope, e1, e2) -> union (freevars e1, freevars e2)

(* Alternative definition of closed *)

let closed2 e = (freevars e = []);;
let _ = List.map closed2 [e1;e2;e3;e4;e5;e6;e7;e8;e9;e10]

(* ---------------------------------------------------------------------- *)

(* Compilation to target expressions with numerical indexes instead of
   symbolic variable names.  *)

type texpr =                            (* target expressions *)
  | TCstI of int
  | TVar of int                         (* index into runtime environment *)
  | TLet of texpr * texpr               (* erhs and ebody                 *)
  | TPrim of string * texpr * texpr;;


(* Map variable name to variable index at compile-time *)

let rec getindex vs x = 
    match vs with 
    | []    -> failwith "Variable not found"
    | y::yr -> if x=y then 0 else 1 + getindex yr x;;

(* Compiling from expr to texpr *)

(*
NOTE: Modified
Exercise 2.3

Updated tcomp to work with the update expr language

uses a recursive accumulator helper function to stack
all variables in the list of bindings into the environment
to compile each right hand side against the environment built so far
and at the end to run the ebody against the accumulated environment.

instead of a list tcomp creates nested TLet's


*)
let rec tcomp (e : expr) (cenv : string list) : texpr =
    match e with
    | CstI i -> TCstI i
    | Var x  -> TVar (getindex cenv x)
    | Let(bindings, ebody) -> 
      
      let rec helper bs cenvi =
        match bs with
        | [] -> tcomp ebody cenvi
        | (x, erhs) :: rest ->
            TLet(tcomp erhs cenvi, helper rest (x :: cenvi))
      helper bindings cenv
      
    | Prim(ope, e1, e2) -> TPrim(ope, tcomp e1 cenv, tcomp e2 cenv);;

(* Evaluation of target expressions with variable indexes.  The
   run-time environment renv is a list of variable values (ints).  *)

let rec teval (e : texpr) (renv : int list) : int =
    match e with
    | TCstI i -> i
    | TVar n  -> List.item n renv
    | TLet(erhs, ebody) -> 
      let xval = teval erhs renv
      let renv1 = xval :: renv 
      teval ebody renv1 
    | TPrim("+", e1, e2) -> teval e1 renv + teval e2 renv
    | TPrim("*", e1, e2) -> teval e1 renv * teval e2 renv
    | TPrim("-", e1, e2) -> teval e1 renv - teval e2 renv
    | TPrim _            -> failwith "unknown primitive";;

(* Correctness: eval e []  equals  teval (tcomp e []) [] *)


(* ---------------------------------------------------------------------- *)

(* Stack machines *)

(* Stack machine instructions.  An expressions in postfix or reverse
   Polish form is a list of stack machine instructions. *)

type rinstr =
  | RCstI of int
  | RAdd 
  | RSub
  | RMul 
  | RDup
  | RSwap;;

(* A simple stack machine for evaluation of variable-free expressions
   in postfix form *)

let rec reval (inss : rinstr list) (stack : int list) : int =
    match (inss, stack) with 
    | ([], v :: _) -> v
    | ([], [])     -> failwith "reval: no result on stack!"
    | (RCstI i :: insr,             stk)  -> reval insr (i::stk)
    | (RAdd    :: insr, i2 :: i1 :: stkr) -> reval insr ((i1+i2)::stkr)
    | (RSub    :: insr, i2 :: i1 :: stkr) -> reval insr ((i1-i2)::stkr)
    | (RMul    :: insr, i2 :: i1 :: stkr) -> reval insr ((i1*i2)::stkr)
    | (RDup    :: insr,       i1 :: stkr) -> reval insr (i1 :: i1 :: stkr)
    | (RSwap   :: insr, i2 :: i1 :: stkr) -> reval insr (i1 :: i2 :: stkr)
    | _ -> failwith "reval: too few operands on stack";;

let rpn1 = reval [RCstI 10; RCstI 17; RDup; RMul; RAdd] [];;

(* Compilation of a variable-free expression to a rinstr list *)

let rec rcomp (e : expr) : rinstr list =
    match e with
    | CstI i            -> [RCstI i]
    | Var _             -> failwith "rcomp cannot compile Var"
    | Let _             -> failwith "rcomp cannot compile Let"
    | Prim("+", e1, e2) -> rcomp e1 @ rcomp e2 @ [RAdd]
    | Prim("*", e1, e2) -> rcomp e1 @ rcomp e2 @ [RMul]
    | Prim("-", e1, e2) -> rcomp e1 @ rcomp e2 @ [RSub]
    | Prim _            -> failwith "unknown primitive";;
            
(* Correctness: eval e []  equals  reval (rcomp e) [] *)
eval e0 [];;
rcomp e0;;
reval (rcomp e0) [];;



(* Storing intermediate results and variable bindings in the same stack *)

type sinstr =
  | SCstI of int                        (* push integer           *)
  | SVar of int                         (* push variable from env *)
  | SAdd                                (* pop args, push sum     *)
  | SSub                                (* pop args, push diff.   *)
  | SMul                                (* pop args, push product *)
  | SPop                                (* pop value/unbind var   *)
  | SSwap;;                             (* exchange top and next  *)
 
let rec seval (inss : sinstr list) (stack : int list) =
    match (inss, stack) with
    | ([], v :: _) -> v
    | ([], [])     -> failwith "seval: no result on stack"
    | (SCstI i :: insr,          stk) -> seval insr (i :: stk) 
    | (SVar i  :: insr,          stk) -> seval insr (List.item i stk :: stk) 
    | (SAdd    :: insr, i2::i1::stkr) -> seval insr (i1+i2 :: stkr)
    | (SSub    :: insr, i2::i1::stkr) -> seval insr (i1-i2 :: stkr)
    | (SMul    :: insr, i2::i1::stkr) -> seval insr (i1*i2 :: stkr)
    | (SPop    :: insr,    _ :: stkr) -> seval insr stkr
    | (SSwap   :: insr, i2::i1::stkr) -> seval insr (i1::i2::stkr)
    | _ -> failwith "seval: too few operands on stack";;


(* A compile-time variable environment representing the state of
   the run-time stack. *)

type stackvalue =
  | Value                               (* A computed value *)
  | Bound of string;;                   (* A bound variable *)

(* Compilation to a list of instructions for a unified-stack machine *)

let rec scomp (e : expr) (cenv : stackvalue list) : sinstr list =
    match e with
    | CstI i -> [SCstI i]
    | Var x  -> [SVar (getindex cenv (Bound x))]
    | Let(x, erhs, ebody) -> 
          scomp erhs cenv @ scomp ebody (Bound x :: cenv) @ [SSwap; SPop]
    | Prim("+", e1, e2) -> 
          scomp e1 cenv @ scomp e2 (Value :: cenv) @ [SAdd] 
    | Prim("-", e1, e2) -> 
          scomp e1 cenv @ scomp e2 (Value :: cenv) @ [SSub] 
    | Prim("*", e1, e2) -> 
          scomp e1 cenv @ scomp e2 (Value :: cenv) @ [SMul] 
    | Prim _ -> failwith "scomp: unknown operator";;

let s1 = scomp e1 [];;
let s2 = scomp e2 [];;
let s3 = scomp e3 [];;
let s5 = scomp e5 [];;

(* Output the integers in list inss to the text file called fname: *)

let intsToFile (inss : int list) (fname : string) = 
    let text = String.concat " " (List.map string inss)
    System.IO.File.WriteAllText(fname, text);;

(* Takes Jonhnys stack instruction(s) and converts them to their respective Bytecode representation
SCstI and var i adds "instructionvalue" ; i to the list, which the machine then reads as, arh we have 0, now I will read the next as the constant and not a operation. Same with var; value 1 tells the same story.
Machines reads list from left to right which we will also have to account for in assemble function. So yeah, prepares the machine for what is coming so that it cant be misinterpereted as a instruction.
all the remaining just send their instruction values to the list.

*)

let sinstrToInt (sins : sinstr) : int list =
    match sins with
    | SCstI i -> [0; i]
    | SVar i -> [1; i]
    | SAdd -> [2]
    | SSub -> [3]
    | SMul -> [4]
    | SPop -> [5]
    | SSwap -> [6]


(*Eats sInstruction list, runs through it with helper function with accumulator
since adding from behind is expensive we have to account for having to reverse the list at the end.sinList
if we had 0 i 0 i 2
We would get 2 @ 0 i 0 i when appending the last int list returned from sinstr, which would give us the following when reversed
i 0 i 0 2 which the machine would have less of a great time with.
Thats why we also have to reverse the returned int list from sinstr before appending, so when list is reversed the machine gets warned about the next "value"
not being an instruction.
*)
let assemble (sinList : sinstr List) : int list =
    let rec assHelper lst acc =
        match lst with
        | [] -> List.rev acc
        | sin :: remSins ->
            assHelper remSins ((List.rev (sinstrToInt sin)) @ acc)
    assHelper sinList []

(* -----------------------------------------------------------------  *)
