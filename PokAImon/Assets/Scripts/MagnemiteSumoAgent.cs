using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class MagnemiteSumoAgent : SumoAgent
{
    public float MoveForce = 1f;
    public float Torque = 1f;
    public bool EnemyPositionFeature = true;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        m_StartingPos = transform.localPosition;
        m_StartingRot = transform.localRotation;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = m_StartingPos;
        transform.localRotation = m_StartingRot;

        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
    }

    //Heuristic. Used for testing our environment
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continousActionsOut = actionsOut.ContinuousActions;
        continousActionsOut[0] = Input.GetAxis("Horizontal");
        continousActionsOut[1] = Input.GetAxis("Vertical");
        continousActionsOut[2] = Input.GetAxis("Mouse X"); //Controller Second Joystick
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //Step 1: Apply Actions

        //Get actions values from model, and do them
        Vector3 moveSignal = Vector3.zero;

        //Actions are continuous
        moveSignal.x = actionBuffers.ContinuousActions[0];
        moveSignal.z = actionBuffers.ContinuousActions[1];
        moveSignal = moveSignal.normalized;

        Vector3 torqueSignal = Vector3.zero;
        torqueSignal.y = actionBuffers.ContinuousActions[2];

        rBody.AddForce(moveSignal * MoveForce);
        rBody.AddTorque(torqueSignal * Torque);

        //Step 2: Collect Rewards (Note: Dense Rewards Here)
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Time left
        sensor.AddObservation(timeRemaining);

        //Radial Distance to Edge
        Vector2 local_XZ  = new Vector2(this.transform.localPosition.x, this.transform.localPosition.z); //Offset from center

        Vector2 toEdge = local_XZ.normalized * (arenaRadius - local_XZ.magnitude);

        sensor.AddObservation(toEdge);

        //Relative oppoenent location

        if (EnemyPositionFeature)
        {
            Vector2 opponent_XZ = new Vector2(opponent.transform.localPosition.x, opponent.transform.localPosition.z);
            Vector2 rel_XZ = opponent_XZ - local_XZ;
            sensor.AddObservation(rel_XZ);
        }

        //Agent velocity
        //sensor.AddObservation(rBody.velocity.x);
        //sensor.AddObservation(rBody.velocity.z);

        //Agent angularVelocity
        //sensor.AddObservation(rBody.angularVelocity.magnitude);

        //Rotation
        //sensor.AddObservation(this.transform.eulerAngles.y);

        //Opponent velocity
        //sensor.AddObservation(opponentRBody.velocity.x);
        //sensor.AddObservation(opponentRBody.velocity.z);
    }
}
