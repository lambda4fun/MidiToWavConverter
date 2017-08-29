module Convert

open System.Diagnostics
open System.IO
open Utilities

/// This function won't throw exceptions.
let convertMidiToWave timidityCommand midiPath wavePath =
    match Path.getFullPath timidityCommand with
    | None ->
        Error "Could'nt get the full path of timidity"
    | Some timidityFullPath ->
        let args = sprintf """-Ow --output-16bit --output-stereo --output-file="%s" "%s" """
                           wavePath midiPath
        use p = new Process ()
        p.StartInfo.FileName <- timidityCommand
        p.StartInfo.Arguments <- args
        p.StartInfo.UseShellExecute <- false
        p.StartInfo.RedirectStandardError <- true
        p.StartInfo.CreateNoWindow <- true
        p.StartInfo.WorkingDirectory <- Path.GetDirectoryName timidityCommand

        try
            p.Start () |> ignore
            let err = p.StandardError.ReadToEnd ()
            p.WaitForExit ()

            match p.ExitCode with
            | 0 -> Ok ()
            | _ -> Error err
        with e ->
            Error (e.Message)
