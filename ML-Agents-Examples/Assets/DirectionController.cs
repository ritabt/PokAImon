using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class DirectionController : MonoBehaviour
{
    public Transform transformToFollow;
    public float y_off;
    public enum Mode { UserControlled, Training };
    public enum Direction {North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, Stop}

    public Mode mode;
    public Direction direction = Direction.North;


    public bool visualize;
    public GameObject Indicator;
    public GameObject IndicatorArrow;

    private int m_timer = 0;
    private float m_timeInterval = 0.0f;

    public Vector3 GetTargetDirection() {
        Vector2 dir2 = GetTargetDirection2();
        return new Vector3(dir2.x, 0f, dir2.y);

    }

    public void Reset() {
        ChooseNewTimeInterval();
        m_timer = 0;
    }

    private Vector2 GetTargetDirection2() {
        switch (direction) {
            case Direction.North: return new Vector2(0.0f, 1.0f).normalized;
            case Direction.NorthEast: return new Vector2(1.0f, 1.0f).normalized;
            case Direction.East: return new Vector2(1.0f, 0.0f).normalized;
            case Direction.SouthEast: return new Vector2(1.0f, -1.0f).normalized;
            case Direction.South: return new Vector2(0.0f, -1.0f).normalized;
            case Direction.SouthWest: return new Vector2(-1.0f, -1.0f).normalized;
            case Direction.West: return new Vector2(-1.0f, 0.0f).normalized;
            case Direction.NorthWest: return new Vector2(-1.0f, 1.0f).normalized;
            case Direction.Stop: return new Vector2(0.0f, 0.0f).normalized;
        };
        return new Vector2(0.0f, 0.0f).normalized; 
    }


    // Start is called before the first frame update
    void Start()
    {
        ChooseNewTimeInterval();

        direction = Direction.East;

        if(!visualize) Indicator.SetActive(false);
        else Indicator.SetActive(true);
    }

    void ChooseNewTimeInterval() {
        if (UnityEngine.Random.value < 0.5f) {
            m_timeInterval = UnityEngine.Random.Range(1f, 15f);
        }
        else{
            m_timeInterval = UnityEngine.Random.Range(0f, 1f);
        }
        
    }

    void FixedUpdate()
    {
        if (mode == Mode.Training) {
            m_timer+=1;
            if (((float)m_timer / 50f) >= m_timeInterval) {
                m_timer = 0;
                //Choose new timeInterval (in secs)
                ChooseNewTimeInterval();

                direction = (Direction)UnityEngine.Random.Range(0, 8); //TODO: Increase to 9
            }
        }
    }

    void SetDirectionUser() {
        bool up = false;
        bool down = false;
        bool l = false;
        bool r = false;

        if (Input.GetKey(KeyCode.UpArrow)) { up = true; }
        if (Input.GetKey(KeyCode.DownArrow)) { down = true; }
        if (Input.GetKey(KeyCode.LeftArrow)) { l = true; }
        if (Input.GetKey(KeyCode.RightArrow)) { r = true; }

        //print(up.ToString() + down.ToString() + l.ToString() + r.ToString());

        if (up && !down)
        {
            if (r && !l) direction = Direction.NorthEast;
            else if (l && !r) direction = Direction.NorthWest;
            else direction = Direction.North;
        }
        else if (down && !up)
        {
            if (r && !l) direction = Direction.SouthEast;
            else if (l && !r) direction = Direction.SouthWest;
            else direction = Direction.South;
        }
        else if (r && !l) direction = Direction.East;
        else if (l && !r) direction = Direction.West;
        else direction = Direction.North;
    }


    void IndicateDirection() {
        float newY = 0f;
        IndicatorArrow.SetActive(true);

        if (direction == Direction.North) newY = 0f;
        else if (direction == Direction.South) newY = 180f;
        else if (direction == Direction.East) newY = 90f;
        else if (direction == Direction.West) newY = -90f;
        else if (direction == Direction.NorthEast) newY = 45;
        else if (direction == Direction.SouthEast) newY = 135f;
        else if (direction == Direction.NorthWest) newY = -45f;
        else if (direction == Direction.SouthWest) newY = -135f;
        else if (direction == Direction.Stop) IndicatorArrow.SetActive(false);

        Indicator.transform.eulerAngles = new Vector3(90.0f, newY, 90.0f);


    }

    // Update is called once per frame
    void Update()
    {

        transform.position = new Vector3(transformToFollow.position.x, y_off, transformToFollow.position.z);

        if (mode == Mode.UserControlled)
        {
            SetDirectionUser();
        }

        if (visualize)
        {
            IndicateDirection();
        } 
    }


}
