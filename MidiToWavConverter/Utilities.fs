module Utilities

open System.IO

let inline (^) f x = f x

module String =
    let isEmptyOrSpaces (s : string) =
        s.Trim().Length = 0

    let split (separators : char[]) (s : string) = s.Split separators

module File =
    let existsInPath fileName =
        System.Environment.GetEnvironmentVariable "PATH"
        |> String.split [|':'; ';'|]
        |> Seq.exists ^ fun path ->
            Path.Combine (path, fileName)
            |> File.Exists
