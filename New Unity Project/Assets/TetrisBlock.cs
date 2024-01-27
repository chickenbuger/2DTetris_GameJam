using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class TetrisBlock : MonoBehaviour
{
    public bool fall = false;
    public float fallTime = 1f;
    private float previousTime;
    public Vector3 rotationPoint;
    public static int height = 20;
    public static int width = 10;

    private static Transform[,] grid = new Transform[width, height];

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (fall == false)
            {
                transform.position += new Vector3(-1, 0, 0);
                if (!ValidMove())
                {
                    transform.position -= new Vector3(-1, 0, 0);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (fall == false)
            {
                transform.position += new Vector3(1, 0, 0);
                if (!ValidMove())
                {
                    transform.position -= new Vector3(1, 0, 0);
                }
            }
        }

        if (Time.time - previousTime > (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ? fallTime / 10 : fallTime))
        {
            if (fall == false)
            {
                transform.position += new Vector3(0, -1, 0);
                if (!ValidMove())
                {
                    transform.position -= new Vector3(0, -1, 0);
                    AddToGrid();
                    CheckForLines();
                    this.enabled = false;
                    FindAnyObjectByType<spawntetris>().NewTetris();
                }
                previousTime = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            if (!ValidMove())
            {
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            }
        }


    }

    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            int roundX = Mathf.RoundToInt(children.transform.position.x);
            int roundY = Mathf.RoundToInt(children.transform.position.y);

            if (roundX < 0 || roundX >= width || roundY < 0 || roundY >= height)
            {
                return false;
            }

            if (grid[roundX, roundY] != null)
                return false;
        }
        return true;
    }

    void AddToGrid()
    {
        foreach (Transform childern in transform)
        {
            int roundX = Mathf.RoundToInt(childern.transform.position.x);
            int roundY = Mathf.RoundToInt(childern.transform.position.y);

            grid[roundX, roundY] = childern;
        }
    }

    void CheckForLines()
    {
        for (int i = height - 1; i >= 0; i--)  //��Ʈ���� ����� �� ���ٺ��� �Ʒ����� �˻��Ѵ�
        {
            if (HasLine(i)) //���� ������� ���� �������
            {
                DeleteLine(i); // �� ���� �����ϰ�
                RowDown(i);  //���� ��ĭ ������
            }
        }
    }

    bool HasLine(int i)  //���� ������� �� �� �ִ��� Ȯ���ϱ�
    {
        for (int j = 0; j < width; j++) //���� �˻�
        {
            if (grid[j, i] == null)
                return false;
        }
        return true;
    }

    void DeleteLine(int i)//���� �����Ѵ�
    {
        for (int j = 0; j < width; j++)
        {
            Destroy(grid[j, i].gameObject);
            grid[j, i] = null;
        }
    }

    void RowDown(int i) //���� �Ʒ��� ������
    {
        for (int y = i; y < height; y++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[j, y] != null)
                {
                    grid[j, y - 1] = grid[j, y];
                    grid[j, y] = null;
                    grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }

}