//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Character_Dokkaebi : MonoBehaviour
//{
//    private int highestRow = 0;
//    private bool hasHighestRow = false;

//    private int[] arrayOfRandomXAxis = new int[3];
//    private int randomXAxisCount = 0;
//    private int[] arrayOfRandomYAxis = new int[3];
//    private int randomYAxisCount = 0;

//    Player1_TetrisBlock player1_TetrisBlock;

//    private void Start()
//    {
//        player1_TetrisBlock = FindAnyObjectByType<Player1_TetrisBlock>();  
//    }
//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.A)) UseDokkaebiAbility();
//    }

//    void UseDokkaebiAbility()
//    {
//       CheckRows();
//    }

//    void CheckRows()
//    {
//        for (int i = Player1_TetrisBlock.height - 1; i >= 0; --i)
//        {
//            if (HasBlock(i))
//            {
//                if (!hasHighestRow)
//                {
//                    highestRow = i;
//                    hasHighestRow = true;
//                }

//            }
//        }

//        SelectRandomBlocks();
//    }

//    bool HasBlock(int i)
//    {
//        bool isNotEmpty = false;
//        for (int j = 2; j < Player1_TetrisBlock.width; ++j)
//        {
//            if (Player1_TetrisBlock.grid_1[j, i] != null) isNotEmpty = true;
//        }

//        return isNotEmpty;
//    }

//    private void SelectRandomBlocks() //���⿡ x�� y�� �ּ� 2ĭ �̻� �������� ��ġ��..?
//    {
//        bool isOverlapping = true;

//        int randomX = Random.Range(2, Player1_TetrisBlock.width);
//        int randomY = Random.Range(0, highestRow);
//        arrayOfRandomXAxis[randomXAxisCount] = randomX; randomXAxisCount++;
//        arrayOfRandomYAxis[randomYAxisCount] = randomY; randomYAxisCount++;
//        StartCoroutine(DestroyBlock(randomX, randomY));

//        while (randomXAxisCount < 3)
//        {
//            randomX = Random.Range(2, Player1_TetrisBlock.width);
//            randomY = Random.Range(0, highestRow);

//            for (int i = 0; i < randomXAxisCount; ++i)
//            {
//                if ((randomX >= arrayOfRandomXAxis[i] + 3 || randomX <= arrayOfRandomXAxis[i] - 3) && (randomY >= arrayOfRandomYAxis[i] + 3 || randomY <= arrayOfRandomYAxis[i] - 3))
//                {
//                    isOverlapping = false;
//                }
//                else isOverlapping = true;
//            }

//            if (!isOverlapping)
//            {
//                arrayOfRandomXAxis[randomXAxisCount] = randomX; randomXAxisCount++;
//                arrayOfRandomYAxis[randomYAxisCount] = randomY; randomYAxisCount++;

//                StartCoroutine(DestroyBlock(randomX, randomY));

//                isOverlapping = true;
//            }
//        }

//    }

//    IEnumerator DestroyBlock(int xAxis, int yAxis)
//    {
//        Debug.Log("(x,y): " + xAxis + "," + yAxis);

//        int highestDeletedYAxis = yAxis;
//        if (Player1_TetrisBlock.grid_1[xAxis, yAxis] != null)
//        {
//            Destroy(Player1_TetrisBlock.grid_1[xAxis, yAxis].transform.parent.gameObject);
//            Player1_TetrisBlock.grid_1[xAxis, yAxis] = null;  //�߾�
//        }
//        //else highestDeletedYAxis = yAxis - 1;

//        //�߾� �ٷ� ���� && �ٷ� ������
//        if (xAxis < Player1_TetrisBlock.width && Player1_TetrisBlock.grid_1[xAxis + 1, yAxis] != null)
//        {
//            Destroy(Player1_TetrisBlock.grid_1[xAxis + 1, yAxis].transform.parent.gameObject);
//            Player1_TetrisBlock.grid_1[xAxis + 1, yAxis] = null;  
//        }
//        if (xAxis > 2 && Player1_TetrisBlock.grid_1[xAxis - 1, yAxis] != null)
//        {
//            Destroy(Player1_TetrisBlock.grid_1[xAxis - 1, yAxis].transform.parent.gameObject);
//            Player1_TetrisBlock.grid_1[xAxis - 1, yAxis] = null;
//        }

//        //�߾� �ٷ� �� && �ٷ� �Ʒ�
//        if (yAxis < Player1_TetrisBlock.height && Player1_TetrisBlock.grid_1[xAxis, yAxis + 1] != null)
//        {
//            Destroy(Player1_TetrisBlock.grid_1[xAxis, yAxis + 1].transform.parent.gameObject);
//            Player1_TetrisBlock.grid_1[xAxis, yAxis + 1] = null;
//        }

//        //������ �� && ������ �Ʒ�
//        if (xAxis < Player1_TetrisBlock.width && yAxis < Player1_TetrisBlock.height && Player1_TetrisBlock.grid_1[xAxis + 1, yAxis + 1] != null)
//        {
//            Destroy(Player1_TetrisBlock.grid_1[xAxis + 1, yAxis + 1].transform.parent.gameObject);
//            Player1_TetrisBlock.grid_1[xAxis + 1, yAxis + 1] = null;
//        }
//        if (xAxis < Player1_TetrisBlock.width && yAxis > 1 && Player1_TetrisBlock.grid_1[xAxis + 1, yAxis - 1] != null)
//        {
//            Destroy(Player1_TetrisBlock.grid_1[xAxis + 1, yAxis - 1].transform.parent.gameObject);
//            Player1_TetrisBlock.grid_1[xAxis + 1, yAxis - 1] = null;
//        }

//        //���� �� && ���� �Ʒ�
//        if (xAxis > 2 && yAxis < Player1_TetrisBlock.height && Player1_TetrisBlock.grid_1[xAxis - 1, yAxis + 1] != null)
//        {
//            Destroy(Player1_TetrisBlock.grid_1[xAxis - 1, yAxis + 1].transform.parent.gameObject);
//            Player1_TetrisBlock.grid_1[xAxis - 1, yAxis + 1] = null;
//        }
//        if (xAxis > 2 && yAxis > 1 && Player1_TetrisBlock.grid_1[xAxis - 1, yAxis + 1] != null)
//        {
//            Destroy(Player1_TetrisBlock.grid_1[xAxis - 1, yAxis - 1].transform.parent.gameObject);
//            Player1_TetrisBlock.grid_1[xAxis - 1, yAxis + 1] = null;
//        }

//        yield return new WaitForSeconds(1f);
        
//        for (int y = 1; y < Player1_TetrisBlock.height; ++y)
//            RowDown();
        
//    }

//    void RowDown()
//    {
//        Debug.Log("Row Down called");
//        for (int y = 1; y < Player1_TetrisBlock.height; ++y)
//        {
//            for (int x = 2; x < Player1_TetrisBlock.width; ++x) //��� ũ�� �����Ҷ� ���� Ȯ�� �ʼ� (width - n) & j = n
//            {
//                if (Player1_TetrisBlock.grid_1[x, y] != null)
//                {
//                    if (Player1_TetrisBlock.grid_1[x, y-1] == null)
//                    {
//                        Player1_TetrisBlock.grid_1[x, y - 1] = Player1_TetrisBlock.grid_1[x, y];
//                        Player1_TetrisBlock.grid_1[x, y] = null;
//                        Player1_TetrisBlock.grid_1[x, y - 1].transform.position -= new Vector3(0, 1, 0);

//                    }
//                }
//            }
//        }

//        player1_TetrisBlock.CheckForLines();
//    }

//    private void ResetVariables()
//    {

//    }
//}
