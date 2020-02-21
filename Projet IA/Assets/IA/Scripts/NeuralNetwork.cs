using System.Collections.Generic;
using System;
using UnityEngine;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
	private int[] layers; 		//liste des layers
	private float[][] neurons; 	//matrice des neurones
	private float[][][] weights;//matrice des poids
	private float fitness; 		//fitness du reseau
	
	
	//Initialise notre reseau de neurones
	public NeuralNetwork(int[] layers)
	{
		this.layers = new int[layers.Length];
		for (int i = 0; i < layers.Length; i++)
		{
			this.layers[i] = layers[i];
		}
		
		InitNeurons();
		InitWeights();
	}
	
	//Initialise notre reseau de neurones en copiant celui qu'on donne en parametre
	public NeuralNetwork(NeuralNetwork copyNetwork)
	{
		this.layers = new int[copyNetwork.layers.Length];
		for (int i = 0; i < copyNetwork.layers.Length; i++)
		{
			this.layers[i] = copyNetwork.layers[i];
		}
		
		InitNeurons();
		InitWeights();
		CopyWeights(copyNetwork.weights);
	}
	
	//Copie tout les poids du reseau donne en parametres
	private void CopyWeights(float[][][] copyWeights)
	{
		for (int i = 0; i < weights.Length; i++)
		{
			for (int j = 0; j < weights[i].Length; j++)
			{
				for (int k = 0; k < weights[i][j].Length; k++)
				{
					weights[i][j][k] = copyWeights[i][j][k];
				}
			}
		}
	}
	
	//Cree notre matrice de neurones
	private void InitNeurons()
	{
		//Neuron Initilization
		List<float[]> neuronsList = new List<float[]>();
		
		for (int i = 0; i < layers.Length; i++) //run through all layers
		{
			neuronsList.Add(new float[layers[i]]); //add layer to neuron list
		}
		
		neurons = neuronsList.ToArray(); //convert list to array
	}
	
	//Cree notre matrices de poids
	private void InitWeights()
	{
		List<float[][]> weightsList = new List<float[][]>(); //weights list which will later will converted into a weights 3D array
		
		//Iteration sur tout les neurones ayant des connexions entrantes
		for (int i = 1; i < layers.Length; i++)
		{
			List<float[]> layerWeightsList = new List<float[]>();
			
			int neuronsInPreviousLayer = layers[i - 1]; 
			
			//Iteration sur tout les neurones du layer actuel
			for (int j = 0; j < neurons[i].Length; j++)
			{
				float[] neuronWeights = new float[neuronsInPreviousLayer]; 
				
				//Iteration sur tout les neurones du layer precedent, et set leurs poids entre -1 et 1
				for (int k = 0; k < neuronsInPreviousLayer; k++)
				{
					neuronWeights[k] = UnityEngine.Random.Range(-1f,1f);
				}
				layerWeightsList.Add(neuronWeights); 
			}
			weightsList.Add(layerWeightsList.ToArray()); //Convertis les poids de cette couche en liste 2D et l'ajoute a la liste des poids 
		}
		weights = weightsList.ToArray(); //convertis en tableau 3D
	}

	//FeedForward
	public float[] FeedForward(float[] inputs)
	{
		//Ajoute nos inputs dans la matrices des neurones d'entree
		for (int i = 0; i < inputs.Length; i++)
		{
			neurons[0][i] = inputs[i];
		}
		
		//Calculs
		//Iteration sur toute les couches, tout les neurones, puis toutes les connexions entrante et fait les calculs du feedforward
		for (int i = 1; i < layers.Length; i++)
		{
			for (int j = 0; j < neurons[i].Length; j++)
			{
				float value = 0f;
				for (int k = 0; k < neurons[i-1].Length; k++)
				{
					value += weights[i - 1][j][k] * neurons[i - 1][k]; //sum off all weights connections of this neuron weight their values in previous layer
				}
				neurons[i][j] = (float)Math.Tanh(value); //Fonction d'activation : tangente hyperbolique
			}
		}
		return neurons[neurons.Length-1]; //retourne le resultat
	}
	
	//Mutation
	public void Mutate(float condition)
	{
		//Iteration sur tout les poids du reseau
		for (int i = 0; i < weights.Length; i++)
		{
			for (int j = 0; j < weights[i].Length; j++)
			{
				for (int k = 0; k < weights[i][j].Length; k++)
				{
					
					//Un pourcent de chance de faire muter notre poids en une valeure aleatoire entre -1 et 1
					float weight = weights[i][j][k];
					float randomNumber = UnityEngine.Random.Range(0f,100f);
					if (randomNumber <= condition)
					{
						float randomNumber2 = UnityEngine.Random.Range(-1f, 1f);
						weight = randomNumber2;
					}
					weights[i][j][k] = weight;
				}
			}
		}
	}
	
	//Ajoute du fitness au reseau
	public void AddFitness(float fit)
	{
		fitness += fit;
	}
	
	//donne une valeur fixe de fitness au reseau
	public void SetFitness(float fit)
	{
		fitness = fit;
	}
	
	//Recupere le fitness du reseau
	public float GetFitness()
	{
		return fitness;
	}
	
	//Compare le fitness de deux reseaux
	public int CompareTo(NeuralNetwork other)
	{
		if (other == null) return 1;
		
		if (fitness > other.fitness)
			return 1;
		else if (fitness < other.fitness)
			return -1;
		else
			return 0;
	}
}
