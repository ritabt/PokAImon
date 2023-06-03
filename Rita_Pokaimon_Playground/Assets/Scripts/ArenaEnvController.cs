using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaEnvController : MonoBehaviour
{

    //Simple class to contain references to agent components
    [System.Serializable]
    public class PlayerInfo
    {
        public BallSumoAgent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    //Max steps before environment resets (ie, stalemate)
    [Tooltip("Max Secs. Set to -1 for no time limit")] public int MaxSecs = 10;

    //List of Agents
    public PlayerInfo BluePlayer;
    public PlayerInfo RedPlayer;

    private float m_ResetTimer;

    public GameObject platform;

    public float ArenaRadius = 4.0f;


    // Start is called before the first frame update
    void Start()
    {
        platform.transform.localScale = new Vector3(100f * ArenaRadius, 100f * ArenaRadius, 1);
        BluePlayer.Agent.opponent = RedPlayer.Agent;
        RedPlayer.Agent.opponent = BluePlayer.Agent;

        BluePlayer.Agent.arenaRadius = ArenaRadius;
        RedPlayer.Agent.arenaRadius = ArenaRadius;

        BluePlayer.Agent.opponentRBody = RedPlayer.Agent.GetComponent<Rigidbody>();
        RedPlayer.Agent.opponentRBody = BluePlayer.Agent.GetComponent<Rigidbody>();

        ResetScene();
    }

    //Called 50 times/sec
    void FixedUpdate()
    {
        m_ResetTimer += 1;

        BluePlayer.Agent.timeRemaining = (float)MaxSecs - (m_ResetTimer / 50f);
        RedPlayer.Agent.timeRemaining = (float)MaxSecs - (m_ResetTimer / 50f);

        //Check for sparse rewards. ie - have agents lost
        if (BluePlayer.Agent.transform.localPosition.y < 0)
        {
            RedPlayer.Agent.SetReward(1.0f);
            BluePlayer.Agent.SetReward(-1.0f);

            RedPlayer.Agent.EndEpisode();
            BluePlayer.Agent.EndEpisode();

            ResetScene();
        }
        else if (RedPlayer.Agent.transform.localPosition.y < 0) {
            RedPlayer.Agent.SetReward(-1.0f);
            BluePlayer.Agent.SetReward(1.0f);

            RedPlayer.Agent.EndEpisode();
            BluePlayer.Agent.EndEpisode();

            ResetScene();
        }
        else if (MaxSecs > 0 && m_ResetTimer / 50f >= MaxSecs) {
            RedPlayer.Agent.SetReward(-1.0f);
            BluePlayer.Agent.SetReward(-1.0f);

            RedPlayer.Agent.EndEpisode();
            BluePlayer.Agent.EndEpisode();
            ResetScene();
        }
    }

    public void ResetScene()
    {
        m_ResetTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
