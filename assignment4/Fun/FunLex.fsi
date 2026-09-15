
module FunLex
open FSharp.Text.Lexing
open FunPar/// Rule Token
val Token: lexbuf: LexBuffer<char> -> token
/// Rule SkipComment
val SkipComment: lexbuf: LexBuffer<char> -> token
