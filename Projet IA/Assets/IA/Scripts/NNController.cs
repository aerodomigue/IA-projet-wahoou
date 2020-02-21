using UnityEngine;

    /// <summary>
    /// A basic gamepad implementation of the IInput interface for all the input information a kart needs.
    /// </summary>
    public class NNController : MonoBehaviour, KartGame.KartSystems.IInput
    {
        private Transform transf;
        private Rigidbody rb;
        public int layerMask = 0;

        public float minDistance = 1.10f;

        public float[] results;
        public float fitness = 0f;
        public float maxDistance = 10;
        public float distForward = 30f;
        public float distBackward = 30f;
        public float distLeft = 30f;
        public float distRight = 30f;
        public float distDiagLeft = 30f;
        public float distDiagRight = 30f;
        public float distBackDiagLeft = 30f;
        public float distBackDiagRight = 30f;
        public float currentVelocity = 0.0f;
        private Vector3 lastPosition;
        private float distanceTraveled;
        private Vector3 positionStart;
        public bool active = true;
        private float nextActionTime = 0.0f;
        public float period = 2f;
        public float vitesseMin = 6;

        public float Acceleration
        {
            get { return m_Acceleration; }
        }
        public float Steering
        {
            get { return m_Steering; }
        }
        public bool BoostPressed
        {
            get { return m_BoostPressed; }
        }
        public bool FirePressed
        {
            get { return m_FirePressed; }
        }
        public bool HopPressed
        {
            get { return m_HopPressed; }
        }
        public bool HopHeld
        {
            get { return m_HopHeld; }
        }

        float m_Acceleration;
        float m_Steering;
        bool m_HopPressed;
        bool m_HopHeld;
        bool m_BoostPressed;
        bool m_FirePressed;

        bool m_FixedUpdateHappened;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            nextActionTime += Time.time + period;
            lastPosition = transform.position;
            positionStart = transform.position;
            layerMask = LayerMask.GetMask("Ground");
            distForward = maxDistance;
            distBackward = maxDistance;
            distLeft = maxDistance;
            distRight = maxDistance;
            distDiagLeft = maxDistance;
            distDiagRight = maxDistance;
            distBackDiagLeft = maxDistance;
            distBackDiagRight = maxDistance;
        }

        void Update ()
        {
        if (active)
        {
            if (results.Length > 1)
            {
                if (results[0] < 0) // set acceleration
                {
                    m_Acceleration = results[0];
                    fitness -= 0.002f * m_Acceleration; //recompense de deceleration
                }
                else if (results[0] > 0)
                {
                    m_Acceleration = results[0];
                    fitness += 0.004f * m_Acceleration; // recompense d'acceleration
                }
                else
                    m_Acceleration = 0f;
                currentVelocity = rb.velocity.magnitude;

                m_Steering = results[1];  //set direction


                // les controle de base non utilise --
                m_HopHeld = Input.GetButton("Hop");

                if (m_FixedUpdateHappened)
                {
                    m_FixedUpdateHappened = false;

                    m_HopPressed = false;
                    m_BoostPressed = false;
                    m_FirePressed = false;
                }

                m_HopPressed |= Input.GetButtonDown("Hop");
                m_BoostPressed |= Input.GetButtonDown("Boost");
                m_FirePressed |= Input.GetButtonDown("Fire");
                //--
            }
            if (Time.time > nextActionTime) //recompense par rapport au temps
            {
                nextActionTime += period;
                if (Vector3.Distance(transform.position, positionStart) < 5f) // si je ne parcour pas asse de distance je suis disqualifier
                {
                    active = false;
                    fitness /= 3f;
                }
                if (currentVelocity < vitesseMin) // si je ne vais pas asser vite aussi discalifier
                {
                    active = false;
                    fitness /= 2f;
                }

            }
            InteractRaycast();
            if (distForward < minDistance || distBackward < minDistance || distDiagLeft < minDistance || distDiagRight < minDistance ||
                distBackDiagLeft < minDistance || distBackDiagRight < minDistance || distRight < minDistance || distLeft < minDistance)
            {
                active = false;
                fitness /= 5f;
            }
            distanceTraveled += Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
            fitness += distanceTraveled / 1000;//Augmente le fitness en fonction de la distance parcourue
            fitness -= 0.002f;
        }
        else
        {
            m_Acceleration = 0;
            m_Steering = 0;
        }
    }

        void FixedUpdate ()
        {
            m_FixedUpdateHappened = true;
        }

        public float GetVelocity()
        {
            return currentVelocity;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "checkpoint" && active)
            {
                fitness += 5f;
            }
        }

        void InteractRaycast() //les capteur autour du de la voiture il y en a 8
        {

            transf = GetComponent<Transform>();
            Vector3 playerPosition = transform.position;

            //Cree les directions de chaque rayons
            Vector3 forwardDirection = transf.forward;
            Vector3 backwardDirection = -transf.forward;
            Vector3 leftDirection = -transf.right;
            Vector3 rightDirection = transf.right;
            Vector3 diagLeft = Quaternion.Euler(0, -45, 0) * transform.forward;
            Vector3 diagRight = Quaternion.Euler(0, 45, 0) * transform.forward;
            Vector3 diagBackLeft = Quaternion.Euler(0, -45, 0) * -transform.forward;
            Vector3 diagBackRight = Quaternion.Euler(0, 45, 0) * -transform.forward;

            //Cree les rayons
            Ray myRay = new Ray(playerPosition, forwardDirection);
            Ray backRay = new Ray(playerPosition, backwardDirection);
            Ray leftRay = new Ray(playerPosition, leftDirection);
            Ray rightRay = new Ray(playerPosition, rightDirection);
            Ray diagLeftRay = new Ray(playerPosition, diagLeft);
            Ray diagRightRay = new Ray(playerPosition, diagRight);
            Ray diagBackLeftRay = new Ray(playerPosition, diagBackLeft);
            Ray diagBackRightRay = new Ray(playerPosition, diagBackRight);

            //Gere les collisions des rayons et recupere la distance si ils rencontrent quelque chose
            RaycastHit hit;
            if (Physics.Raycast(myRay, out hit, maxDistance, layerMask) && hit.transform.tag == "Road")
            {
                distForward = hit.distance;
                Debug.DrawRay(transform.position, forwardDirection* hit.distance, Color.red);
            }
            else
                Debug.DrawRay(transform.position, forwardDirection * maxDistance, Color.green);
             if (Physics.Raycast(backRay, out hit, maxDistance, layerMask) && hit.transform.tag == "Road")
            {
                distBackward = hit.distance;
                Debug.DrawRay(transform.position, backwardDirection* hit.distance, Color.red);
            }
            else
                Debug.DrawRay(transform.position, backwardDirection * maxDistance, Color.green);
            if (Physics.Raycast(leftRay, out hit, maxDistance, layerMask) && hit.transform.tag == "Road")
            {
                distLeft = hit.distance;
                Debug.DrawRay(transform.position, leftDirection * hit.distance, Color.red);
            }
            else
                Debug.DrawRay(transform.position, leftDirection * maxDistance, Color.green);
            if (Physics.Raycast(rightRay, out hit, maxDistance, layerMask) && hit.transform.tag == "Road")
            {
                distRight = hit.distance;
                Debug.DrawRay(transform.position, rightDirection * hit.distance, Color.red);
            }
            else
                Debug.DrawRay(transform.position, rightDirection * maxDistance, Color.green);
            if (Physics.Raycast(diagLeftRay, out hit, maxDistance, layerMask) && hit.transform.tag == "Road")
            {
                distDiagLeft = hit.distance;
                Debug.DrawRay(transform.position, diagLeft * hit.distance, Color.red);
            }
            else
                Debug.DrawRay(transform.position, diagLeft * maxDistance, Color.green);
            if (Physics.Raycast(diagRightRay, out hit, maxDistance, layerMask) && hit.transform.tag == "Road")
            {
                distDiagRight = hit.distance;
                Debug.DrawRay(transform.position, diagRight * hit.distance, Color.red);
            }
            else
                Debug.DrawRay(transform.position, diagRight * maxDistance, Color.green);
              if (Physics.Raycast(diagBackLeftRay, out hit, maxDistance, layerMask) && hit.transform.tag == "Road")
            {
                distBackDiagLeft = hit.distance;
                Debug.DrawRay(transform.position, diagBackLeft * hit.distance, Color.red);
            }
            else
                Debug.DrawRay(transform.position, diagBackLeft * maxDistance, Color.green);
            if (Physics.Raycast(diagBackRightRay, out hit, maxDistance, layerMask) && hit.transform.tag == "Road")
            {
                distBackDiagRight = hit.distance;
                Debug.DrawRay(transform.position, diagBackRight * hit.distance, Color.red);
            }
            else
                Debug.DrawRay(transform.position, diagBackRight * maxDistance, Color.green);

            
                
        }
    }