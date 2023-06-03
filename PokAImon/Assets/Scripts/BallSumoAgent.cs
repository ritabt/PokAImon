using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class BallSumoAgent : SumoAgent
{
    // Start is called before the first frame update
    void Start()
    {
        this.rBody = GetComponent<Rigidbody>();
        this.m_StartingPos = transform.localPosition;
        this.m_StartingRot = transform.localRotation;
    }

    //Reset agent's state at the beginning of a new episode
    public override void OnEpisodeBegin()
    {
        transform.localPosition = m_StartingPos;
        transform.localRotation = m_StartingRot;

        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
    }

    //Pass observations to rl model as inputs
    public override void CollectObservations(VectorSensor sensor)
    {
        //Time left
        sensor.AddObservation(timeRemaining);

        //Radial Distance to Edge
        Vector2 local_XZ  = new Vector2(this.transform.localPosition.x, this.transform.localPosition.z); //Offset from center
        float distanceToCenter = local_XZ.magnitude;
        float distanceFromEdge = arenaRadius - distanceToCenter;
        sensor.AddObservation(distanceFromEdge);

        //Relative oppoenent location
        Vector2 opponent_XZ = new Vector2(opponent.transform.localPosition.x, opponent.transform.localPosition.z);
        Vector2 rel_XZ = opponent_XZ - local_XZ;
        sensor.AddObservation(rel_XZ);

        //Agent velocity
        sensor.AddObservation(this.rBody.velocity.x);
        sensor.AddObservation(this.rBody.velocity.z);

        //Opponent velocity
        sensor.AddObservation(opponentRBody.velocity.x);
        sensor.AddObservation(opponentRBody.velocity.z);

        //print("Time Remaining:");
        //print(timeRemaining);
        //print("Distance From Edge: ");
        //print(distanceFromEdge);
        //print("Rel Opponent Loc: ");
        //print(rel_XZ);
    }

    //Actions and rewards
    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //Step 1: Apply Actions

        //Get actions values from model, and do them
        Vector3 controlSignal = Vector3.zero;

        //Actions are continuous
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        //Step 2: Collect Rewards (Note: Dense Rewards Here)
    }

    //Heuristic. Used for testing our environment
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continousActionsOut = actionsOut.ContinuousActions;
        continousActionsOut[0] = Input.GetAxis("Horizontal");
        continousActionsOut[1] = Input.GetAxis("Vertical");
    }

}
