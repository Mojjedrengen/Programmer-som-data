
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