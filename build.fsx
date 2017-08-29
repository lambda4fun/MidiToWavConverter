// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake

// Directories
let buildDir  = "./build/"
let deployDir = "./deploy/"


// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"

// Application info
let appName = "MidiToWavConverter"
let version = "0.1.0"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
    |> Log "AppBuild-Output: "
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*")
    -- "*.zip"
    |> Zip buildDir (deployDir + appName + "." + version + ".zip")
)

Target "Run" (fun _ ->
    let exeFileName = buildDir + appName + ".exe"
    let result = ExecProcess (fun info -> info.FileName <- exeFileName) System.TimeSpan.MaxValue
    if result <> 0 then
        failwithf "Process abend with code %d" result)

// Build order
"Clean"
    ==> "Build"
    ==> "Deploy"

"Build"
    ==> "Run"

// start build
RunTargetOrDefault "Run"
