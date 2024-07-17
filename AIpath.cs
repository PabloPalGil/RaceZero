using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIpath : MonoBehaviour {   //USAR ESTA IA, MAS BARATA (RECTILINEA)

    public Transform path; //v1-2
    public int currentNode = 0; //v1-2
    private Transform[] pathTransforms; //v1-2
    [HideInInspector]public List<Transform> nodes; //Lista de nodos del path. Hecha PUBLIC para pruebas de randomizar paths/cpu.


    public float speed; //v1-v2
    //private float originalSpeed;//Esto lo uso para alterar la vel en funcion de la IA (solo para cpus).
    public float reachDist = 3.0f; //v1-2


    //Posicionado en carrera:
    public float count;
    //public int lap = 0;

    
    //Semaforo
    Semaphore semaphore;

    //Player control:
    public bool manualPilot = true;



    //Rotacion y traccion de la nave hacia el obetivo:
    private float RotationSpeed = 4;
    public float angle;
    public Quaternion lookRotation;
    public Vector3 direction;

    public float autoTurn;


    //Cte para aligerar frecuencia de calculo:
    private float timeStamp;
    public int cteCalc = 10;
    //private float myCalcTurn;//Este numero será el que escalone el calculo de las IA entre naves para aligerar procesado.
    public Vector3 nextNodePos;

    //Random path a partir del base:
    public float pathVariation;
    public float myRandomX;//Esta variable se crea al cargar partida una vez, es distinta para cada cpu y altera el path del mismo.
    public float myRandomY;
    public Vector3 randomVector;
    public int randomSpeed;

    //Boost npcs:
    public ParticleSystem[] ps;
    

    //Valores para la IA respecto al player (handicap):
    public Laps lapsPlayer;
    public Laps myLaps;
    private AIpath AIplayer;

    //AIspeed:
    private float designSpeed;
    public bool accelering;
    public bool decelering;
    //public bool AIboosting;
    public float currentAccelSpeed;
    public float currentDecelSpeed;

    //Almacenar corutinas para detenerlas bien:
    private Coroutine a;
    private Coroutine d;

    //GPEnding vars:
    private bool gpEnding;
    public Transform initialPos;

    //Calculo tiempo extrapolado cpu en Laps:
    public float mVel;//Vel media a lo largo de toda la carrera de cada nave
    public int i;//Iteraciones para calcular la vel media
    public float sumVel;//Sumatorio de velocidades (necesario para calcular la mVel)


    //Handicap bien:
    //public bool handicap;
    private float handicapVel;
    private CompletePlayerController player;
    public float originalDesignSpeed;//Almaceno la designSpeed antes de modificarla por el handicap para recuperarla al final
    //public bool top3;//Bool para indicar si esta cpu pertenece a los 3 primeros de la clasif.

    void Start()
    {

        gpEnding = false;

        //Detecto y asigno el GO Path para no tener que hacerlo manualmente desde el inspector:
        path = GameObject.FindGameObjectWithTag("Path").transform;

        //Asigno el path y sus nodos a la lista nodes
        pathTransforms = path.GetComponentsInChildren<Transform>(); //Esta funcion tambien almacena el transform del contenedor (no solo los nodos)
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform) //Con esto filtro el trasform del contenedor path
            {
                nodes.Add(pathTransforms[i]);
            }
        }

        //Para la IA de las cpus:
        if (tag != "Player")//Esto me servirá para que las cpus puedan comparar su posicion respecto al player para modificar su IA:
        {
            lapsPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Laps>();
            myLaps = GetComponent<Laps>();
            AIplayer = lapsPlayer.GetComponent<AIpath>();
        }

        player = FindObjectOfType<CompletePlayerController>();


        //Inicializo variables de velocidad:
        if (gameObject.name != "Ghost")//Si no soy el ghost:
        {
            if (GameManager.Instance.difficultyGP == "BEGINNER")
                designSpeed = 142;//Velocidad de diseño (a la que tienden las cpus de manera habitual)      //AJUSTAR DIF GP
            else if (GameManager.Instance.difficultyGP == "STANDARD")
                designSpeed = 148;
            else if (GameManager.Instance.difficultyGP == "EXPERT")
                designSpeed = 155;
            else if (GameManager.Instance.difficultyGP == "MASTER")
                designSpeed = 165;
        }
        else//Si soy el ghost:
        {
            designSpeed = GameManager.Instance.GhostSpeed();
            accelering = false;
        }

        decelering = false;
        //AIboosting = false;

        //top3 = false;

        semaphore = FindObjectOfType<Semaphore>();

        if (tag == "Player")
        {
            manualPilot = true;
        }
        else manualPilot = false;




        //Randomness:
        if (gameObject.name != "Ghost")
        {
            pathVariation = 10;
            Debug.Log("randomness");
            myRandomX = Random.Range(-pathVariation, pathVariation);//Mi numero random varia en un rango definido por pathVariation.
            myRandomY = Random.Range(-pathVariation, pathVariation);
            randomVector = new Vector3(myRandomX, myRandomY, 0);
            randomSpeed = Random.Range(-10, 10);
            //speed = designSpeed + randomSpeed;

        }

        //Depende de en que pista esté les doy una velocidad a las cpus?


        //Durante el start aun no se ha asignado si estoy en modoGp o timeBreak porque tarda un frame
        //así que me llevo el siguiente código a una subrutina y le hago esperar un poco antes:
        StartCoroutine(AIspeed());

        //Uso MeanVelocity para sacar la vel media de cada cpu y poder calcular lo que tardara en llegar a meta al forzar el final
        InvokeRepeating("MeanVelocity", 2 + Random.Range(0, 1), 2);

        originalDesignSpeed = 0;
        //if (gameObject.name != "Ghost" && tag != "Player")
        //    InvokeRepeating("Handicap", 0.5f + Random.Range(0, 1), 2);

    }




    //Comentado para probar orden secuenciado con coroutines desde LevelManager
    //SIMPLE AI FUNCIONA!! :D
    void Update () {

        //Semaforo:
        if (semaphore.lightsOn) return;



        //Piloto automatico:
        if (manualPilot) return;



        //Handicap comentado por pruebas de balanceo:
        if (Time.time > timeStamp) //Este if determina cada cuanto tiempo (cteDist) se comprueba el handicap de las cpus. Otra opción sería usar InvokeRepeating!!
        {
            timeStamp = Time.time + cteCalc;//cteCalc me indica cada cuanto tiempo las cpus ajustan el handicap:
            if (gameObject.name != "Ghost" && tag != "Player")//Si no soy ni el player ni un ghost:
            {
                Handicap();
            }

        }




        //Versión original en Update de la IA---------------------------------------------
        //ROTACION DE NAVE SOLO EN NPCs Y EN PILOTO AUTOMATICO:
        if (tag != "Player" || !manualPilot)
        {
            //dist comenada por pruebas con trigger col:
            float dist = Vector3.Distance(nodes[currentNode].position + randomVector, transform.position);

            //Versión por software de comprobación de nodos (alternativa mediante triggers comentada mas abajo, más eficiente pero requiere ajustar los colliders)
            if (dist < reachDist)
            {
                currentNode++;

                if (currentNode >= nodes.Count)
                {
                    currentNode = 0;
                }
            }

            direction = ((nodes[currentNode].position + randomVector) - transform.position).normalized;//Rotar la nave hacia siguiente nodo:

            //Lógica v1 de rotación:
            if (tag == "Player")//Para el player al llegar a meta uso esta lógica para que cuadre con el scroll de fondo y la camara:
            {
                angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90;//Saco el ángulo entre mi direccion actual y la direccion que quiero tomar.
                lookRotation = Quaternion.AngleAxis(angle, Vector3.forward); //Tengo que rotar angle grados alrededor de mi direccion actual:
                //Suavizo mi giro, la vel.Rotacion va de 0 a 1:
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);
            }
            else
            {
                angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90;//Saco el ángulo entre mi direccion actual y la direccion que quiero tomar.
                lookRotation = Quaternion.AngleAxis(angle, Vector3.forward); //Tengo que rotar angle grados alrededor de mi direccion actual:
                //Suavizo mi giro, la vel.Rotacion va de 0 a 1:
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 1);

                //transform.up = nodes[currentNode].position - transform.position;//Línea clave que mira hacia le objetivo en 2D, no borrar.

            }
        }


        //Línea original con MoveTowards:
        //transform.position = Vector3.MoveTowards(transform.position, nodes[currentNode].position + randomVector, Time.deltaTime * (speed));

        //Con translate da el mismo resultado:
        transform.Translate(
            (direction.x * speed * Time.deltaTime),
            (direction.y * speed * Time.deltaTime),
            0,
            Space.World);


    }//Fin de Update



    public void Handicap()
    {
        //Comento porque estas condiciones ya se tienen en cuenta antes de llamar Handicap():
        //if (semaphore.lightsOn || gameObject.name == "Ghost" || tag == "Player")
        //    return;
        if(originalDesignSpeed == 0)
        {
            originalDesignSpeed = designSpeed;
        }

        if(myLaps.currentLap >= 2 && myLaps.currentLap < myLaps.finishLap - 1)//Solo aplico handicap si no estoy ni en la primera ni en la ultima vuelta
        {
            if (myLaps.currentPos > lapsPlayer.currentPos/* && top3*/)//Si voy detras del player y soy una de las cpus del top3:
            {
                //Según el modo de dificultad, ajusto el handicap de las cpus (modo GP):
                if(GameManager.Instance.difficultyGP == "BEGINNER")
                    designSpeed = Mathf.Clamp(handicapVel * 1f, 100, 180);//AJUSTAR DIF GP
                else if (GameManager.Instance.difficultyGP == "STANDARD")
                    designSpeed = Mathf.Clamp(handicapVel * 1.1f, 120, 190);//AJUSTAR DIF GP
                else if (GameManager.Instance.difficultyGP == "EXPERT")
                    designSpeed = Mathf.Clamp(handicapVel * 1.1f, 120, 220);//AJUSTAR DIF GP
                else if (GameManager.Instance.difficultyGP == "MASTER")
                    designSpeed = Mathf.Clamp(handicapVel * 1.2f, 120, 250);//AJUSTAR DIF GP
            }
            else//Si voy delante del player:
            {
                if(GameManager.Instance.difficultyGP == "BEGINNER")
                    designSpeed = Mathf.Clamp(handicapVel * 0.8f, 90, 180);//AJUSTAR DIF GP
                else if (GameManager.Instance.difficultyGP == "STANDARD")
                    designSpeed = Mathf.Clamp(handicapVel * 0.85f, 110, 190);//AJUSTAR DIF GP
                else if (GameManager.Instance.difficultyGP == "EXPERT")
                    designSpeed = Mathf.Clamp(handicapVel * 0.9f, 130, 220);//AJUSTAR DIF GP
                else if (GameManager.Instance.difficultyGP == "MASTER")
                    designSpeed = Mathf.Clamp(handicapVel * 0.95f, 140, 250);//AJUSTAR DIF GP
            }
            CheckSpeed();
        }
        else
        {
            designSpeed = originalDesignSpeed;
            CheckSpeed();
        }

    }


    void OnCollisionEnter2D(Collision2D col)//Al chocarme con algo, hasta que no me separo bastante y vuelvo a chocarme no se reproduce.
    {
        if (col.gameObject.CompareTag("Racer") || col.gameObject.CompareTag("Player"))
        {
            if (tag != "Player")
            {
                Vector2 dirForce = col.transform.position - transform.position;
                GetComponent<Rigidbody2D>().AddRelativeForce(dirForce * speed * 1.5f);

                Debug.Log("CPu-Ostion");
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (!LevelManager.Instance.playerFinished && tag == "Player") return;//Si soy el player y aun no he acabado la carrera esto a mí no me afecta (es solo para las IA)


        //if (col.tag == "ArrowBoost")
        //{
        //    if (LevelManager.Instance.gameOver) return;
        //    if (!decelering && !accelering)
        //    {
        //        currentAccelSpeed = (designSpeed + randomSpeed) * 1.2f;
        //        //if(speed < currentAccelSpeed)
        //        StartCoroutine(AccelAI(currentAccelSpeed));
        //    }
        //}

        if (col.tag == "DecelZone")//Hacer que las cpus desaceleren cuando pisan estas zonas (triggers)
        {
            Debug.Log("DecelZone!");
            if(decelering)//Antes que nada compruebo si estoy en medio de un ciclo de acel/deceleracion para detenerlo.
            {
                decelering = false;
                StopCoroutine(d);
                //StopCoroutine("DecelAI");
            }
            if (accelering)
            {
                accelering = false;
                StopCoroutine(a);
                //StopCoroutine("AccelAI");
            }

            currentDecelSpeed = col.name == "Decel50" ? 50 : col.name == "Decel30" ? 30 : col.name == "Decel70" ? 70 : col.name == "Decel110" ? 110 : 90;//El nombre del GameObject trigger collider2D será la velocidad de frenado
            if (speed > currentDecelSpeed)
            {
                Debug.Log("Decelero a " + currentDecelSpeed);
                d = StartCoroutine(DecelAI(currentDecelSpeed));
                //StartCoroutine(DecelAI(currentDecelSpeed));
            }
            else
            {
                Debug.Log("No decelero, acelero hasta mi vel diseño");
                currentAccelSpeed = designSpeed + randomSpeed;
                a = StartCoroutine(AccelAI(currentAccelSpeed));
                //StartCoroutine(AccelAI(currentAccelSpeed));
            }

        }
    }



    private void CheckSpeed()//Si no estoy en mi vel de diseño ni en camino hacia ella, voy hacia mi vel de diseño
    {
        float mySpeed = designSpeed + randomSpeed;
        if(speed < mySpeed && !accelering)
        {
            decelering = false;
            if(d != null)
                StopCoroutine(d);
            //StopCoroutine("DecelAI");
            //StartCoroutine(AccelAI(mySpeed));
            a = StartCoroutine(AccelAI(mySpeed));//Pongo las corutinas como variables privadas para que funcione bien el tema de detenerlas, si no se ralla
        }
        else if(speed > mySpeed && !decelering)
        {
            accelering = false;
            if (a != null)
                StopCoroutine(a);
            //StopCoroutine("AccelAI");
            //StartCoroutine(DecelAI(mySpeed));
            d = StartCoroutine(DecelAI(mySpeed));
        }
    }



    private IEnumerator DecelAI(float lowSpeedGoal)//Corutina para desacelerar npcs hasta la vel indicada (lowSpeedGoal) y luego aceleren con AccelAI (comportamiento ante curvas)
    {
        decelering = true;
        while(speed > lowSpeedGoal)
        {
            speed -= 150 * Time.deltaTime;
            yield return null;
        }
        speed = lowSpeedGoal;
        decelering = false;

        CheckSpeed();
        //if (speed < designSpeed + randomSpeed)//Si no he llegado a mi vel de diseño, tengo que acelerar pues estoy aqui por una decelZone y no por un boost:
        //{
        //    currentAccelSpeed = designSpeed + randomSpeed;
        //    StartCoroutine(AccelAI(currentAccelSpeed));
        //}
    }


    private IEnumerator AccelAI(float speedGoal)//Acelera las cpus desde vel inicial (0) hasta su velocidad de diseño + su random de esta pista:
    {
        accelering = true;
        while (speed < speedGoal)
        {
            float a = speed < 70 ? 80 : 50f;
            speed += a * Time.deltaTime;
            yield return null;
        }
        speed = speedGoal;
        accelering = false;

        CheckSpeed();
        ////Si he acelerado por un turbo:
        //if (speed > designSpeed + randomSpeed)//Si no he llegado a mi vel de diseño, tengo que desacelerar pues estoy aqui por un boost y no por una decelZone:
        //{
        //    currentDecelSpeed = designSpeed + randomSpeed;
        //    StartCoroutine(DecelAI(currentDecelSpeed));
        //}
    }




    private IEnumerator AIspeed()//Aquí se asigna la velocidad inicial según el modo y el tipo de nave:
    {
        //yield return null;
        yield return new WaitForFixedUpdate();

        if (GameManager.Instance.modeTimeBreak && tag == "Player")//Si estoy en modo timebreak y no soy el ghost:
        {
            Debug.Log("modo tbreak");
            speed = designSpeed + randomSpeed;
        }
        else if (GameManager.Instance.ModeGP && tag != "Player")//Si estoy en modo GP y no soy el player:
        {
            Debug.Log("modo GP");
            speed = 10;//Velocidad actual / instantanea inicial de todos en el arranque.

            ////Handicap solo a los 3 primeros clasificados (a partir de la segunda carrera):
            //if (!GameObject.FindGameObjectWithTag("GrandPrixStart"))
            //{
            //    initialPos = transform;

            //    if (initialPos.position == LevelManager.Instance.posList[0].position)//El 1º:
            //    {
            //        Debug.Log("Soy el 1º " + gameObject.name);
            //        top3 = true;//Señalo este racer con su top3 para que actue el handicap luego
            //    }
            //    else if (initialPos.position == LevelManager.Instance.posList[1].position)//El 2º:
            //    {
            //        Debug.Log("Soy el 2º " + gameObject.name);
            //        top3 = true;//Señalo este racer con su top3 para que actue el handicap luego
            //    }
            //    else if (initialPos.position == LevelManager.Instance.posList[2].position)//El 3º:
            //    {
            //        Debug.Log("Soy el 3º " + gameObject.name);
            //        top3 = true;//Señalo este racer con su top3 para que actue el handicap luego
            //    }
            //}
        }


        while (semaphore.lightsOn)
        {
            yield return null;//Esto hace que pase un frame y vuelva a este punto (pero cuando se apaga el semaforo se reompe el while y avanza al siguiente if
        }

        if (LevelManager.Instance.gpEnding)
        {
            initialPos = transform;
            gpEnding = true;
            manualPilot = false;
            randomSpeed = 0;
            designSpeed = 100;
            speed = designSpeed;

            //Cada racer por su lado segun podio:
            if (initialPos.position == LevelManager.Instance.posList[1].position)//El 2º:
            {
                Debug.Log("Soy el 2º " + gameObject.name);
                randomVector = new Vector3(-10, 0, 0);
            }
            else if (initialPos.position == LevelManager.Instance.posList[2].position)//El 3º:
            {
                Debug.Log("Soy el 3º " + gameObject.name);
                randomVector = new Vector3(10, 0, 0);
            }
            else//El 1º:
            {
                Debug.Log("Soy el 1º " + gameObject.name);
                randomVector = new Vector3(0, 0, 0);
            }


        }

        currentAccelSpeed = designSpeed + randomSpeed;
        a = StartCoroutine(AccelAI(currentAccelSpeed));
        //StartCoroutine(AccelAI(currentAccelSpeed));//Y ahora ejecuto AccelAI(introduzco la vel que quiero que alcancen) para la aceleración de las cpus en la salida (ya que si no no tiene mucho sentido salir 1º o ultimo)
    }



    public void MeanVelocity()
    {
        if (semaphore.lightsOn)
            return;
        i++;
        sumVel += speed;
        //mVel = (sumVel) / i;

        //Pruebas handicap:
        if(tag != "Player")
        {
            handicapVel = player.mVel;
        }
    }
}