namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("FSharp.NeuralNetwork")>]
[<assembly: AssemblyProductAttribute("FSharp.NeuralNetwork")>]
[<assembly: AssemblyDescriptionAttribute("An F# neural network library")>]
[<assembly: AssemblyVersionAttribute("0.0.2")>]
[<assembly: AssemblyFileVersionAttribute("0.0.2")>]
[<assembly: GuidAttribute("f503e24f-4aa6-4b98-a1c2-789fdb5f13f2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.2"
