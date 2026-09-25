### 5.7

Extending the monomorphic type checker required an extensions of the type typ which in turn required the extension of tyexpr as well as the function typ.

We extended the tyexpr language with the ability to create EmptyLists, AddToList, Head, Tail, and IsEmpty.

Emptylist just had to have a typ, which in turn became the TypL of typ to show the type of that list.

Addtolist held to tyexpr's, one for the element to add, and one for the list to add it too.

Head, Tail, and IsEmpty all held just one tyexpr.

in the typ function, Emptylist i just returned its type wrapped in a TypL(i)

Addtolist built the types of both the list and the element and checked it was a list to begin with and that the type of the list matched the type of the element.

IsEmpty just returned a Bool type TypB is it was a list, otherwise it would fail.

Head and Tail both made sure it was a list they were dealing with and then just handed the types accordingly.

### 6.1

## Third program

```fsharp
let add x = let f y = x+y in f end
in let addtwo = add 2
in let x = 77 in addtwo 5 end
end
end
```

The result of the third one is 7 as expected.
The confusion stems from x = 77 which is added to an environment, but not the env which f's body is evaluated in.
we first make a closure of add which has an empty environment: let add x... creates the first closure
then when we say let addtwo = add 2, we call add with the
variable x set to 2 and add being the closure from before.
This runs the body of add which returns f with its own environment now holding x = 2 and wont be touched anymore, so it doesnt matter what x is set to later on as in x = 77, as it wont affect the x created earlier on in now f's environment


## Fourth program

```fsharp
let add x = let f y = x+y in f end
in add 2 end
```

The fourth program returns a closure as the result.
This is due to add 2 calling and with x = 2 and then the body of add runs defining and returning f, so nothing calls f afterwards.



### 6.2
Extended type expr with anonymous function type. Found in Absyn.fs 

```fsharp
Fun of string * expr
```
Extended type value with new closure that support the anonymous new function type.
Constructed like 
```fsharp
Closure of string*string*expr*value env
```
But without parsing of the function itself.
```fsharp
Clos of string * expr * value env
```
Eval; First thing added is when expr matches with Fun it should evaluate to Close. 
Packaging string expr and current environment together and returning it. When it then gets found in a "Call"
we have added the following for supporting the operation.
```fsharp
    | Call(eFun, eArg) -> 
      let fClosure = eval eFun env  (* Different from Fun.fs - to enable first class functions *)
      let xVal = eval eArg env
      match fClosure with
      | Clos(s,ex, aFDeclEnv) ->
        eval ex ((s,xVal) :: aFDeclEnv) 
      | Closure (f, x, fBody, fDeclEnv) ->
        let fBodyEnv = (x, xVal) :: (f, fClosure) :: fDeclEnv
        in eval fBody fBodyEnv
```
Works like Call on a identifiable function. But without adding the functions own name to the environment.

Instead only groups, binding variable to value (x , xVal) and adds it to the environment list of values. 
Then calling an evaluation on the expression from the FUN with that new environment containing the binding.

### 6.3
In FunLex.fsl adding handling of fun to keyword 
```    | "fun"   -> FUN ```
and under token adding ```  | "->"            { ARROW }```
Making the lexer handle identify fun and -> as input and giving us our wanted outputs FUN and ARROW that we use later on in the parser.


In the parser FunPar.fsy we added those to identifiers as tokens ```%token FUN ARROW```
and under expressions added ```  | FUN NAME ARROW Expr                 { Fun($2, $4)            }```
Parsing FUN NAME ARROW Expr to our previously created anonymous function type "Fun" containing location whats found in location 2 and 4 in it.
Mapping into: 
Fun(NAME, Expr)


