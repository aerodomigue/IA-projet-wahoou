using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {
	
	public GameObject objectPrefab;			//Prefab de notre chat
	
	private bool isTraning = false;
	public int populationSize = 10;			//Nombre d'agent a faire spawner. Gardez un multiple de 4 svp
    private int popAct = 0;
	public float timer = 15f;				//Temps par generation	
    private float timerBack;
    public float TimeRatio = 1;
    public float RatioGenie = 0.1f;
    public float RatioeNorm = 0.9f;
    public float RatioBad = 0f;
    public float RatioNoBrain = 0f;
    public bool NoTimer = false;
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;
    private int generationNumber = 0;		//Numero de la generation actuelle
	
	private List<NeuralNetwork> nets;		//Liste de neural networks de generation x
	public List<GameObject> objectList = null;	//Liste des chats de notre generation

	
	private int[] layers = new int[] { 9, 7, 4, 2 }; //Dimension de nos reseaux de neurones
	
	private float fit=0;					//On calculera la moyenne de fitnes de la generation grace a cette variable

    private void Start()
    {
        timerBack = timer;
    }

    // Met fin a la generation actuelle (cheh)
    void Timer()
	{
        if (!NoTimer)
        {
            isTraning = false;
            popAct = 0;
        }

    }

    void Update()
    {
        if(Time.timeScale != TimeRatio)
        {
            Time.timeScale = TimeRatio;
            //timerBack = TimeRatio * timer;
        }
        //Changement de generation
        if (isTraning == false)
        {
            // Si c'est la premiere generation, instancie les objets
            if (generationNumber == 0)
            {
                InitCatNeuralNetworks();
                CreateObjectBodies();
            }
            else
            {
                //Transfer le score de fitness du controleur vers le reseau de neurones
                for (int i = 0; i < populationSize; i++)
                {
                    NNController script = objectList[i].GetComponent<NNController>();
                    float fitness = script.fitness;
                    nets[i].SetFitness(fitness);
                }

                //Trie les agents pour ne garder que les plus performants
                nets.Sort();
                nets.Reverse();

                //Affiche la moyenne de fitness de la generation
                fit = 0;
                for (int i = 0; i < populationSize; i++)
                {
                    fit += nets[i].GetFitness();
                }
                fit /= populationSize;
                Debug.Log("Average fitness: " + fit + "Generation: " + generationNumber);

                //Instancie la liste de la generation suivante
                List<NeuralNetwork> newNets = new List<NeuralNetwork>();

                //Recupere les plus intelligent de nos objets
                for (int i = 0; i < populationSize * RatioGenie && popAct < populationSize; i++)
                {
                    NeuralNetwork net = new NeuralNetwork(nets[i]);
                    newNets.Add(net);
                    popAct++;
                }

                //Recupere les plus intelligent de nos objets et les fait  muter
                for (int i = 0; i < populationSize * RatioeNorm && popAct < populationSize; i++)
                {
                    NeuralNetwork net = new NeuralNetwork(nets[i]);
                    net.Mutate(0.25f);
                    newNets.Add(net);
                    popAct++;
                }

                //Recupere les plus intelligent de nos objetset les fait plus muter
                for (int i = 0; i < populationSize * RatioBad && popAct < populationSize; i++)
                {
                    NeuralNetwork net = new NeuralNetwork(nets[i]);
                    net.Mutate(0.5f);
                    newNets.Add(net);
                    popAct++;
                }

                //Recupere les plus intelligent de nos objets et leurs defonce le cerveau
                for (int i = 0; i < populationSize * RatioNoBrain && popAct < populationSize; i++)
                {
                    NeuralNetwork net = new NeuralNetwork(nets[i]);
                    net.Mutate(4f);
                    newNets.Add(net);
                    popAct++;
                }

                for (int i = 0; popAct < populationSize; i++)
                {
                    NeuralNetwork net = new NeuralNetwork(nets[i]);
                    net.Mutate(8f);
                    newNets.Add(net);
                    popAct++;
                }

                //Changement d'agents entre les deux generation
                nets = newNets;

            }

            //A la fin du decompte du timer, passe a la generation suivante
            generationNumber++;
            Invoke("Timer", timerBack);
            CreateObjectBodies();


            isTraning = true;
        }

        //------------------FEEDFORWARD
        //Transfer les informations du NNController vers l'input layer du reseau de neurones
        for (int i = 0; i < populationSize; i++)
        {
            NNController script = objectList[i].GetComponent<NNController>();

            float[] result;
            float vel = script.GetVelocity();
            float distForward = script.distForward;
            float distBackward = script.distBackward;
            float distLeft = script.distLeft;
            float distRight = script.distRight;
            float distDiagLeft = script.distDiagLeft;
            float distDiagRight = script.distDiagRight;
            float distBackDiagLeft = script.distBackDiagLeft;
            float distBackDiagRight = script.distBackDiagRight;

            float[] tInputs = new float[] { vel, distForward, distBackward, distLeft, distRight, distDiagLeft, distDiagRight, distBackDiagLeft, distBackDiagRight };
            result = nets[i].FeedForward(tInputs);
            script.results = result;//Envoie le resultat au controleur

        }

        //Les generation ne meurent pas assez vite

        bool end = true;
        for (int i = 0; i < populationSize; i++)
        {
            if (objectList[i].GetComponent<NNController>().active)
                end = false;
        }

        if (end || Input.GetKeyDown(KeyCode.Space))
        {
            CancelInvoke("Timer");
            Timer();
        }
    }

	//Instancie tout nos objets
	private void CreateObjectBodies()
	{
		//Detruit tout nos objets (muahaha)
		for (int i = 0; i < objectList.Count; i++)
		{
			Destroy(objectList[i]);
		}
			
		//Recree une generation de objets (mooooooh)
		objectList = new List<GameObject>();
		for (int i = 0; i < populationSize; i++)
		{
			GameObject cat = Instantiate(objectPrefab, spawnPosition, spawnRotation);
			objectList.Add(cat);
			objectList[i] = cat;
		}
	}	

	// Initialise notre liste d'agent
	void InitCatNeuralNetworks()
	{
		nets = new List<NeuralNetwork>();
		
		for (int i = 0; i < populationSize; i++)
		{
			NeuralNetwork net = new NeuralNetwork(layers);
			net.Mutate(0.5f);
			nets.Add(net);
		}
	}
}
