using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{

    //public int pacchiMassimi=10;
    private CarDriver carDriver;
    Rigidbody rBody;

    private void Start()
    {
        InvokeRepeating("setterReward", 0f, 0.5f);
    }
    private void setterReward()
    {
        SetReward(0.005f);
    }

    public override void Initialize()
    {
        carDriver = GetComponent<CarDriver>();
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        //Fermiamo i movimenti per sicurezza ad ogni nuovo episodio
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;

        //Calcolare l'edificio pi� conveniente da cui iniziare
    }

    void Update()
    {


    }

    void Reset()
    {
        RespawnObject();
    }

    public static List<Transform> GetAllChildren(GameObject parent)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent.transform)
            if (child.GetComponent<Transform>() != null)
                children.Add(child.GetComponent<Transform>());
        return children;
    }


    public override void CollectObservations(VectorSensor sensor)
    {

        //Agent Velocity
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(carDriver.GetSpeed());
    }

    void RespawnObject()
    {
        carDriver.StopCompletely();
        rBody.transform.localPosition =
            new Vector3(0, 0f, 0);
        rBody.transform.localRotation =
            new Quaternion(0, 0, 0, 1);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Wall")
        {
            Debug.Log("here");
            Debug.Log("CollisionDetected: Building");
            SetReward(-2f);
            Reset();
            startEndingEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            Debug.Log("Trigger CheckPoint");
            SetReward(1f);
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        int forwardAction = 0;
        if (Input.GetKey(KeyCode.UpArrow)) forwardAction = 1;
        if (Input.GetKey(KeyCode.DownArrow)) forwardAction = 2;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.RightArrow)) turnAction = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) turnAction = 2;

        actionsOut[0] = forwardAction;
        actionsOut[1] = turnAction;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;
        //Debug.Log("0: "+ actions.DiscreteActions[0]);
        //Debug.Log("1: "+ actions.DiscreteActions[1]);
        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = 0.3f; break;
            case 2: forwardAmount = -0.1f; break;
        }
        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = 3f; break;
            case 2: turnAmount = -3f; break;

        }

        float speed = carDriver.GetSpeed();

        if(speed < 0.2f && speed > 0f)
        {
            SetReward(-0.5f);
        }
        if(speed == 0)
        {
            SetReward(-2.0f);
        }
        carDriver.SetInputs(forwardAmount, turnAmount);

        if (transform.localPosition.y < -1)
        {
            //Debug.Log("FellOff");
            AddReward(-1f);
            Reset();
            
            startEndingEpisode();
        }
    }
    void startEndingEpisode()
    {

        EndEpisode();
    }

}
