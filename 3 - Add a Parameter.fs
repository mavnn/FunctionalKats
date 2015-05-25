module Mavnn.MythMaker.MythProvider3

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type MythProvider3() as this =
    inherit TypeProviderForNamespaces()

    let ns = "Mavnn.MythMaker.MythTypes3"
    let asm = Assembly.GetExecutingAssembly()

    let createType typeName (args : obj []) =
        let myString = args.[0] :?> string
        let myProp =
            ProvidedProperty(
                "StaticProperty",
                typeof<string>, 
                IsStatic = true,
                GetterCode = fun _ -> <@@ myString @@>)
        let myType = 
            ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
        myType.AddMember myProp
        myType

    let provider = ProvidedTypeDefinition(asm, ns, "MythProvider", Some typeof<obj>)
    let parameters =
        [ProvidedStaticParameter("AString", typeof<string>)]

    do
        provider.DefineStaticParameters(parameters, createType)
        this.AddNamespace(ns, [provider])

(*
#r @"Mavnn.MythMaker/bin/Debug/Mavnn.MythMaker.dll"

open Mavnn.MythMaker.MythTypes3

type OurProvidedTypeWithParameter = MythProvider<"A string here">

OurProvidedTypeWithParameter.StaticProperty
*)