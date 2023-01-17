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
    public GameObject checkParent;
    List<Transform> checkPoints = new List<Transform>();
    public GameObject CheckError;

    int active = 0;
    int numChecks = 0;
    bool last = false;

    private void Start()
    {
        InvokeRepeating("setterReward", 0f, 1f);
    }

    private void setterReward()
    {
        SetReward(-0.0005f);
    }

    public override void Initialize()
    {
        numChecks = 0;
        carDriver = GetComponent<CarDriver>();
        rBody = GetComponent<Rigidbody>();
        checkPoints = GetAllChildren(checkParent);
        numChecks = checkPoints.Count();
        foreach (Transform item in checkPoints)
        {
            item.gameObject.SetActive(false);
        }
        checkPoints[0].gameObject.SetActive(true);

    }

    public override void OnEpisodeBegin()
    {
        //Fermiamo i movimenti per sicurezza ad ogni nuovo episodio
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;

        //Calcolare l'edificio più conveniente da cui iniziare
    }

    void Update()
    {


    }

    void Reset()
    {
        active = 0;
        foreach (Transform item in checkPoints)
        {
            item.gameObject.SetActive(false);
        }
        checkPoints[0].gameObject.SetActive(true);
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
        float rndm = Random.Range(-4.0f, 4.0f);
        carDriver.StopCompletely();
        rBody.transform.localPosition =
            new Vector3(rndm, 0f, 0);
        rBody.transform.localRotation =
            new Quaternion(0, 0, 0, 1);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Wall")
        {
            SetReward(-2f);
            Reset();
            startEndingEpisode();
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("Checkpoint"))
        {
            checkPoints[active].gameObject.SetActive(false);
            active = active + 1;
            Debug.Log(active);
            if(active == numChecks)
            {
                last = true;
                active = 0;
            }
            checkPoints[active].gameObject.SetActive(true);
            SetReward(0.5f);
        }
        if (other.gameObject.tag == ("CheckError"))
        {
            if (last == true)
            {
                other.gameObject.SetActive(false);
                last = false;
                SetReward(1f);
                
            }
            else
            {
                SetReward(-2f);
                Reset();
                startEndingEpisode();
            }
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

        if (speed < 0.1f && speed > 0f)
        {
            SetReward(-0.5f);
        }
        else if (speed == 0)
        {
            SetReward(-1.0f);
        }
        if (speed < 0)
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
