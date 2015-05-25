module Mavnn.MythMaker.MythProvider2

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type MythProvider2() as this =
    inherit TypeProviderForNamespaces()

    let ns = "Mavnn.MythMaker.MythTypes2"
    let asm = Assembly.GetExecutingAssembly()

    let myProp =
        ProvidedProperty(
            "StaticProperty",
            typeof<string>, 
            IsStatic = true,
            GetterCode = fun _ -> <@@ "Hello world" @@>)

    let myType = ProvidedTypeDefinition(asm, ns, "StaticType", Some typeof<obj>)

    do
        myType.AddMember(myProp)
        this.AddNamespace(ns, [myType])

(*
#r @"Mavnn.MythMaker/bin/Debug/Mavnn.MythMaker.dll"

open Mavnn.MythMaker.MythTypes2

StaticType.StaticProperty
*)