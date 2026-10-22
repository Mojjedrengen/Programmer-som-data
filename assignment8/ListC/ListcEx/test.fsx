// Script to compile and run example programs with list-C.

// Script expects started from the ListC/ListcEx directory, e.g.:
//   dotnet fsi test.fsx

#load "cmd.fsx"
open Cmd
open System.IO
open System

let nl = System.Environment.NewLine
let mutable verbose = true

let println s = if verbose then printfn "%s" s else ()

// Make sure current directory is ListC required to use dotnet run.
let testdir = "ListcEx"

// microVM executable depends on OS
let microvm =
  if System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) then
    "microvm.exe -silent"
  else
    "microvm -silent" 

let setCurDir() =
  System.Environment.CurrentDirectory <- "..";

setCurDir()

if Path.GetFileName(System.Environment.CurrentDirectory) <> "ListC"
  then failwith "test.fsx: Not in ListC folder - need to load test.fsx from ListcEx folder"

// Delete *.out files
let delOut() = runCmd ("rm -f " + Path.Combine(testdir,"*.out"))
delOut();

// Compile with list-C
let compListC f = runCmd ("dotnet run " + Path.Combine (testdir,f))

// Run list-C program, that is *.out file, using the microVM machine.
let microVM (f:string) arg =
  let stem = if f.EndsWith(".lc")
               then f.Substring(0,f.Length-3)
               else if f.EndsWith(".c")
                      then f.Substring(0,f.Length-2)
                      else f
  let target = stem + ".out"
  runCmd(Path.Combine ([|"..";"MicroVM";microvm|]) + " " + Path.Combine(testdir,target) + " " + arg)

// Run a program f with arguments.
// Assumes programs are already compiled.
// Compares output.
let runOne f arg (expected:string) =
  let (out,err,code) = microVM f arg
  println ("  list-C out: [" + out + "]")
  let ok = if out.Trim() <> expected.Trim() then "ERROR" else "OK"
  if err <> ""
    then println ("    Error " + err)
    else println out
  (f,ok,out,err,code)

// Compile and run program f with given arguments cs.
let test (f,arg,expected) =
  printfn "Compiling file %s" f;
  let (out, err, code) = compListC f
  if err <> "" 
    then println ("with error " + err)
    else println "";
  runOne f arg expected    

// Tests that succeed with list-C
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
             ("ex21.c","","117");
             ("ex22.c","10","");
             ("ex23.c","","2 3 8 6 7 11 22 8 2 3 22 11");
             ("ex24.c","","666");
             ("ex25.c","","5 10345");
             ("ex26.c","10","10 4");
             ("ex27.c","","4 " + nl + "42 42 " + nl + "44 2 47 47");
             ("ex30.lc","10","10 9 8 7 6 5 4 3 2 1");
             ("ex31.lc","10","1 2 3 4 5 6 7 8 9 10");             
             ("ex33.lc","10","1 2 3 4 5 6 7 8 9 10 " + nl + "10 9 8 7 6 5 4 3 2 1 " + nl + "55 110 ");
             ("ex34.lc","10","11 33");             
             ("ex35.lc","10","33 33 44 44");
             ("ex36.lc","10","1 1");
             ("ex37.lc","10","9 8 7 6 5 4 3 2 1 0");
             ("exGC01.lc","","42 9999");
             ]
             
// Compile and run tests with both Java SE 25 and micro-Java.
let listcRes = List.map test tests

verbose <- true
println "Result of test:"
List.map (fun (f,ok,_,_,_) -> println (f + ": " + ok)) listcRes



