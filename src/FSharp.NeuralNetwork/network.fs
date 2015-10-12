namespace FSharp

open MathNet.Numerics.LinearAlgebra

#nowarn "25"

module NeuralNetwork =
    
    let private prepend value (vec : Vector<float>) = vector (value :: Vector.toList vec)

    let private prependForBias = prepend 1.

    let shuffleInPlace = 
        let rng = System.Random()
        fun (arr : _ []) -> 
            let swap i j = 
                let item = arr.[i]
                arr.[i] <- arr.[j]
                arr.[j] <- item
            
            let len = arr.Length
            for i = 0 to len - 2 do
                swap i (rng.Next(i, len))

    let neuron bias weights : float list = (bias :: Seq.toList weights)

    let layer (weights : seq<#seq<_>>) : float [,] = array2D weights

    let sigmoid x = 1.0 / (1.0 + exp (-x))

    type NeuronLayers = private Layers of Matrix<float> list

    type Network = 
        { Layers : NeuronLayers
          Weights : float [,] list
          Activations : (float -> float) list
          Momentum : float
          LearningRate : float
          Error : float
          Epoch : int }

    let inline private toLayers weights = Layers weights

    let inline private weightsFor network = let (Layers weights) = network.Layers in weights

    let inline private outputCount network = Seq.last (weightsFor network) |> Matrix.rowCount

    let inline private inputCount network = (weightsFor network |> List.head |> Matrix.columnCount) - 1

    let network layers momentum learningRate =
        let weights, activations = List.unzip layers
        { Layers = List.map DenseMatrix.ofArray2 weights |> toLayers
          Weights = weights
          Activations = activations
          Momentum = momentum
          LearningRate = learningRate
          Error = nan
          Epoch = 0 }
    
    let private compute weights activations input = 
        let ( *>> ) f g x = f x, g x
        let derivative eps f = fun x -> ((f (x + eps / 2.0) - f (x - eps / 2.0)) / eps)
        let prc = 1e-6

        List.zip weights activations
        |> List.scan (fun (input, _) (layer, activation) -> 
            layer * prependForBias input
            |> Vector.map activation *>> Vector.map (derivative prc activation)) (input, vector [ 0.0 ]) 
        |> List.tail

    let private errorSignals weights target layerOutputs  = 
        let (.*) (a : Vector<_>) b = a.PointwiseMultiply(b)
        let trp (layer : Matrix<_>) = layer.Transpose()

        let weights = weights |> List.tail |> List.map trp |> List.rev

        match List.rev layerOutputs with
        | (output, output') :: hiddenOutputs ->
            List.fold2 (fun acc (layer: Matrix<_>) (_, output') ->
                match acc with
                | prevError :: _ -> output' .* (layer * prevError).[1..] :: acc
            ) [output' .* (target - output) ] weights hiddenOutputs
        | [] -> []

    let private gradients weights input target layerOutputs = 
        let init l = 
            let rec loop l acc = 
                match l with
                | [ x ] -> acc
                | x :: xs -> loop xs (x :: acc)
            if List.isEmpty l then []
            else loop l []
        
        let outputs = List.unzip layerOutputs |> fst |> init
        let errors = errorSignals weights target layerOutputs

        List.map2 (fun out (err : Vector<_>) -> err.OuterProduct(prependForBias out))  (input :: outputs) errors

    let private updateWeights weights learningRate momentum prevDeltas gradients = 
        List.map3 (fun weight gradient (delta : Matrix<_>) -> 
            let deltaWeight = learningRate * gradient + (momentum * delta)
            weight + deltaWeight, deltaWeight) weights gradients prevDeltas
    
    let private error target output = 
        (target - output
         |> Vector.map (fun x -> x * x)
         |> Vector.sum)
        / 2.
    
    let computeResult network input = 
        try 
            Seq.fold(fun input (layer, activation) -> 
                Vector.map activation (layer * prependForBias input)) 
                (Seq.toList input |> vector) (List.zip (weightsFor network) network.Activations)
            |> Vector.toArray
        with 
        // bad input is a fatal exception, so we're not worried about the performance cost here
        | :? System.ArgumentException when inputCount network <> Seq.length input ->
            failwithf "Input size must be %i (given %i)." (inputCount network) (Seq.length input)
        | _ -> reraise ()

    let train (network : Network) epoches samples =
        if epoches < 1 then invalidArg "epoches" "Epoches must be a positive non-zero integer."
        let samples' = 
            let (|>>) x f = f (fst x), f (snd x)
            let inputSize, outputSize = inputCount network, outputCount network
            Seq.fold (fun acc sample -> 
                match sample |>> Seq.length with
                | inCount, outCount when inCount = inputSize && outCount = outputSize -> 
                    (sample |>> vector) :: acc
                | inCount, outCount -> 
                    failwithf "Sample size must be %ix%i (given %ix%i)." 
                        inputSize outputSize inCount outCount) [] samples
            |> Seq.toArray
        
        let initDeltas = 
            (weightsFor network)
            |> List.map (fun (layer : Matrix<_>) -> DenseMatrix.zero layer.RowCount layer.ColumnCount)
        
        let step weights prevDeltas input target = 
            (compute weights network.Activations input)
            |> gradients weights input target
            |> updateWeights weights network.LearningRate network.Momentum prevDeltas
            |> List.unzip
        
        let rec loop i weights deltas = 
            if i < epoches then 
                shuffleInPlace samples'
                let (newWeights, newDeltas) =
                    Array.fold (fun (w, d) (input, output) ->
                        step w d input output) (weights, deltas) samples'
                loop (i + 1) newWeights newDeltas
            else 
                let err = 
                    Array.fold (fun acc (input, expectedOut) -> 
                        let err = 
                            compute weights network.Activations input
                            |> Seq.last
                            |> fst
                            |> error expectedOut
                        err :: acc) [] samples'
                    |> List.average
                { network with Layers = toLayers weights
                               Weights = List.map Matrix.toArray2 weights
                               Error = err
                               Epoch = network.Epoch + epoches }
        
        loop 0 (weightsFor network) initDeltas
