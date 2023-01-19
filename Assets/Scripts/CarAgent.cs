using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class CarAgent : Agent
{

    //public int pacchiMassimi=10;
    Rigidbody rBody;
    public GameObject checkParent;
    List<Transform> checkPoints = new List<Transform>();
    public GameObject sampleObstacle;
    public Text lapText;

    private List<Transform> obstacles;
    private CarDriver carDriver;
   [SerializeField] private int obstacleRange=5;
    private int active = 0;
    private int numChecks = 0;
    [SerializeField] private int lap = 0;


    private void Start()
    {
        InvokeRepeating("setterReward", 0f, 0.5f);
    }

    private void setterReward()
    {
        SetReward(-0.005f);
    }

    public override void Initialize()
    {
        lapText.text = lap.ToString();
        carDriver = GetComponent<CarDriver>();
        rBody = GetComponent<Rigidbody>();
        checkPoints = GetAllChildren(checkParent);
        numChecks = checkPoints.Count();
        obstacles = GetAllChildren(sampleObstacle);
        activeObstales(false);

        /*        foreach (Transform item in checkPoints)
                {
                    item.gameObject.SetActive(false);
                }
                checkPoints[0].gameObject.SetActive(true);*/

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

        lap = 0;
        lapText.text = lap.ToString();
        active = 0;
        /*        foreach (Transform item in checkPoints)
                {
                    item.gameObject.SetActive(false);
                }
                checkPoints[0].gameObject.SetActive(true);*/
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
        /*Vector3 checkpointForward = checkPoints[active].transform.forward;
        float directionalDot = Vector3.Dot(transform.forward, checkpointForward);
        sensor.AddObservation(directionalDot);*/

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
        if (col.gameObject.tag == "Obstacle")
        {
            SetReward(-1f);
            Reset();
            startEndingEpisode();
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("Checkpoint"))
        {
            if (checkPoints[active].gameObject == other.gameObject)
            {

                active = active + 1;
                if (active == numChecks)
                {
                    activeObstales(false);
                    active = 0;
                    lap++;
                    lapText.text = lap.ToString();
                    spawnNewObstacles(Mathf.FloorToInt(lap / obstacleRange));
                }
                AddReward(1f);
            }
            else
            {
                AddReward(-1.0f);
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

        if (speed >= 0f && speed <=0.15f)
        {
            SetReward(-1.0f);
        }
        if (speed == 0)
        {
            SetReward(-1.0f);
        }
        if (speed < 0)
        {
            SetReward(-1.0f);
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


    void spawnNewObstacles(int n)
    {
        for(int i=0; i<n; i++)
        {
            int randomIndex = Random.Range(0, obstacles.Count);
            obstacles[randomIndex].gameObject.SetActive(true);

        }
    }

    void activeObstales(bool flag)
    {
        foreach (Transform t in obstacles)
            t.gameObject.SetActive(flag);
    }
}
