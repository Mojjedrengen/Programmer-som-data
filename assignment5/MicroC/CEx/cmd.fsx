// Run shell command across windows, linux and macos.
module Cmd
open System
open System.Diagnostics
open System.Runtime.InteropServices



let runCmd cmd =
  let isWin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
  let (shell, args) =
    if isWin
      then "cmd.exe", "/c " + cmd
      else "/bin/bash", "-c \"" + cmd + "\""

  let psi = ProcessStartInfo()
  psi.FileName <- shell
  psi.Arguments <- args
  psi.UseShellExecute <- false
  psi.RedirectStandardOutput <- true
  psi.RedirectStandardError <- true

  use p = Process.Start(psi)
  let output = p.StandardOutput.ReadToEnd()
  let error  = p.StandardError.ReadToEnd()
  p.WaitForExit()
  (output, error, p.ExitCode)
