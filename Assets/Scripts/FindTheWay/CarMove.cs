﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

   
public class CarMove : MonoBehaviour
{
    private GameState gs;

    //Enumerado con las cuatro direcciones en las que puede avanzar el coche: Suroeste, Sureste, Noreste y Noroeste respectivamente.
    public enum Direction { SW, SE, NE, NW };
    private enum Tag { S, L, R, SL, SR, LR, SLR }

    //Struct con la información relativa al coche eléctrico.
    [System.Serializable]
    public struct Car
    {
        public Sprite front, back;
     
        public Direction dir;
        public float speed;       
        
        private Collider2D _coll;                           //Variable privada para el collider del coche
        public Collider2D coll
        {
            get { return this._coll; }                      //Definición de getters y setters para el coche
            set { this._coll = value; }
        }     

    }
    public Car electricCar;                                 //Coche eléctrico.
    public Car gasCar;                                      //Coche de gasolina.
    public CanvasManager cv;                                //Canvads manager 

    private bool _intersection = false;                     //Booleano que indica si el coche está en una intersección
    public bool intersection
    {
        get { return this._intersection; }                  //Definición de getters y setters para la variable interseccion
        set { this._intersection = value; }
    }

    private bool move = true;                                //Booleano que indica si el juego está en marcha (si el coche está andando)
    private bool turn = true;                                //Booleano que indica si el coche tiene que girar.


    private bool arrowsEnabled = true;                       //Booleano que indica si se deben mostrar las flechas en una intersección.
    private bool _mapOpened = false;                         //Booleano que permite saber si el mapa está pulsado.
    public GameObject carArrow;                              //Flecha indicador de posicion
    private Car car;                                         //Coche de con el que se va a jugar
   
    //private Animator anim;

    //======================================================================================================
    // Use this for initialization
    void Start()
    {
        gs = GameObject.FindObjectOfType<GameState>();                              //Se actualiza el estado del juego

        if (gs.carType == GameState.Car.ELECTRIC) car = electricCar;                //Se actualiza el tipo de coche.
        else car = gasCar;
        move = true;
       //anim = gameObject.GetComponent<Animator>();
        ChangeCarView();
        carArrow.SetActive(false);

    }

    private void ChangeCarView()
    {
        Vector3 v = this.GetComponent<Transform>().localScale;
        this.gameObject.GetComponent<Transform>().localScale = new Vector3(-v.x, v.y, v.z);     //Se actualiza la escala del coche.Para hacer espejo con el sprite.

       if (car.dir == Direction.SW || car.dir == Direction.SE)
            this.GetComponent<SpriteRenderer>().sprite = car.front;                             //Según la dirección del coche se muestra un sprite u otro.
        else
            this.GetComponent<SpriteRenderer>().sprite = car.back;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (move)
            MoveCar();                                                                          //Si el coche puede moverse, actualizamos su movimiento.
    }

    private void MoveCar()
    {
        Vector3 v = this.GetComponent<Transform>().position;                                    

        switch (car.dir)                                                                        //Movemos el coche en función de su dirección.
        {
            case Direction.SW: //+x
                this.gameObject.GetComponent<Transform>().position = new Vector3(v.x - (float)car.speed, v.y - (float)(1 / Math.Sqrt(3)) * car.speed, v.z);
                break;
            case Direction.SE: //+y
                this.gameObject.GetComponent<Transform>().position = new Vector3(v.x + (float)car.speed, v.y - (float)(1 / Math.Sqrt(3)) * car.speed, v.z);
                break;
            case Direction.NE: //-x
                this.gameObject.GetComponent<Transform>().position = new Vector3(v.x + (float)car.speed, v.y + (float)(1 / Math.Sqrt(3)) * car.speed, v.z);
                break;
            case Direction.NW: //-y
                this.gameObject.GetComponent<Transform>().position = new Vector3(v.x - (float)car.speed, v.y + (float)(1 / Math.Sqrt(3)) * car.speed, v.z);
                break;
        }
    }

    public void ResumeCar()
    {
        this.move = true;                                                       //El coche ya puede moverse
    }

    public void stopCar()
    {
        this.move = false;                                                      //El coche no puede moverse
    }

    public void destroyCar()
    {
        Destroy(this);
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        cv.incrDistance(other.gameObject);                                        //Actualizamos el canvas.
        //Recto
        if (other.gameObject.tag == Tag.S.ToString())
        {
            move = true;
            if (turn == false)
                turn = true;
        }
        //Intersección
        else if(turn){

            if (other.gameObject.tag == Tag.L.ToString())                       //Si el giro es hacia la izquierda debe girar más tarde para continuar por el carril derecho.
                StartCoroutine(WaitToTurn());      
            else if (other.gameObject.tag == Tag.R.ToString())                  //Si el giro es hacia la derecha gira en la dirección correspondiente.
            {
                if (car.dir > 0) car.dir--;                                     //Actualizamos la direccion.
                else car.dir = (Direction)3;
                ChangeCarView();                                                //Se actualiza el sprite
            }
            turn = false;
        }   
    }


    //La parte de cógido que quieres que se detenga debe estar dentro del método IEnumerator
    public IEnumerator WaitToTurn()
    {
        float delay = 0.17f;
        yield return new WaitForSeconds(delay);

        car.dir = (Direction)(((int)car.dir + 1) % 4);                           //Cambiamos la dirección con aritmetica modular sobre el enumerado Direction
        ChangeCarView();

    }

    //aparecen las flechas en los sitios a los que puede ir
    void OnTriggerEnter2D(Collider2D other)
    {
       if ((other.gameObject.name == "NW" ||
            other.gameObject.name == "NE" ||
            other.gameObject.name == "SE" ||
            other.gameObject.name == "SW") && (car.dir.ToString() != other.gameObject.name)
            && other != car.coll)
        {
            //Solo se muestran las flechas tras haber pasado por una carretera con la etiqueta Straight
            if (arrowsEnabled)
            {
                other.transform.parent.gameObject.GetComponent<Intersection>().ShowArrows(other.gameObject);
                car.coll = other;
                //Tras mostrar las flechas hay que esperar a pasar por una carretera recta.
                arrowsEnabled = false;
            }
        }
        
        //Tras pasar por una carretera recta ya se pueden volver a mostrar las flechas.
        if (other.gameObject.tag == "S")
            arrowsEnabled = true;

        else if (other.gameObject.tag == "I")
        {
            this.intersection = true;
            stopCar();
        }
            
    }

    public bool mapOpened
    {
        get { return this._mapOpened; }
        set { this._mapOpened = value; }
    }

    public Direction dir() { return car.dir; }          //Getter para la variable dir del coche
}
//=====================================================================================================================



