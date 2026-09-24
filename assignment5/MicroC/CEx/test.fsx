// Script to compile and run example programs with micro-C.

// You need to update project file microcc.fsproj for either forwards
// or backwards compiler. See README file.

// Remember to build the compiler before running this test suite.

// Script expects started from the MicroC/CEx directory, e.g.:
//   dotnet fsi test.fsx

#load "cmd.fsx"
open Cmd
open System.IO
open System

let nl = System.Environment.NewLine
let mutable verbose = true

let println s = if verbose then printfn "%s" s else ()

// Make sure current directory is ListC required to use dotnet run.
let testdir = "CEx"

// microVM executable depends on OS
let microvm =
  if System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) then
    "microvm.exe"
  else
    "microvm" 

let machine =
  if System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) then
    "machine.exe"
  else
    "machine" 

let setCurDir() =
  System.Environment.CurrentDirectory <- "..";

setCurDir()

if Path.GetFileName(System.Environment.CurrentDirectory) <> "MicroC"
  then failwith "test.fsx: Not in CEx folder - need to load test.fsx from CEx folder."

// Delete *.out files
let delOut() = runCmd ("rm -f " + Path.Combine(testdir,"*.out"))
delOut();

// Compile with micro-C
let compMicroC f = runCmd ("dotnet run " + Path.Combine (testdir,f) + " --project microcc.fsproj")

// Run micro-C program, that is *.out file, using the micro-C machine.
let runVM (f:string) arg =
  let stem = if f.EndsWith(".c")
               then f.Substring(0,f.Length-2)
               else f
  let target = stem + ".out"
  (runCmd(Path.Combine ([|"..";"MicroVM";microvm|]) + " -silent " + Path.Combine(testdir,target) + " " + arg),"microvm.c")

let runCmachine (f:string) arg =
  let stem = if f.EndsWith(".c")
               then f.Substring(0,f.Length-2)
               else f
  let target = stem + ".out"
  (runCmd(Path.Combine ([|".";machine|]) + " " + Path.Combine(testdir,target) + " " + arg),
   "machine.c")

let runJavaMachine (f:string) arg =
  let stem = if f.EndsWith(".c")
               then f.Substring(0,f.Length-2)
               else f
  let target = stem + ".out"
  (runCmd("java Machine " + Path.Combine [|testdir;target|] + " " + arg),
   "Machine.java")

// Run a program f with arguments.
// Assumes programs are already compiled.
// Compares output.
let runOne (fVM: string -> string -> (string*string*int)*string) f arg (expected:string) =
  let ((out,err,code),vm) = fVM f arg
  let outMatch = 
    let postfixIdx = out.IndexOf("Used")
    if postfixIdx >= 0
      then out[..postfixIdx-2]
      else out
  println ("outMatch[" + outMatch.Trim() + "] and expected [" + expected.Trim() + "]")
  let ok = if outMatch.Trim() <> expected.Trim() then "ERROR" else "OK"
  if err.Trim() <> ""
    then println ("    Error [" + err + "]")
    else println out
  (f,ok,out,err,code,vm)

// Compile and run program f with given arguments cs.
let test (f,arg,expected) =
  printfn "Compiling file %s" f;
  let (out, err, code) = compMicroC f
  if err <> "" 
    then println ("with error " + err)
    else println "";
  List.map (fun fVM -> runOne fVM f arg expected) [runVM;runCmachine;runJavaMachine]

// Tests that succeed with micro-C
let tests = [("ex01.c","10","10 9 8 7 6 5 4 3 2 1");
             ("ex02.c","","0 0 3 0 3 227 12 12 14 114 4 1 1");
             ("ex03.c","10","0 1 2 3 4 5 6 7 8 9");
             ("ex04.c","10","1 1 2 6 24 120 720 5040 40320 362880");
             ("ex05.c","10","100 10");
             ("ex06.c","10","1 1 2 6 24 120 720 5040 40320 362880 10");
             ("ex08.c","","");
             ("ex09.c","10","3628800");
             ("ex10.c","10","1 1 2 6 24 120 720 5040 40320 362880 10");             
             ("ex11.c","4","2 4 1 3 " + nl + "3 1 4 2");
             ("ex12.c","10","");
             ("ex13.c","1900","1892 1896");
             ("ex14.c","10","4");
             ("ex15.c","10","10 9 8 7 6 5 4 3 2 1 999999");
             ("ex16.c","10","2222");
             ("ex18.c","0 0","1111");
             ("ex18.c","42 0","3333");
             ("ex18.c","0 42","");
             ("ex19.c","0","33");             
             ("ex19.c","10","44");
             ("ex20.c","0 0","1111 1");
             ("ex20.c","10 0","2222 0");
             ("ex20.c","0 10","2222 0");                          
             //("ex21.c","","117"); // This test case only works with Comp.fs - not with Contcomp.fs.
             ("ex22.c","10","");
             ("ex23.c","","2 3 8 6 7 11 22 8 2 3 22 11");
             ("ex24.c","","666");
             ("ex25.c","","5 10345");
             ("ex26.c","10","10 4");
             ("ex27.c","","4 " + nl + "42 42 " + nl + "44 2 47 47");
             ]
             
// Compile and run tests with micro-C compiler and virtual machines.
let listcRes = List.concat (List.map test tests)

verbose <- true
println "Programs that succeed."
List.map (fun (f,ok,_,_,_,vm) -> println (f + "(" + vm + "): " + ok)) listcRes



