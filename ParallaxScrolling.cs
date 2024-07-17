using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(SpriteRenderer))]

public class ParallaxScrolling : MonoBehaviour {


    public bool scrolling, paralax;

    public float backgroundSize;
    public float paralaxSpeed;
    public Transform cameraTransform;

    //private Transform cameraTransform;
    private Transform[] layers;
    private float viewZone = 10;
    private int leftIndex;
    private int rightIndex;
    private float lastCameraX;

    private CompletePlayerController player;

    private void Start()
    {
        //cameraTransform = Camera.main.transform;
        lastCameraX = cameraTransform.position.x;
        layers = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            layers[i] = transform.GetChild(i);

        leftIndex = 0;
        rightIndex = layers.Length - 1;

        player = FindObjectOfType<CompletePlayerController>();

    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            ScrollLeft();
        if (Input.GetKeyDown(KeyCode.E))
            ScrollRight();

        if (paralax)
        {
            float deltaX = cameraTransform.position.x - lastCameraX;
            transform.position += Vector3.right * (deltaX * paralaxSpeed);

        }

        lastCameraX = cameraTransform.position.x;

        if (scrolling)
        {
            if (cameraTransform.position.x < (layers[leftIndex].transform.position.x + viewZone))
                ScrollLeft();

            if (cameraTransform.position.x > (layers[rightIndex].transform.position.x - viewZone))
                ScrollRight();

        }
    }


    private void ScrollLeft()
    {
        int lastRight = rightIndex;
        layers[rightIndex].position = Vector3.right * (layers[leftIndex].position.x - backgroundSize);
        leftIndex = rightIndex;
        rightIndex--;
        if (rightIndex < 0)
            rightIndex = layers.Length - 1;

    }

    private void ScrollRight()
    {
        int lastLeft = leftIndex;
        layers[leftIndex].position = Vector3.right * (layers[rightIndex].position.x + backgroundSize);
        rightIndex = leftIndex;
        leftIndex++;
        if (leftIndex == layers.Length)
            leftIndex = 0;

    }


    /*public Rigidbody2D target;//Aquí va el player normalmente.

    public float speed; //Velocidad de scrolling.

    private float initPos;//Posicion inicial en X.

    void Start()
    {
        initPos = transform.position.x;

        GameObject objectCopy = GameObject.Instantiate(this.gameObject);//Crea una instancia para llenar el resto de la pantalla.

        Destroy(objectCopy.GetComponent<ParallaxScrolling>());//Destruye el script en las instancias.

        objectCopy.transform.SetParent(this.transform);//La instancia aparecera como hijo de este gameObject.
        objectCopy.transform.localPosition = new Vector3(GetWidth(), 0, 0);//Lugar donde aparecera la instancia.

    }

    void Update()
    {
        float targetVelocity = target.velocity.x; //Se define la velocidad base (del player).

        this.transform.Translate(new Vector3(-speed * targetVelocity, 0, 0) * Time.deltaTime);//Mueve el sprite segun la velocidad target.

        float width = GetWidth();

        if(targetVelocity > 0)
        {
            //Muevo el sprite a la derecha si el player se mueve a la derecha.
            if(initPos - this.transform.localPosition.x > width)
            {
                this.transform.Translate(new Vector3(width, 0, 0));
            }
        } else
        {
            //Muevo a la izq si el player se mueve a la izq:
            if(initPos - this.transform.localPosition.x < 0)
            {
                this.transform.Translate(new Vector3(-width, 0, 0));
            }
        }
    }

    float GetWidth()
    {
        //Get sprite width:
        return this.GetComponent<SpriteRenderer>().bounds.size.x;
    }
    */

}
