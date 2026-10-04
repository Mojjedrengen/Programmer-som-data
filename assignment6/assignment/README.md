# Exercise 8.4

## (i)
Symbolic bytecode for "CEx/ex08"

  [LDARGS 0; CALL (0, "L1"); STOP; 
  
  Label "L1"; 
    INCSP 1; 
    GETBP; 
    CSTI 0; 
    ADD;
    CSTI 20000000; 
    STI; 
    INCSP -1; 
    GOTO "L3"; 

  Label "L2"; 
    GETBP; 
    CSTI 0; 
    ADD;
    GETBP; 
    CSTI 0; 
    ADD; 
    LDI; 
    CSTI 1; 
    SUB; 
    STI; 
    INCSP -1; 
    INCSP 0; 

  Label "L3";
    GETBP; 
    CSTI 0; 
    ADD; 
    LDI; 
    IFNZRO "L2"; 
    INCSP -1; 
    RET -1]

Whereas the handwritten code translates to:

0 CSTI 20000000; 
2 GOTO 7;
4 CSTI 1;
6 SUB;
7 DUP;
8 IFNZRO 4;
10 STOP;

The reason for the handwritten code being much faster than the above symbolic bytecode, is that the bytecode
has to find the address of i, then the value before it den decrease i by 1, whereas the handwritten code just stores
the counter on top of the stack where it doesnt need to fetch and address and a value and can instead just it directly without storing
it in a variable.

## (ii)

Symbolic bytecode for "CEx/ex13.out"

  [LDARGS 1; CALL (1, "L1"); STOP; 
  
  Label "L1"; INCSP 1; GETBP; CSTI 1; ADD;
   CSTI 1889; STI; INCSP -1; GOTO "L3"; 
   
   
  Label "L2"; GETBP; CSTI 1; ADD; GETBP;
   CSTI 1; ADD; LDI; CSTI 1; ADD; STI; INCSP -1; GETBP; CSTI 1; ADD; LDI;
   CSTI 4; MOD; CSTI 0; EQ; IFZERO "L7"; GETBP; CSTI 1; ADD; LDI; CSTI 100;
   MOD; CSTI 0; EQ; NOT; IFNZRO "L9"; GETBP; CSTI 1; ADD; LDI; CSTI 400; MOD;
   CSTI 0; EQ; GOTO "L8"; 
  
  Label "L9"; CSTI 1; 
  
  Label "L8"; GOTO "L6";
  
  Label "L7"; CSTI 0; 
  
  Label "L6"; IFZERO "L4"; GETBP; CSTI 1; ADD; LDI;
   PRINTI; INCSP -1; GOTO "L5"; 
  
  Label "L4"; INCSP 0; 
  
  Label "L5"; INCSP 0;
  
  Label "L3"; GETBP; CSTI 1; ADD; LDI; GETBP; CSTI 0; ADD; LDI; LT;
   IFNZRO "L2"; INCSP -1; RET 0]

The program first jumps down to L3 to test whether or not to continue the loop, if it it holds, then
IFNZRO jumps back up to L2 where the if statement is tested after incrementing, the and and or statements are made into jumps
which skips the rest of the conditions once the result is already known leaving either a 0 or a 1 on the stack for when L6 runs.
at L6, if a 0 was left on the stack from the condition (if it was false), then it jumps to L4 which is empty, otherwise if runs the body and prints, goes to L5 (which is also empty)
meaning it then goes straight through to L3 to run the loop once again.