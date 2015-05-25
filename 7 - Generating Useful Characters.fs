module Mavnn.MythMaker.MythProvider7

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open System
open System.Reflection
open System.ComponentModel
open Microsoft.FSharp.Reflection
open Mavnn.MythMaker.Parsing

type Character() = class end

[<TypeProvider>]
type MythProvider7() as this =
    inherit TypeProviderForNamespaces()

    let ns = "Mavnn.MythMaker.MythTypes7"
    let asm = Assembly.GetExecutingAssembly()

    let props (valueType, cstor) name values =
        let getter =
            values
            |> List.map Expr.Value
            |> fun vs -> Expr.NewObject(cstor, [Expr.NewArray(typeof<string>, vs)])

        ProvidedProperty(
            name,
            getter.Type,
            GetterCode = fun _ -> getter)

    let mapToChar valueTypes char =
        let name =
            match Map.tryFind "Name" char with
            | Some (n::_) -> n
            | _ -> sprintf "%A" <| System.Guid.NewGuid()
        let properties =
            char
            |> Map.map
                    (fun propName values ->
                         let propType = Map.find propName valueTypes
                         props propType propName values)
            |> Map.toList
            |> List.map snd
        let cstor =
            ProvidedConstructor(
              [],
              InvokeCode = fun args -> <@@ Character() @@>)
        let charType = ProvidedTypeDefinition(name, Some typeof<Character>)
        charType.AddMembers properties
        charType.AddMember cstor
        charType

    let createValueType (valueTypesHolder : ProvidedTypeDefinition) name =
        let this = ProvidedTypeDefinition(name, Some typeof<string []>)
        let cstor = ProvidedConstructor(
                        [ProvidedParameter("Value", typeof<string []>)],
                        InvokeCode = fun args ->
                            <@@ box (%%args.[0]:string []) @@>)
        this.AddMember cstor
        valueTypesHolder.AddMember this
        this, cstor

    let createType typeName (args : obj []) =
        let valueTypesHolder =
            ProvidedTypeDefinition("ValueTypes", Some typeof<obj>, HideObjectMethods = true)

        let myString = args.[0] :?> string

        let charsData = parse myString

        let allValueTypes =
            charsData
            |> List.map (fun m -> m |> Map.toSeq |> Seq.map fst)
            |> Seq.concat
            |> Seq.distinct
            |> Seq.map (fun n -> n, createValueType valueTypesHolder n)
            |> Map.ofSeq

        let charsTypes =
            List.map (mapToChar allValueTypes) charsData

        let myType =
            ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
        myType.AddMember valueTypesHolder
        myType.AddMembers charsTypes
        myType

    let provider = ProvidedTypeDefinition(asm, ns, "MythProvider", Some typeof<obj>)
    let parameters =
        [ProvidedStaticParameter("AString", typeof<string>)]

    do
        provider.DefineStaticParameters(parameters, createType)
        this.AddNamespace(ns, [provider])

(*
#r @"Mavnn.MythMaker/bin/Debug/Mavnn.MythMaker.dll"

open Mavnn.MythMaker.MythTypes7

let [<Literal>] data = """Name : ODIN
Name means : All Father
Location : Scandinavia
Gender : Male
Type : deity
In charge of : Ruling
Celebration or Feast Day : Unknown at present
Good/Evil Rating : NEUTRAL, may not care
Pronunciation : Coming soon
Alternative names : GANGLERI, ODINN, OTHINN, VAK, VALTAM
-----
Name : BALDUR
Location : Scandinavia
Gender : Male
Type : deity
In charge of : Goodness
Celebration or Feast Day : Unknown at present
Good/Evil Rating : Unknown at present
Pronunciation : Coming soon
Alternative names : BALDER, BALDR
-----"""


type Norse = MythProvider<data>

let inline isMale character =
    (^a : (member Gender : Norse.ValueTypes.Gender) character)
    :> array<string>
    |> Array.exists (fun g -> g.ToLower() = "male")

let baldur = Norse.BALDUR()

isMale baldur

let inline nameMeaning character =
    ((^a : (member ``Name means`` : Norse.ValueTypes.``Name means``) character)
     :> array<string>).[0]

nameMeaning (Norse.ODIN())

// (* Does not compile *)
// nameMeaning baldur
*)
