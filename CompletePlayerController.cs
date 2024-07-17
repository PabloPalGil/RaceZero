using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;//Para formato string timeSpan
using UnityStandardAssets.CrossPlatformInput;

//Adding this allows us to access members of the UI namespace including Text.
using UnityEngine.UI;

public class CompletePlayerController : MonoBehaviour
{
    //Estas dos lineas son para poder acceder a este script desde cualquier lugar del juego:
    private static CompletePlayerController instance;
    public static CompletePlayerController Instance { get { return instance; } }


    //public float speed, turnSpeed;				//OLD Floating point variable to store the player's movement speed.
    public Text countText;          //Store a reference to the UI Text component which will display the number of pickups collected.
    public Text winText;            //Store a reference to the UI Text component which will display the 'You win' message.
                                    //public float boostMultiplier = 3;

    private Rigidbody2D rb2d;       //Store a reference to the Rigidbody2D component required to use 2D Physics.
    private int count;				//Integer to store the number of pickups collected so far.

    //Vars para orienar sprite hacia camara (no hecho):
    public Camera cam;

    //Parámetros para el control en Update:
    /*Ship handling parameters*/
    public float fwd_accel;
    //public float fwd_max_speed;
    //public float brake_speed;
    //public float turn_speed;
    //public float turnConstant;
    //public float minTurnSpeed, maxTurnSpeed;

    //Definicion curva aceleracion:
    //public float limit1;
    //public float limit2;
    //public float limit3;

    /*Auto adjust to track surface parameters*/
    //public float hover_height = 3f;     //Distance to keep from the ground
    //public float height_smooth = 10f;   //How fast the ship will readjust to "hover_height"
    //public float pitch_smooth = 5f;     //How fast the ship will adjust its rotation to match track normal

    /*We will use all this stuff later*/
    //private Vector3 prev_up;
    private float tilt;
    //private float smooth_y;
    //private float current_speed;

    //Nuevo sistema de giro
    [HideInInspector] public float turn; //Valor final de giro calculado en cada frame. Public porque lo lee el script Scroll para girar el fondo.

    //Estos valores publicos modifican el gameplay de la nave (junto a fwd_accel mas arriba).
    public float tBase; //Valor base de giro
    public float tResist;//A mayor vel, menos gira
    public float tInertia;//lo que tarda la nave en responder al control del volante del jugador (aumentar hasta 1 progresivamente cada vez que se toca el volante)
    public float tTension;//Modifica la intensidad de giro en el tiempo, este solo acumula si se gira en la misma direccion y a tope...
    public float tDecel;//Valor de pérdida de vel al girar
    public float shiftValue;

    private float lastTurnValue;//Almacena el valor turn del frame anterior
    private float lastTurnValue2;
    private bool startSteering;
    private bool stopSteering;
    private bool steering;
    private bool changeDirection;
    private float Kt;
    private float Ki;
    private float steerTimeStamp;
    private bool lerpKi;
    private float Kext;//Para modificar el giro por causas externas, como el hielo.




    //Jumping
    [HideInInspector]
    public float lift_speed;
    [HideInInspector]
    public float fall_speed;
    [HideInInspector]
    public float ceiling;
    private bool jumping, rising, falling;
    private float max_height;
    private float groundZ;
    private float currentZ;
    float newVelocityZ;
    [HideInInspector]
    public float maxFallSpeed;

    //Animator
    Animator anim;
    bool tilting;
    [HideInInspector]
    public float lastTiltValue;
    [HideInInspector]
    public bool turningRight = false;
    [HideInInspector]
    public bool turningLeft = false;

    //Variables raycasts
    bool forwardButton;
    bool buttonPressedLastTime;
    public float acceleration;
    //public float friction;
    public float maxSpeed;
    float newVelocityY;
    float newVelocityX;//Añadido luego para amortiguar las colisiones con rebote


    //Variables shift:
    public float newShiftValue;
    public float newFrictionValue;
    private float shiftSpeed;
    private float shiftFriction;
    private bool shifting;
    private bool shiftButton;


    //Vars para boton freno:
    bool brakeButton;
    public float brakeFriction;

    //Vars para BOOST POWER:
    bool boostButton;
    public float boostTime = 1;
    public float boost = 2f;
    float boostMultiplier = 1;
    bool boosting = false;
    float boostStart;
    float boostRemain;
    public bool boostAvailable = false;
    ParticleSystem[] ps;
    private bool arrowBoost = false;
    public bool firstLap = true;

    //Variable velocímetro
    public Text speedmeter;
    public float maxRecordSpeed;

    //Variable cronometro
    public Text cronos;

    //Variable posicion:
    public Text position;

    //my 2D velocity I use for most calculations. Lo paso a Vector3 para incluir los saltos
    //Vector2 velocity;
    public Vector3 velocity;

    //a Rectangle class has some useful tools for us
    Rect box;

    //a layer mask that I set in the Start() function
    int layerMask;

    //variables for raycasting: how many rays, etc
    int horizontalRays = 7;
    int verticalRays = 5;

    CircleCollider2D hurtBox;
    //Estas vars son para calcular el angulo del muro:
    Vector2 leftPoint;
    Vector2 rightPoint;


    //Semaforo
    Semaphore semaphore;


    //Piloto automatico (para que la IA mueva la nave al acabar la partida):
    public bool autoPilot = false;
    //public int lastLap = 4;

    //Cargar sprite skin:
    private SpriteRenderer spriter;//Defino mi componente SpriteRenderes de dnd elijo mi sprite skin.

    //Colisiones con rebote:
    //public float bumpness;


    //Healthbar
    public Healthbar playerHP;
    public bool lowHp;
    private bool alarm;


    //Variables para efectos de giro de camara al girar nave:
    private float angleShift = 5;
    private float tiltCam;
    private Quaternion rotF, rotB, rotFx, rotBx;
    private int angleX;
    private Vector3 currentAngle;
    private Vector3 originalAngle;

    //Variables para efectos de twister:
    private Vector3 twisterPos;
    private float startTwisterPos;
    private bool twisting = false;

    //Bg camera:
    private Transform bgCamTransform;

    //Vars traccion y velocidad:
    private bool driving;
    [HideInInspector] public float extInfluence; //Para modificar la velocidad resultante de la nave ante condiciones ambientales.
    private bool hitStun = false;
    private float hitStunTimeStamp;

    //Vars particle systems para instanciar:
    public GameObject burst;

    //Death anim:
    public GameObject explosions;
    public GameObject smallExplosions;
    [HideInInspector] public bool dyingTrigger;
    private float timerSeconds;
    float deadTurn = 0;
    [HideInInspector] public bool deadStun;
    public Sprite shipBurnt;
    private float fade = 0;
    private Image fullScreen;
    public GameObject smoke;
    private bool deadBurnt;

    //Raycasting detection
    private Vector3 lastConnection;//Almacena la posicion de la última colision con la pista
    private Vector2 lastHitNormal;//Almacena la normal de la última colisión con la pista
    private Vector2 myHitDir;//Vector con la dirección que llevaba en el momento de chocarme
    private bool bounceHitStun = false;//Stun específico al chocar contra barreras
    private Transform lastTransform;


    //Audio engine. Sacado del tuto de unity "Creating a hover car with pysics"
    public AudioSource jetSound;//Arratro el audio source component del player desde el inspector
    private float jetPitch;//El pitch es la frecuencia sonora del sonido (permite variar el sonido como si acelerara o decelerara)
    private const float LowPitch = .1f;//El punto de frecuencia bajo
    private const float HighPitch = 2.0f;//El punto de frecuencia alto
    private const float SpeedToRevs = .01f;//Constante de ajuste entre los numeros del script y el sonido
    //varios sonidos:
    public AudioClip[] engineClips;//Poner aquí los sonidos de motor que tenga (en teoría solo una vez en el prefab)

    //Variable para el display de las vueltas
    public Text lapsDisplay;

    //Vars para display de las vidas retsantes:
    public Text lifesUItext;
    public Image lifeImageUI;


    //NeoJumping
    private float jump;

    //Variables GPEnding:
    private bool gpEnding;


    //Asignar propulsores segun nave:
    public GameObject[] afterBurners;


    //Efecto de camara:
    private ParticleSystem psCam;

    //Vel media para el handicap de las cpus GP
    public float mVel;
    private float sumVel;
    private int i;


    //Sonidos:
    public AudioSource hitWallSfx, frontHitSfx;
    public AudioSource boostSfx;
    public AudioSource lowEnergySfx;
    public AudioSource lavaSfx;
    public AudioSource healSfx;
    //public AudioSource loopHealSfx;
    //private float healTimeStamp;//Aqui marco el tiempo de entrada a zona de heal para saber cuando meter el healLoopSfx
    public AudioSource explosion1, explosionBig;


    //Tutorial text:
    public Text tutorialText;
    public GameObject tutorialCanvas;

    //MODO INFINITY:
    private bool infinity;
    public BrakeBar brakeBar;//ref al script BrakeBar para el modo infinity
    private float brakeStun;//Numero para indicar el tiempo que detengo la carga de la brakeBar al chocarme
    public bool infinityEnd;//Bool para saber cuando he acabado

    //OUtcourse
    private int previousPoint;//Indica el ultimo checkpoint de laps para ir si me salgo de la pista
    private int nextPoint;//Indica la dirección a la que miro al volver a la pista
    public bool outCourse;//Indica que estoy en medio de la animacion de vuelta

    //Control movimiento
    private Quaternion calibrationQuaternion;



    void Start()
    {
        instance = this;//Para que funcione las llamadas mediante Instance.
        gpEnding = false;
        infinityEnd = false;
        
        //Piloto automatico:
        autoPilot = false;


        //Me llevo toda la morralla de asignar cada elemento de la UI mediante tags a una corutina para que no estorbe los starts importantes del resto de scripts
        StartCoroutine(UIasigner());

        ////Get and store a reference to the Rigidbody2D, camera and animator component so that we can access it.
        //rb2d = GetComponent<Rigidbody2D>();
        //cam = GetComponentInChildren<Camera>();
        //anim = GetComponent<Animator>();
        //spriter = GetComponent<SpriteRenderer>();
        //playerHP = FindObjectOfType<Healthbar>();//De esta forma solo hay una barra de vida que es para el player, si en algun momento hubieran varios players, habria que meterlas como public y añadirlas desde el inspector a cada uno.
        //alarm = false;
        //lowHp = false;
        //lapsDisplay = GameObject.FindGameObjectWithTag("LapUI").GetComponent<Text>();//Lo busco por su tag para no tener que meterlo manualmente en el inspector
        //lapsDisplay.text = "1 / " + (GetComponent<Laps>().finishLap - 1).ToString();
        //speedmeter = GameObject.FindGameObjectWithTag("Speedmeter").GetComponent<Text>();//Lo busco por su tag para no tener que meterlo manualmente en el inspector
        //position = GameObject.FindGameObjectWithTag("PositionUI").GetComponent<Text>();//Lo busco por su tag para no tener que meterlo manualmente en el inspector
        //cronos = GameObject.FindGameObjectWithTag("chronometer").GetComponent<Text>();
        //winText = GameObject.FindGameObjectWithTag("WinText").GetComponent<Text>();//unused todavía
        //countText = GameObject.FindGameObjectWithTag("CountText").GetComponent<Text>();//Unused todavia


        //Cargo el skin que he elegido en el menu antes de empezar el nivel o el que hay por defecto en PlayerPrefs:
        //En PlayerPrefs, accesible mediante GameManager se guarda el int que apunta al sprite dentro de la carpeta Resources/Player.
        Sprite[] textures = Resources.LoadAll<Sprite>("Player");//Aqui guardare mis sprites.
        spriter.sprite = textures[GameManager.Instance.currentSkinIndex];

        //Definiendo las caracteristicas del player segun la skin:
        PlayerStats(GameManager.Instance.currentSkinIndex);//Funcion muy importante: aplica los cambios de caracteristicas jugables de cada nave, segun la skin elegida (el int currentSkinIndex).

        //Todo lo que sigue es simplemente para diferenciar los particle systems (propulsores de efectos de camera/ambientales)
        ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>();
        List<ParticleSystem> psList = new List<ParticleSystem>();
        foreach (ParticleSystem p in pss)
        {
            if (p.tag == "Burner")
                psList.Add(p);
            else
                psCam = p;//Por ahora solo hay un ps que no sea propulsores
        }
        ps = psList.ToArray();

        //Inicializo los propulsores en idle:
        for (int i = 0; i < ps.Length; i++)
        {
            var main = ps[i].main;
            main.startSpeed = 0;
        }

            //Initialize count to zero.
            count = 0;

        //Initialze winText to a blank string since we haven't won yet at beginning.
        //winText.text = "";

        //Call our SetCountText function which will update the text with the current value for count.
        //SetCountText();

        //Semaforo:
        //semaphore = FindObjectOfType<Semaphore>();

        //boost:
        boosting = false;

        //Cosas raycast:
        layerMask = 1 << LayerMask.NameToLayer("NormalCollisions");
        hurtBox = GetComponent<CircleCollider2D>();

        //Busco mis turbos://Aplicado en PlayerStats()
        //ps = GetComponentsInChildren<ParticleSystem>();

        //Cojo los angulos de la camara:
        if (!gpEnding)
        {
            originalAngle = cam.transform.localEulerAngles;
            currentAngle = cam.transform.localEulerAngles;
            rotF = Quaternion.AngleAxis(5 * Time.deltaTime, Vector3.forward);
            rotB = Quaternion.AngleAxis(-5 * Time.deltaTime, Vector3.forward);

        }

        bgCamTransform = GameObject.FindGameObjectWithTag("BgCam").transform;

        //Variables para el giro: //Todas estas variables se declaran en la funcion PlayerStats().
        //tBase = 4; //Cuanto mayor sea más gira la nave en general.
        //tResist = 1 / (1.85f * maxSpeed); //El 1.85f indica que tResist quitara un 15% a maxima velocidad.
        //tInertia = 2f; //A mayor tInertia, menos inercia y mas rapido responde la nave.
        Kext = 1;//Influencia externa como el hielo.
        //shiftValue = 2;

        //Variables acceleracion:
        //fwd_accel = 1.5f;//Aceleración.
        //brakeFriction = 1.3f;//Potencia de frenado.
        extInfluence = 1;

        //Var death anim
        dyingTrigger = true;
        fullScreen = GameObject.FindGameObjectWithTag("FullScreen").GetComponent<Image>();
        fade = 0;
        deadBurnt = false;

        //MODO INFINITY:
        if (infinity)
        {
            maxSpeed *= 0.5f;//Esto tiene que estar aquí porque la maxSpeed se asigna en PlayerStats, despues del UIasigner
            brakeBar = GetComponent<BrakeBar>();//Referencio el script brakeBar (que va unido al player)
        }

        //Cálculo de la vel media para que lo usen las cpus en el handicap:
        InvokeRepeating("VelM", 10, 2);

        //Reseteo los inputs tactiles para evitar el bug de empezar carrera frenado o boosting:
        //CrossPlatformInputManager.GetButton("Boost")



        //Control por movimiento:

        //Calibrar acelerometro:
        Vector3 accelerationSnapshot = Input.acceleration;

        Quaternion rotateQuaternion = Quaternion.FromToRotation(
            new Vector3(0.0f, 0.0f, -1.0f), accelerationSnapshot);

        calibrationQuaternion = Quaternion.Inverse(rotateQuaternion);
    }





    //CONTROL CON UPDATE, SIN USAR FISICAS v2 (raycasts): (FIJARSE EN EL CODIGO DE JUMPING DE PLAYERCONTRAYCAST)
    void Update()
    {

        if (gpEnding || infinityEnd)//Si estoy en la escena GPEnding la nave va sola (Como autopilot)
        {
            return;//Pongo aqui al principio el return porque no quiero nada de UI ni turbos ni muerte
        }


        //Define el velocímetro
        if (!deadBurnt)
            speedmeter.text = (4 * Mathf.Abs(velocity.y)).ToString("F0") + " km/h";   //F1 muestra 1 decimal, F2 muestra 2, etc.
        else
            speedmeter.text = (4 * Mathf.Abs(rb2d.velocity.magnitude)).ToString("F0") + " km/h";
        maxRecordSpeed = (velocity.y > maxRecordSpeed ? velocity.y : maxRecordSpeed);//Habría que procesar la velocity.y si al final la multiplico para sacar mas vel en pantalla


        //Semaforo:
        if (semaphore.lightsOn) return;


        //Este if tiene que estar aqui arriba para que se vaya el turbo si se entra en meta con él.
        if (boosting)
        {
            Debug.Log("boosting!");
            boostMultiplier = boost;

            //MODO INFINITY:
            if (infinity)
            {
                brakeBar.GainBrakePoints(0.1f);//Al usar turbo relleno brakeBar
                brakeStun = Time.time;//Y cancelo el brakeSTun
            }

            if (Time.time > boostRemain)
            {
                Debug.Log("fuera turbo");
                boostMultiplier = 1f;
                boosting = false;
                boostAvailable = true;

                for (int i = 0; i < ps.Length; i++)
                {
                    var main = ps[i].main; //Método engorroso introducido en unity 5.5 para modificar ParticleSystems via Scripts...
                    main.startColor = new Color(0.2f, 1f, 1f, 1f);
                    //main.startSpeed = Mathf.Lerp(0, 50, velocity.y / maxSpeed);
                    main.startSize = 1.2f;
                    main.startLifetime = 0.1f;

                }
            }
        }


        //Fuego propulsores adaptativo a velocidad, meto esto aquí arriba para que apliquen los graficos del turbo igual al terminar la carrera.
        if (!deadStun)
        {
            if (!brakeButton)
            {
                for (int i = 0; i < ps.Length; i++)
                {
                    var main = ps[i].main;
                    //main.startSpeed = 10;
                    if (boosting)
                    {
                        main.startSpeed = Mathf.Lerp(0, 50, velocity.y / maxSpeed);
                        main.startSize = 1.4f;
                        main.startLifetime = 0.15f;
                        if (!arrowBoost && playerHP.hp > 1)
                            playerHP.TakeDamage(3f * Time.deltaTime);
                    }
                    else
                    {
                        main.startSpeed = Mathf.Lerp(0, 30, velocity.y / maxSpeed);
                        main.startSize = 1.2f;
                        main.startLifetime = 0.1f;
                        arrowBoost = false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < ps.Length; i++)
                {
                    var main = ps[i].main;
                    main.startSpeed = 0;
                    main.startLifetime = 0.1f;
                    main.startSize = 1.2f;
                }
            }

        }
        else if (deadBurnt)
        {
            Destroy(GameObject.Find("Firethrowers(Clone)"));
        }


        //Piloto automatico:
        if (autoPilot)
        {
            if (GameManager.Instance.sfx)
            {
                if (lowEnergySfx.isPlaying)
                    lowEnergySfx.Stop();
            }
            return;//De aqui para abajo el update son todo controles de la nave. Cancelo esto porque la movere sola con la AIpath.
        }

        //Define la posición, esta línea la pongo aquí para que no se ejecute una vez pasada meta:
        if (GameManager.Instance.ModeGP || GameManager.Instance.ghosting)//Para que no se muestre la posicion si no hay rivales
            position.text = GetComponent<Laps>().currentPos.ToString();

        if (infinity)
        {
            position.text = "";
            if (dyingTrigger)
            {
                //Define el cronometro, la pongo aquí para que no se ejecute una vez pasada meta:
                string minutes = ((int)GetComponent<Laps>().timer / 60).ToString("00");
                string seconds = (GetComponent<Laps>().timer % 60).ToString("00.000");
                cronos.text = minutes + ":" + seconds;//Cronometro en minutos y segundos 
            }
        }
        else
        {
            //Define el cronometro, la pongo aquí para que no se ejecute una vez pasada meta:
            string minutes = ((int)GetComponent<Laps>().timer / 60).ToString("00");
            string seconds = (GetComponent<Laps>().timer % 60).ToString("00.000");
            cronos.text = minutes + ":" + seconds;//Cronometro en minutos y segundos 
        }


        //Oscurecer pantalla tras morir y explotar:
        if (deadBurnt)
            StartCoroutine(FadeBlack());


        //Parpadear en rojo si hp > 20%: Ojo, lowHp sólo se activa desde Healthbar al bajar del 20%
        if (lowHp && !alarm)
        {
            StartCoroutine(RedBlink());
        }



        //Siempre reducir la velocidad.x:
        if (velocity.x != 0)
        {
            newVelocityX = velocity.x;
            if (newVelocityX > 0)
            {
                newVelocityX -= 11;
            }
            else
            {
                newVelocityX += 1;
            }

            if (velocity.x < 1.1 && velocity.x > -1.1) newVelocityX = 0;

            velocity = new Vector3(newVelocityX, velocity.y, velocity.z);
        }


        //Modo INFINITY:
        if (infinity)
        {
            maxSpeed += 0.05f;//Aumento mi velMax (mi aceleracion depende de ella) de manera constante siempre

            if(Time.time > brakeStun)//brakeStun es un numero que se actualiza superior a Time.time cada vez que me choco o he frenado
            {
                brakeBar.GainBrakePoints(0.1f);//De normal se va llenando la barra de freno
            }
            
        }




        AccelerationRamp();//Aquí obtengo mi aceleracion en función de fwd_accel, maxSpeed y mi velocidad.y actual.



        



        /*Version con boton de acelerador:
        //Here we get user input to calculate the speed the ship will get
        if (forwardButton)
        {
            anim.SetBool("Accel", true);
            velocity = new Vector3(velocity.x, Mathf.Min(velocity.y + acceleration, maxSpeed), velocity.z); //Comprobar signos
            newVelocityY = velocity.y;
        }
        else if (velocity.y > 0)
        {       //apply deceleration due to no input. ESTO ES MEJORABLE, SI MI VEL.Y NUNCA SERÁ NEGATIVA, SOBRA EL IF  Y EL MODIFIER, SIMPLEMENTE LE VOY RESTANDO VELOCITY...pero puede ser neativa en golpes.
            anim.SetBool("Accel", false);
            int modifier = velocity.y > 0 ? -1 : 1; //modifier indica sirve para frenar en ambos sentidos del eje Y
            newVelocityY += (friction * modifier) * shiftFriction; //shiftFriction solo influye cuando se derrapa.
        }
        else
        {
            anim.SetBool("Accel", false);
            newVelocityY = 0f;
        }
        */ //Hasta aquí version con acelerador.



        //Añadiendo el boton del TURBO:
        boostButton = Input.GetKey(KeyCode.K) || CrossPlatformInputManager.GetButton("Boost") || Input.GetKey(KeyCode.Joystick1Button2) || Input.GetKey(KeyCode.Joystick1Button3);
        if (boostButton && boostAvailable && !autoPilot && !firstLap && playerHP.hp > 1)//Sólo tengo turbo a apartir de la segunda vuelta y si me queda más de 1 de vida
        {
            BoostPower();
            Debug.Log("BOOST POWER!!!");
        }


        //ACELERACION Y FRENO:
        //Versión con boton de freno y SIN acelerador (acelera sola):
        brakeButton = Input.GetKey(KeyCode.S) || CrossPlatformInputManager.GetButton("Brake") || Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick1Button1);
        
        Braking();//Me llevo el control de traccion y freno a la funcion: Braking(): Aplico mi aceleración para aumenar o disminuir mi velocidad segun frene o no


        if (!hitStun)//La idea es que en HitStun la trayectoria sea la definida por la causa del hitStun y no se modifique el vector velocidad hasta que pase el stun (aunque sí que debería haber fricción)
        {

            velocity = new Vector3(velocity.x, newVelocityY, velocity.z);//Aplico la nueva newVelocityY obtenida de Braking().
            //Debug.Log("velocity cp");
        }


        //if (!brakeButton)
        //{
        //    anim.SetBool("Accel", true);
        //    velocity = new Vector3(velocity.x, Mathf.Min(velocity.y + acceleration, 500), velocity.z); //Comprobar signos.Esta linea limita la velocidad a 500 (caso hipotetico).
        //    newVelocityY = velocity.y; //Ojo, el turbo esta aplicado mas arriba con boostMultiplier, en la rampa de aceleracion.
        //}

        //else if (brakeButton && velocity.y > 0)//Este if es sospechoso. Nunca será -1 el modifier por definicion...
        //{
        //    //Aplica el freno
        //    anim.SetBool("Accel", false);
        //    int modifier = velocity.y > 0 ? -1 : 1; //modifier indica sirve para frenar en ambos sentidos del eje Y
        //    newVelocityY += (brakeFriction * modifier) * shiftFriction; //shiftFriction solo influye (es != 1) cuando se derrapa.

        //}
        //else if (brakeButton && newVelocityY <= 0)
        //{
        //    anim.SetBool("Accel", false);
        //    newVelocityY = 0f;
        //}
        //Hasta aquí version sin acelerador.




        /* //Físicas con turn_speed obsoletas por otra mas compleja
        //La capacidad de giro disminuye conforme aumenta la velocidad:
        turn_speed = (maxTurnSpeed - (turnConstant * newVelocityY)) * shiftSpeed; //A mayor velocidad, menor capacidad de giro, la constante de giro tiene que ser positiva. shiftSpeed solo influye al derrapar.
        if (turn_speed < minTurnSpeed) { turn_speed = minTurnSpeed; }
        //Debug.Log("turn_speed " + turn_speed);
        */

        /*Funcional, version inicial:
        //Fuego propulsores adaptativo a velocidad:
        if (velocity.y <= 0)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                var main = ps[i].main;
                main.startSpeed = 0;
            }
        } else if (velocity.y <= maxSpeed * 0.5f)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                var main = ps[i].main;
                //main.startSpeed = 10;
                main.startSpeed = Mathf.Lerp(0, 10, velocity.y / maxSpeed);
            }
        }
        else
        {
            for (int i = 0; i < ps.Length; i++)
            {
                var main = ps[i].main;
                main.startSpeed = Mathf.Lerp(0, 40, velocity.y / maxSpeed);
            }
        }
        */ //Hasta aqui.




        //Debug.Log("vector3 velocity: " + velocity);
        //Debug.Log("steerInput " + CrossPlatformInputManager.GetAxis("Horizontal"));//Va de 0 a 1 a lo joystick.


        /* //Este if sería para trasladar la nave horizontalmente, lo cual NO quiero.
        if (forwardButton != 0)
        {       //apply movement according to input
            newVelocityX += acceleration * horizontalAxis;
            newVelocityX = Mathf.Clamp(newVelocityX, -maxSpeed, maxSpeed);
        }
        */

        //Increase our current speed only if it is not greater than fwd_max_speed. Este método de moverme lo deshecho en favor del traslade usando el vector velocity (más arriba).
        //current_speed += (current_speed >= fwd_max_speed) ? 0f : fwd_accel * Time.deltaTime; //Comprobar esta linea

        /*
        else
        {
            if (current_speed > 0)
            {
                //The ship will slow down by itself if we dont accelerate
                current_speed -= brake_speed * Time.deltaTime;
            }
            else
            {
                current_speed = 0f;
            }
        }
        */

        /*
        //MOVIMIENTO HORIZONTAL
        float horizontalAxis = Input.GetAxisRaw("Horizontal");
        horizontalAxis = CrossPlatformInputManager.GetAxisRaw("Horizontal");

        float newVelocityX = velocity.x;
        if (horizontalAxis != 0)
        {       //apply movement according to input
            newVelocityX += acceleration * horizontalAxis;
            newVelocityX = Mathf.Clamp(newVelocityX, -maxSpeed, maxSpeed);
        }
        else if (velocity.x != 0)
        {       //apply deceleration due to no input
            int modifier = velocity.x > 0 ? -1 : 1;
            newVelocityX += acceleration * modifier;
        }

        velocity = new Vector2(newVelocityX, velocity.y);
        */

        //We get the user input and modifiy the direction the ship will face towards

        /*
        //GIRAR://ESTAS LINEAS SON LAS BUENAS!!!!!                                      //FUNCIONAL, COMENTADO PARA PRUEBAS CON TURN
        tilt += turn_speed * Time.deltaTime * -Input.GetAxis("Horizontal");
        tilt += turn_speed * Time.deltaTime * -CrossPlatformInputManager.GetAxis("Horizontal");

        TurningDetection();

                //Now we set all angles to zero except for the Z which corresponds to the tilt

        //transform.rotation = Quaternion.Euler(0, 0, tilt);                                        //FUNCIONAL, COMENTADO PARA PRUEBAS CON TURN
        //lastTiltValue = tilt;                                                                     //FUNCIONAL, COMENTADO PARA PRUEBAS CON TURN


        //HASTA AQUIIII!!!!!
        */

        /*if(turningRight || turningLeft) //Este if me calcula Kt, que modifica la intensidad de giro en el tiempo independientemente de todo lo demas. Creo que es prescindible...asi que lo comento por ahora aunque es funcional)
        {
            Kt += tTension * Time.deltaTime;
            if (Kt > tBase * 0.3f) Kt = tBase * 0.3f;

        }
        else
        {
            Kt = 0f;
        }*/



        //Boton SHIFT - derrapes:   //En desuso temporal, posible descarte del boton shift si no aporta nada de valor...
        shiftButton = Input.GetKey(KeyCode.L) || CrossPlatformInputManager.GetButton("Shift") || Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick1Button9) || Input.GetKey(KeyCode.Joystick1Button10);

        //MODO INFINITY:
        if (infinity)//Si infinity, shifting me quita bps
        {
            if (shiftButton)
            {
                shifting = true;
                brakeBar.TakeBrakePoints(0.05f);//Shiftear me quita bps porque me frena
                brakeStun = Time.time + 0.5f;//0.5s de brakeStun
                if (brakeBar.bp > 0)//Solo hago que shiftear me frene si me quedan bps
                    shiftFriction = newFrictionValue;
                //shiftSpeed = newShiftValue;
            }
            else
            {
                shifting = false;
                shiftFriction = 1;
                //shiftSpeed = 1;
            }
        }
        else//Si no infinity, condiciones normales
        {
            if (shiftButton)
            {
                shifting = true;
                shiftFriction = newFrictionValue;
                //shiftSpeed = newShiftValue;
            }
            else
            {
                shifting = false;
                shiftFriction = 1;
                //shiftSpeed = 1;
            }
        }





        //NUEVAS FÍSICAS DE GIRO (FUNCIONAL):                       TURNINNNNNG                 TURNING!!



        if (!LevelManager.Instance.gamePaused)
        {
            if (changeDirection) Ki = 0f;

            if (lerpKi)//lerpKi indica flancos de activación de tocar el volante (steering)
            {
                Ki = Mathf.Lerp(Ki, 1, tInertia * Time.deltaTime);//tInertia es un float propio de cada nave e indica la velocidad con la que se define el tiempo que tarda en responder cada nave al tocar el volante
                if (Ki >= 1) Ki = 1;
            }


            if (shiftButton)
            {
                turn += (tBase * 0.25f * shiftValue * (-Input.GetAxis("Horizontal") - CrossPlatformInputManager.GetAxis("Horizontal"))) * Time.deltaTime;
                //Control por movimientos:
                //turn += (tBase * 0.25f * shiftValue * (-calibrationQuaternion.x * Input.acceleration.x * 4)) * Time.deltaTime;
            }
            else
            {
                float kTurn = (tResist * velocity.y > 0.5f ? 0.5f : tResist * velocity.y);//kTurn la necesito para que al pasar de la velMax con turbos y demás, la ralentización no supere la de diseño (ni se invierta el giro)
                turn += ((tBase/* - Kt*/) * (1 - kTurn) * (-Input.GetAxis("Horizontal") - CrossPlatformInputManager.GetAxis("Horizontal"))) * Ki * Kext * Time.deltaTime;

                //Control por movimientos:
                //La siguiente linea no va bien
                //turn += ((tBase/* - Kt*/) * (1 - kTurn) * (- calibrationQuaternion.x * Input.acceleration.x * 20)) * Ki * Kext * Time.deltaTime;
            }

            //turn += (tBase * (-Input.GetAxis("Horizontal") - CrossPlatformInputManager.GetAxis("Horizontal"))) * Ki * Kext;//Obsoleto


            TurningDetectionDX();

            if (!deadStun)
            {
                transform.rotation = Quaternion.Euler(0, 0, turn);
            }
            else
            {
                deadTurn -= timerSeconds;
                transform.rotation = Quaternion.Euler(0, 0, deadTurn);
            }
            lastTurnValue2 = lastTurnValue;
            lastTurnValue = turn;

        }

        //Debug.Log("Acceleration: " + acceleration);
        //Debug.Log("Velocity.y: " + velocity.y);


        //HASTA AQUI COMENTADO POR BUGG

        //Perder vel al girar:
        //TurningFriction();//Funcon en pruebas.


        //Efectos de camara al girar:

        //DynamicTurning();//Esta linea llama a la funcion que mueve la camara al girar la nave. Comentada solo para probar el twister effect (bool twisting creada para tal efecto).



        /*
        //Set turning animations. No me convencen, habría que hacer que salten las animaciones solo si se gira lo suficiente.
        if (tilt != lastTiltValue)
        {
            tilting = true;
            if(tilt > lastTiltValue)
            {
                anim.SetBool("Left", true);
            }
            else
            {
                anim.SetBool("Right", true);
            }
        }
        else
        {
            tilting = false;
            anim.SetBool("Right", false);
            anim.SetBool("Left", false);
        }

        //Debug.Log("tilting " + tilting);
        */




        //Now we set all angles to zero except for the Z which corresponds to the tilt

        //transform.rotation = Quaternion.Euler(0, 0, tilt);                                        //FUNCIONAL, COMENTADO PARA PRUEBAS CON TURN
        //lastTiltValue = tilt;                                                                     //FUNCIONAL, COMENTADO PARA PRUEBAS CON TURN




        //Finally we move the ship forward according to the speed we calculated before
        //transform.position += transform.up * (current_speed * Time.deltaTime);


        /*//----------------------------------------------------------
        //RAYCASTS
        
        float upRayLength = velocity.y * Time.deltaTime;

        bool connection = false;
        int lastConnection = 0;
        Vector2 min = new Vector2(transform.position.x - hurtBox.radius, hurtBox.offset.y);
        Vector2 max = new Vector2(transform.position.x + hurtBox.radius, hurtBox.offset.y);
        RaycastHit2D[] upRays = new RaycastHit2D[verticalRays];
        

        for (int i = 0; i < verticalRays; i++)
        {
            Vector2 start = Vector2.Lerp(min, max, (float)i / verticalRays); //Poner el (float) antes de i es para invocar ("Cast") i como valor float, ya que si divido un int/ int obtendria un int y yo quiero un float.
            Vector2 end = start + Vector2.up * (upRayLength + hurtBox.radius / 2);
            upRays[i] = Physics2D.Linecast(start, end, Raylayers.upRay);
            Debug.Log("upRays: " + upRays[i].fraction);

            if (upRays[i].fraction > 0)
            {
                connection = true;
                lastConnection = i;
            }
        }


        if (connection)
        {
            velocity = new Vector2(velocity.x, 0);
            transform.position += Vector3.forward * (upRays[lastConnection].point.y - hurtBox.radius);
            SendMessage("OnHeadHit", SendMessageOptions.DontRequireReceiver);
        }
        */



        //----------------------------------------------------------
        //RAYCASTS
        //Version propia v1: FUNCIONA! :D

        float upRayLength = velocity.magnitude * Time.deltaTime;//El raycast tiene que ser exactamente la distancia que voy a recorrer en el siguiente frame

        bool connection = false;

        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), transform.up, upRayLength, layerMask);//Comentado temp. para probar colisiones con rebotes
        //RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), new Vector2(velocity.x, velocity.y), upRayLength, layerMask);//Experimental
        //Debug.DrawRay(transform.position, transform.up, Color.red, 10f, false);

        if (hit.collider != null)
        {
            if (GameManager.Instance.sfx && !deadStun)
                frontHitSfx.Play();
            Debug.Log("You hit: " + hit.collider.gameObject.name);
            connection = true;
            //lastConnection = i;
            //lastConnection = hit.transform.position;//De esta forma siempre devuelve el centro del collider total, no el punto de contacto
            lastConnection = new Vector2(hit.point.x, hit.point.y);//Guardo el punto exacto de la colision
            lastHitNormal = hit.normal.normalized;//Guardo tambien el vector normal a la superficie colisionada
            myHitDir = (lastConnection - transform.position).normalized;//Guardo el vector con mi dirección en el momento de la colisión
            //Debug.DrawLine(lastConnection, lastHitNormal, Color.blue, 10);//Esto dibuja una linea azul en la escena
            
        }

        //COMENTADO TEMPORALMENTE (ya no), ESTE ES EL BUENO ORIGINAL:
        if (connection && !jumping && !deadStun)
        {
            playerHP.TakeDamage(4);
            StartCoroutine(HitStun(1f));//Meto un instante (en segundos) de stun para que no pueda moverme hasta que no estoy lo suficientemente alejado del borde como para atravesarlo?
            transform.Translate(transform.up * (upRayLength - (hurtBox.radius * 1f)));//Comentado temporalmente. Esto me transporta al lugar exacto del impacto.
            velocity = new Vector3(velocity.x, -velocity.y * 0.3f, velocity.z); // Usar esta linea para detener la nave en seco. Comentado temporalmente para hacer pruebas de colisiones:

            //MODO INFINITY:
            if (infinity)
            {
                brakeBar.TakeBrakePoints(10f);
                brakeStun = Time.time + 5;//5s de brakeStun (tiempo de espera de recarga de brakebar)
            }
        }

        //V.2:
        //if (connection && !jumping && !deadStun && !hitStun)
        //{
        //    playerHP.TakeDamage(4);
        //    StartCoroutine(HitStun(Mathf.Clamp(Mathf.Abs(velocity.y) * 0.002f, 0.1f, 1.5f)));//Meto stun para que no pueda moverme hasta que no estoy lo suficientemente alejado del borde como para atravesarlo?
        //    transform.Translate(transform.up * (upRayLength - (hurtBox.radius * 1f)));//Comentado temporalmente. Esto me transporta al lugar exacto del impacto.
        //    rb2d.AddRelativeForce(myHitDir * Mathf.Abs(velocity.y) * 10f);//Antes 0.05f


        //    //MODO INFINITY:
        //    if (infinity)
        //    {
        //        brakeBar.TakeBrakePoints(10f);
        //        brakeStun = Time.time + 5;//5s de brakeStun (tiempo de espera de recarga de brakebar)
        //    }
        //}





        //JUMPING UNUSED POR AHORA
        /*
        //Jumping v1: la idea es usar un Vector3 y enchufar el movimiento con el transform.traslade igual que en el avance
        //La nave se eleva si esta jumping y su altura no supera la altura maxima ceiling:
        if (jumping && rising && transform.position.z >= -max_height)
        {
            velocity = new Vector3(velocity.x, velocity.y, Mathf.Max(velocity.z - lift_speed * 50 * Time.deltaTime, -max_height));
            newVelocityZ = velocity.z;
            //Debug.Log("newVel Z " + newVelocityZ);
            //currentZ -= lift_speed * Time.deltaTime; //Resto componente Z porque los ejes en mi escena estan invertidos (eje -Z sube)...
            //Debug.Log("Subiendo");
        }
        else
        {
            if (jumping && transform.position.z < groundZ)
            {
                rising = false;
                falling = true;
                //Si la nave esta por encima del nivel de la pista (groundZ), seguira cayendo:
                newVelocityZ = Mathf.Min(velocity.z + fall_speed * 50 * Time.deltaTime, maxFallSpeed);
                //currentZ += fall_speed * Time.deltaTime; //Sumo Z porque el eje Z esta invertido en mi escena...
                //Debug.Log("Bajando");
            }
            else
            {
                if (jumping && falling)
                {
                    //Debug.Log("Landing!");
                    SetTransformZ(groundZ);
                    //currentZ = transform.position.z;
                    newVelocityZ = 0f;
                    //Debug.Log("currentZ " + currentZ);
                    velocity = new Vector3(velocity.x, velocity.y, newVelocityZ);
                    jumping = false;
                }
            }
        }

        if (jumping)
        {
            //Movimiento con traslade:
            //transform.Translate(velocity * Time.deltaTime); //OPTIMIZAR: Aquí en lugar de moverme debería sumar el termino Z al vector3 velocity!
            velocity = new Vector3(velocity.x, velocity.y, newVelocityZ);
            //transform.position += transform.forward * (currentZ * Time.deltaTime); //Método viejo de moverme
        }
        //JUMPING hasta aquí
        */


        //Jumping v1: la idea es usar un Vector3 y enchufar el movimiento con el transform.traslade igual que en el avance
        //La nave se eleva si esta jumping y su altura no supera la altura maxima ceiling:
        if (jumping && rising && transform.position.z >= -max_height)
        {
            velocity = new Vector3(velocity.x, velocity.y, Mathf.Max(velocity.z - lift_speed * 50 * Time.deltaTime, -max_height));
            newVelocityZ = velocity.z;
            //Debug.Log("newVel Z " + newVelocityZ);
            //currentZ -= lift_speed * Time.deltaTime; //Resto componente Z porque los ejes en mi escena estan invertidos (eje -Z sube)...
            //Debug.Log("Subiendo");
        }
        else
        {
            if (jumping && transform.position.z < groundZ)
            {
                rising = false;
                falling = true;
                //Si la nave esta por encima del nivel de la pista (groundZ), seguira cayendo:
                newVelocityZ = Mathf.Min(velocity.z + fall_speed * 50 * Time.deltaTime, maxFallSpeed);
                //currentZ += fall_speed * Time.deltaTime; //Sumo Z porque el eje Z esta invertido en mi escena...
                //Debug.Log("Bajando");
            }
            else
            {
                if (jumping && falling)
                {
                    //Debug.Log("Landing!");
                    SetTransformZ(groundZ);
                    //currentZ = transform.position.z;
                    newVelocityZ = 0f;
                    //Debug.Log("currentZ " + currentZ);
                    velocity = new Vector3(velocity.x, velocity.y, newVelocityZ);
                    jumping = false;
                }
            }
        }

        if (jumping)
        {
            //Movimiento con traslade:
            //transform.Translate(velocity * Time.deltaTime); //OPTIMIZAR: Aquí en lugar de moverme debería sumar el termino Z al vector3 velocity!
            velocity = new Vector3(velocity.x, velocity.y, newVelocityZ);
            //transform.position += transform.forward * (currentZ * Time.deltaTime); //Método viejo de moverme
        }
        //JUMPING hasta aquí


        //Botón del pausa no táctil:
        if (Input.GetKeyDown("space") || Input.GetKey(KeyCode.Joystick1Button7))
        {
            LevelManager.Instance.PauseToogle();
        }


    }


    void LateUpdate()
    {
        if (semaphore.lightsOn)
        {
            if (GameManager.Instance.engineSfx)
                jetSound.pitch = LowPitch;//Motor al ralentí hasta que se apaga el semáforo.
            return;
        }

        //Piloto automatico:
        if (autoPilot)
            return;

        //if (hitStun)
        //    return;

        if (deadStun)
            return;

        //apply movement. Time.deltaTime=time since last frame
        //if(!hitStun)//Pruebas
        transform.Translate(velocity * Time.deltaTime);

        //this.transform.forward = FindObjectOfType<Camera>().transform.forward;  //TU FUMAS, ¿CÓMO?...
        //transform.LookAt(Camera.main.transform.position, -Vector3.up); //TENGO UNA CHINA...

        /*//Quito la animacion de los giros aqui porque tal cual esta hecha tiene lag en desaparecer.. Prefiero como queda sin animacion lateral
        if (tilting)
        {
            tilting = false;
            anim.SetBool("Right", false);
            anim.SetBool("Left", false);
        }*/

        //Audio motor:
        if (GameManager.Instance.engineSfx)
        {
            if (!LevelManager.Instance.gamePaused && !LevelManager.Instance.gameOver)
            {
                float engineRevs = Mathf.Abs(velocity.y) * SpeedToRevs;
                jetSound.pitch = Mathf.Clamp(engineRevs, LowPitch, HighPitch);
            }
        }


    }

    void Braking()
    {
        if (LevelManager.Instance.gamePaused) return;

        if (infinity)//Si estoy en infinity duplico código pero no freno si no me quedan brakepoints, los sustraigo y en su caso acelero aunque pulse el freno
        {
            if (!brakeButton || brakeBar.bp <= 0)//Si no freno o no me quedan bp, acelero:
            {
                anim.SetBool("Accel", true);
                if (newVelocityY < maxSpeed * 0.5f)//Aqui he puesto como umbral para que no desacelere shiftear, la mitad de la vel max, pero podría variarlo e incluso hacer que la desaceleración varie segun la vel (a lo accelRamp).
                {
                    //velocity = new Vector3(velocity.x, Mathf.Min(velocity.y + acceleration, 999), velocity.z);
                    velocity = new Vector3(velocity.x, Mathf.Min(velocity.y + acceleration * Time.deltaTime, 999), velocity.z);
                }
                else
                {
                    //velocity = new Vector3(velocity.x, Mathf.Min(shiftButton ? velocity.y - 1 : velocity.y + acceleration, 999), velocity.z); //Si se derrapa con shift, se frena de manera constante. Velocity.y siempre es positivo, si no la nave iria para atras.
                    velocity = new Vector3(velocity.x, Mathf.Min(shiftButton ? velocity.y - 50 * Time.deltaTime : velocity.y + acceleration * Time.deltaTime, 999), velocity.z); //Si se derrapa con shift, se frena de manera constante. Velocity.y siempre es positivo, si no la nave iria para atras.
                }
                newVelocityY = velocity.y; //Ojo, el turbo esta aplicado mas arriba con boostMultiplier, en la rampa de aceleracion.
                                           //if (newVelocityY <= 0) newVelocityY = 0;//Cuando la velocidad baja de 0 evitamos que vaya marcha atras fijandola en 0.
            }

            else if (brakeButton)//Si freno (Infinity):
            {
                brakeBar.TakeBrakePoints(0.5f);//Pierdo energía de frenado al frenar
                brakeStun = Time.time + 1;//Añado un segundo de brakeStun (tiempo hasta que vuelva a rellenarse la barra de freno autom.)
                if (brakeBar.bp > 0)//Si me quedan bp, freno. Si no, no:
                {
                    //Aplica el freno
                    anim.SetBool("Accel", false);
                    int modifier = velocity.y > 0 ? -1 : 1; //modifier indica sirve para frenar en ambos sentidos del eje Y
                    newVelocityY += (brakeFriction * modifier) * shiftFriction * (1 + (1 - extInfluence)) * Time.deltaTime; //shiftFriction solo influye (es != 1) cuando se derrapa.

                    if (velocity.y < 2)//Cuando la velocidad tiende a 0 al frenar, que sea 0.
                    {
                        anim.SetBool("Accel", false);
                        newVelocityY = 0f;

                    }
                    else//Aplico freno regenerativo
                    {
                        //playerHP.HealDamage(brakeFriction * 0.1f);
                        playerHP.HealDamage(brakeFriction * 0.2f * Time.deltaTime);
                    }
                }

            }
        }
        else//Si no estoy en infinity, aplico normas originales
        {
            if (!brakeButton)//Si no freno (lo normal vamos)
            {
                anim.SetBool("Accel", true);
                if (newVelocityY < maxSpeed * 0.5f)//Aqui he puesto como umbral para que no desacelere shiftear, la mitad de la vel max, pero podría variarlo e incluso hacer que la desaceleración varie segun la vel (a lo accelRamp).
                {
                    //velocity = new Vector3(velocity.x, Mathf.Min(velocity.y + acceleration, 999), velocity.z);
                    velocity = new Vector3(velocity.x, Mathf.Min(velocity.y + acceleration * Time.deltaTime, 999), velocity.z);
                }
                else
                {
                    //velocity = new Vector3(velocity.x, Mathf.Min(shiftButton ? velocity.y - 1 : velocity.y + acceleration, 999), velocity.z); //Si se derrapa con shift, se frena de manera constante. Velocity.y siempre es positivo, si no la nave iria para atras.
                    velocity = new Vector3(velocity.x, Mathf.Min(shiftButton ? velocity.y - 50 * Time.deltaTime : velocity.y + acceleration * Time.deltaTime, 999), velocity.z); //Si se derrapa con shift, se frena de manera constante. Velocity.y siempre es positivo, si no la nave iria para atras.
                }
                newVelocityY = velocity.y; //Ojo, el turbo esta aplicado mas arriba con boostMultiplier, en la rampa de aceleracion.

            }

            else if (brakeButton)//Si freno:
            {
                //Aplica el freno
                anim.SetBool("Accel", false);
                int modifier = velocity.y > 0 ? -1 : 1; //modifier indica sirve para frenar en ambos sentidos del eje Y
                newVelocityY += (brakeFriction * modifier) * shiftFriction * (1 + (1 - extInfluence)) * Time.deltaTime; //shiftFriction solo influye (es != 1) cuando se derrapa.
                                                                                                                        //newVelocityY = (velocity.y > 0 ? newVelocityY -= brakeFriction * shiftFriction * (1 + (1 - extInfluence)) * Time.deltaTime : newVelocityY += brakeFriction * (1 + (1 - extInfluence)) * shiftFriction * Time.deltaTime);

                if (velocity.y < 2)//Cuando la velocidad tiende a 0 al frenar, que sea 0.
                {
                    anim.SetBool("Accel", false);
                    newVelocityY = 0f;

                }
                else//Aplico freno regenerativo
                {
                    //playerHP.HealDamage(brakeFriction * 0.1f);
                    playerHP.HealDamage(brakeFriction * 0.2f * Time.deltaTime);
                }

            }
        }

    }




    void AccelerationRamp()
    {
        //Parto de las float aceleracion (fwd_accel) y velMax (maxSpeed) que dependen de cada nave.

        if (velocity.y < maxSpeed * 0.4f * extInfluence)
        {
            acceleration = fwd_accel * boostMultiplier * extInfluence;
            //acceleration = fwd_accel * boostMultiplier * extInfluence * Time.deltaTime;//Tengo que aumentar fwd_accel x50 y quitar el 50 de aquí para eficiencia

        }
        else if (velocity.y < maxSpeed * 0.8f * extInfluence)
        {
            acceleration = fwd_accel * 0.5f * boostMultiplier * extInfluence;//BoostMultiplier actua cuando hay turbo, si no es 0.
            //acceleration = fwd_accel * 0.5f * boostMultiplier * extInfluence * 50 * Time.deltaTime;//BoostMultiplier actua cuando hay turbo, si no es 0.

        }
        else if (velocity.y < maxSpeed * 0.95f * extInfluence)
        {
            acceleration = fwd_accel * 0.25f * boostMultiplier * extInfluence;//BoostMultiplier actua cuando hay turbo, si no es 0.
            //acceleration = fwd_accel * 0.5f * boostMultiplier * extInfluence * 50 * Time.deltaTime;//BoostMultiplier actua cuando hay turbo, si no es 0.

        }
        else if (velocity.y < maxSpeed * extInfluence)
        {
            acceleration = fwd_accel * 0.05f * boostMultiplier * extInfluence;//Lo suyo sería que interpolase linealmente de 0.5f a 0.25f en lugar de un cambio abrupto...Implementar mas adelante!!
            //acceleration = fwd_accel * 0.25f * boostMultiplier * extInfluence * Time.deltaTime;//Lo suyo sería que interpolase linealmente de 0.5f a 0.25f en lugar de un cambio abrupto...Implementar mas adelante!!
        }
        else
        {
            //acceleration = boosting ? fwd_accel * 0.3f * extInfluence : fwd_accel * -0.05f/** (1 - extInfluence)*/;//Una vez llego a mi vel max, si tengo turbo acelero un 20%, y si no freno un 10% hasta mi velMax.
            acceleration = boosting ? fwd_accel * 0.3f * extInfluence : fwd_accel * -0.05f * extInfluence;//Una vez llego a mi vel max, si tengo turbo acelero un 20%, y si no freno un 10% hasta mi velMax.
        }

    }




    //YOU GOT BOOST POWER!!!!
    void BoostPower()
    {
        boostAvailable = false;
        boosting = true;
        boostStart = Time.time;
        //Debug.Log("Boost Start " + boostStart);
        boostRemain = boostStart + boostTime;
        Instantiate(burst, transform, false);
        if (GameManager.Instance.sfx)
            boostSfx.Play();

        for (int i = 0; i < ps.Length; i++)
        {
            //ps[i].startColor = new Color(1, 1, 0.2f, 1);  //Este metodo esta obsoleto en Unity 5.5, pero funciona bien igual.

            var main = ps[i].main; //Método engorroso introducido en unity 5.5 para modificar ParticleSystems via Scripts...
            main.startColor = new Color(0, 1, 0.5f, 1);
        }
    }

    //YOU GOT BOOST POWER!!!!
    void BoostPowerArrow()
    {
        boostAvailable = false;
        boosting = true;
        boostStart = Time.time;
        //Debug.Log("Boost Start " + boostStart);
        boostRemain = boostStart + boostTime;
        Instantiate(burst, transform, false);
        if (GameManager.Instance.sfx)
            boostSfx.Play();

        for (int i = 0; i < ps.Length; i++)
        {
            //ps[i].startColor = new Color(1, 1, 0.2f, 1);  //Este metodo esta obsoleto en Unity 5.5, pero funciona bien igual.

            var main = ps[i].main; //Método engorroso introducido en unity 5.5 para modificar ParticleSystems via Scripts...
            main.startColor = new Color(1, 1, 0.2f, 1);
        }
    }


    //void GetColliderShape()
    //{
    //    /*
    //    //Añadidos para lanzar varios rayos y poder discernir la geometría del muro:
    //    int lastConnection = 0;

    //    Vector2 min = new Vector2(transform.position.x - hurtBox.radius, hurtBox.offset.y);
    //    Vector2 max = new Vector2(transform.position.x + hurtBox.radius, hurtBox.offset.y);

    //    RaycastHit2D[] upRays = new RaycastHit2D[verticalRays];
    //    //IDEA: llevarme el for a una funcion aparte y que mi nave responda solo a un rayo, como antes...
    //    for (int i = 0; i < verticalRays; i++)
    //    {
    //        Vector2 startPoint = Vector2.Lerp(min, max, (float)i / verticalRays); //Esto huele a BUG
    //        Vector2 direction = startPoint + new Vector2(transform.up.x, transform.up.y); //Y esto tmb.
    //        upRays[i] = Physics2D.Raycast(startPoint, direction, upRayLength, layerMask);   //SEGUIR AQUI CONSTRUCCION FOR...
    //        Debug.Log("StarPoints " + startPoint);
    //        Debug.Log("Direccion relativa" + direction);
    //        Debug.Log("Yo voy " + transform.up);

    //        if (upRays[i].collider != null)
    //        {
    //            Debug.Log("You hit: " + upRays[i].collider.gameObject.name);
    //            connection = true;
    //            //Meollo para calcular el ángulo del muro:
    //            if (lastConnection > 1)
    //            {
    //                if (i > lastConnection)
    //                {
    //                    rightPoint = upRays[i].point; //Este es el punto de la colision del último Ray (el de más a la derecha).
    //                }
    //            }
    //            else
    //            {
    //                leftPoint = upRays[i].point; //Este es el punto de la colisión del primer Ray (el de más a la izquierda).
    //                rightPoint = new Vector2(0, 0); //
    //            }
    //            Debug.Log("leftPoint " + leftPoint);
    //            Debug.Log("rightPoint " + rightPoint);
    //            lastConnection = i;

    //            float angle = Vector2.Angle(leftPoint - rightPoint, Vector2.right); //Este es el ángulo que forma el muro con la horizontal
    //            Debug.Log("Angulo " + angle);
    //        }
    //    }
    //    //Hasta aqui la version con varios raycasts propia que no funca.*/
    //}

    //OnTriggerEnter2D is called whenever this object overlaps with a trigger collider.
    void OnTriggerEnter2D(Collider2D other)
    {
        //Check the provided Collider2D parameter other to see if it is tagged "PickUp", if it is...
        if (other.gameObject.CompareTag("PickUp"))
        {
            //... then set the other object we just collided with to inactive.
            other.gameObject.SetActive(false);

            //Add one to the current value of our count variable.
            count = count + 1;

            //Update the currently displayed count by calling the SetCountText function.
            //SetCountText();
        }

        else if(other.tag == "Tutorial" && !deadStun)
        {
            Time.timeScale = 0;
            tutorialText.text = other.GetComponent<Text>().text;
            tutorialCanvas.GetComponent<Image>().SetTransparency(1);
            other.gameObject.SetActive(false);
        }

        else if (other.gameObject.CompareTag("Jumper"))
        {
            groundZ = transform.position.z;
            currentZ = groundZ;
            //max_height = Mathf.Abs(ceiling * velocity.y);
            max_height = Mathf.Abs(jump * velocity.y);
            jumping = true;
            rising = true;
        }

        else if (other.gameObject.CompareTag("ArrowBoost"))
        {
            if (!deadStun)
            {
                arrowBoost = true;
                BoostPowerArrow();
            }
        }

        //else if (other.gameObject.CompareTag("Twister"))
        //{
        //    startTwisterPos = transform.position.y;//Esto es la marca para conocer el progreso del twister.
        //}

        //else if (other.gameObject.CompareTag("OutTrack"))
        //{
        //    if (transform.position.z == 0)
        //    {
        //        Debug.Log("OUT COURSE!!");
        //        //LevelManager.Instance.GameOver();
        //        StartCoroutine(OutCourse());
        //        //transform.position = GetComponent<AIpath>().nodes[GetComponent<AIpath>().currentNode - 1].transform.position;

        //    }
        //}
        else if (other.tag == "Dirt")
        {
            extInfluence = 0.3f;
            velocity = new Vector3(velocity.x, velocity.y * 0.75f, velocity.z);
        }

        else if (other.tag == "Ice")
        {
            Kext = 0.4f;
        }
        else if (GameManager.Instance.sfx)
        {
            if (other.tag == "Heal" && !deadStun)
            {
                if (!semaphore.lightsOn)
                    healSfx.Play();//sonido de heal (comienzo)
            }
            else if (other.tag == "Lava")
            {
                lavaSfx.Play();
            }
        }

    }



    void OnCollisionEnter2D(Collision2D col)//Al chocarme con algo, hasta que no me separo bastante y vuelvo a chocarme no se reproduce.
    {

        if (col.gameObject.CompareTag("Bordes"))
        {//If para determinar el daño al chocarme contra bordes de la pista (lo separo del chocque entre naves para asignar distintos daños segun el tipo de borde):
            playerHP.TakeDamage(2);
            if(GameManager.Instance.sfx && !deadStun)
                hitWallSfx.Play();

            //Rebotes al chocar con pared, muy buggy:
            //StartCoroutine(HitStun(0.5f));
            //StartCoroutine(HitStunBounce(50));
            //lastHitNormal = col.contacts[0].normal;
            //Vector2 dir = Vector2.Reflect(transform.up, lastHitNormal);
            //rb2d.AddRelativeForce(dir * velocity.y);

            //Vector2 dirForce = col.transform.position - transform.position;
            //Vector2 dirForce = Vector2.Reflect(col.transform.position - transform.position, lastHitNormal);
            //GetComponent<Rigidbody2D>().AddRelativeForce(dirForce * (1 + velocity.y * 0.02f));
            //Debug.Log("Pared-Ostion");

        }

        if (col.gameObject.CompareTag("Racer") || col.gameObject.CompareTag("Player"))//Esta funcion provoca colisiones entre naves. Aquí se implantan en el player. En las cpus esta implantado en AIpath.
        {
            if (GameManager.Instance.sfx && !deadStun)
                hitWallSfx.Play();
            playerHP.TakeDamage(1);
            Vector2 dirForce = col.transform.position - transform.position;
            GetComponent<Rigidbody2D>().AddRelativeForce(dirForce * (col.gameObject.GetComponent<AIpath>().speed + velocity.y * 3f));
            velocity = new Vector3(velocity.x, velocity.y * 0.9f, velocity.z); // Usar esta linea para detener la nave (PRUEBA)
            //Debug.Log("Ostion");

        }

        if (col.gameObject.CompareTag("Mine"))//Esta funcion provoca salir disparado al tocar una mina:
        {
            if(GameManager.Instance.sfx)
                explosionBig.Play();
            Vector2 dirForce = col.transform.position - transform.position;
            GetComponent<Rigidbody2D>().AddRelativeForce(dirForce * (2000));
            StartCoroutine(HitStun(0.5f));
            playerHP.TakeDamage(10);

            //Debug.Log("Ostion");

        }

    }




    void OnCollisionStay2D(Collision2D bump)//Cuando me "calo" contra un borde esta funcion es continua.
    {
        //Debug.Log("staybump");
        if (bump.gameObject.CompareTag("Bordes"))
        {
            if (GameManager.Instance.sfx && !deadStun)
                hitWallSfx.Play();
            playerHP.TakeDamage(1f * Time.deltaTime);//Pensado para cuando me choco contra bordes y me sigo chocando.
        }
    }


    //This function updates the text displaying the number of objects we've collected and displays our victory message if we've collected all of them.
    //void SetCountText()
    //{
    //    //Set the text property of our our countText object to "Count: " followed by the number stored in our count variable.
    //    countText.text = "Count: " + count.ToString();

    //    //Check if we've collected all 12 pickups. If we have...
    //    if (count >= 12)
    //        //... then set the text property of our winText object to "You win!"
    //        winText.text = "You win!";
    //}

    void SetTransformZ(float n)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, n);
        falling = false;
        //Debug.Log("Puesta a cero" + transform.position.z);
    }

    void DynamicTurning()
    {
        if (twisting) return;
        //rotF = Quaternion.AngleAxis(5 * Time.deltaTime, Vector3.forward);//Si descomento estas dos lineas se utilizan los rotF/B declarados en Start() y por laguna razon el giro es mucho mas rapido...
        //rotB = Quaternion.AngleAxis(-5 * Time.deltaTime, Vector3.forward);

        currentAngle = cam.transform.localEulerAngles;//Muy importante poner LOCAL para trabajar con los angulos relativos al gameobject camara, si no coge los absolutos y no cudra nada.

        if (tilt != lastTiltValue)//Si giro:
        {
            if (tilt > lastTiltValue)//Si giro a la IZQUIERDA
            {
                if (currentAngle.z > 360 - angleShift || currentAngle.z < angleShift)
                {
                    cam.transform.rotation *= rotB;//Angulo negativo giro a la izq.

                }
                else
                {
                    cam.transform.localEulerAngles = new Vector3(currentAngle.x, currentAngle.y, 360 - angleShift);
                }

            }
            else if (tilt < lastTiltValue) //Si giro a la derecha:
            {

                if (currentAngle.z < angleShift || currentAngle.z > 360 - angleShift)//Si aun tengo margen para girar la camara:
                {
                    cam.transform.rotation *= rotF;//Angulo negativo giro a la drcha.

                }
                else//Y si no, hago que la camara no gire mas.
                {
                    cam.transform.localEulerAngles = new Vector3(currentAngle.x, currentAngle.y, angleShift);
                }
            }

        }
        else
        {
            currentAngle.z = Mathf.LerpAngle(currentAngle.z, 0, 2 * Time.deltaTime);
            //currentAngle = new Vector3(Mathf.Lerp(currentAngle.x, originalAngle.x, Time.deltaTime), Mathf.Lerp(currentAngle.y, originalAngle.y, Time.deltaTime), Mathf.LerpAngle(currentAngle.z, 0, 2 * Time.deltaTime));
            cam.transform.localEulerAngles = currentAngle;
        }

    }//Unused (Funcional)


    void Twister360()
    {
        twisting = true;
        currentAngle = cam.transform.localEulerAngles;
        twisterPos = transform.position;

        //El collider mide 300 unidades de largo (el ancho lo adapto a la pista, me da igual).
        float largo = 300;//Esto es el recorrido donde se ubica el twister.

        float relativePos = twisterPos.y - startTwisterPos;

        currentAngle.z = (Mathf.Abs(twisterPos.y - startTwisterPos) / largo) * 360;

        cam.transform.localEulerAngles = currentAngle;//Asigno el angulo de la camara segun las modificaciones.

    }//Unused (Funcional)

    void SmoothTwister360(float startAnglePos)
    {
        currentAngle = cam.transform.localEulerAngles;
        twisting = true;
        //twisterPos = transform.position;

        ////El collider mide 300 unidades de largo (el ancho lo adapto a la pista, me da igual).
        //float largo = 300;//Esto es el recorrido donde se ubica el twister.

        //float relativePos = twisterPos.y - startTwisterPos;

        //currentAngle.z = (Mathf.Abs(twisterPos.y - startTwisterPos) / largo) * 360;

        currentAngle.z = Mathf.LerpAngle(currentAngle.z, startAnglePos + 360, 2 * Time.deltaTime);

        cam.transform.localEulerAngles = currentAngle;//Asigno el angulo de la camara segun las modificaciones.

    }//Unused (funcional)

    void OnTriggerStay2D(Collider2D col)
    {
        //if (col.tag == "Heal" && Time.time >= healTimeStamp + 3.75f)
        //{
        //    Debug.Log("LoopHeal? stamp:" + healTimeStamp);
        //    loopHealSfx.Play();//Reproduzco el sonido en loop de heal cuando acaba el healSfx 
        //}


        //else if (col.transform.tag == "Twister")//Unused (funcional)
        //{

        //    Twister360();//Va algo caladete y es confuso de cojones para el player.

        //    //Version Smooth pero que no se adapta a la posicion del player, si este girase hacia el otro lado rotaria igual...
        //    currentAngle = cam.transform.localEulerAngles;
        //    float startAnglePos = currentAngle.z;
        //    SmoothTwister360(startAnglePos);
        //}

        if (col.gameObject.CompareTag("OutTrack"))
        {
            if (transform.position.z == 0)
            {
                Debug.Log("OUT COURSE!!");
                //LevelManager.Instance.GameOver();
                if(!outCourse)
                    StartCoroutine(OutCourse());
                //transform.position = GetComponent<AIpath>().nodes[GetComponent<AIpath>().currentNode - 1].transform.position;

            }
        }

    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Dirt")
        {
            extInfluence = 1;
        }

        else if (col.tag == "Ice")
        {
            Ki = 2;
            Kext = 1;
        }

        else if (GameManager.Instance.sfx)
        {
            if (col.tag == "Heal")
            {
                healSfx.Stop();
                //loopHealSfx.Stop();
            }

            else if (col.tag == "Lava")
            {
                if (GameManager.Instance.sfx)
                    lavaSfx.Stop();
            }
        }

        //if (col.transform.tag == "Twister")
        //{
        //    twisting = false;
        //}
    }



    void TurningDetectionDX()
    {
        if ((CrossPlatformInputManager.GetAxis("Horizontal") + Input.GetAxis("Horizontal")) != 0)
        {
            steering = true;//steering me indica que se esta tocando el volante

            if (lastTurnValue == lastTurnValue2)//Este if es para detectar flancos de subida de steering (startSteering).
            {
                startSteering = true;
                lerpKi = true;
                //steerTimeStamp = Time.time;
            }
            else
            {
                startSteering = false;
            }

            if (turn > lastTurnValue)//Si giro a la IZQUIERDA (Este if detecta si giro a derecha o izquierda):
            {
                //bgCamTransform.position += new Vector3(-tilt, 0, 0) * Time.deltaTime;
                turningLeft = true;

                if (turningRight)//Este if detecta los cambios de sentido de volante (izq a derecha y viceversa sin soltar el volante):
                {
                    changeDirection = true;
                    turningRight = false;

                }
                else
                {
                    changeDirection = false;
                }


            }
            if (turn < lastTurnValue) //Si giro a la derecha:
            {
                //bgCamTransform.position += new Vector3(tilt, 0, 0) * Time.deltaTime;
                turningRight = true;

                if (turningLeft)//Este if detecta los cambios de sentido de volante (izq a derecha y viceversa sin soltar el volante):
                {
                    changeDirection = true;
                    turningLeft = false;

                }
                else
                {
                    changeDirection = false;
                }


            }
        }
        else//No oestoy tocando el volante:
        {
            steering = false;
            turningLeft = false;
            turningRight = false;
            changeDirection = false;
            Ki = 0;
            lerpKi = false;
            //Kt = 0;//He quitado esta influencia porque no le encuentro mucho sentido.
        }
    }


    //void TurningFriction()//Funcion para perder velocidad al girar:
    //{
    //    //if(shifting && (turningLeft || turningRight)){
    //    //    acceleration *= -1;
    //    //}
    //}

    public IEnumerator HitStun(float t)//Esta corrutina hara que la bool hitStun este activada t segundos:
    {
        hitStun = true;
        yield return null;
        yield return new WaitForSeconds(t);
        //velocity = new Vector3(0, rb2d.velocity.magnitude, 0);
        //rb2d.velocity = Vector3.zero;
        if (!deadStun)
            hitStun = false;

    }

    public void imSoDead()//Animacion al quedarme sin energía en pista (esta funcion se activa desde healthbar al quedarme sin hp)
    {
        if (dyingTrigger)//Este if hace que se ejecute esta funcion solo una vez.
        {
            if (infinity)
            {
                LevelManager.Instance.duration = GetComponent<Laps>().timer;
            }
            boosting = false;
            deadStun = true;//Bool que me indica que estoy muerto cobra
            Vector2 dir = new Vector2(0, transform.localPosition.y);//Calculo dirección hacia delante
            GetComponent<Rigidbody2D>().AddRelativeForce(dir * velocity.y);//Aplico fuerza hacia delante para darle vidilla a la muerte
            //StartCoroutine(HitStun(20));//Hitstun para no moverme aunque con deadStun esto es redundante...revisar para optimizar
            timerSeconds = 4;//Lo que durara la animacion de la nave mini-explotando antes de explotar del todo...
            Instantiate(explosions, transform, false);//Gran explosion inicial
            if (GameManager.Instance.sfx)
            {
                explosion1.Play();
                lavaSfx.Play();
            }
            Instantiate(smallExplosions, transform, false);//Explosiones pequeñitas (al estar el prefab en looping, se van sucediendo solas.
            InvokeRepeating("CountDownGameOver", 0.1f, 0.5f);//Esto es para la cuenta atras hasta que explota definitamente
            dyingTrigger = false;//para que no se repita esta funcion
            Debug.Log("imSoDead despues coroutine");

        }


    }



    private void CountDownGameOver()
    {
        Debug.Log("timerSeconds " + timerSeconds);
        if (timerSeconds >= 0.4f)
        {
            //Explode();
            timerSeconds -= 0.5f;
        }
        else
        {
            if (GameManager.Instance.sfx)
            {
                lavaSfx.Stop();
                explosion1.Stop();

            }
            Destroy(GameObject.Find("SmallExplosionEffect(Clone)"));//Detiene las miniexplosiones
            Instantiate(explosions, transform, false);//Gran pete final
            if (GameManager.Instance.engineSfx)
                jetSound.Stop();
            if (GameManager.Instance.sfx)
                explosionBig.Play();
            Instantiate(smoke, transform, false);//Instancio humo
            spriter.sprite = shipBurnt;//Cambio a sprite de nave chamuscada
            CancelInvoke("CountDownGameOver");//Cancelo volver a llamar a esta funcion
            timerSeconds = 0;//Esto es para dejar la camara fija al explotar la nave...

            for (int i = 0; i < ps.Length; i++)
            {
                var main = ps[i].main; //Método engorroso introducido en unity 5.5 para modificar ParticleSystems via Scripts...
                main.startSize = 0f;
                main.startLifetime = 0.0f;

            }

            //Para poder oscurecer la pantalla necesito llamar la corrutina cada frame, por lo que tengo que poner la línea StartCoroutine en el update
            //Creo una bool para acceder a ella:
            deadBurnt = true;
        }
    }

    private IEnumerator FadeBlack()
    {
        
        Debug.Log("start de FadeBlack Coroutine");
        if (GameManager.Instance.music)
        {
            if (!infinity && !LevelManager.Instance.gameOverIntro.isPlaying && !LevelManager.Instance.gameOverClip.isPlaying && !LevelManager.Instance.gameOver)
                LevelManager.Instance.gameOverIntro.Play();
        }

        if (fullScreen.color.a < (infinity ? 0.75f : 1))
        {
            fade += 0.2f * Time.deltaTime;
            fullScreen.color = new Color(0, 0, 0, fade);
            if (GameManager.Instance.music)
            {
                if (!infinity)
                    LevelManager.Instance.stageClip.volume -= 0.01f;
            }
        }
        else
        {
            //MODO INFINITY:
            if (infinity)
            {
                LevelManager.Instance.hideHUDrace();//Escondo el HUDRace
                infinityEnd = true;//Con esto detengo el update
                //transform.localScale = Vector3.zero;//Hago desaparecer al player una vez la pantalla esta oscura para que no haya solidos de golpe extraños...
                LevelManager.Instance.InfinityResults();
                fullScreen.gameObject.SetActive(false);
            }
            else
            {
                LevelManager.Instance.GameOver();
                fullScreen.gameObject.SetActive(false);
                yield return new WaitForSeconds(1);
            }
        }
    }

    public IEnumerator RedBlink()//Esta corutina se llama desde el script Healthbar sólo una vez al llegar o bajar del 20% de energía.
    {
        alarm = true;
        if (GameManager.Instance.sfx)
            lowEnergySfx.Play();
        //Alarm sound
        while (lowHp && !deadStun)
        {
            Debug.Log("1");
            spriter.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriter.color = new Color(1, 1, 1, 1);
            Debug.Log("2");
            
            yield return new WaitForSeconds(0.4f);
        }
        alarm = false;
        if (GameManager.Instance.sfx)
            lowEnergySfx.Stop();
    }

    private void PlayerStats(int index)
    {
        switch (index)
        {
            case 7://Nave nº7: Sky Falcon  Boost:B, Accel:C, Speed:B, Turn:B
                fwd_accel = 77;//Aceleracion estandar
                tBase = 226f;//Potencia de giro estandar
                tResist = 1 / (0.83f * maxSpeed);//Pérdida de capacidad de giro al aumentar la velocidad: 1.85f indica que se pierde un 15% a maxima velocidad.
                tInertia = 3.8f;//Tiempo que tarda en responder la nave al poner las manos en el volante o cambiar de dirección
                tTension = 1;//Unused
                tDecel = 1;//Unused
                shiftValue = 2.2f;//Potecia de giro al derrapar
                //acceleration = 2;//Se calcula en accelerationRamp en cada frame
                maxSpeed = 157;//Velocidad maxima sin usar turbo
                newShiftValue = 2.1f;//Unused
                newFrictionValue = 2.1f;//Perdida de velocidad al derrapar
                boostTime = 1.6f;//Tiempo en s que dura el turbo
                boost = 2.2f;//Potencia de aceleracion del turbo
                brakeFriction = 82f;//Potencia de freno estandar

                //Sonidos motor:
                GetComponent<AudioSource>().clip = engineClips[0];
                if(GameManager.Instance.engineSfx)
                    jetSound.Play();
                else
                    jetSound.Stop();

                //Propulsores:
                Instantiate(afterBurners[7], transform, false);

                break;

            case 6://Nave nº6: Snow Unicorn  Boost:B, Accel:D, Speed:D, Turn:A
                fwd_accel = 73f;//Aceleracion estandar
                tBase = 232f;//Potencia de giro estandar
                tResist = 1 / (0.79f * maxSpeed);//Pérdida de capacidad de giro al aumentar la velocidad: 1.85f indica que se pierde un 15% a maxima velocidad.
                tInertia = 4.2f;//Tiempo que tarda en responder la nave al poner las manos en el volante o cambiar de dirección
                tTension = 1;//Unused
                tDecel = 1;//Unused
                shiftValue = 2.2f;//Potecia de giro al derrapar
                //acceleration = 2;//Se calcula en accelerationRamp en cada frame
                maxSpeed = 138;//Velocidad maxima sin usar turbo
                newShiftValue = 2f;//Unused
                newFrictionValue = 1.8f;//Perdida de velocidad al derrapar
                boostTime = 1.5f;//Tiempo en s que dura el turbo
                boost = 2.3f;//Potencia de aceleracion del turbo
                brakeFriction = 84f;//Potencia de freno estandar

                //Sonidos motor:
                GetComponent<AudioSource>().clip = engineClips[0];
                if (GameManager.Instance.engineSfx)
                    jetSound.Play();
                else
                    jetSound.Stop();

                //Propulsores:
                Instantiate(afterBurners[6], transform, false);

                break;

            case 5://Nave nº5: Storm Piercer  Boost:C, Accel:B, Speed:C, Turn:B
                fwd_accel = 85f;//Aceleracion estandar
                tBase = 228f;//Potencia de giro estandar
                tResist = 1 / (0.83f * maxSpeed);//Pérdida de capacidad de giro al aumentar la velocidad: 1.85f indica que se pierde un 15% a maxima velocidad.
                tInertia = 3.5f;//Tiempo que tarda en responder la nave al poner las manos en el volante o cambiar de dirección
                tTension = 1;//Unused
                tDecel = 1;//Unused
                shiftValue = 2f;//Potecia de giro al derrapar
                //acceleration = 2;//Se calcula en accelerationRamp en cada frame
                maxSpeed = 143;//Velocidad maxima sin usar turbo
                newShiftValue = 2f;//Unused
                newFrictionValue = 2.2f;//Perdida de velocidad al derrapar
                boostTime = 1.2f;//Tiempo en s que dura el turbo
                boost = 2.1f;//Potencia de aceleracion del turbo
                brakeFriction = 77f;//Potencia de freno estandar

                //Sonidos motor:
                GetComponent<AudioSource>().clip = engineClips[0];
                if (GameManager.Instance.engineSfx)
                    jetSound.Play();
                else
                    jetSound.Stop();

                //Propulsores:
                Instantiate(afterBurners[5], transform, false);

                break;

            case 4://Nave nº4: Formula Classic  Boost:C, Accel:B, Speed:B, Turn:D
                fwd_accel = 87f;//Aceleracion estandar
                tBase = 223f;//Potencia de giro estandar
                tResist = 1 / (0.85f * maxSpeed);//Pérdida de capacidad de giro al aumentar la velocidad: 1.85f indica que se pierde un 15% a maxima velocidad.
                tInertia = 3.8f;//Tiempo que tarda en responder la nave al poner las manos en el volante o cambiar de dirección
                tTension = 1;//Unused
                tDecel = 1;//Unused
                shiftValue = 1.9f;//Potecia de giro al derrapar
                //acceleration = 2;//Se calcula en accelerationRamp en cada frame
                maxSpeed = 158;//Velocidad maxima sin usar turbo
                newShiftValue = 2f;//Unused
                newFrictionValue = 2.1f;//Perdida de velocidad al derrapar
                boostTime = 1.4f;//Tiempo en s que dura el turbo
                boost = 1.6f;//Potencia de aceleracion del turbo
                brakeFriction = 77f;//Potencia de freno estandar

                //Pruebas sonido motor:
                GetComponent<AudioSource>().clip = engineClips[3];
                if (GameManager.Instance.engineSfx)
                    jetSound.Play();
                else
                    jetSound.Stop();

                //Propulsores:
                Instantiate(afterBurners[4], transform, false);

                break;

            case 3://Nave nº3: Heavy-A  Boost:D, Accel:E, Speed:A, Turn:B
                fwd_accel = 69f;//Aceleracion estandar
                tBase = 226f;//Potencia de giro estandar
                tResist = 1 / (0.8f * maxSpeed);//Pérdida de capacidad de giro al aumentar la velocidad: 1.85f indica que se pierde un 15% a maxima velocidad.
                tInertia = 4.3f;//Tiempo que tarda en responder la nave al poner las manos en el volante o cambiar de dirección
                tTension = 1;//Unused
                tDecel = 1;//Unused
                shiftValue = 2.1f;//Potecia de giro al derrapar
                //acceleration = 2;//Se calcula en accelerationRamp en cada frame
                maxSpeed = 165;//Velocidad maxima sin usar turbo
                newShiftValue = 2f;//Unused
                newFrictionValue = 1.7f;//Perdida de velocidad al derrapar
                boostTime = 1.2f;//Tiempo en s que dura el turbo
                boost = 1.7f;//Potencia de aceleracion del turbo
                brakeFriction = 78f;//Potencia de freno estandar

                //Sonidos motor:
                GetComponent<AudioSource>().clip = engineClips[2];
                if (GameManager.Instance.engineSfx)
                    jetSound.Play();
                else
                    jetSound.Stop();

                //Propulsores:
                Instantiate(afterBurners[3], transform, false);

                break;

            case 2://Nave nº2: Rose Thunder Boost:A, Accel:A, Speed:E, Turn:D
                fwd_accel = 90f;//Aceleracion estandar
                tBase = 223f;//Potencia de giro estandar
                tResist = 1 / (0.82f * maxSpeed);//Pérdida de capacidad de giro al aumentar la velocidad: 1.85f indica que se pierde un 15% a maxima velocidad.
                tInertia = 3.8f;//Tiempo que tarda en responder la nave al poner las manos en el volante o cambiar de dirección
                tTension = 1;//Unused
                tDecel = 1;//Unused
                shiftValue = 1.9f;//Potecia de giro al derrapar
                //acceleration = 2;//Se calcula en accelerationRamp en cada frame
                maxSpeed = 131;//Velocidad maxima sin usar turbo
                newShiftValue = 2.1f;//Unused
                newFrictionValue = 1.8f;//Perdida de velocidad al derrapar
                boostTime = 1.7f;//Tiempo en s que dura el turbo
                boost = 2.5f;//Potencia de aceleracion del turbo
                brakeFriction = 83f;//Potencia de freno estandar

                //Sonidos motor:
                GetComponent<AudioSource>().clip = engineClips[1];
                if (GameManager.Instance.engineSfx)
                    jetSound.Play();
                else
                    jetSound.Stop();

                //Propulsores:
                Instantiate(afterBurners[2], transform, false);

                break;

            case 1://Nave nº1: Winged Cloud  Boost:E, Accel:B, Speed:D, Turn:A
                fwd_accel = 83f;//Aceleracion estandar
                tBase = 230f;//Potencia de giro estandar
                tResist = 1 / (0.87f * maxSpeed);//Pérdida de capacidad de giro al aumentar la velocidad: 1.85f indica que se pierde un 15% a maxima velocidad.
                tInertia = 3.5f;//Tiempo que tarda en responder la nave al poner las manos en el volante o cambiar de dirección
                tTension = 1;//Unused
                tDecel = 1;//Unused
                shiftValue = 2.3f;//Potecia de giro al derrapar
                //acceleration = 2;//Se calcula en accelerationRamp en cada frame
                maxSpeed = 140;//Velocidad maxima sin usar turbo
                newShiftValue = 2.1f;//Unused
                newFrictionValue = 2.1f;//Perdida de velocidad al derrapar
                boostTime = 0.9f;//Tiempo en s que dura el turbo
                boost = 1.5f;//Potencia de aceleracion del turbo
                brakeFriction = 80f;//Potencia de freno estandar

                //Sonidos motor:
                GetComponent<AudioSource>().clip = engineClips[0];
                if (GameManager.Instance.engineSfx)
                    jetSound.Play();
                else
                    jetSound.Stop();

                //Propulsores:
                Instantiate(afterBurners[1], transform, false);

                break;

            case 0://Nave nº 0: Golden Arrow  Boost:B, Accel:B, Speed:C, Turn:C
                fwd_accel = 80f;//Aceleracion estandar
                tBase = 225f;//Potencia de giro estandar
                tResist = 1 / (0.85f * maxSpeed);//Pérdida de capacidad de giro al aumentar la velocidad: 1.85f indica que se pierde un 15% a maxima velocidad.
                tInertia = 4f;//Tiempo que tarda en responder la nave al poner las manos en el volante o cambiar de dirección
                tTension = 1;//Unused
                tDecel = 1;//Unused
                shiftValue = 2f;//Potecia de giro al derrapar
                //acceleration = 2;//Se calcula en accelerationRamp en cada frame
                maxSpeed = 150;//Velocidad maxima sin usar turbo
                newShiftValue = 2f;//Unused
                newFrictionValue = 2f;//Perdida de velocidad al derrapar
                boostTime = 1.5f;//Tiempo en s que dura el turbo
                boost = 2f;//Potencia de aceleracion del turbo
                brakeFriction = 80f;//Potencia de freno estandar
                jump = 2;//Capacidad de salto (experimental)

                //Sonidos motor:
                GetComponent<AudioSource>().clip = engineClips[0];
                if (GameManager.Instance.engineSfx)
                    jetSound.Play();
                else
                    jetSound.Stop();

                //Propulsores:
                Instantiate(afterBurners[0], transform, false);
                break;
        }

        shiftValue *= 1.25f;

#if UNITY_STANDALONE
        tInertia = 100f;
        shiftValue *= 1.7f;
#elif UNITY_EDITOR
        //tInertia = 100f;
        //shiftValue *= 1.7f;
#endif
    }

    public IEnumerator BounceHitStun(float t)//Esta corrutina hara que la bool hitStun este activada t segundos:
    {
        bounceHitStun = true;
        yield return new WaitForSeconds(t);
        if (!deadStun)
            bounceHitStun = false;

    }

    public void LapsDisplay(int lap)//Funcion que actualiza las vueltas al pasar por meta (se llama desde Laps)
    {
        lapsDisplay.text = "Lap " + lap + " / " + (GetComponent<Laps>().finishLap - 1).ToString();

        if (GameManager.Instance.introName == "")//Si estoy en el tutorial:
        {
            lapsDisplay.text = "";
        }

        if (infinity)
        {
            lapsDisplay.text = "Lap " + lap;
        }

    }


    public IEnumerator UIasigner()
    {
        if (GameManager.Instance.gpEnding)
        {
            gpEnding = true;
            //autoPilot = true;//Activar para el scroll del fondo en piloto automatico
            rb2d = GetComponent<Rigidbody2D>();
            cam = GetComponentInChildren<Camera>();
            anim = GetComponent<Animator>();
            spriter = GetComponent<SpriteRenderer>();
            semaphore = FindObjectOfType<Semaphore>();
            playerHP = GetComponent<Healthbar>();
        }
        else
        {
            rb2d = GetComponent<Rigidbody2D>();
            cam = GetComponentInChildren<Camera>();
            anim = GetComponent<Animator>();
            spriter = GetComponent<SpriteRenderer>();
            speedmeter = GameObject.FindGameObjectWithTag("Speedmeter").GetComponent<Text>();//Lo busco por su tag para no tener que meterlo manualmente en el inspector
            semaphore = FindObjectOfType<Semaphore>();
            playerHP = GetComponent<Healthbar>();
            lapsDisplay = GameObject.FindGameObjectWithTag("LapUI").GetComponent<Text>();//Lo busco por su tag para no tener que meterlo manualmente en el inspector
            lapsDisplay.text = "Lap " + "1 / " + (GetComponent<Laps>().finishLap - 1).ToString();

            //MODO INFINITY:
            if (GameManager.Instance.infinityMode)
            {
                infinity = true;
                lapsDisplay.text = "Lap 1";
            }

            //yield return null;

            //GameManager.Instance.introName = GameObject.FindGameObjectWithTag("IntroName").name;

            //Get and store a reference to the Rigidbody2D, camera and animator component so that we can access it.
            //playerHP = FindObjectOfType<Healthbar>();//De esta forma solo hay una barra de vida que es para el player, si en algun momento hubieran varios players, habria que meterlas como public y añadirlas desde el inspector a cada uno.
            alarm = false;
            lowHp = false;

            //TUTORIAL:
            if(GameManager.Instance.introName == "")//Si estoy en el tutorial:
            {
                lapsDisplay.text = "";
                tutorialCanvas = GameObject.FindGameObjectWithTag("UItutorial");
                tutorialText = tutorialCanvas.transform.GetChild(0).GetComponent<Text>();
                tutorialCanvas.GetComponent<Image>().SetTransparency(0);
                tutorialText.text = "";
            }


            yield return null;
            position = GameObject.FindGameObjectWithTag("PositionUI").GetComponent<Text>();//Lo busco por su tag para no tener que meterlo manualmente en el inspector
            cronos = GameObject.FindGameObjectWithTag("chronometer").GetComponent<Text>();
            //yield return null;
            winText = GameObject.FindGameObjectWithTag("WinText").GetComponent<Text>();//unused todavía
                                                                                       //Initialze winText to a blank string since we haven't won yet at beginning.
            winText.text = "";
            //countText = GameObject.FindGameObjectWithTag("CountText").GetComponent<Text>();//Unused todavia (dejar comentado hasta que se use)
            //yield return null;
            if (GameManager.Instance.ModeGP)
            {
                lifesUItext = GameObject.FindGameObjectWithTag("LifesUI").GetComponent<Text>();
                lifeImageUI = GameObject.FindGameObjectWithTag("LifeImageUI").GetComponent<Image>();
                lifesUItext.text = "x" + GameManager.Instance.lifes.ToString();
                lifeImageUI.sprite = spriter.sprite;
                lifeImageUI.color = new Color(1, 1, 1, 1);
            }
            yield return null;
        }
    }

    public IEnumerator ShowMessage(string m)//Muestra el mensaje m de manera intermitente durante 4 segundos:
    {
        winText.text = m;
        for(int i = 0; i < 5; i++)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            winText.SetTransparency(0);
            yield return new WaitForSecondsRealtime(0.2f);
            winText.SetTransparency(1);
        }
        winText.text = "";
    }


    public IEnumerator ShowMessageShort(string m)//Muestra el mensaje m de manera intermitente durante 2 segundos:
    {
        winText.text = m;
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            winText.SetTransparency(0);
            yield return new WaitForSecondsRealtime(0.2f);
            winText.SetTransparency(1);
        }
        winText.text = "";
    }


    private void VelM()//Llamada desde InvokeRepeating en Start()
    {
        i++;
        sumVel += velocity.y;
        mVel = sumVel / i;
    }

    private IEnumerator OutCourse()
    {
        outCourse = true;//Esta bool evita que se llame la corutina hasta que se transporte al player

        Image fScreen = GameObject.FindGameObjectWithTag("FullScreen").GetComponent<Image>();
        float a = 0;

        while(a < 1)//Lleno el alfa de fScreen hasta que no se ve nada (todo blanco)
        {
            a += 0.1f;
            fScreen.color = new Color(1, 1, 1, a);
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1);//Espero 1 s con la pantalla en blanco

        //Teleportear la nave y enfocarla bien:
        nextPoint = GetComponent<Laps>().currentPoint;
        previousPoint = GetComponent<Laps>().currentPoint == 0 ? GetComponent<Laps>().points.Count - 1 : nextPoint - 1;
        transform.position = GetComponent<Laps>().points[previousPoint].transform.position;//Transporto al player

        //Coloco la nave mirando al siguiente checkpoint:
        Vector3 direction = ((GetComponent<Laps>().points[nextPoint].position) - transform.position);
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90;//Saco el ángulo entre mi direccion actual y la direccion que quiero tomar.
        turn = angle;//En el update se usa turn para enfocar la nave, turn es la cantidad de rotacion que necesito

        velocity = new Vector3(0, 0, 0);


        while (a > 0)//Aclaro la pantalla
        {
            a -= 0.2f;
            fScreen.color = new Color(1, 1, 1, a);
            yield return null;
        }
        
        StartCoroutine(ShowMessageShort("Out Course!"));
        outCourse = false;
    }

    public void continueTutorial()
    {
        tutorialCanvas = GameObject.FindGameObjectWithTag("UItutorial");
        tutorialText = tutorialCanvas.transform.GetChild(0).GetComponent<Text>();
        tutorialCanvas.GetComponent<Image>().SetTransparency(0);
        tutorialText.text = "";
        Time.timeScale = 1;
    }


    private IEnumerator HitStunBounce(float v)//hit stun que dura hasta que se reduce la vel a cierto valor v (para choques borde)
    {
        hitStun = true;
        yield return null;
        while(rb2d.velocity.magnitude > v)
        {
            yield return null;
        }
        velocity = new Vector3 (0, rb2d.velocity.magnitude, 0);
        rb2d.velocity = Vector3.zero;
        hitStun = false;
    }

}
