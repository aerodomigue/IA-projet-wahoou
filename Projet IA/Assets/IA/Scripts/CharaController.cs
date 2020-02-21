using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaController : MonoBehaviour 
{
	//Parametres propre a chaque agent/au joueur
	private Transform transf; 


	
	private Vector3 movDir = Vector3.zero; 	//Vecteur de deplacement
	private float rotSpeed = 300f;			//Vitesse de rotation
	private float speed = 0.5f;				//Facteur de vitesse		
	private float initialVelocity = 0.0f;	//Velocite initiale
	private float finalVelocity  = 6f;	//Velocite finale
	private float currentVelocity  = 0.0f;	//Velocite actuelle
	private float accelerationRate = 2f;	//Vitesse d'acceleration
	private float decelerationRate = 0.8f;	//Vitesse de deceleration
	
	//Gere chacuns des rayons de detection
	public float maxDistance = 30f;
	public float distForward = 0f;
	public float distLeft = 0f;
	public float distRight = 0f;
	public float distDiagLeft = 0f;
	public float distDiagRight = 0f;
	
	//Gere le score de fitness
	public float fitness = 0f;
	private Vector3 lastPosition;	//Permet le calcul de la distance parcourue
	private float distanceTraveled;
	
	private Animator anim;

	//Recupere le schemas d'animation
	void Start(){
		anim = GetComponent<Animator>();
		lastPosition = transform.position;
	}
	 
	// Chaque frame :
	void Update () 
	{
		CharacterController controller = gameObject.GetComponent<CharacterController> ();

		//Si on appuie sur la fleche du haut, augmente la velocite. Si on appuie pas, la diminue
		if(Input.GetKey(KeyCode.UpArrow))
		{
			currentVelocity = currentVelocity + (accelerationRate * Time.deltaTime);
			anim.SetBool("isWalking", true);
		}
		else 
		{
			currentVelocity = currentVelocity - (decelerationRate * Time.deltaTime);
			anim.SetBool("isWalking", false);
		}
		currentVelocity = Mathf.Clamp(currentVelocity, initialVelocity, finalVelocity);// Clamp la velocite entre la valeur initiale et finale

		movDir = new Vector3 (0, 0, currentVelocity); //Met la velocite actuelle sous forme de vecteur
		movDir *= speed; 
		movDir = transform.TransformDirection (movDir);

		controller.Move (movDir);//Deplace le personnage
		transform.Rotate (0, Input.GetAxis ("Horizontal") * rotSpeed * Time.deltaTime, 0);//Tourne le personnage

		InteractRaycast ();// Cree les rayons
		
		//Gestion du fitness
		distanceTraveled += Vector3.Distance(transform.position, lastPosition);
		lastPosition = transform.position;
		fitness += distanceTraveled/10000;	//Augmente le fitness en fonction de la distance parcourue
		fitness -= 0.01f; 					//Decroit le fitness au long du temps
	}

	//Se declenche en entrant en collision avec un mur
    void OnTriggerEnter(Collider other)
    {
 		if(other.gameObject.tag == "checkpoint")
    	{
    		fitness += 2f;
    	}

    }

	//Gere les rayons pour detecter les murs
	void InteractRaycast()
	{
		transf = GetComponent<Transform>();
		Vector3 playerPosition = transform.position;
		
		//Cree les directions de chaque rayons
		Vector3 forwardDirection = transf.forward*10;
		Vector3 leftDirection = transf.right * -10;
		Vector3 rightDirection = transf.right*10;
		Vector3 diagLeft = transf.TransformDirection (new Vector3 (maxDistance / 5, 0f, maxDistance / 5));
		Vector3 diagRight = transf.TransformDirection(new Vector3(-maxDistance/5, 0f, maxDistance/5));
		
		//Cree les rayons
		Ray myRay = new Ray(playerPosition, forwardDirection);
		Ray leftRay = new Ray(playerPosition, leftDirection);
		Ray rightRay = new Ray(playerPosition, rightDirection);
		Ray diagLeftRay = new Ray(playerPosition, diagLeft);
		Ray diagRightRay = new Ray(playerPosition, diagRight);

		//Gere les collisions des rayons et retourne la distance si ils rencontrent quelque chose
		RaycastHit hit;
		 if (Physics.Raycast(myRay, out hit, maxDistance*10)&& hit.transform.tag == "Walls")
        {
			distForward = hit.distance;
        }
		if(Physics.Raycast(leftRay, out hit, maxDistance*10))
		{
			distLeft = hit.distance;
		}
		if(Physics.Raycast(rightRay, out hit, maxDistance*10))
		{
			distRight = hit.distance;
		}
		if(Physics.Raycast(diagLeftRay, out hit, maxDistance*10))
		{
			distDiagLeft = hit.distance;
		}
		if(Physics.Raycast(diagRightRay, out hit, maxDistance*10))
		{
			distDiagRight = hit.distance;
		}
		
		//Affiche les rayons
		Debug.DrawRay(transform.position, forwardDirection * maxDistance, Color.green);
		Debug.DrawRay(transform.position, leftDirection * maxDistance, Color.green);
		Debug.DrawRay(transform.position, rightDirection * maxDistance, Color.green);
		Debug.DrawRay(transform.position, diagLeft * maxDistance, Color.green);
		Debug.DrawRay(transform.position, diagRight * maxDistance, Color.green);
	}
}
