module Mavnn.MythMaker.MythProvider1

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

[<TypeProvider>]
type MythProvider1() = 
    inherit TypeProviderForNamespaces()


