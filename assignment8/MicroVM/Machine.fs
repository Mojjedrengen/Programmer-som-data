// File microJ/Machine.fs 
//
// Instructions and code emission for a stack-based
// abstract machine microVM

// An implementation of the machine is found in file
// microVM/miccrovm.c.

module Machine

open System.IO

type label = string

type instr =
  | Label of label            // Symbolic label; pseudo-instruction
  | LabelAddr of label        // Label address part of code, pseudo-instruction

  // Constants
  | CSTI of int               // Integer constant
  | NIL                       // NIL or null constant

  // Arithmetic
  | ADD                       // addition
  | SUB                       // subtraction
  | MUL                       // multiplication
  | DIV                       // division
  | MOD                       // modulus

  // Logical  
  | EQ                        // equality: s[sp-1] == s[sp]
  | LT                        // less than: s[sp-1] < s[sp]
  | NOT                       // logical negation: s[sp] != 0

  // Stack
  | DUP                       // duplicate stack top
  | SWAP                      // swap s[sp-1] and s[sp]

  // Address, load and store  
  | LDI                       // Stack, load indirect
  | STI                       // Stack, store indirect
  | HEAPSTI of int            // Heap, store indirect
  | HEAPLDI of int            // Heap, load indirect
  | HEAPALLOC of int * int    // Allocate heap object with some tag, tag * size.
  | HEAPCOPY of int           // Copy from stack to heap object.
  | LDD                       // Load direct - both heap and stack.
  | STD                       // Store direct - both heap and stack.
  | STACKADDR                 // Calculate direct address on stack.
  | HEAPADDR                  // Calculate direct address on heap.
  
  // Stack and stack frame  
  | GETBP                     // Get base pointer
  | GETSP                     // Get stack pointer
  | INCSP of int              // Increase stack top by m

  // Code flow
  | GOTO of label             // Go to label
  | IFZERO of label           // Go to label if s[sp] == 0
  | IFNZRO of label           // Go to label if s[sp] != 0
  | PUSHLAB of label          // Push label on stack

  // Call and return
  | CALL of int * label         // Move m args up 2, push pc, bp and jump
  | TCALL of int * int * label  // Move m args down n, jump  
  | CLOSCALL of int             // Move m args up 2, push pc, bp and jump to addr in closure
  | TCLOSCALL of int            // Move m args down to bp, and jump to addr in closure
  | VCALL of int * int          // Move m args up 2, push pc, bp and jump to index into vTable
  //| TVCALL of int * int       // Exercise for compiling micro-Java
  | RET of int                  // Pop m and return to s[sp]

  // Print on std. out  
  | PRINTI                    // Print s[sp] as integer
  | PRINTN                    // Print s[sp] as NIL or null
  | PRINTB                    // Print s[sp] as true/false
  | PRINTO                    // Print s[sp] as Object
  | PRINTNL                   // Print new line
  | PRINTC                    // Print s[sp] as character
  | PRINTL                    // Print s[sp] as list
  | PRINTVAL                  // Polymorphic print

  // Start and stop program  
  | LDARGS of int             // Load command line arguments on stack
  | STOP                      // Stop program execution

  // Cons and Pairs
  | CONS                      // Allocate Cons cell.
  | CAR                       // Load first component
  | CDR                       // Load second component
  | SETCAR                    // Set first component
  | SETCDR                    // Set second component 

  // Exceptions
  | THROW                     // Search for exception handle and execute affiliated exception code
  | PUSHHDLR of label         // Push exception handler on stack 
  | POPHDLR                   // Pop exception handler from stack

// Tags must match microvm.c
let consTag = 0
let closTag = 1
let stringTag = 2
let arrayTag = 3
let objectTag = 4

// Encoding of constants
let nilValue = 0 // Encoding of NIL or null value.

// Generate new distinct labels
let resetLabels, newLabelWName = 
  let lastlab = ref -1
  ((fun () -> lastlab.Value <- 0),
    (fun name -> (lastlab.Value <- 1 + lastlab.Value;
                  (if name = ""
                     then ""
                     else name + "_") + "L" + (lastlab.Value).ToString())))
let newLabel() = newLabelWName ""

// Simple environment operations
type 'data env = (string * 'data) list

let rec lookup env x = 
  match env with 
  | []         -> failwith ("Machine.lookup: " + x + " not found")
  | (y, v)::yr -> if x=y then v else lookup yr x

// An instruction list is emitted in two phases:
//   * pass 1 builds an environment labenv mapping labels to addresses 
//   * pass 2 emits the code to file, using the environment labenv to 
//     resolve labels

// These numeric instruction codes must agree with microVM/microvm.c
let code = function
  | Label _ -> failwith "Machine.code: Label has no instruction number"
  | LabelAddr _ -> failwith "Machine.code: LabelAddr has no instruction number"

  // Constants
  | CSTI _ -> 0
  | NIL    -> 1

  // Arithmetic
  | ADD -> 10
  | SUB -> 11
  | MUL -> 12
  | DIV -> 13
  | MOD -> 14

  // Logical  
  | EQ  -> 20
  | LT  -> 21
  | NOT -> 22

  // Stack
  | DUP  -> 30
  | SWAP -> 31

  // Address, load and store  
  | LDI  -> 40
  | STI  -> 41
  | HEAPSTI _ -> 42
  | HEAPLDI _ -> 43
  | HEAPALLOC _ -> 44
  | HEAPCOPY _ -> 45
  | LDD      -> 46
  | STD      -> 47
  | STACKADDR -> 48
  | HEAPADDR  -> 49
  
  // Stack and stack frame  
  | GETBP  -> 60
  | GETSP  -> 61
  | INCSP _ -> 62

  // Code flow
  | GOTO _ -> 70
  | IFZERO _ -> 71
  | IFNZRO _ -> 72
  | PUSHLAB _ -> 73

  // Call and return
  | CALL _ -> 80
  | TCALL _ -> 81
  | CLOSCALL _ -> 82
  | TCLOSCALL _ -> 83
  | VCALL _ -> 84
  //| TVCALL _ -> 85  // Reserved for exercise
  | RET _ -> 86

  // Print on std. out  
  | PRINTI -> 90
  | PRINTN -> 91
  | PRINTB -> 92
  | PRINTO -> 93
  | PRINTNL -> 94
  | PRINTC  -> 95
  | PRINTL  -> 96
  | PRINTVAL -> 97

  // Start and stop program  
  | LDARGS _ -> 100
  | STOP  -> 101

  // Cons / Pairs
  | CONS   -> 110
  | CAR    -> 111
  | CDR    -> 112
  | SETCAR -> 113
  | SETCDR -> 114 

  // Exceptions
  | THROW      -> 120
  | PUSHHDLR _ -> 121
  | POPHDLR    -> 122

// Bytecode emission, first pass: build environment that maps each
// label to an integer address in the bytecode.

let sizeInstr instr = 
  match instr with
  | Label lab      -> 0
  | CSTI i         -> 2
  | ADD            -> 1
  | SUB            -> 1
  | MUL            -> 1
  | DIV            -> 1
  | MOD            -> 1
  | EQ             -> 1
  | LT             -> 1
  | NOT            -> 1
  | DUP            -> 1
  | SWAP           -> 1
  | LDI            -> 1
  | STI            -> 1
  | GETBP          -> 1
  | GETSP          -> 1
  | INCSP m        -> 2
  | GOTO lab       -> 2
  | IFZERO lab     -> 2
  | IFNZRO lab     -> 2
  | CALL(m,lab)    -> 3
  | CLOSCALL m     -> 2
  | TCLOSCALL m    -> 2
  | TCALL(m,n,lab) -> 4
  | VCALL(m,i)     -> 3  
  | RET m          -> 2
  | PRINTI         -> 1
  | PRINTB         -> 1
  | PRINTC         -> 1
  | PRINTL         -> 1      
  | PRINTN         -> 1
  | PRINTO         -> 1    
  | LDARGS _       -> 2
  | STOP           -> 1
  | NIL            -> 1
  | CONS           -> 1
  | CAR            -> 1
  | CDR            -> 1
  | SETCAR         -> 1
  | SETCDR         -> 1
  | PUSHLAB _      -> 2
  | HEAPALLOC _    -> 3  // tag and size
  | HEAPSTI _      -> 2
  | HEAPLDI _      -> 2
  | THROW          -> 1  
  | PUSHHDLR _     -> 2
  | POPHDLR        -> 1
  | PRINTNL        -> 1
  | LabelAddr _    -> 1  // It is just the label in the vTable - no instruction to execute.
  | HEAPCOPY _     -> 2
  | LDD            -> 1
  | STD            -> 1
  | STACKADDR      -> 1
  | HEAPADDR       -> 1
  | PRINTVAL       -> 1  

let makeLabEnv (addr, labenv) instr =
  let size = sizeInstr instr      
  match instr with
  | Label lab -> (addr, (lab, addr) :: labenv)
  | _         -> (addr+size, labenv)

// Bytecode emission, second pass: output bytecode as integers
let emitInstr getlab instr C = 
  match instr with
  | Label lab         -> C
  | LabelAddr lab     -> getlab lab :: C
  | CSTI i            -> code instr :: i :: C
  | INCSP m           -> code instr :: m :: C
  | GOTO lab          -> code instr :: getlab lab :: C
  | IFZERO lab        -> code instr :: getlab lab :: C
  | IFNZRO lab        -> code instr :: getlab lab :: C
  | CALL(m,lab)       -> code instr :: m :: getlab lab :: C
  | CLOSCALL m        -> code instr :: m :: C
  | TCLOSCALL m       -> code instr :: m :: C
  | TCALL(m,n,lab)    -> code instr :: m :: n :: getlab lab :: C
  | VCALL(m,i)        -> code instr :: m :: i :: C  
  | RET m             -> code instr :: m :: C
  | LDARGS n          -> code instr :: n :: C
  | PUSHLAB lab       -> code instr :: getlab lab :: C
  | HEAPALLOC (tag,n) -> code instr :: tag :: n :: C  
  | HEAPSTI n         -> code instr :: n :: C
  | HEAPLDI n         -> code instr :: n :: C
  | PUSHHDLR lab      -> code instr :: getlab lab :: C
  | HEAPCOPY n        -> code instr :: n :: C
  | _                 -> code instr :: C

let emitInstrs getLab instrs C =
  List.foldBack (emitInstr getLab) instrs C
  
let ppInstr (addr,strs) instr =
  let indent s = (addr + sizeInstr instr,"  " + (addr.ToString().PadLeft(4)) + ": " + s :: strs)
  match instr with
  | Label lab      -> (addr, "LABEL " + lab :: strs)
  | CSTI i         -> indent ("CSTI " + i.ToString())
  | ADD            -> indent "ADD"
  | SUB            -> indent "SUB"
  | MUL            -> indent "MUL"
  | DIV            -> indent "DIV"
  | MOD            -> indent "MOD"
  | EQ             -> indent "EQ" 
  | LT             -> indent "LT" 
  | NOT            -> indent "NOT"
  | DUP            -> indent "DUP"
  | SWAP           -> indent "SWAP"
  | LDI            -> indent "LDI" 
  | STI            -> indent "STI" 
  | GETBP          -> indent "GETBP"
  | GETSP          -> indent "GETSP" 
  | INCSP m        -> indent ("INCSP " + m.ToString())
  | GOTO lab       -> indent ("GOTO " + lab)
  | IFZERO lab     -> indent ("IFZERO " + lab)
  | IFNZRO lab     -> indent ("IFNZRO " + lab)
  | CALL(m,lab)    -> indent ("CALL " + m.ToString() + " " + lab)
  | CLOSCALL m     -> indent ("CLOSCALL " + m.ToString())
  | TCLOSCALL m    -> indent ("TCLOSCALL " + m.ToString())
  | TCALL(m,n,lab) -> indent ("TCALL " + m.ToString() + " " + n.ToString() + " " + lab)
  | VCALL(m,i)     -> indent ("VCALL " + m.ToString() + " " + (i.ToString()))  
  | RET m          -> indent ("RET " + m.ToString())
  | PRINTI         -> indent "PRINTI"
  | PRINTB         -> indent "PRINTB"
  | PRINTC         -> indent "PRINTC"
  | PRINTL         -> indent "PRINTL"    
  | PRINTN         -> indent "PRINTN"
  | PRINTO         -> indent "PRINTO"  
  | LDARGS n       -> indent ("LDARGS " + n.ToString())
  | STOP           -> indent "STOP"  
  | NIL            -> indent "NIL"   
  | CONS           -> indent "CONS"  
  | CAR            -> indent "CAR"   
  | CDR            -> indent "CDR"   
  | SETCAR         -> indent "SETCAR"
  | SETCDR         -> indent "SETCDR"
  | PUSHLAB lab    -> indent ("PUSHLAB " + lab)
  | HEAPALLOC (tag,n) -> indent ("HEAPALLOC " + tag.ToString() + " " + n.ToString())
  | HEAPSTI n      -> indent ("HEAPSTI " + n.ToString())
  | HEAPLDI n      -> indent ("HEAPLDI " + n.ToString())
  | THROW          -> indent "THROW"
  | PUSHHDLR lab   -> indent ("PUSHHDLR " + lab)
  | POPHDLR        -> indent "POPHDLR"
  | PRINTNL        -> indent "PRINTNL"
  | LabelAddr lab  -> indent ("LabelAddr " + lab)
  | HEAPCOPY n     -> indent ("HEAPCOPY " + n.ToString())
  | LDD            -> indent "LDD"
  | STD            -> indent "STD"
  | STACKADDR      -> indent "STACKADDR"
  | HEAPADDR       -> indent "HEAPADDR"
  | PRINTVAL       -> indent ("PRINTVAL")  

let ppInstrs (code : instr list) : string =
  String.concat "\n" (List.rev (snd (List.fold ppInstr (0,[]) code)))

let code2ints fnDebug (instrs:instr list) : int list =
  let (_, labEnv) = List.fold makeLabEnv (0, []) instrs
  let getLab lab = lookup labEnv lab
  fnDebug (sprintf "Machine.code2insts.LabEnv: %A" labEnv)
  emitInstrs getLab instrs []

let intsToFile (inss : int list) (fname : string) = 
  File.WriteAllText(fname, String.concat " " (List.map string inss))

