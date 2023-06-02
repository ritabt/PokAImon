using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using System.Runtime.InteropServices;
using System;

[RequireComponent(typeof(JointDriveController))] // Required to set joint forces
public class CaterpieAgent : Agent
{
    const float m_MaxWalkingSpeed = 10; //The max walking speed

    public bool Controllable = false;
    private Vector3 m_prevControllerDir;

    [Header("Stopping")]
    public bool IncludeStopping = false;
    public float StopProb = 0.001f;
    public float MaxStopSecs = 5f;

    private bool m_stopped = false;
    private int m_stopTimer = 0;
    private float m_currStopSecs = 0f;

    [Header("Target Prefabs")] public Transform TargetPrefab; //Target prefab to use in Dynamic envs
    private Transform m_Target; //Target the agent will walk towards during training.

    public Transform EmptyTarget;

    [Header("Body Parts")] public Transform bodySegment0;
    public Transform bodySegment1;
    public Transform bodySegment2;
    public Transform bodySegment3;
    public Transform bodySegment4;
    public Transform bodySegment5;
    public Transform bodySegment6;

    //This will be used as a stabilized model space reference point for observations
    //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
    OrientationCubeController m_OrientationCube;

    //The indicator graphic gameobject that points towards the target
    DirectionIndicator m_DirectionIndicator;
    JointDriveController m_JdController;

    private Vector3 m_StartingPos; //starting position of the agent


    public override void Initialize()
    {


        SpawnTarget(TargetPrefab, transform.position); //spawn target


        m_StartingPos = bodySegment0.position;
        m_OrientationCube = GetComponentInChildren<OrientationCubeController>();
        m_DirectionIndicator = GetComponentInChildren<DirectionIndicator>();
        m_JdController = GetComponent<JointDriveController>();

        m_prevControllerDir = new Vector3(0f, 0f, 0f);
        m_stopped = false;

        UpdateOrientationObjects();

        //Setup each body part
        m_JdController.SetupBodyPart(bodySegment0);
        m_JdController.SetupBodyPart(bodySegment1);
        m_JdController.SetupBodyPart(bodySegment2);
        m_JdController.SetupBodyPart(bodySegment3);
        m_JdController.SetupBodyPart(bodySegment4);
        m_JdController.SetupBodyPart(bodySegment5);
        m_JdController.SetupBodyPart(bodySegment6);
    }


    /// <summary>
    /// Spawns a target prefab at pos
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="pos"></param>
    void SpawnTarget(Transform prefab, Vector3 pos)
    {
        if (Controllable) m_Target = Instantiate(EmptyTarget, pos, Quaternion.identity, transform.parent);
        else m_Target = Instantiate(prefab, pos, Quaternion.identity, transform.parent);
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        foreach (var bodyPart in m_JdController.bodyPartsList)
        {
            bodyPart.Reset(bodyPart);
        }

        //Random start rotation to help generalize
        bodySegment0.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0.0f, 360.0f), 0);

        UpdateOrientationObjects();

        m_stopped = false;
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp, VectorSensor sensor)
    {
        //GROUND CHECK
        sensor.AddObservation(bp.groundContact.touchingGround ? 1 : 0); // Whether the bp touching the ground

        //Get velocities in the context of our orientation cube's space
        //Note: You can get these velocities in world space as well but it may not train as well.
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.velocity));
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.angularVelocity));


        if (bp.rb.transform != bodySegment0)
        {
            //Get position relative to hips in the context of our orientation cube's space
            sensor.AddObservation(
                m_OrientationCube.transform.InverseTransformDirection(bp.rb.position - bodySegment0.position));
            sensor.AddObservation(bp.rb.transform.localRotation);
        }

        if (bp.joint)
            sensor.AddObservation(bp.currentStrength / m_JdController.maxJointForceLimit);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        RaycastHit hit;
        float maxDist = 10;
        if (Physics.Raycast(bodySegment0.position, Vector3.down, out hit, maxDist))
        {
            sensor.AddObservation(hit.distance / maxDist);
        }
        else
            sensor.AddObservation(1);

        var cubeForward = m_OrientationCube.transform.forward;

        var maxSpeed = m_MaxWalkingSpeed;

        if (m_stopped) maxSpeed = 0f;

        var velGoal = cubeForward * maxSpeed;
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(velGoal));
        sensor.AddObservation(Quaternion.Angle(m_OrientationCube.transform.rotation,
                                  m_JdController.bodyPartsDict[bodySegment0].rb.rotation) / 180);
        sensor.AddObservation(Quaternion.FromToRotation(bodySegment0.forward, cubeForward));

        //Add pos of target relative to orientation cube
        if (!m_stopped) sensor.AddObservation(m_OrientationCube.transform.InverseTransformPoint(m_Target.transform.position));
        else sensor.AddObservation(m_OrientationCube.transform.InverseTransformPoint(m_OrientationCube.transform.position)); //Zero out rel distance for stopped state

        foreach (var bodyPart in m_JdController.bodyPartsList)
        {
            CollectObservationBodyPart(bodyPart, sensor);
        }
    }



    /// <summary>
    /// Agent touched the target NOTE: I don't think this is called anywhere
    /// </summary>
    public void TouchedTarget()
    {
        AddReward(1f);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // The dictionary with all the body parts in it are in the jdController
        var bpDict = m_JdController.bodyPartsDict;

        var i = -1;
        var continuousActions = actionBuffers.ContinuousActions;
        // Pick a new target joint rotation
        bpDict[bodySegment0].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], 0);
        bpDict[bodySegment1].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], 0);
        bpDict[bodySegment2].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], 0);
        bpDict[bodySegment3].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], 0);
        bpDict[bodySegment4].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], 0);
        bpDict[bodySegment5].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], 0);

        // Update joint strength
        bpDict[bodySegment0].SetJointStrength(continuousActions[++i]);
        bpDict[bodySegment1].SetJointStrength(continuousActions[++i]);
        bpDict[bodySegment2].SetJointStrength(continuousActions[++i]);
        bpDict[bodySegment3].SetJointStrength(continuousActions[++i]);
        bpDict[bodySegment4].SetJointStrength(continuousActions[++i]);
        bpDict[bodySegment5].SetJointStrength(continuousActions[++i]);

        //Reset if Worm fell through floor;
        if (bodySegment0.position.y < m_StartingPos.y - 2)
        {
            //print("Worm fell through floor. Resetting episode.");
            EndEpisode();
        }
    }

    void UpdateTgtCube()
    {
        if (Controllable) m_stopped = false;
        float dist = 10f;
        Vector3 direction = m_prevControllerDir;

        var up = false;
        var down = false;
        var l = false;
        var r = false;

        if (Input.GetKey(KeyCode.UpArrow)) { up = true; }
        if (Input.GetKey(KeyCode.DownArrow)) { down = true; }
        if (Input.GetKey(KeyCode.LeftArrow)) { l = true; }
        if (Input.GetKey(KeyCode.RightArrow)) { r = true; }

        //print(up.ToString() + down.ToString() + l.ToString() + r.ToString());

        if (up && !down)
        {
            if (r && !l) direction = new Vector3(1f, 0f, 1f).normalized; //Direction.NorthEast;
            else if (l && !r) direction = new Vector3(-1f, 0f, 1f).normalized;//Direction.NorthWest;
            else direction = new Vector3(0f, 0f, 1f).normalized;//Direction.North;
        }
        else if (down && !up)
        {
            if (r && !l) direction = new Vector3(1f, 0f, -1f).normalized; //Direction.SouthEast;
            else if (l && !r) direction = new Vector3(-1f, 0f, -1f).normalized; //Direction.SouthWest;
            else direction = new Vector3(0f, 0f, -1f).normalized;//Direction.South;
        }
        else if (r && !l) direction = new Vector3(1f, 0f, 0f).normalized;//Direction.East;
        else if (l && !r) direction = new Vector3(-1f, 0f, 0f).normalized; //Direction.West;
        else m_stopped = true;

        m_prevControllerDir = direction;

        m_Target.position = m_JdController.bodyPartsDict[bodySegment0].rb.transform.position + dist * direction;
    }

    //Called 50x/sec if stopping is included. Checks if should be stopped and updates related variables.
    void Stopping()
    {
        if (m_stopped) //If currently stopped, check if should go. Otherwise inc timer
        {
            if ((float)m_stopTimer / 50f > m_currStopSecs) //Check if wait time is over. Go
            {
                m_stopped = false;
                m_stopTimer = 0;
                //print("Resumed Agent");
            }
            else //Inc timer
            {
                m_stopTimer++;
            }
        }
        else if (UnityEngine.Random.value < StopProb) //If going, see if should be stopped
        {
            m_stopTimer = 0;
            m_currStopSecs = MaxStopSecs;
            m_stopped = true;
            //print("Stopped Agent");
        }
    }

    void FixedUpdate()
    {
        if(Controllable) UpdateTgtCube();
        if (IncludeStopping) Stopping();

        var maxSpeed = m_MaxWalkingSpeed;
        if (IncludeStopping && m_stopped)
        {
            maxSpeed = 0;
        }

        UpdateOrientationObjects();

        var velReward =
            GetMatchingVelocityReward(m_OrientationCube.transform.forward * maxSpeed,
                m_JdController.bodyPartsDict[bodySegment0].rb.velocity);


        //Angle of the rotation delta between cube and body.
        //This will range from (0, 180)
        var rotAngle = Quaternion.Angle(m_OrientationCube.transform.rotation,
            m_JdController.bodyPartsDict[bodySegment0].rb.rotation);

        //The reward for facing the target
        var facingRew = 0f;
        //If we are within 30 degrees of facing the target
        if (rotAngle < 30)
        {
            //Set normalized facingReward
            //Facing the target perfectly yields a reward of 1
            facingRew = 1 - (rotAngle / 180);
        }

        if (m_stopped) //Trying penalty for not stopping when should
        {
            AddReward(-m_JdController.bodyPartsDict[bodySegment0].rb.velocity.magnitude/m_MaxWalkingSpeed);
        }
        else
        {
            AddReward(velReward * facingRew); //Add the product of these two rewards
        }
    }

    /// <summary>
    /// Normalized value of the difference in actual speed vs goal walking speed.
    /// </summary>
    public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
    {
        //distance between our actual velocity and goal velocity
        var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, m_MaxWalkingSpeed);

        //return the value on a declining sigmoid shaped curve that decays from 1 to 0
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / m_MaxWalkingSpeed, 2), 2);
    }

    /// <summary>
    /// Update OrientationCube and DirectionIndicator
    /// </summary>
    void UpdateOrientationObjects()
    {
        m_OrientationCube.UpdateOrientation(bodySegment0, m_Target);
        if (m_DirectionIndicator)
        {
            m_DirectionIndicator.MatchOrientation(m_OrientationCube.transform);
        }
    }
}
