using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


//Creo un struct para levelData:
public class LevelData//Con esto creo una clase nueva llamada LevelData (aunque la usaré tambien para las copas (CupData).
{
    public LevelData(string levelName)
    {
        string data = PlayerPrefs.GetString(levelName);
        if (data == "")
            return;

        string[] allData = data.Split('&');//Separo en strings cada vez que lea un "&" en levelName de Playerprefs.
        BestTime = float.Parse(allData[0]);//Mi BestTime será el primer segmento string de allData (cada segmento (o "parse") los voy separando con "&").
        SilverTime = float.Parse(allData[1]);
        GoldTime = float.Parse(allData[2]);
        //Añdiendo la posicion en cada nivel:
        bestPosition = int.Parse(allData[3]);

        ghosted = int.Parse(allData[4]);//Cojo 0 (no superado) ó 1 (sí superado) Esto se define en FinishRace() de LevelManager.

    }

    public float BestTime { set; get; }
    public float GoldTime { set; get; }
    public float SilverTime { set; get; }

    //PRUEBAAAS (funciona!)
    public int bestPosition { set; get; }//Se usa en FinishRace() de LevelManager script.
    public Sprite positionMedal;

    public int ghosted { set; get; }
}

public class GrandPrixData//Clase equivalente a LevelData pero para las copas:
{
    public GrandPrixData(string cupName)
    {
        string data = PlayerPrefs.GetString(cupName);
        if (data == "")
        {
            bestPosition = "0";//Lo pongo a "0" para que no me dé errores de null o vacío "" al hacerle int.Parse desde finishGP() en LevelMnger.
            return;
        }

        bestPosition = data;
    }
    public string bestPosition { set; get; }
    public Sprite positionMedal;

}


public class InfData//Con esto creo una clase nueva llamada InfData
{
    public InfData(string InfName)
    {
        string data = PlayerPrefs.GetString(InfName);
        if (data == "")
            return;

        string[] allData = data.Split('&');//Separo en strings cada vez que lea un "&" en levelName de Playerprefs.
        BestTime = float.Parse(allData[0]);//Mi BestTime será el primer segmento string de allData (cada segmento (o "parse") los voy separando con "&").
        BronceTime = float.Parse(allData[1]);
        SilverTime = float.Parse(allData[2]);
        GoldTime = float.Parse(allData[3]);
        //Añdiendo la posicion en cada nivel:
        bestRank = int.Parse(allData[4]);

    }

    public float BestTime { set; get; }
    public float GoldTime { set; get; }
    public float SilverTime { set; get; }
    public float BronceTime { set; get; }
    public int bestRank { set; get; }//Se usa en InfinityResults() de LevelManager script.
    public Sprite positionMedal;

}



public class MainMenu : MonoBehaviour {

    private const float CAMERA_TRANSITION_SPEED = 6.0f;

    //Arrastrar en inspector al MainMenu los siguientes public GameObjects:
    public GameObject LevelButtonPrefab;
    public GameObject LevelButtonContainer;
    public GameObject shopButtonPrefab;
    public GameObject shopButtonContainer;
    public GameObject cupButtonPrefab;//Arrastrar el prefab button correspondiente
    public GameObject cupButtonContainer;//Arrastrar el GameObject dentro de cada submenu correspondiente (el parent donde se instanciaran los botones en el gameplay)
    public GameObject infinityButtonPrefab;//Arrastrar el prefab button correspondiente
    public GameObject infinityButtonContainer;//Arrastrar el GameObject dentro de cada submenu correspondiente (el parent donde se instanciaran los botones en el gameplay)

    public Text currencyText;//ARRASTRAR EN EL INSPECTOR EL TEXTO DE LA STORE NORMAL QUE INDICA LAS COINS ACTUALES

    private Transform cameraTranform;
    private Transform cameraDesiredLookAt;

    private bool nextLevelLocked = false;//Bool para bloquear los niveles.
    private bool nextCupLocked = false; //Bool para bloquear las copas.
    private bool nextInfLocked = false;//Bool para bloquear los inf levels.

    //Pruebas mias para asignar el srpite al player:
    //public GameObject playerSpriteContainer;//Para almacenar el sprite que usara luego el player.
    //public SpriteRenderer playerSprite;

    private Sprite[] textures;//Defino mi matriz de sprites aqui fuera para que pueda acceder la funcion ChangePlayerSkin.

    private Sprite[] stats;//Defino matriz con las imagenes de los stats de cada nave

    //Matriz de costes de cada racer en la tienda en orden:
    private int[] costs =   {0,50000,50000,50000,
                            50000,50000,50000,50000,
                            50000,50000,50000,50000,
                            50000,50000,50000,50000};


    //Matriz con los nombres de cada racer (en orden tal cual estan en la carpeta resources/Player:
    private string[] racerNames = { "Dawn Arrow", "Winged Cloud", "Rose Thunder", "Heavy Fog",
                                    "Wildfire Formula", "Storm Stingray", "Northern Unicorn", "Sky Falcon" };

    //Pruebas ranking de cada nivel:
    public Sprite gold;
    public Sprite silver;
    public Sprite bronze;
    public Sprite unwon;
    public Sprite star;



    //Imagen de mi nave seleccionada actualmente:
    public GameObject[] myShipRefs;


    //Texto boton ghost:
    public Text ghostBtnText;//ARRASTRAR el text del Boton ghost challenge aquí desde el INSPECTOR

    //Texto al cargar el menu:
    private Text newsText;//Aquí pondre el texto de los mensajes nuevos (desde el Text newsMainMenu de GameManager)
    private Text moneyText;//Aquí mostrare el dinero actual
    private Button NewsButton;
    public int tap;//Para ir pasando los mensajes
    private string[] allNews;//Aquí almacenare cada string "noticia" de newsMainMenu del GameManager


    //Menu In Ap Purchases:
    public GameObject IAPMenu;//HAY QUE ARRASTRAR AQUÍ EL GAMEOBJECT DE LA TIENDA IAP (micropagos)
    public Button btn40kCoins;//Boton para comprar 40000 coins
    public Button btn150kCoins;//Boton para comprar 150000 coins
    public Button removeAds;//Boton para quitar los ads involuntarios
    public GameObject shipStore;//HAY QUE ARRASTRAR EN EL INSPECTOR el menu GO Store (para ocultarlo al abrir el IAPmenu)

    //GP dificulties
    //private Sprite[] cups;
    private GameObject diffGPBtn;//Texto que indica en qué modo de dificultad estoy

    //Infinity msgBox
    public GameObject infinityMsgBox;//Aqui va la caja de texto con la descripcion del modo infinity drive
    public GameObject gpMsgBox;
    public GameObject timebreakMsgBox;

    private void Start()
    {
        //Pruebas:
        GameManager.Instance.ModeGP = false;//Sea como fuere, al llegar al menu ppal ya no estoy en el modo GP (ni en ningún otro).
        GameManager.Instance.modeTimeBreak = false;
        GameManager.Instance.gpEnding = false;
        GameManager.Instance.infinityMode = false;
        GameManager.Instance.inMainMenu = true;
        //Fin de pruebas.

        ChangePlayerSkin(GameManager.Instance.currentSkinIndex);//Indica el skin inicial al cargar.
        currencyText.text = "Coins: " + GameManager.Instance.currency.ToString();//Le digo al texto currencyText el dinero que tengo.
        cameraTranform = Camera.main.transform;//Indico la ubicacion inicial de la camara.

        GetPlayerName();//Asigno el nombre del racer seleccionado por el player a la variable playerName del GameManager



        //SUBMENÚ LEVEL SELECTION TIMEBREAK:
        Sprite[] thumbnails = Resources.LoadAll<Sprite>("Levels");//Creo una matriz de thumbnails de tipo Sprites que recoge mis assets de la carpeta Levels (dentro de carpeta Resources).

        foreach (Sprite thumbnail in thumbnails)//Creo un boton con mi imagen de cada nivel, tantos como imagenes tenga en la carpeta Resources/Levels y lo ubico en el panel del menu donde estoy.
        {
            GameObject container = Instantiate(LevelButtonPrefab) as GameObject;
            container.GetComponent<Image>().sprite = thumbnail;
            container.transform.SetParent(LevelButtonContainer.transform, false);

            //La siguiente linea es para indicar el texto de debajo de cada nivel:
            LevelData level = new LevelData(thumbnail.name);

            //Poner el tiempo en minutos y seg:
            string minutes = ((int)level.BestTime / 60).ToString("00");
            string seconds = (level.BestTime % 60).ToString("00.000");
            string position = (level.bestPosition).ToString();//PRUEBA DE POSICION la var position indica la mejor posicion, habría que asignarsela a un Text component en el button prefab (no hecho).

            container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = (level.BestTime != 0.0f) ? minutes + ":" + seconds : "";

            //PRUEBAS(funciona!):
            if (level.bestPosition == 1)
            {
                level.positionMedal = gold;
            }
            else if (level.bestPosition == 2)
            {
                level.positionMedal = silver;
            }
            else if (level.bestPosition == 3)
            {
                level.positionMedal = bronze;
            }
            else
            {
                level.positionMedal = unwon;
            }

            //Estrella por derrotar al ghost:
            if (level.ghosted == 1)
            {
                level.positionMedal = star;
            }
            else
            {
                level.positionMedal = unwon;
            }


            //container.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = level.positionMedal;//PRUEBA DE POSICION. Uso componente Image en vez de SpriteRenderer para poder enmascarar el sprite en el menu.
            container.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = level.positionMedal;//PRUEBA DE POSICION

            //Hasta aqui pruebas.

            //Bloqueo de niveles:
            container.transform.GetChild(1).GetComponent<Image>().enabled = nextLevelLocked;//La imagen que oscurece el boton del nivel se activa segun su variable nextlevellocked.
            container.GetComponent<Button>().interactable = !nextLevelLocked;//Activo o no la funcion del boton segun la bool nextlevellocked.
            //if para el bloqueo de niveles:
            if(level.BestTime == 0.0f)
            {
                nextLevelLocked = true;
            }

            string sceneName = thumbnail.name;//Creo una variable string con el nombre de cada thumbnail (esto se tiene que crear en el bucle foreach porque es dinamico, no puedo asignar previamente un nombre de un objeto que no existe.
            container.GetComponent<Button>().onClick.AddListener(() => LoadTBLevel(sceneName));


            //Compruebo si esta lo de retar ghost:
            if (GameManager.Instance.ghosting)
                ghostBtnText.color = new Color(1, 1, 1, 1);
            else
                ghostBtnText.color = new Color(1, 1, 1, 0.3f);

        }
        //Hasta aqui Level Selection (Timebreak).



        //SUBMENU STORE:
        int textureIndex = 0;//Defino un int para etiquetar cada imagen en Resources.
        //Lo mismo pero para el menu de compras (Store o shop):
        textures = Resources.LoadAll<Sprite>("Player");//Ya he definido mi matriz Sprite[] como privada para que la pueda leer la funcion ChangePlayerSkin.
        stats = Resources.LoadAll<Sprite>("Stats");//Estan en la carpeta Stats en Resources
        int i = 0;

        foreach(Sprite texture in textures)
        {
            GameObject container = Instantiate(shopButtonPrefab) as GameObject;
            container.GetComponent<Image>().sprite = texture;
            container.transform.SetParent(shopButtonContainer.transform, false);

            int index = textureIndex;//Esta linea me actualiza el int index dentro del bucle foreach para que me salga el que yo quiero en la funcion ChangePlayerSkin (igual que en string sceneName).
            container.GetComponent<Button>().onClick.AddListener(() => ChangePlayerSkin(index));//Esta funcion asocia el clic con el sprite de la nave.

            //La siguiente linea es para detectar las etiquetas de cada item y darles el precio segun su posicion respecto a la matriz costes.
            container.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = costs[index].ToString();

            //También pongo negra la imagen de la silueta de la nave (para no verla si no tengo la nave comprada):
            container.transform.GetChild(0).GetComponent<Image>().sprite = texture;//Le doy la misma imagen texture de la nave que sea.
            container.transform.GetChild(0).GetComponent<Image>().color = new Color(0, 0, 0, 1);//Y la pongo en negro
            container.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = racerNames[index];//Nombre de la nave desconocido hasta que la desbloquee (nah da igual, que salga yavan)

            //El siguiente if es para desactivar la etiqueta del precio del item de la tienda si ya lo he desbloqueado (Overlay, el primer hijo en el shopButtonPrefab:
            if ((GameManager.Instance.skinAvailability & 1 << index) == 1 << index)//Esta es la misma condicion fumada de ChangePlayerSkin para saber qué items tengo desbloqueados ya en la tienda.
            {
                container.transform.GetChild(1).gameObject.SetActive(false);//Desactivo Overlay.
                container.transform.GetChild(0).GetComponent<Image>().color = new Color(0, 0, 0, 0);//Hago transparente la silueta que la ocultaba
                container.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = racerNames[index];//Y le pongo nombre segun la matriz de nombres

                //Asigno la imagen stats si tengo la nave y le quito la transparencia:
                container.transform.GetChild(3).GetComponent<Image>().sprite = stats[index];
                container.transform.GetChild(3).GetComponent<Image>().SetTransparency(1);

                //Aquí cuento cuantas naves tengo disponibles para los logros:
                i++;
                if (i >= 8)//Si tengo disponibles 8 naves o mas:
                    GPlayGamesManager.UnlockAchievement(GPlayGamesResources.achievement_machine_collector);//Saco el logro de que tengo todas las naves
            }
            textureIndex++;//Aumenta el indice por cada sprite que detecta dentro de Resources\Player.
        }
        //Hasta aquí submenu Store.




        
        //SUBMENÚ GRAND PRIX:

        Sprite[] cups = Resources.LoadAll<Sprite>("GP");//Creo una matriz de thumbnails de tipo Sprites que recoge mis assets de la carpeta GP (dentro de carpeta Resources).


        foreach (Sprite cup in cups)//Creo un boton con mi imagen de cada copa, tantos como imagenes tenga en la carpeta Resources/GP y lo ubico en el panel del menu donde estoy.
        {
            GameObject container = Instantiate(cupButtonPrefab) as GameObject;//Instancio el Boton en sí que es un prefab.
            container.GetComponent<Image>().sprite = cup;//Le doy la imagen de Resources correspondiente (iré una a una según las que haya en la carpeta Resources/GP/).
            container.transform.SetParent(cupButtonContainer.transform, false);//Ubico el boton instanciado en el container (arrastrado desde el inspector al GameObject MainMenu fuera del canvas).

            //La siguiente linea es para indicar el texto de debajo de cada copa:
            GrandPrixData Cup = new GrandPrixData(cup.name + GameManager.Instance.difficultyGP);//Le pongo el nombre que tiene mi archivo en resources.

            //PRUEBAS (funciona!):
            if (Cup.bestPosition == "1")
            {
                Cup.positionMedal = gold;
                container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "1st";
            }
            else if (Cup.bestPosition == "2")
            {
                Cup.positionMedal = silver;
                container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "2nd";
            }
            else if (Cup.bestPosition == "3")
            {
                Cup.positionMedal = bronze;
                container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "3rd";
            }
            else
            {
                Cup.positionMedal = unwon;
                container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = (Cup.bestPosition != "0") ? Cup.bestPosition + "th" : "";

            }

            //container.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = level.positionMedal;//PRUEBA DE POSICION. Uso componente Image en vez de SpriteRenderer para poder enmascarar el sprite en el menu.
            container.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = Cup.positionMedal;//PRUEBA DE POSICION

            //Hasta aqui pruebas.

            //Bloqueo de niveles:
            container.transform.GetChild(1).GetComponent<Image>().enabled = nextCupLocked;//La imagen que oscurece el boton del nivel se activa segun su variable nextcuplocked.
            container.GetComponent<Button>().interactable = !nextCupLocked;//Activo o no la funcion del boton segun la bool nextcuplocked.
            //if para el bloqueo de niveles:
            if (Cup.bestPosition != "1" && Cup.bestPosition != "2" && Cup.bestPosition != "3")
            {
                nextCupLocked = true;
            }

            string sceneName = cup.name;//Creo una variable string con el nombre de cada thumbnail (esto se tiene que crear en el bucle foreach porque es dinamico, no puedo asignar previamente un nombre de un objeto que no existe.
            container.GetComponent<Button>().onClick.AddListener(() => LoadGPLevel(sceneName));

        }

        //Pongo el texto del modo de dificultad actual:
        diffGPBtn = GameObject.FindGameObjectWithTag("DifficultyGP");
        diffGPBtn.transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.difficultyGP;
        diffGPBtn.GetComponent<Button>().onClick.AddListener(() => ChangeDifficultyGP());

        //Hasta aqui GrandPrix cup Selection.



        //SUBMENÚ INFINITY:
        Sprite[] infLevels = Resources.LoadAll<Sprite>("Infinity");//Creo una matriz de thumbnails de tipo Sprites que recoge mis assets de la carpeta Infinity (dentro de carpeta Resources).
        int infCount = 0;

        foreach (Sprite inf in infLevels)//Creo un boton con mi imagen de cada copa, tantos como imagenes tenga en la carpeta Resources/GP y lo ubico en el panel del menu donde estoy.
        {
            GameObject container = Instantiate(infinityButtonPrefab) as GameObject;//Instancio el Boton en sí que es un prefab.
            container.GetComponent<Image>().sprite = inf;//Le doy la imagen de Resources correspondiente (iré una a una según las que haya en la carpeta Resources/GP/).
            container.transform.SetParent(infinityButtonContainer.transform, false);//Ubico el boton instanciado en el container (arrastrado desde el inspector al GameObject MainMenu fuera del canvas).

            //La siguiente linea es para indicar el texto de debajo de cada nivel:
            InfData infLevel = new InfData(inf.name);

            //Poner el tiempo en minutos y seg:
            string minutes = ((int)infLevel.BestTime / 60).ToString("00");
            string seconds = (infLevel.BestTime % 60).ToString("00.000");
            //string position = (infLevel.bestRank).ToString();//PRUEBA DE POSICION la var position indica la mejor posicion, habría que asignarsela a un Text component en el button prefab (no hecho).

            container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = (infLevel.BestTime != 0.0f) ? minutes + ":" + seconds : "";

            //PRUEBAS(funciona!):
            if (infLevel.bestRank == 1)
            {
                infLevel.positionMedal = gold;
                if(inf.name == "Infinity 1")
                    GPlayGamesManager.UnlockAchievement(GPlayGamesResources.achievement_infinity_skills_i);//Saco el logro del oro de Infinity 1
                if (inf.name == "Infinity 2")
                    GPlayGamesManager.UnlockAchievement(GPlayGamesResources.achievement_infinity_skills_ii);//Saco el logro del oro de Infinity 2
            }
            else if (infLevel.bestRank == 2)
            {
                infLevel.positionMedal = silver;
            }
            else if (infLevel.bestRank == 3)
            {
                infLevel.positionMedal = bronze;
            }
            else
            {
                infLevel.positionMedal = unwon;
            }

            ////Estrella por derrotar al ghost:
            //if (level.ghosted == 1)
            //{
            //    level.positionMedal = star;
            //}
            //else
            //{
            //    level.positionMedal = unwon;
            //}


            //Medalla imagen:
            container.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = infLevel.positionMedal;//PRUEBA DE POSICION

            //Hasta aqui pruebas.

            //Bloqueo de niveles:
            container.transform.GetChild(1).GetComponent<Image>().enabled = nextInfLocked;//La imagen que oscurece el boton del nivel se activa segun su variable nextInflocked.
            container.GetComponent<Button>().interactable = !nextInfLocked;//Activo o no la funcion del boton segun la bool nextlevellocked.
            //if para el bloqueo de niveles:
            if (infLevel.bestRank != 1 && infLevel.bestRank != 2 && infLevel.bestRank != 3)
            {
                nextInfLocked = true;
            }

            string sceneName = inf.name;//Creo una variable string con el nombre de cada thumbnail (esto se tiene que crear en el bucle foreach porque es dinamico, no puedo asignar previamente un nombre de un objeto que no existe.
            container.GetComponent<Button>().onClick.AddListener(() => LoadInfLevel(sceneName));


            //Compruebo si esta lo de retar ghost:
            //if (GameManager.Instance.ghosting)
            //    ghostBtnText.color = new Color(1, 1, 1, 1);
            //else
            //    ghostBtnText.color = new Color(1, 1, 1, 0.3f);

        }
        //Hasta aqui INFINITY cup Selection.



        //Asigno imagen de nave seleccionada:
        //MyShip();
        //Asigno las imagenes MyShip a la matriz de GameObjects myShipRefs:
        myShipRefs = GameObject.FindGameObjectsWithTag("myShip");

        foreach (GameObject g in myShipRefs)
        {
            g.GetComponent<Image>().sprite = textures[GameManager.Instance.currentSkinIndex];
        }

        //Mostrar mensajes nuevos:
        CheckNews();//Funcion para mostrar mensajes nuevos al cargar el MainMenu (o no)

    }


    private void Update()
    {
        if(cameraDesiredLookAt != null)//Si le he dicho a la camera que mire a algun sitio:
        {
            cameraTranform.rotation = Quaternion.Slerp(cameraTranform.rotation, cameraDesiredLookAt.rotation, CAMERA_TRANSITION_SPEED * Time.deltaTime);//Cambia del lugar actual al lugar donde quiero mirar,...
            //...interpolando esféricamente con Slerp. La interpolacion la hago a la velocidad de CAMERA_TRANSITION_SPEED.
        }
    }


    public void LoadLevel(string sceneName)//Función para cargar el nivel que sea.
    {
        if (GameManager.Instance.sfx)
            GameManager.Instance.backSfx.Play();
        SceneManager.LoadScene(sceneName);
    }

    public void LoadGPLevel(string sceneName)//Función para cargar el nivel que sea.
    {
        //if (GameManager.Instance.sfx)
        //    GameManager.Instance.PressBtn(GameManager.Instance.retrySfx);
        if(GameManager.Instance.music)
            GameManager.Instance.menutheme.Stop();
        GameManager.Instance.modeTimeBreak = false;
        GameManager.Instance.ModeGP = true;
        GameManager.Instance.infinityMode = false;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadTBLevel(string sceneName)//Función para cargar el nivel que sea.
    {
        //if (GameManager.Instance.sfx)
        //    GameManager.Instance.PressBtn(GameManager.Instance.retrySfx);
        if (GameManager.Instance.music)
            GameManager.Instance.menutheme.Stop();
        GameManager.Instance.modeTimeBreak = true;
        GameManager.Instance.ModeGP = false;
        GameManager.Instance.infinityMode = false;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadInfLevel(string sceneName)//Función para cargar el nivel que sea.
    {
        //if (GameManager.Instance.sfx)
        //    GameManager.Instance.PressBtn(GameManager.Instance.retrySfx);
        if(GameManager.Instance.music)
            GameManager.Instance.menutheme.Stop();
        GameManager.Instance.modeTimeBreak = false;
        GameManager.Instance.ModeGP = false;
        GameManager.Instance.infinityMode = true;
        SceneManager.LoadScene(sceneName);
    }



    public void LookAtMenu(Transform menuTransform)//Función que hace que la camara enfoque al transform que le indico desde el inspector (onClic event).
    {//Instrucciones: Hay que arrastrar el GameObject submenu que sea al hueco del componente botón del GameObject botón en el main menu.
        if (GameManager.Instance.sfx)
            GameManager.Instance.okBtnSfx.Play();
        cameraDesiredLookAt = menuTransform;//Cuando invoco esta funcion, le digo a la camara que tiene que mirar al transform que le digo.

        //Camera.main.transform.LookAt(menuTransform.position);//Esto es una opcion de hacerlo pero mejor uso cameraDesiredLookAt:
    }

    private void ChangePlayerSkin(int index)//Funcion para cambiar el sprite del player (esto debería cambiar la configuracion del player y que se cargue al empezar cada carrera)...
        //POR HACER: crear un case switch donde segun el index, cambien las propiedades del player para cambiar de naves, no solo pintura. NADA: esto está implantado en el propio CompletePlayerController correctly.
    {
        //El if de la FUMADA: digamos que asigno a cada item de la tienda un valor, segun la serie x^2 (primer item es 1, segundo item es 2, tercero es 4, cuarto es 8, etc..)
        //En este if voy a desplazar un bit hacia la izquierda en mi index de forma que solo me filtre los items que aun no poseo. Explicación en el video "Unity Mobile Game Tutorial 7 Saving Shops State de N3K, min 25:
        if((GameManager.Instance.skinAvailability & 1 << index) == 1 << index)
        {
            //FindObjectOfType<CompletePlayerController>().GetComponent<SpriteRenderer>().sprite = textures[index];//Actualmente apunta al gameobject player, pero tendría que establecer una variable de configuracion a la que llame al cargar un nivel.

            //Pruebas con sprites, creo que no necesito estas dos lineas, simplemente le dire al script del player que coja como sprite la textures[index] guardada en PlayerPrefs...
            //playerSprite = playerSpriteContainer.GetComponent<SpriteRenderer>();
            //playerSprite.sprite = textures[index];

            //GameManager.Instance.okBtnSfx.Play();

            GameManager.Instance.currentSkinIndex = index;//Esto le asigna la skin elegida a la skin que usaré cuando cargue al player.
            GameManager.Instance.Save();//Guardo mi elección.
            GetPlayerName();//Asigno el nombre del racer seleccionado por el player a la variable playerName del GameManager

            if (myShipRefs.Length >= 2)//Este if es para que no me ejecute la funcion antes de que acabe el start porque aun no se han asignado los myShipRefs.
            {
                MyShip();
                //myShipRefs = GameObject.FindGameObjectsWithTag("myShip");

                //foreach (GameObject g in myShipRefs)
                //{
                //    g.GetComponent<Image>().sprite = textures[index];
                //}

            }
        }
        else
        {
            //You do not have the skin, do you want to buy it?
            int cost = costs[index];

            if(GameManager.Instance.currency >= cost)
            {
                if (GameManager.Instance.sfx)
                    GameManager.Instance.okBtn2Sfx.Play();
                GameManager.Instance.currency -= cost;
                GameManager.Instance.skinAvailability += 1 << index;
                GameManager.Instance.Save();
                currencyText.text = "Coins: " + GameManager.Instance.currency.ToString();//Para actualizar mi dinero cada vez que compro algo.
                //Para desactivar la silueta negra, la etiqueta del precio y aparecer el nombre de la nave cuando compre el item en la tienda:
                shopButtonContainer.transform.GetChild(index).GetChild(0).gameObject.SetActive(false);
                shopButtonContainer.transform.GetChild(index).GetChild(1).gameObject.SetActive(false);
                shopButtonContainer.transform.GetChild(index).GetChild(2).GetChild(0).GetComponent<Text>().text = racerNames[index];//Y le pongo nombre segun la matriz de nombres

                //Asigno la imagen stats si tengo la nave y le quito la transparencia:
                shopButtonContainer.transform.GetChild(index).GetChild(3).GetComponent<Image>().sprite = stats[index];
                shopButtonContainer.transform.GetChild(index).GetChild(3).GetComponent<Image>().SetTransparency(1);

                ChangePlayerSkin(index);//Para actualizar mi skin y ponerme la que acabo de comprar.
            }
        }

    }

    private void MyShip()
    {
        //En PlayerPrefs, accesible mediante GameManager se guarda el int que apunta al sprite dentro de la carpeta Resources/Player.
        //Sprite[] textures = Resources.LoadAll<Sprite>("Player");//Aqui guardare mis sprites.
        if (GameManager.Instance.sfx)
            GameManager.Instance.okBtnSfx.Play();

        //Asigno las imagenes MyShip a la matriz de GameObjects myShipRefs:
        myShipRefs = GameObject.FindGameObjectsWithTag("myShip");

        foreach (GameObject g in myShipRefs)
        {
            g.GetComponent<Image>().sprite = textures[GameManager.Instance.currentSkinIndex];
        }

    }


    public void ChallengeGhost()
    {
        if (GameManager.Instance.sfx)
            GameManager.Instance.okBtnSfx.Play();
        GameManager.Instance.ghosting = !GameManager.Instance.ghosting;
        if (GameManager.Instance.ghosting)
            ghostBtnText.color = new Color(1, 1, 1, 1);
        else
            ghostBtnText.color = new Color(1, 1, 1, 0.3f);
    }


    private void GetPlayerName()
    {
        GameManager.Instance.playerName = racerNames[GameManager.Instance.currentSkinIndex];
    }

    public void CheckNews()
    {
        //Referencio los objetos:
        GameObject messageBox = GameObject.FindGameObjectWithTag("MessageBox");
        newsText = messageBox.transform.GetChild(1).GetComponent<Text>();
        moneyText = messageBox.transform.GetChild(2).GetComponent<Text>();
        NewsButton = messageBox.transform.GetChild(3).GetComponent<Button>();

        //Asigno los textos:
        if (GameManager.Instance.newsMainMenu != "")//Si hay mensajes
        {
            NewsButton.gameObject.transform.localScale = new Vector3(1, 1, 1);
            GameObject.FindGameObjectWithTag("MessageBox").transform.localScale = new Vector3(1, 1, 1);

            tap = 0;
            allNews = GameManager.Instance.newsMainMenu.Split('&');//Separo en strings cada vez que lea un "&" en newsMainMenu de GameManager (PlayerPrefs)
            newsText.text = allNews[tap];//Muestro mi primera noticia (cada segmento (o "parse") los voy separando con "&").
            if (newsText.text == "")//Porque al poner mensajes siempre empiezo con & para los parses y me introduce blancos
            {
                tap++;
                newsText.text = allNews[tap];
                GameManager.Instance.currency += GameManager.Instance.earnedCoins;
                GameManager.Instance.earnedCoins = 0;
            }
            if (newsText.text == "Save Data is broken. Reseting data...")
            {
                StartCoroutine(CheaterPunishment());
            }
            moneyText.text = "Coins: " + GameManager.Instance.currency;
        }
        else//Si no hay mensajes
        {
            newsText.text = "";
            moneyText.text = "";
            NewsButton.gameObject.transform.localScale = Vector3.zero;
            GameObject.FindGameObjectWithTag("MessageBox").transform.localScale = Vector3.zero;
            //NewsButton.gameObject.SetActive(false);
            //GameObject.FindGameObjectWithTag("MessageBox").SetActive(false);
        }

    }

    public void Tap()//Esta funcion se asigna a un boton en el menu para que al tocar la pantalla se ejecute
    {
        tap++;

        if (tap >= allNews.Length)//Si no hay más mensajes:
        {
            GameManager.Instance.newsMainMenu = "";//Borro los mensajes
            GameManager.Instance.earnedCoins = 0;//Quito las monedas que acabo de recibir
            GameManager.Instance.Save();//Y guardo

            newsText.text = "";
            moneyText.text = "";
            NewsButton.gameObject.transform.localScale = Vector3.zero;
            GameObject.FindGameObjectWithTag("MessageBox").transform.localScale = Vector3.zero;
            currencyText.text = "Coins: " + GameManager.Instance.currency.ToString();//Actualizo mi coins en la store
        }
        else//Si hay muestro el siguiente mensaje:
        {
            NewsButton.gameObject.transform.localScale = new Vector3(1, 1, 1);
            GameObject.FindGameObjectWithTag("MessageBox").transform.localScale = new Vector3(1, 1, 1);

            newsText.text = allNews[tap];
            moneyText.text = "Coins: " + GameManager.Instance.currency;
        }
    }



    //Tienda de IAP:
    public void ShowIAPmenu()
    {
        if (GameManager.Instance.sfx)
            GameManager.Instance.okBtnSfx.Play();

        //Cambio de menu:
        shipStore.transform.localScale = Vector3.zero;//Oculto el menu store normal
        IAPMenu.transform.localScale = new Vector3(1, 1, 1);//Muestro el menu IAP

        //Muestro el dinero en su text (ejecutar esta linea cada vez que compro algo para actualizar el dinero):
        //IAPMenu.transform.GetChild(3).GetComponent<Text>().text = "Coins: " + GameManager.Instance.currency.ToString();
        IAPMenu.transform.GetChild(3).GetComponent<Text>().text = currencyText.text;

        if (GameManager.Instance.noAds)//Si ya he comprado RemoveAds:
        {
            IAPMenu.transform.GetChild(2).GetChild(2).GetChild(0).GetComponent<Text>().text = "Purchased";
        }

    }

    public void BackToStore()
    {
        if (GameManager.Instance.sfx)
            GameManager.Instance.okBtnSfx.Play();

        IAPMenu.transform.localScale = Vector3.zero;//Oculto el menu store IAP
        shipStore.transform.localScale = new Vector3(1, 1, 1);//Muestro el menu normal
    }


    public void CheckNewsStore()//Igual que CheckNews() pero con la rferencia de la messageBox del menu IAP
    {
        //Referencio los objetos:
        GameObject messageBox = IAPMenu.transform.GetChild(7).gameObject;
        newsText = messageBox.transform.GetChild(1).GetComponent<Text>();
        moneyText = messageBox.transform.GetChild(2).GetComponent<Text>();
        NewsButton = messageBox.transform.GetChild(3).GetComponent<Button>();

        //Asigno los textos:
        if (GameManager.Instance.newsMainMenu != "")//Si hay mensajes
        {
            NewsButton.gameObject.transform.localScale = new Vector3(1, 1, 1);
            messageBox.transform.localScale = new Vector3(1, 1, 1);

            tap = 0;
            allNews = GameManager.Instance.newsMainMenu.Split('&');//Separo en strings cada vez que lea un "&" en newsMainMenu de GameManager (PlayerPrefs)
            newsText.text = allNews[tap];//Muestro mi primera noticia (cada segmento (o "parse") los voy separando con "&").
            if (newsText.text == "")//Porque al poner mensajes siempre empiezo con & para los parses y me introduce blancos
            {
                tap++;
                newsText.text = allNews[tap];
                GameManager.Instance.currency += GameManager.Instance.earnedCoins;
                GameManager.Instance.earnedCoins = 0;
            }
            moneyText.text = "Coins: " + GameManager.Instance.currency;
        }
        else//Si no hay mensajes
        {
            newsText.text = "";
            moneyText.text = "";
            NewsButton.gameObject.transform.localScale = Vector3.zero;
            messageBox.transform.localScale = Vector3.zero;
            //NewsButton.gameObject.SetActive(false);
            //GameObject.FindGameObjectWithTag("MessageBox").SetActive(false);
        }

    }


    public void TapStore()//Igual que Tap() pero con la referencia messageBox del IAP Menu:
    {
        GameObject messageBox = IAPMenu.transform.GetChild(7).gameObject;
        tap++;

        if (tap >= allNews.Length)//Si no hay más mensajes:
        {
            GameManager.Instance.newsMainMenu = "";//Borro los mensajes
            GameManager.Instance.earnedCoins = 0;//Quito las monedas que acabo de recibir
            GameManager.Instance.Save();//Y guardo

            newsText.text = "";
            moneyText.text = "";
            NewsButton.gameObject.transform.localScale = Vector3.zero;
            messageBox.transform.localScale = Vector3.zero;
            currencyText.text = "Coins: " + GameManager.Instance.currency.ToString();//Actualizo mi coins en la store
        }
        else//Si hay muestro el siguiente mensaje:
        {
            NewsButton.gameObject.transform.localScale = new Vector3(1, 1, 1);
            messageBox.transform.localScale = new Vector3(1, 1, 1);

            newsText.text = allNews[tap];
            moneyText.text = "Coins: " + GameManager.Instance.currency;
        }
    }


    //Cambiar dificultad de modo GP:
    public void ChangeDifficultyGP()//Asignar esta funcion al boton DificultyGP del GP menu
    {
        if(GameManager.Instance.difficultyGP == "BEGINNER")//Si estoy en modo novice
        {
            GameManager.Instance.difficultyGP = "STANDARD";//Paso a experto
            GameManager.Instance.Save();
            diffGPBtn.transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.difficultyGP;
            nextCupLocked = false;
            MenuGP();
        }
        else if (GameManager.Instance.difficultyGP == "STANDARD")//Si estoy en modo novice
        {
            GameManager.Instance.difficultyGP = "EXPERT";//Paso a experto
            GameManager.Instance.Save();
            diffGPBtn.transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.difficultyGP;
            nextCupLocked = false;
            MenuGP();
        }

        else if (GameManager.Instance.difficultyGP == "EXPERT")//Si estoy en modo experto
        {
            if (GameManager.Instance.masterUnlocked)//Y el modo master esta desbloqueado, paso a master
            {
                GameManager.Instance.difficultyGP = "MASTER";
                GameManager.Instance.Save();
                diffGPBtn.transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.difficultyGP;
                nextCupLocked = false;
                MenuGP();
            }
            else//Si no esta desbloqueado, vuelvo a novice
            {
                GameManager.Instance.difficultyGP = "BEGINNER";
                GameManager.Instance.Save();
                diffGPBtn.transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.difficultyGP;
                nextCupLocked = false;
                MenuGP();
            }
        }
        else if (GameManager.Instance.difficultyGP == "MASTER")//Si estoy en modo novice
        {
            GameManager.Instance.difficultyGP = "BEGINNER";//Paso a experto
            GameManager.Instance.Save();
            diffGPBtn.transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.difficultyGP;
            nextCupLocked = false;
            MenuGP();
        }

        else//Caso hipotetico en que difficlutGP tenga un valor extraño, lo pongo en Novice:
        {
            GameManager.Instance.difficultyGP = "BEGINNER";
            GameManager.Instance.Save();
            diffGPBtn.transform.GetChild(0).GetComponent<Text>().text = GameManager.Instance.difficultyGP;
            nextCupLocked = false;
            MenuGP();
        }
    }

    //SUBMENÚ GRAND PRIX:
    private void MenuGP()//Llamo a esta funcion cada vez que cambio la dificultad de GP para actualizar los datos de qué se ha pasado y qué no
    {
        //Tengo que eliminar las copas cargadas previamente:
        Transform[] tgos = cupButtonContainer.GetComponentsInChildren<Transform>();
        foreach (Transform tgo in tgos)
        {
            if(tgo.transform == cupButtonContainer.transform)//Resulta que el getCompoinChildren tambien recoge al padre, así que lo filtro para no eliminarlo
            {

            }
                else
            {
                if (tgo != null)//Opcional, para que no de error en caso de querer borrar algo que no existe
                    Destroy(tgo.gameObject);
            }
        }
            

        Sprite[] cups = Resources.LoadAll<Sprite>("GP");//Creo una matriz de thumbnails de tipo Sprites que recoge mis assets de la carpeta GP (dentro de carpeta Resources).

        foreach (Sprite cup in cups)//Creo un boton con mi imagen de cada copa, tantos como imagenes tenga en la carpeta Resources/GP y lo ubico en el panel del menu donde estoy.
        {
            GameObject container = Instantiate(cupButtonPrefab) as GameObject;//Instancio el Boton en sí que es un prefab.
            container.GetComponent<Image>().sprite = cup;//Le doy la imagen de Resources correspondiente (iré una a una según las que haya en la carpeta Resources/GP/).
            container.transform.SetParent(cupButtonContainer.transform, false);//Ubico el boton instanciado en el container (arrastrado desde el inspector al GameObject MainMenu fuera del canvas).

            //La siguiente linea es para indicar el texto de debajo de cada copa:
            GrandPrixData Cup = new GrandPrixData(cup.name + GameManager.Instance.difficultyGP);//El nombre aqui tiene que coincidir con el nombre guardado en PlayerPrefs (diferentes para cada nivel de dificultad)

            //PRUEBAS (funciona!):
            if (Cup.bestPosition == "1")
            {
                Cup.positionMedal = gold;
                container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "1st";
            }
            else if (Cup.bestPosition == "2")
            {
                Cup.positionMedal = silver;
                container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "2nd";
            }
            else if (Cup.bestPosition == "3")
            {
                Cup.positionMedal = bronze;
                container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "3rd";
            }
            else
            {
                Cup.positionMedal = unwon;
                container.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = (Cup.bestPosition != "0") ? Cup.bestPosition + "th" : "";

            }

            //container.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = level.positionMedal;//PRUEBA DE POSICION. Uso componente Image en vez de SpriteRenderer para poder enmascarar el sprite en el menu.
            container.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = Cup.positionMedal;//PRUEBA DE POSICION

            //Hasta aqui pruebas.

            //Bloqueo de niveles:
            container.transform.GetChild(1).GetComponent<Image>().enabled = nextCupLocked;//La imagen que oscurece el boton del nivel se activa segun su variable nextcuplocked.
            container.GetComponent<Button>().interactable = !nextCupLocked;//Activo o no la funcion del boton segun la bool nextcuplocked.
            //if para el bloqueo de niveles:
            if (Cup.bestPosition != "1" && Cup.bestPosition != "2" && Cup.bestPosition != "3")
            {
                nextCupLocked = true;
            }

            string sceneName = cup.name;//Creo una variable string con el nombre de cada thumbnail (esto se tiene que crear en el bucle foreach porque es dinamico, no puedo asignar previamente un nombre de un objeto que no existe.
            container.GetComponent<Button>().onClick.AddListener(() => LoadGPLevel(sceneName));
        }
        //Hasta aqui GrandPrix cup Selection.
    }

    public void ToogleInfinityInfo()
    {
        if(infinityMsgBox.transform.localScale == Vector3.zero)
        {
            infinityMsgBox.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            infinityMsgBox.transform.localScale = Vector3.zero;
        }
    }

    public void ToogleGPInfo()
    {
        if (gpMsgBox.transform.localScale == Vector3.zero)
        {
            gpMsgBox.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            gpMsgBox.transform.localScale = Vector3.zero;
        }
    }

    public void ToogleTimebreakInfo()
    {
        if (timebreakMsgBox.transform.localScale == Vector3.zero)
        {
            timebreakMsgBox.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            timebreakMsgBox.transform.localScale = Vector3.zero;
        }
    }

    private IEnumerator CheaterPunishment()
    {
        yield return new WaitForSecondsRealtime(3);
        GameManager.Instance.DeleteSavedGameData();
    }


}
