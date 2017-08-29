module Utilities

open System.IO

let inline (^) f x = f x

module String =
    let isEmptyOrSpaces (s : string) =
        s.Trim().Length = 0

    let split (separators : char[]) (s : string) = s.Split separators

module File =
    let getFullPathIfExistsInPath fileName =
        System.Environment.GetEnvironmentVariable "PATH"
        |> String.split [|':'; ';'|]
        |> Seq.tryPick ^ fun path ->
            let fullPath = Path.Combine (path, fileName)
            if File.Exists fullPath then
                Some fullPath
            else
                None

    let existsInPath = getFullPathIfExistsInPath >> Option.isSome

module Path =
    let getFullPath fileName =
        if File.Exists fileName then
            Some ^ Path.GetFullPath fileName
        else
            File.getFullPathIfExistsInPath fileName
