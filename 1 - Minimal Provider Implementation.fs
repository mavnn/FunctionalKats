module Mavnn.MythMaker.MythProvider1

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

[<TypeProvider>]
type MythProvider1() = 
    inherit TypeProviderForNamespaces()

// We also need to add an assembly level attribute. Check AssemblyInfo.fs