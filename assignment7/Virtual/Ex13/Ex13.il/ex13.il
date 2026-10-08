// IL code: ex13
.class private auto ansi '<Module>'
{
} // end of class <Module>

.class private auto ansi beforefieldinit ex13
	extends [System.Runtime]System.Object
{
	// Methods
	.method private hidebysig static 
		void Main (
			string[] args
		) cil managed 
	{
		.custom instance void [System.Runtime]System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = (
			01 00 01 00 00
		)
		// Method begins at RVA 0x2050
		// Header size: 12
		// Code size: 59 (0x3b)
		.maxstack 2
		.entrypoint
		.locals init (
			[0] int32,
			[1] int32
		)

		IL_0000: ldarg.0
		IL_0001: ldc.i4.0
		IL_0002: ldelem.ref
		IL_0003: call int32 [System.Runtime]System.Int32::Parse(string)
		IL_0008: stloc.0
		IL_0009: ldc.i4 1889
		IL_000e: stloc.1
		IL_000f: br.s IL_002f
		// loop start (head: IL_002f)
			IL_0011: ldloc.1
			IL_0012: ldc.i4.1
			IL_0013: add
			IL_0014: stloc.1
			IL_0015: ldloc.1
			IL_0016: ldc.i4.4
			IL_0017: rem
			IL_0018: brtrue.s IL_002f

			IL_001a: ldloc.1
			IL_001b: ldc.i4.s 100
			IL_001d: rem
			IL_001e: brtrue.s IL_0029

			IL_0020: ldloc.1
			IL_0021: ldc.i4 400
			IL_0026: rem
			IL_0027: brtrue.s IL_002f

			IL_0029: ldloc.1
			IL_002a: call void InOut::PrintI(int32)

			IL_002f: ldloc.1
			IL_0030: ldloc.0
			IL_0031: blt.s IL_0011
		// end loop

		IL_0033: ldc.i4.s 10
		IL_0035: call void InOut::PrintC(int32)
		IL_003a: ret
	} // end of method ex13::Main

	.method public hidebysig specialname rtspecialname 
		instance void .ctor () cil managed 
	{
		// Method begins at RVA 0x2097
		// Header size: 1
		// Code size: 7 (0x7)
		.maxstack 8

		IL_0000: ldarg.0
		IL_0001: call instance void [System.Runtime]System.Object::.ctor()
		IL_0006: ret
	} // end of method ex13::.ctor

} // end of class ex13

.class private auto ansi beforefieldinit InOut
	extends [System.Runtime]System.Object
{
	// Methods
	.method public hidebysig static 
		void PrintI (
			int32 i
		) cil managed 
	{
		// Method begins at RVA 0x209f
		// Header size: 1
		// Code size: 23 (0x17)
		.maxstack 8

		IL_0000: ldarga.s i
		IL_0002: call instance string [System.Runtime]System.Int32::ToString()
		IL_0007: ldstr " "
		IL_000c: call string [System.Runtime]System.String::Concat(string, string)
		IL_0011: call void [System.Console]System.Console::Write(string)
		IL_0016: ret
	} // end of method InOut::PrintI

	.method public hidebysig static 
		void PrintC (
			int32 c
		) cil managed 
	{
		// Method begins at RVA 0x20b7
		// Header size: 1
		// Code size: 8 (0x8)
		.maxstack 8

		IL_0000: ldarg.0
		IL_0001: conv.u2
		IL_0002: call void [System.Console]System.Console::Write(char)
		IL_0007: ret
	} // end of method InOut::PrintC

	.method public hidebysig specialname rtspecialname 
		instance void .ctor () cil managed 
	{
		// Method begins at RVA 0x20c0
		// Header size: 1
		// Code size: 7 (0x7)
		.maxstack 8

		IL_0000: ldarg.0
		IL_0001: call instance void [System.Runtime]System.Object::.ctor()
		IL_0006: ret
	} // end of method InOut::.ctor

} // end of class InOut

