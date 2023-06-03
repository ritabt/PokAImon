using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
public class SumoAgent : Agent
{

    [HideInInspector]
    public Agent opponent;

    protected Vector3 m_StartingPos;
    protected Quaternion m_StartingRot;
    protected Rigidbody rBody;

    [HideInInspector]
    public Rigidbody opponentRBody;

    [HideInInspector]
    public float arenaRadius;

    [HideInInspector]
    public float timeRemaining;
}
