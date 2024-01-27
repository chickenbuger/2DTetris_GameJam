using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class TetrisBlock : MonoBehaviour
{
    public bool fall = false; // ����� �Ʒ��� �������� ������ ���θ� ��Ÿ���� �÷���
    public float fallTime = 1f; // ����� �� ĭ �Ʒ��� �������� �� �ɸ��� �ð�
    private float previousTime; // ������ ����� ������ �ð�
    public Vector3 rotationPoint; // ȸ�� �߽���
    public static int height = 20; // ���� ������ ����
    public static int width = 10; // ���� ������ �ʺ�

    private static Transform[,] grid = new Transform[width, height]; // ���� ������ ��Ÿ���� 2D �迭

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

    // ����� �������� ��ȿ���� Ȯ��
    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            int roundX = Mathf.RoundToInt(children.transform.position.x);
            int roundY = Mathf.RoundToInt(children.transform.position.y);

            if (roundX < 0 || roundX >= width || roundY < 0 || roundY >= height)
            {
                return false; // ������ ����� �������� ��ȿ���� ����
            }

            if (grid[roundX, roundY] != null)
                return false; // �̹� ����� �ִ� ��ġ���� �������� ��ȿ���� ����
        }
        return true; // ��� �ڽĵ��� ��ġ�� ���� ��ȿ�ϸ� true ��ȯ
    }

    // ����� ���� ������ �߰�
    void AddToGrid()
    {
        foreach (Transform childern in transform)
        {
            int roundX = Mathf.RoundToInt(childern.transform.position.x);
            int roundY = Mathf.RoundToInt(childern.transform.position.y);

            grid[roundX, roundY] = childern; // �ش� ��ġ�� ��� �߰�
        }
    }

    // ���� �� ���� �ִ��� Ȯ���ϰ� ó��
    void CheckForLines()
    {
        for (int i = height - 1; i >= 0; i--) // �� �Ʒ��������� ���� �˻�
        {
            if (HasLine(i)) // �ش� ���� ���� á����
            {
                DeleteLine(i); // �� ����
                RowDown(i); // ���� �ٵ��� �� ĭ�� ����
            }
        }
    }

    // Ư�� ���� ���� á���� Ȯ��
    bool HasLine(int i)
    {
        for (int j = 0; j < width; j++) // �ش� ���� �˻�
        {
            if (grid[j, i] == null)
                return false; // �ϳ��� ��������� false ��ȯ
        }
        return true; // ��� ĭ�� �������� true ��ȯ
    }

    // Ư�� �� ����
    void DeleteLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            Destroy(grid[j, i].gameObject); // �ش� ĭ�� ����� ����
            grid[j, i] = null; // �ش� ��ġ�� ���
        }
    }

    // ������ �� ���� �ٵ��� �� ĭ�� ����
    void RowDown(int i)
    {
        for (int y = i; y < height; y++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[j, y] != null)
                {
                    grid[j, y - 1] = grid[j, y]; // ���� ĭ�� �� ĭ ����
                    grid[j, y] = null; // ���� ��ġ�� ���
                    grid[j, y - 1].transform.position -= new Vector3(0, 1, 0); // ���� ��ġ�� ����
                }
            }
        }
    }
}
