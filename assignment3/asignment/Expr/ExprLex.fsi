
module ExprLex
open FSharp.Text.Lexing
open ExprPar/// Rule Token
val Token: lexbuf: LexBuffer<char> -> token
