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

# Exercose 9.1
Couldnt find anything on CSC and Ildasm on mac, but used dotnet build and ilspycmd instead.
```Powershell
dotnet build -p:Optimize=true

pwd;;  - "Virtual/Selsort" 

ilspycmd -il bin/Debug/net10.0/Selsort.dll > Selsort.il

```
### Selsort.il
```C#

	// Methods
	
	.method public hidebysig static 
		void SelectionSort (
			int32[] arr
		) cil managed 
	{
		// Method begins at RVA 0x20b8
		// Header size: 12
		// Code size: 57 (0x39)
		.maxstack 4
		.locals init (
			[0] int32, // i
			[1] int32, // least
			[2] int32, // tmp
			[3] int32 // j
		)

		IL_0000: ldc.i4.0 // Load constant int32 0. Push 0 onto stack
		IL_0001: stloc.0 // Pop 0 from stack and store into LOCal variable 0 from locals init. We see i = 0, this is likely it. 
		IL_0002: br.s IL_0032 // branch to target IL_0032, jump to 32
		// loop start (head: IL_0032)
			IL_0004: ldloc.0 // Loads i
			IL_0005: stloc.1 // Pops I value from stack and stores it in "Least"
			IL_0006: ldloc.0 // Loads i
			IL_0007: ldc.i4.1 //Push 1 onto stack. Load constant int32 1. 
			IL_0008: add // i 1 add
			IL_0009: stloc.3 // stores i 1 add into local variable 3. Being j
			IL_000a: br.s IL_001a // Branch to 001a
			// loop start (head: IL_001a)
				IL_000c: ldarg.0 // push arr onto stack
				IL_000d: ldloc.3 // push j onto stack
				IL_000e: ldelem.i4 // loads/pushes element number j from arr
				IL_000f: ldarg.0 // pushes arr onto stack
				IL_0010: ldloc.1 // pushes least onto stack
				IL_0011: ldelem.i4 // loads/pushes element number "least" from arr
				IL_0012: bge.s IL_0016 // jump to IL_0016 if arr[j] greater or equals arr[least]
                // but if arr[j] less than arr[least]
				IL_0014: ldloc.3 // push j onto stack
				IL_0015: stloc.1 // store j in least

				IL_0016: ldloc.3 // push j
				IL_0017: ldc.i4.1 // constant val 1
				IL_0018: add // j 1 add
				IL_0019: stloc.3 // stores j 1 add in j

				IL_001a: ldloc.3 // loads j
				IL_001b: ldarg.0 // loads arr
				IL_001c: ldlen // Pops arr, gets length and pushes it to the stack
				IL_001d: conv.i4 // converts length to int32
				IL_001e: blt.s IL_000c // branch to IL_000c if j < arr.length
			// end loop

			IL_0020: ldarg.0 // push arr
			IL_0021: ldloc.0 // push i
			IL_0022: ldelem.i4 // push arr[i]
			IL_0023: stloc.2 // store arr[i] in tmp
			IL_0024: ldarg.0 // push arr
			IL_0025: ldloc.0 // push i
			IL_0026: ldarg.0 // push arr
			IL_0027: ldloc.1 // push least
			IL_0028: ldelem.i4 // push arr[least]
			IL_0029: stelem.i4 // store element in arr[i], so arr[i] = arr[least]
			IL_002a: ldarg.0 // push arr
			IL_002b: ldloc.1 // push least
			IL_002c: ldloc.2 // push tmp
			IL_002d: stelem.i4 // store element in arr[least], so arr[least] = tmp
			IL_002e: ldloc.0 // Load i onto stack
			IL_002f: ldc.i4.1 // load const 1 onto stack
			IL_0030: add // add, i 1 add
			IL_0031: stloc.0 // store i 1 add in i; i = i+1

			IL_0032: ldloc.0 // Loads local variable i onto the stack.
			IL_0033: ldarg.0 // load argument 0 onto the stack. Being int[] arr
			IL_0034: ldlen // pops top arr from stack, loads length onto it
			IL_0035: conv.i4 // converts this length to i4 int32 
			IL_0036: blt.s IL_0004 // pops and compares two top value i and length with eachother, if i less than length then branch/jump to il_0004
		// end loop

		IL_0038: ret
	} // end of method Selsort::SelectionSort
```
### Selsort.jvmbytecode
```C#
public static void SelectionSort(int[]);
descriptor: ([I)V
flags: (0x0009) ACC_PUBLIC, ACC_STATIC
Code:
  stack=4, locals=4, args_size=1
     0: iconst_0 // Push const 0 onto stack
     1: istore_1 // Pop and store const 0 in "Int var 1" - in i 
     2: iload_1 // Push value of i-; load i -onto stack
     3: aload_0 // Load "array var 0" onto stack
     4: arraylength // Load arr.length onto stack. Pop arr, call length on it, push value onto stack
     5: if_icmpge     57 // Pops two top ints, being i and arr.length, compares them. If i >= arr.length then "branch/jump to 57" else ->
     8: iload_1 // load i
     9: istore_2 // pop value, store value in int var 2; least = i
    10: iload_1 // load i
    11: iconst_1 // push const 1 onto stack
    12: iadd // (val i) 1 add
    13: istore_3 // store i+1 into intvar 3, being j. So j=i+1
    14: iload_3 // load j
    15: aload_0 // load arr
    16: arraylength // get arr.length
    17: if_icmpge     37 // same as step 5; if j>=arr.Length then "Jump to 37" else ->
    20: aload_0 // load arr
    21: iload_3 // load j
    22: iaload // loads index j of arr ; arr[j]
    23: aload_0 // loads arr
    24: iload_2 // loads least
    25: iaload // arr[least]
    26: if_icmpge     31 // as 5: if arr[j]>=arr[least] then jump_31 else ->
    29: iload_3 // load j
    30: istore_2 // least = j
    31: iinc          3, 1 // increment local var 3 ( j ) with 1
    34: goto          14 // jump to 14 (new loop)
    37: aload_0 // load arr
    38: iload_1 // load i
    39: iaload // load arr[i] 
    40: istore_3 // tmp = arr[i] ; Okay so apparently j has been freed due to lack of its use outside of its forloop :) Therefore the new var 3 is tmp.
    41: aload_0 // load arr
    42: iload_1 // load i
    43: aload_0 // load arr
    44: iload_2 // load least
    45: iaload // load value arr[least]
    46: iastore // store value in arr[i] ; arr[i] = arr[least]
    47: aload_0 // load arr
    48: iload_2 // load least
    49: iload_3 // load tmp 
    50: iastore // arr[least] = tmp 
    51: iinc          1, 1 // increment var 1 = i with 1, i = i+1
    54: goto          2 // goto jump branch whatever to step 2
    57: return // return; END of function, no mas
```
# Exercise 9.3

The fix lies in the method `get()` for `SentinelLockQueue`

```java
public synchronized int get() {
    if (head.next == null)
      return -999;
    Node first = head;
    head = first.next;
    first.next = null;  // > [!NOTE]
                        // > This is change that fixes the gc.
                        // > This is due to we now removes the reference of the old node.
                        // > In the original if we had a list `a->b->c->d` where `a` was the head
                        // > If we then call get() we would still have the list `a->b->c->d` in memory but the head would be `b`.
                        // > Since the gc checks for unrefrenced variables we would have a memory leak,
                        // > since the variables never loses a reference to it.
                        // > Now that we set the field `next` the gc can deallocate the memory from the other objects in the list.
                        // > Going back to out example from before when we call get() now we have the list `b->c->d` with `b` as the head.
    return head.item;
  }
```
