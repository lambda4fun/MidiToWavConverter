module MidiToWavConverter

open System
open Eto.Forms

type MainForm () as this =
    inherit Form (Width = 640, Height = 480, Title = "Hello World")

    do
        this.Content <- new Button (Text = "Click me!")

[<EntryPoint; STAThread>]
let main _ =
    let app = new Application ()
    let form = new MainForm ()
    app.Run form
    0
