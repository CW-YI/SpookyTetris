using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviourPun
{
    #region ��� �����϶� ���̴� ������
    public float previousTime;
    public float fallTime = 0.8f;
    public float inputTimer = 0f;
    public float inputDelay = 0.15f;

    [SerializeField] protected float placementDelay_timer = 0f;
    [SerializeField] public float placementDelay = 0.5f;

    #endregion

    #region ��� ������ ���̴� ������
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

    #region ��Ʈ �ǽ� (��� ������ ��ġ) ������
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
