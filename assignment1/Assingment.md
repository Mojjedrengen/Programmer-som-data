# 1.1

```fsharp
(* Taken from the code *)
let env = [ ("a", 3); ("c", 78); ("baf", 666); ("b", 111) ]

let rec lookup env x =
    match env with
    | [] -> failwith (x + " not found")
    | (y, v) :: r -> if x = y then v else lookup r x

```

```fsharp
type expr =
    | CstI of int
    | Var of string
    | Prim of string * expr * expr
    | If of expr * expr * expr (* <- Added *)
```

```fsharp
(* Evaluation within an environment *)

let rec eval e (env: (string * int) list) : int =
    match e with
    | CstI i -> i
    | Var x -> lookup env x
    | Prim(ope, e1, e2) ->
        let i1 = eval e1 env
        let i2 = eval e2 env

        match ope with
        | "+" -> i1 + i2
        | "*" -> i1 * i2
        | "-" -> i1 - i2
        | "max" -> max i1 i2
        | "min" -> min i1 i2
        | "==" -> if i1 = i2 then 1 else 0
        | _ -> failwith "unknown primitive"
    | If(e1, e2, e3) -> if (eval e1 env) <> 0 then (eval e2 env) else (eval e3 env)
```

```fsharp
(* Examples *)
let my_e1 = Prim("max", CstI 3, Var "c")
let my_e2 = Prim("min", CstI 3, Var "c")
let my_e3 = Prim("==", CstI 3, Var "c")
let my_e4 = Prim("==", CstI 3, Var "a")
let my_e5 = If(Var "a", CstI 11, CstI 22)
let my_e1v = eval my_e1 env
let my_e2v = eval my_e2 env
let my_e3v = eval my_e3 env
let my_e4v = eval my_e4 env
let my_e5v = eval my_e5 env
```

# 1.2

```fsharp
type aexpr =
    | CastI of int
    | Var of string
    | Add of aexpr * aexpr
    | Mul of aexpr * aexpr
    | Sub of aexpr * aexpr
```

```fsharp
let rec fmt =
    function
    | CastI(e) -> string e
    | Var(e) -> e
    | Add(e1, e2) -> "(" + fmt e1 + " + " + fmt e2 + ")"
    | Mul(e1, e2) -> "(" + fmt e1 + " * " + fmt e2 + ")"
    | Sub(e1, e2) -> "(" + fmt e1 + " - " + fmt e2 + ")"
```

```fsharp
let rec simplify =
    function
    | CastI(e) -> CastI e
    | Var(e) -> Var e
    | Add(e1, e2) when e1 = CastI 0 -> e2
    | Add(e1, e2) when e2 = CastI 0 -> e1
    | Sub(e1, e2) when e2 = CastI 0 -> e1
    | Mul(e1, e2) when e1 = CastI 1 -> e2
    | Mul(e1, e2) when e2 = CastI 1 -> e1
    | Mul(e1, e2) when e1 = CastI 0 || e2 = CastI 0 -> CastI 0
    | Sub(e1, e2) when e1 = e2 -> CastI 0
    | Add(e1, e2) -> Add(e1, e2)
    | Sub(e1, e2) -> Sub(e1, e2)
    | Mul(e1, e2) -> Mul(e1, e2)
```

```fsharp
let ae1 = Sub(Var "v", Add(Var "w", Var "z"))
let ae2 = Mul(CastI 2, Sub(Var "v", Add(Var "w", Var "z")))
let ae3 = Add(Var "x", Add(Var "y", Add(Var "z", Var "v")))

let ae1fmt = fmt ae1
let ae2fmt = fmt ae2
let ae3fmt = fmt ae3

let ae4 = Mul(CastI 0, Var "b")
let ae4s = simplify ae4
```
