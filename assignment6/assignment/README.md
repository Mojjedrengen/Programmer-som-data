# Exercuse 8.1

compile "CEx/ex03";;   yielded the following bytecode:
   ```fsharp
   [LDARGS 1; CALL (1, "L1"); STOP; //Call label1

   Label "L1"; 
    INCSP 1; //make space for i
    GETBP; 
    CSTI 1; 
    ADD; //address of i (base pointer + offset)
    CSTI 0; //What to store in i
    STI; //store 0 into i, so i = 0
    INCSP -1; //remove the leftover 0
    GOTO "L3"; Go to while loop
   
   Label "L2"; 
    GETBP; 
    CSTI 1; 
    ADD; 
    LDI; //get the value at i's address
    PRINTI; //print it
    INCSP -1; value no longer needed, so removes it after print
    GETBP; 
    CSTI 1; 
    ADD; //Found address of i for i =
    GETBP; 
    CSTI 1; 
    ADD; 
    LDI; //Got value of i for i = "i"
    CSTI 1; 
    ADD; //add the value of i and 1 "i + 1"
    STI; //Store it into the address of i found earlier "i = i + 1"
    INCSP -1; //remove leftover value (i + 1)
    INCSP 0; //Nothing???

   Label "L3"; 
    GETBP;
    CSTI 1; 
    ADD; //the address of i
    LDI; //get the actual value of i
    GETBP; 
    CSTI 0;
    ADD; //the address of n
    LDI; //get the actual value of n
    LT; //the less than in the while loop
    IFNZRO "L2"; //if true keep while loop going aka go to label 2
    INCSP -1; //free i, so decrease pointer back to n
    RET 0]
   ```
 compile "CEx/ex05";;     yielded the following bytecode:


   ```fsharp
   [LDARGS 1; CALL (1, "L1"); STOP; //Call Label L1
   
   Label "L1"; 
   INCSP 1; //Make space for int r (int r;)
   GETBP; 
   CSTI 1; 
   ADD; //Add address of r onto stack
   GETBP; 
   CSTI 0; 
   ADD; 
   LDI; //Get value of n
   STI; //store value of n into r "r = n"
   INCSP -1; //remove leftover value of n
   INCSP 1; make space for newly scoped int r;
    //This is where it is visible in the generated code that there is a nested scope
    //We have to increase and decrease the pointer accordingly with the newly scoped variable
   GETBP; 
   CSTI 0; 
   ADD; 
   LDI; //get value of n for square(n, _)
   GETBP; 
   CSTI 2; 
   ADD; //get address of r for square(n, &r) (no LDI because they want the address)
   CALL (2, "L2"); //go to label2 with the two top stack values here n and &r and consumes them
   INCSP -1; //remove leftover value from call to square
   GETBP; 
   CSTI 2; 
   ADD; 
   LDI; //get value of r
   PRINTI; //print (print r;)
   INCSP -1; //remove computed r that print used
   INCSP -1; //remove earlier space reserved for newly scoped r
   GETBP; 
   CSTI 1; 
   ADD; 
   LDI; //get value of the first int r;
   PRINTI; //print it
   INCSP -1; //remove leftover value from stack
   INCSP -1; //remove r from stack
   RET 0; 

   Label "L2"; 
   GETBP; 
   CSTI 1; 
   ADD; 
   LDI; //get value of rp (because it's a dereference, so we want the address it points at)
   GETBP; 
   CSTI 0; 
   ADD;
   LDI; //get value of i
   GETBP; 
   CSTI 0; 
   ADD; 
   LDI; //get value of i again
   MUL; //i * i
   STI; //store i * i into *rp (*rp = i * i)
   INCSP -1; remove leftover i * i
   INCSP 0; 
   RET 1]
   ```

The various frames, such as [ 5 -999 4 0 2 1 ] correlate to the return address, the callers basepointer, the value of the basepointer itself, so for us this would be where n would be, and the values after such as 0, 2, and 1 would be the other values on the stack, meaning to go from base pointer (the second index holding n) we would increment the base pointer by 1 to go to the value of 0 and so on.

If we take the following snippet as an example we can trace it as:
```fsharp
[ 5 -999 4 ]{6: INCSP 1} //Currently our base pointer at index [2] has a value of 4 which in our micro C program would be n
//After making space (INCSP1 1) for a new value in the stack
[ 5 -999 4 0 ]{8: GETBP} //We now have a new value at the base pointer + 1 with a value of 0, calling GETBP then gives as the base pointer at index 2:
[ 5 -999 4 0 2 ]{9: CSTI 1} //We then add the value of 1 to the stack
[ 5 -999 4 0 2 1 ] //As of now 4 is the value of n and 2 is the value of the bp, 0 is the new index we want on the stack, 2 is where the base pointer is and 1 is the offset.
[ 5 -999 4 0 2 1 ]{11: ADD} //We then add the base pointer and offset to get the index of our new value 0 which was added to the stack earlier
[ 5 -999 4 0 3 ]{12: CSTI 0} //we then want to add 0 to the index
[ 5 -999 4 0 3 0 ]{14: STI} //we then store 0 into [3] which in this case is the value of 0 we added earlier, yielding the following stack after:
[ 5 -999 4 0 0 ]{15: INCSP -1}
[ 5 -999 4 0 ] //where 0 is the value of i as of this current moment
```
this example is what the micro c program shows as int i; and i = 0;
```fsharp
Another example:
[ 5 -999 4 0 ]{44: GETBP}
[ 5 -999 4 0 2 ]{45: CSTI 1}
[ 5 -999 4 0 2 1 ]{47: ADD}
[ 5 -999 4 0 3 ]{48: LDI} //(get the value at index 3 which is 0 as can be seen on the next step where 3 is replaced with 0)
[ 5 -999 4 0 0 ]{49: GETBP}
[ 5 -999 4 0 0 2 ]{50: CSTI 0}
[ 5 -999 4 0 0 2 0 ]{52: ADD}
[ 5 -999 4 0 0 2 ]{53: LDI} //(here again we take the index of now 2 which has the value of 4, so we push the value instead onto the stack)
[ 5 -999 4 0 0 4 ]{54: LT} //we use the LT on 0 and 4 which produces a flag of 0 or 1 aka true or false, which gets checked in the next step:
[ 5 -999 4 0 1 ]{55: IFNZRO 19} //if it is not zero then continue to step 19, otherwise itll go to next step which would be 57, this indicates the while loop of the program. this is the check to see if the while loop should continue or not
```
this example is what the micro c program shows as while (i < n)

The rest of the execution follows the pattern as described above
