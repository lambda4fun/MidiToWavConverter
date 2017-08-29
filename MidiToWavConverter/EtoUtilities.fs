module EtoUtilities

open Eto.Drawing
open Eto.Forms
open Utilities

[<AbstractClass>]
type PathField (labelText) as this =
    inherit Panel ()

    let browse () =
        this.Browse ()
        |> Option.iter ^ fun path ->
            this.Path <- Some path

    let label = new Label (Text = labelText + ":")
    let textBox = new TextBox ()

    let browseButton = new Button ((fun _ _ -> browse ()), Text = "Browse...")

    do
        this.Content <- new TableLayout (TableRow (TableCell label),
                                         TableRow (TableCell (textBox, scaleWidth = true), TableCell browseButton),
                                         Spacing = Size (8, 6))

    member __.Path
        with get () =
            match textBox.Text with
            | text when text.Trim().Length > 0 ->
                Some text
            | _ ->
                None
        and set x =
            textBox.Text <- x |> Option.defaultValue ""

    abstract member Browse : unit -> string option

type OpenFileField (labelText, fileDialogFilters) as this =
    inherit PathField (labelText)

    override __.Browse () =
        let dialog = new OpenFileDialog (CheckFileExists = true)
        fileDialogFilters |> Seq.iter dialog.Filters.Add
        match dialog.ShowDialog this with
        | DialogResult.Ok ->
            Some dialog.FileName
        | _ ->
            None

type OpenFolderField (labelText) as this =
    inherit PathField (labelText)

    override __.Browse () =
        let dialog = new SelectFolderDialog ()
        match dialog.ShowDialog this with
        | DialogResult.Ok ->
            Some dialog.Directory
        | _ ->
            None
