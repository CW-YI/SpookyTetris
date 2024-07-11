using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviourPun
{
    #region 블록 움직일때 쓰이는 변수들
    public float previousTime;
    public float fallTime = 0.8f;
    public float inputTimer = 0f;
    public float inputDelay = 0.15f;

    [SerializeField] protected float placementDelay_timer = 0f;
    [SerializeField] public float placementDelay = 0.5f;

    #endregion

    #region 블록 돌릴때 쓰이는 변수들
    public Vector3 rotationPoint;

    //0, -90, 180, 90
    //0 --> -90, -90 --> 180, 180 --> 90, 90 --> 0
    protected int[,] WallKick_Clockwise_x = {   { 0, -1, -1, 0, -1 }, {0, 1, 1, 0, 1 },     {0, 1, 1, 0, 1 },    {0, -1, -1, 0, -1 } };
    protected int[,] WallKick_Clockwise_y = {   {0, 0, 1, -2, -2},    {0, 0, -1, 2, 2},     {0, 0, 1, -2, -2},  {0, 0, -1, 2, 2} };
    protected int[,] I_WallKick_Clockwise_x = { {0, -2, 1, -2, 1},    {0, -1, 2, -1, 2},    {0, 2, -1, 2, -1 }, {0, 1, -2, 1, -2} };
    protected int[,] I_WallKick_Clockwise_y = { {0, 0, 0, -1, 2},     {0, 0, 0, 2, -1 },    {0, 0, 0, 1, -2 },  {0, 0, 0, -2, 1} };

    protected int previousRotation, currentRotation;

    Quaternion initialRotation;
    #endregion

    #region 고스트 피스 (블록 떨어질 위치) 변수들
    public GameObject ghostPrefab;
    #endregion

    void Start()
    {
        initialRotation = gameObject.GetComponentInChildren<Transform>().rotation;
    }

    void LateUpdate()
    {
        foreach (Transform children in transform)
        {
            children.transform.rotation = Quaternion.identity;
        }
    }

}
