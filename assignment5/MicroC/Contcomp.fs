(* File MicroC/Contcomp.fs
   A continuation-based (backwards) compiler from micro-C, a fraction of
   the C language, to an abstract machine.  
   sestoft@itu.dk * 2011-11-10, 2026-06-13

   The abstract machine code is generated backwards, so that jumps to
   jumps can be eliminated, so that tail-calls (calls immediately
   followed by return) can be recognized, dead code can be eliminated, 
   etc.

   The compilation of a block, which may contain a mixture of
   declarations and statements, proceeds in two passes:

   Pass 1: elaborate declarations to find the environment in which
           each statement must be compiled; also translate
           declarations into allocation instructions, of type
           bstmtordec.
  
   Pass 2: compile the statements in the given environments.
 *)

module Comp

open System.IO
open Absyn
open Machine

// Compiler message - this is forwards compiler
let compMsg = "backwards"

(* The intermediate representation between passes 1 and 2 above:  *)

type bstmtordec =
     | BDec of instr list                  (* Declaration of local variable  *)
     | BStmt of stmt                       (* A statement                    *)

(* ------------------------------------------------------------------- *)

(* Code-generating functions that perform local optimizations *)

let rec addINCSP m1 c : instr list =
    match c with
    | INCSP m2            :: c1 -> addINCSP (m1+m2) c1
    | RET m2              :: c1 -> RET (m2-m1) :: c1
    | Label lab :: RET m2 :: _  -> RET (m2-m1) :: c  (* Becomes: RET(m2-m1)::Label lab::RET m2 *)
    | _                         -> if m1=0 then c else INCSP m1 :: c

let addLabel c : label * instr list =          (* Conditional jump to c *)
    match c with
    | Label lab :: _ -> (lab, c)
    | GOTO lab :: _  -> (lab, c)
    | _              -> let lab = newLabel() 
                        (lab, Label lab :: c)

let makeJump c : instr * instr list =          (* Unconditional jump to c *)
    match c with
    | RET m              :: _ -> (RET m, c)
    | Label lab :: RET m :: _ -> (RET m, c)
    | Label lab          :: _ -> (GOTO lab, c)
    | GOTO lab           :: _ -> (GOTO lab, c)
    | _                       -> let lab = newLabel() 
                                 (GOTO lab, Label lab :: c)

let makeCall m lab c : instr list =
    match c with
    | RET n            :: c1 -> TCALL(m, n, lab) :: c1
    | Label _ :: RET n :: _  -> TCALL(m, n, lab) :: c
    | _                      -> CALL(m, lab) :: c

let rec deadcode c = (* Remove all code until next label *)
    match c with
    | []              -> []
    | Label lab :: _  -> c
    | _         :: c1 -> deadcode c1

let addNOT c =
    match c with
    | NOT        :: c1 -> c1
    | IFZERO lab :: c1 -> IFNZRO lab :: c1 
    | IFNZRO lab :: c1 -> IFZERO lab :: c1 
    | _                -> NOT :: c

let addJump jump c =                    (* jump is GOTO or RET *)
    let c1 = deadcode c
    match (jump, c1) with
    | (GOTO lab1, Label lab2 :: _) -> if lab1=lab2 then c1 
                                      else GOTO lab1 :: c1
    | _                            -> jump :: c1
    
let addGOTO lab c =
    addJump (GOTO lab) c

let rec addCST i c =
    match (i, c) with
    | (0, ADD        :: c1) -> c1
    | (0, SUB        :: c1) -> c1
    | (0, NOT        :: c1) -> addCST 1 c1
    | (_, NOT        :: c1) -> addCST 0 c1
    | (1, MUL        :: c1) -> c1
    | (1, DIV        :: c1) -> c1
    | (0, EQ         :: c1) -> addNOT c1
    | (_, INCSP m    :: c1) -> if m < 0 then addINCSP (m+1) c1
                               else CSTI i :: c
    | (0, IFZERO lab :: c1) -> addGOTO lab c1
    | (_, IFZERO lab :: c1) -> c1
    | (0, IFNZRO lab :: c1) -> c1
    | (_, IFNZRO lab :: c1) -> addGOTO lab c1
    | _                     -> CSTI i :: c
            
(* ------------------------------------------------------------------- *)

(* Simple environment operations *)

type 'data env = (string * 'data) list

let rec lookup env x = 
    match env with 
    | []         -> failwith (x + " not found")
    | (y, v)::yr -> if x=y then v else lookup yr x

(* A global variable has an absolute address, a local one has an offset: *)

type var = 
    | Glovar of int                   (* absolute address in stack           *)
    | Locvar of int                   (* address relative to bottom of frame *)

(* The variable environment keeps track of global and local variables, and 
   keeps track of next available offset for local variables *)

type varEnv = (var * typ) env * int

(* The function environment maps a function name to the function's label, 
   its return type, and its parameter declarations *)

type paramdecs = (typ * string) list
type funEnv = (label * typ option * paramdecs) env

(* Bind declared variable in varEnv and generate code to allocate it: *)

let allocate (kind : int -> var) (typ, x) (varEnv : varEnv) : varEnv * instr list =
    let (env, fdepth) = varEnv 
    match typ with
    | TypA (TypA _, _) -> failwith "allocate: arrays of arrays not permitted"
    | TypA (t, Some i) ->
      let newEnv = ((x, (kind (fdepth+i), typ)) :: env, fdepth+i+1) (* i+1 because we need room for the array elements and pointer to first element. *)
      let code = [INCSP i; GETSP; CSTI (i-1); SUB]
      (newEnv, code)
    | _ -> 
      let newEnv = ((x, (kind (fdepth), typ)) :: env, fdepth+1)
      let code = [INCSP 1]
      (newEnv, code)

(* Bind declared parameter in env: *)

let bindParam (env, fdepth) (typ, x) : varEnv = 
    ((x, (Locvar fdepth, typ)) :: env, fdepth+1);

let bindParams paras (env, fdepth) : varEnv = 
    List.fold bindParam (env, fdepth) paras;

(* ------------------------------------------------------------------- *)

(* Build environments for global variables and global functions *)

let makeGlobalEnvs(topdecs : topdec list) : varEnv * funEnv * instr list = 
    let rec addv decs varEnv funEnv = 
        match decs with 
        | [] -> (varEnv, funEnv, [])
        | dec::decr -> 
          match dec with
          | Vardec (typ, x) ->
            let (varEnv1, code1) = allocate Glovar (typ, x) varEnv
            let (varEnvr, funEnvr, coder) = addv decr varEnv1 funEnv
            (varEnvr, funEnvr, code1 @ coder)
          | Fundec (tyOpt, f, xs, body) ->
            addv decr varEnv ((f, (newLabel(), tyOpt, xs)) :: funEnv)
    addv topdecs ([], 0) []
    
(* ------------------------------------------------------------------- *)

(* Compiling micro-C statements:

   * stmt    is the statement to compile
   * varenv  is the local and global variable environment 
   * funEnv  is the global function environment
   * c       is the code that follows the code for stmt
*)

let rec cStmt stmt (varEnv : varEnv) (funEnv : funEnv) (c : instr list) : instr list = 
    match stmt with
    | If(e, stmt1, stmt2) -> 
      let (jumpend, c1) = makeJump c
      let (labelse, c2) = addLabel (cStmt stmt2 varEnv funEnv c1)
      cExpr e varEnv funEnv (IFZERO labelse 
       :: cStmt stmt1 varEnv funEnv (addJump jumpend c2))
    | While(e, body) ->
      let labbegin = newLabel()
      let (jumptest, c1) = 
           makeJump (cExpr e varEnv funEnv (IFNZRO labbegin :: c))
      addJump jumptest (Label labbegin :: cStmt body varEnv funEnv c1)
    | Expr e -> 
      cExpr e varEnv funEnv (addINCSP -1 c) (* Remove result of expression from stack, as this is a statement *)
    | Block stmts -> 
      let rec pass1 stmts ((_, fdepth) as varEnv) =
          match stmts with 
          | []     -> ([], fdepth)
          | s1::sr ->
            let (_, varEnv1) as res1 = bStmtordec s1 varEnv
            let (resr, fdepthr) = pass1 sr varEnv1 
            (res1 :: resr, fdepthr) 
      let (stmtsback, fdepthend) = pass1 stmts varEnv
      let rec pass2 pairs c = 
          match pairs with 
          | [] -> c
          | (BDec code,  varEnv) :: sr -> code @ pass2 sr c
          | (BStmt stmt, varEnv) :: sr -> cStmt stmt varEnv funEnv (pass2 sr c)
      pass2 stmtsback (addINCSP(snd varEnv - fdepthend) c) (* Remove variables, declared in the block, from the stack *)
    | Return None -> 
      RET (snd varEnv - 1) :: deadcode c
    | Return (Some e) -> 
      cExpr e varEnv funEnv (RET (snd varEnv) :: deadcode c)

and bStmtordec stmtOrDec varEnv : bstmtordec * varEnv =
    match stmtOrDec with 
    | Stmt stmt    ->
      (BStmt stmt, varEnv) 
    | Dec (typ, x) ->
      let (varEnv1, code) = allocate Locvar (typ, x) varEnv 
      (BDec code, varEnv1)

(* Compiling micro-C expressions: 

   * e       is the expression to compile
   * varEnv  is the compile-time variable environment 
   * funEnv  is the compile-time environment 
   * c       is the code following the code for this expression

   Net effect principle: if the compilation (cExpr e varEnv funEnv c) of
   expression e returns the instruction sequence instrs, then the
   execution of instrs will have the same effect as an instruction
   sequence that first computes the value of expression e on the stack
   top and then executes c, but because of optimizations instrs may
   actually achieve this in a different way.
 *)

and cExpr (e : expr) (varEnv : varEnv) (funEnv : funEnv) (c : instr list) : instr list =
    match e with
    | Access acc     -> cAccess acc varEnv funEnv (LDI :: c)
    | Assign(acc, e) -> cAccess acc varEnv funEnv (cExpr e varEnv funEnv (STI :: c))
    | CstI i         -> addCST i c
    | Addr acc       -> cAccess acc varEnv funEnv c
    | Prim1(ope, e1) ->
      cExpr e1 varEnv funEnv
          (match ope with
           | "!"      -> addNOT c
           | "printi" -> PRINTI :: c
           | "println" -> PRINTNL :: c
           | _        -> failwith "unknown primitive 1")
    | Prim2(ope, e1, e2) ->
      cExpr e1 varEnv funEnv
        (cExpr e2 varEnv funEnv
           (match ope with
            | "*"   -> MUL  :: c
            | "+"   -> ADD  :: c
            | "-"   -> SUB  :: c
            | "/"   -> DIV  :: c
            | "%"   -> MOD  :: c
            | "=="  -> EQ   :: c
            | "!="  -> EQ   :: addNOT c
            | "<"   -> LT   :: c
            | ">="  -> LT   :: addNOT c
            | ">"   -> SWAP :: LT :: c
            | "<="  -> SWAP :: LT :: addNOT c
            | _     -> failwith "unknown primitive 2"))
    | Andalso(e1, e2) ->
      match c with
      | IFZERO lab :: _ ->
         cExpr e1 varEnv funEnv (IFZERO lab :: cExpr e2 varEnv funEnv c)
      | IFNZRO labthen :: c1 -> 
        let (labelse, c2) = addLabel c1
        cExpr e1 varEnv funEnv
           (IFZERO labelse 
              :: cExpr e2 varEnv funEnv (IFNZRO labthen :: c2))
      | _ ->
        let (jumpend,  c1) = makeJump c
        let (labfalse, c2) = addLabel (addCST 0 c1)
        cExpr e1 varEnv funEnv
          (IFZERO labfalse 
             :: cExpr e2 varEnv funEnv (addJump jumpend c2))
    | Orelse(e1, e2) -> 
      match c with
      | IFNZRO lab :: _ -> 
        cExpr e1 varEnv funEnv (IFNZRO lab :: cExpr e2 varEnv funEnv c)
      | IFZERO labthen :: c1 ->
        let(labelse, c2) = addLabel c1
        cExpr e1 varEnv funEnv
           (IFNZRO labelse :: cExpr e2 varEnv funEnv
             (IFZERO labthen :: c2))
      | _ ->
        let (jumpend, c1) = makeJump c
        let (labtrue, c2) = addLabel(addCST 1 c1)
        cExpr e1 varEnv funEnv
           (IFNZRO labtrue 
             :: cExpr e2 varEnv funEnv (addJump jumpend c2))
    | Call(f, es) -> callfun f es varEnv funEnv c

(* Generate code to access variable, dereference pointer or index array: *)

and cAccess access varEnv funEnv c = 
    match access with 
    | AccVar x   ->
      match lookup (fst varEnv) x with
      | Glovar addr, _ -> addCST addr c
      | Locvar addr, _ -> GETBP :: addCST addr (ADD :: c)
    | AccDeref e ->
      cExpr e varEnv funEnv c
    | AccIndex(acc, idx) ->
      cAccess acc varEnv funEnv (LDI :: cExpr idx varEnv funEnv (ADD :: c))

(* Generate code to evaluate a list es of expressions: *)

and cExprs es varEnv funEnv c = 
    match es with 
    | []     -> c
    | e1::er -> cExpr e1 varEnv funEnv (cExprs er varEnv funEnv c)

(* Generate code to evaluate arguments es and then call function f: *)
    
and callfun f es varEnv funEnv c : instr list =
    let (labf, tyOpt, paramdecs) = lookup funEnv f
    let argc = List.length es
    if argc = List.length paramdecs then
      cExprs es varEnv funEnv (makeCall argc labf c)
    else
      failwith (f + ": parameter/argument mismatch")

(* Compile a complete micro-C program: globals, call to main, functions *)

let cProgram (Prog topdecs) : instr list = 
    let _ = resetLabels ()
    let ((globalVarEnv, _), funEnv, globalInit) = makeGlobalEnvs topdecs
    let compilefun (tyOpt, f, xs, body) =
        let (labf, _, paras) = lookup funEnv f
        let (envf, fdepthf) = bindParams paras (globalVarEnv, 0)
        let c0 = [RET (List.length paras-1)] (* -1 because there is no result value on the stack; we leave a dummy one - body of function is always a statement. *)
        let code = cStmt body (envf, fdepthf) funEnv c0
        Label labf :: code
    let functions = 
        List.choose (function 
                         | Fundec (rTy, name, argTy, body) 
                                    -> Some (compilefun (rTy, name, argTy, body))
                         | Vardec _ -> None)
                         topdecs
    let (mainlab, _, mainparams) = lookup funEnv "main"
    let argc = List.length mainparams
    globalInit 
    @ [LDARGS argc; CALL(argc, mainlab); STOP] 
    @ List.concat functions

(* Compile the program (in abstract syntax) and write it to file
   fname; also, return the program as a list of instructions.
 *)

let intsToFile (inss : int list) (fname : string) = 
    File.WriteAllText(fname, String.concat " " (List.map string inss))

let compileToFile program fname = 
    let instrs   = cProgram program 
    let bytecode = code2ints instrs
    intsToFile bytecode fname; instrs

(* Example programs are found in the files ex1.c, ex2.c, etc *)
