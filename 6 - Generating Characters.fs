module Mavnn.MythMaker.MythProvider6

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open Mavnn.MythMaker.Parsing

type Character() = class end

[<TypeProvider>]
type MythProvider6() as this =
    inherit TypeProviderForNamespaces()

    let ns = "Mavnn.MythMaker.MythTypes6"
    let asm = Assembly.GetExecutingAssembly()

    let props value =
        ProvidedProperty(
            value,
            typeof<string>,
            IsStatic = true,
            GetterCode = fun _ -> <@@ value @@>)

    let mapToChar char =
        let name =
            match Map.tryFind "Name" char with
            | Some (n::_) -> n
            | _ -> sprintf "%A" <| System.Guid.NewGuid()
        let subTypes =
            Map.toList char
            |> List.map (fun (key, values) ->
                            let t = ProvidedTypeDefinition(key, Some typeof<obj>)
                            t.HideObjectMethods <- true
                            t.AddMembers <| List.map props values
                            t)
        let charType = ProvidedTypeDefinition(name, Some typeof<Character>)
        charType.AddMembers subTypes
        charType

    let createType typeName (args : obj []) =
        let myString = args.[0] :?> string

        let charsData = parse myString

        let charsTypes =
            List.map mapToChar charsData

        let myType =
            ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
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

open Mavnn.MythMaker.MythTypes6

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

Norse.BALDUR.``Alternative names``.BALDER

Norse.ODIN.``Name means``.``All Father``

*)
