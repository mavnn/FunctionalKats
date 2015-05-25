module Mavnn.MythMaker.Parsing

(*

So, our data is going to look like:

```
Name : ODIN
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
```

So we need: before and after ":", and comma based lists. Let's see what we can do.

*)

type State =
    {
        CharsInLine : char list
        Lines : string list
        Current : Map<string, string list>
        Data : Map<string, string list> list
    }

let (!!) chars =
    chars
    |> List.rev
    |> List.map string
    |> String.concat ""
    |> fun s -> s.Trim()

let (<!) map (key, value) =
    match Map.tryFind key map with
    | Some v ->
        Map.add key (value::v) map
    | None ->
        Map.add key [value] map

let rec nameParser state acc =
    match state with
    | { CharsInLine = '-'::'-'::'-'::'-'::'-'::_; Lines = h::t } ->
        nameParser { state with
                        CharsInLine = h |> List.ofSeq
                        Current = Map.empty
                        Lines = t
                        Data = state.Current::state.Data } []
    | { CharsInLine = '-'::'-'::'-'::'-'::'-'::_; Lines = [] } ->
        state.Current::state.Data
    | { CharsInLine = ':'::t } ->
        valueParser { state with CharsInLine = t } (!!acc) []
    | { CharsInLine = c::t } ->
        nameParser { state with CharsInLine = t } (c::acc)
    | { CharsInLine = [] } ->
        failwith "Name with no value :("

and valueParser state name acc =
    match state with
    | { CharsInLine = ','::t } ->
        valueParser { state with CharsInLine = t; Current = state.Current <! (name, !!acc) } name []
    | { CharsInLine = c::t } ->
        valueParser { state with CharsInLine = t } name (c::acc)
    | { CharsInLine = []; Lines = [] } ->
        (state.Current <! (name, !!acc))::state.Data
    | { CharsInLine = []; Lines = h::t } ->
        nameParser { state with
                        Current = state.Current <! (name, !!acc)
                        CharsInLine = h |> List.ofSeq
                        Lines = t } []

let parse (str : string) =
    match str.Split('\n') |> Seq.toList with
    | h::t ->
        nameParser { CharsInLine = h |> List.ofSeq; Lines = t; Current = Map.empty; Data = [] } []
    | [] ->
        failwith "Cannot parse empty string"
    
(*
#r @"Mavnn.MythMaker/bin/Debug/Mavnn.MythMaker.dll"

open Mavnn.MythMaker.Parsing

parse """Name : ODIN
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
Alternative names : BALDER, BALDR"""

*)