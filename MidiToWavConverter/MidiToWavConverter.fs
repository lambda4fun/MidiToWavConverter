module MidiToWavConverter

open System
open System.Configuration
open System.IO
open Eto.Drawing
open Eto.Forms
open Convert
open Utilities

type Options () =
    let config = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None)

    member __.TimidityCommand
        with get () =
            config.AppSettings.Settings.["TimidityCommand"].Value
        and  set x =
            config.AppSettings.Settings.["TimidityCommand"].Value <- x
            config.Save ()

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
                                         Spacing = Size (8, 2))

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

type OptionsDialog () as this =
    inherit Dialog<DialogResult> (Title = "Options")

    let openFileField =
        let filters = [FileDialogFilter ("All Files", ".*")
                       FileDialogFilter ("EXE Files", ".exe")]
        new OpenFileField ("Timidity Command",
                           filters,
                           Path = Some ^ Options().TimidityCommand,
                           Width = 480)

    let hintLabel = new Label (Text = "You can specify a command in PATH, an absolute path, or a path relative to this program.")

    let ok () =
        Options( ).TimidityCommand <- openFileField.Path |> Option.defaultValue ""
        this.Result <- DialogResult.Ok
        this.Close ()

    let cancel () =
        this.Result <- DialogResult.Cancel
        this.Close ()

    let okButton = new Button ((fun _ _ -> ok ()), Text = "OK")

    let cancelButton = new Button ((fun _ _ -> cancel ()), Text = "Cancel")

    let bottomRow =
        let row = TableRow (TableCell (null, scaleWidth = true),
                            TableCell cancelButton,
                            TableCell okButton)
        new TableLayout (row, Spacing = Size (8, 8))

    do
        let table = new TableLayout (Spacing = Size (8, 8), Padding = Padding 16)
        table.Rows.Add ^ TableRow (TableCell openFileField)
        table.Rows.Add ^ TableRow (TableCell hintLabel)
        table.Rows.Add ^ TableRow (TableCell bottomRow)
        this.Content <- table

        this.KeyDown.Add ^ fun e ->
            if e.Key = Keys.Escape then
                cancel ()

type MainForm () as this =
    inherit Form (Title = "MIDI to WAVE Converter",
                  Resizable = false,
                  Maximizable = false)

    let inputFileField =
        let filters = [FileDialogFilter ("MIDI Files", ".mid", ".midi")
                       FileDialogFilter ("All Files", ".*")]
        new OpenFileField ("Input File", filters, Width = 480)

    // TODO: Add a button to open folder
    //       Application.Instance.Open "/home" // This must be an absolute path
    let outputFolderField = new OpenFolderField ("Output Folder", Width = 480)

    let getConvertParams () =
        try
            // TODO: Use monad
            // TODO: Use record
            let mutable timidityCommand = Options().TimidityCommand
            if String.isEmptyOrSpaces timidityCommand then failwith "Please enter a Timidity command in Options dialog."

            let midiPath = inputFileField.Path |> Option.defaultValue ""
            if String.isEmptyOrSpaces midiPath then failwith "Please enter input file."
            if not ^ File.Exists midiPath then failwith "Input file does not exist."

            let outputFolder = outputFolderField.Path |> Option.defaultValue ""
            if String.isEmptyOrSpaces outputFolder then failwith "Please enter output folder."
            if not ^ Directory.Exists outputFolder then failwith "Output folder does not exist."

            let wavFileName = Path.ChangeExtension (Path.GetFileName(midiPath), "wav")
            let wavPath = Path.Combine (outputFolder, wavFileName)

            Some (timidityCommand, midiPath, wavPath)
        with e ->
            MessageBox.Show (e.Message, "Error", MessageBoxType.Error) |> ignore
            None

    let convert () =
        getConvertParams ()
        |> Option.filter ^ fun (_, _, wavPath) ->
            if File.Exists wavPath then
                let message = sprintf """A file named "%s" already exists. The file will be replaced/overwritten.""" ^ Path.GetFileName wavPath
                match MessageBox.Show (message, this.Title, MessageBoxButtons.OKCancel, MessageBoxType.Question) with
                | DialogResult.Ok -> true
                | _ -> false
            else
                true
        |> Option.iter ^ fun (timidityCommand, midiPath, wavPath) ->
            try
                convertMidiToWave timidityCommand midiPath wavPath
            with e ->
                MessageBox.Show (e.Message, "Error", MessageBoxType.Error) |> ignore

    let bottomRow =
        let buttonImagePosition = ButtonImagePosition.Above

        let optionsButton = new Button (Text = "Options",
                                        Image = Bitmap.FromResource "options.png",
                                        ImagePosition = buttonImagePosition)
        optionsButton.Click.Add ^ fun _ ->
            let dialog = new OptionsDialog ()
            dialog.ShowModal this |> ignore
        let convertButton = new Button ((fun _ _ -> convert ()),
                                        Text = "Convert",
                                        Image = Bitmap.FromResource "convert.png",
                                        ImagePosition = buttonImagePosition)

        new TableLayout (TableRow [TableCell optionsButton; TableCell (new Panel (), scaleWidth = true); TableCell convertButton])

    do
        let layout = new TableLayout (Spacing = Size (8, 8), Padding = Padding 16)
        layout.Rows.Add ^ TableRow [TableCell inputFileField]
        layout.Rows.Add ^ TableRow [TableCell outputFolderField]
        layout.Rows.Add ^ TableRow (ScaleHeight = true)
        layout.Rows.Add ^ TableRow [TableCell bottomRow]
        this.Content <- layout

[<EntryPoint; STAThread>]
let main _ =
    let app = new Application (Eto.Platform.Detect)
    let form = new MainForm ()
    app.Run form
    0
