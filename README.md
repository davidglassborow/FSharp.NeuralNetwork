# FSharp.NeuralNetwork [![AppVeyor build status](https://ci.appveyor.com/api/projects/status/006u68enqaw0lfoo)](https://ci.appveyor.com/project/JahTheDev/fsharp-neuralnetwork) [![Travis build status](https://travis-ci.org/JahTheDev/FSharp.NeuralNetwork.png)](https://travis-ci.org/JahTheDev/FSharp.NeuralNetwork)

FSharp.NeuralNetwork is a simple neural network framework for F#.

### Installation

Build FSharp.NeuralNetwork from the provided .sln file, `build.cmd` or `build.sh`. The solution is configured for .NET 4.6 and F# 4.0 by default.

### Getting Started

FSharp.NeuralNetwork uses [stochastic gradient descent](https://en.wikipedia.org/wiki/Stochastic_gradient_descent)-based learning. Below is an example of a neural network trained to approximate logical XOR.

```fsharp
#r "FSharp.NeuralNetwork"

open FSharp.NeuralNetwork

let hiddenLayer =
    layer [ neuron 1.5 [1.; 1.]
            neuron 0.5 [1.; 1.] ]
let outputLayer =
    layer [ neuron 0.5 [-1.; 1.] ]

let nn = network (List.zip [hiddenLayer; outputLayer] (List.replicate 2 sigmoid)) 0.25 0.8

let samples =
    [ [0.; 0.], [0.]
      [1.; 1.], [0.]
      [0.; 1.], [1.]
      [1.; 0.], [1.] ]

let trainedNetwork = train nn 10000 samples
let output = computeResult trainedNetwork [1.; 0.] |> Seq.exactlyOne
```

### License

FSharp.NeuralNetwork is available under the MIT license. For more information, see the [license file](https://github.com/JahTheDev/FSharp.NeuralNetwork/blob/master/LICENSE.md).