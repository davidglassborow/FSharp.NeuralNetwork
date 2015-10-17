namespace FSharp

open MathNet.Numerics.LinearAlgebra

/// Types and operations for artificial neural networks.
module NeuralNetwork = 

    val private prepend : value:float -> vec:Vector<float> -> Vector<float>

    val private prependForBias : (Vector<float> -> Vector<float>)

    val shuffleInPlace : ((Vector<float> * Vector<float>) [] -> unit)

    /// <summary>
    /// Creates a list of floats that is the result of `bias` prepended to `weights`.
    /// </summary>
    /// <param name="bias">The bias or threshold value.</param>
    /// <param name="weights">A sequence of input weights.</param>
    val neuron : bias:float -> weights:seq<float> -> float list

    /// <summary>
    /// Creates a 2D array from a sequence of rows representing the weights for each neuron in the layer.
    /// </summary>
    /// <param name="weights">The sequence of sequences of neuron weights.</param>
    val layer : weights:seq<#seq<float>> -> float [,]

    /// <summary>
    /// The sigmoid activation function.
    /// </summary>
    /// <param name="x">The value to transform.</param>
    val sigmoid : x:float -> float

    /// <summary>
    /// The internal representation of a neural network.
    /// </summary>
    type NeuronLayers = private | Layers of Matrix<float> list

    /// <summary>
    /// An immutable artifical neural network.
    /// </summary>
    type Network = 
        { 
          /// The internal representation of the neural network.
          Layers : NeuronLayers
          /// A list of 2D arrays containing the neuron weights the network was initialized with.
          Weights : float [,] list
          /// The activation functions for each layer in the network.
          Activations : (float -> float) list
          /// The momentum term for training the network.
          Momentum : float
          /// The learning rate.
          LearningRate : float
          /// The normalized error (sum of squared errors).
          Error : float
          /// The number of iterations the network has been trained for.
          Epoch : int }

    /// <summary>
    /// Creates a new neural network.
    /// </summary>
    /// <param name="layers">A list of tuples containing the weights and activation function for each layer.</param>
    /// <param name="momentum">The momentum term.</param>
    /// <param name="learningRate">The learning rate.</param>
    val network : layers:(float [,] * (float -> float)) list -> momentum:float -> learningRate:float -> Network

    /// <summary>
    /// Feeds `input` through the network and returns the resulting float array.
    /// </summary>
    /// <param name="network">The neural network.</param>
    /// <param name="input">The input sequence.</param>
    val computeResult : network:Network -> input:seq<float> -> float []    

    /// <summary>
    /// Returns a new network trained for `epoches` iterations given a list of training samples.
    /// </summary>
    /// <param name="network">The neural network.</param>
    /// <param name="epoches">The number of iterations to train the network for.</param>
    /// <param name="samples">A list of tuples for each input and desired output in the training set.</param>
    val train : network:Network -> epoches:int -> samples:seq<float list * float list> -> Network

    val inline private toLayers : weights:Matrix<float> list -> NeuronLayers
    val inline private weightsFor : network:Network -> Matrix<float> list
    val inline private outputCount : network:Network -> int
    val inline private inputCount : network:Network -> int

    /// feedforward: returns the list of output values and their partial derivatives for each layer in the network
    val private compute : weights:Matrix<float> list -> activations:(float -> float) list -> input:Vector<float> -> (Vector<float> * Vector<float>) list

    /// backprop 1: returns the list of error signal vectors for the hidden and output layers
    val private errorSignals : weights:#Matrix<float> list -> target:Vector<float> -> layerOutputs:(Vector<float> * #Vector<float>) list -> Vector<float> list

    /// backprop 2: returns a list of error gradient matrices for each layer in the network
    val private gradients : weights:#Matrix<float> list -> input:Vector<float> -> target:Vector<float> -> layerOutputs:(Vector<float> * #Vector<float>) list -> Matrix<float> list

    /// backprop 3: adjusts the weights of a network according to its learning rate, momentum and the error gradients
    val private updateWeights : weights:Matrix<float> list -> learningRate:float -> momentum:float -> prevDeltas:Matrix<float> list -> gradients:Matrix<float> list -> (Matrix<float> * Matrix<float>) list

    /// computes the sum of squared errors
    val private error : target:Vector<float> -> output:Vector<float> -> float
