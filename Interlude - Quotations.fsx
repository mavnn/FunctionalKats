open Microsoft.FSharp.Quotations

let add x y = x + y

// Quotation literals
let adding = <@@ add 1 2 @@>

let addingFunc = <@@ add @@>

let addX x =
    <@@ add %%x @@>

let add5 = addX <@@ 5 @@>

let addExpression =
    addX <@@ add 5 6 @@>

// Or build using the Expr class
let five = Expr.Value 5

let var = Var("x", typeof<float>)
Expr.Lambda(var, Expr.Call(typeof<System.Math>.GetMethod("Cos"), [Expr.Var(var)]))


// Or mix and match
let var' = Var("y", typeof<int>)
Expr.Lambda(var', <@@ add (%%Expr.Var(var')) (%%Expr.Var(var')) @@>)