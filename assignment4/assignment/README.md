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



### 6.3