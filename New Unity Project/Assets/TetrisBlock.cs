using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class TetrisBlock : MonoBehaviour
{
    public bool fall = false; // 블록이 아래로 떨어지는 중인지 여부를 나타내는 플래그
    public float fallTime = 1f; // 블록이 한 칸 아래로 떨어지는 데 걸리는 시간
    private float previousTime; // 이전에 블록이 움직인 시간
    public Vector3 rotationPoint; // 회전 중심점
    public static int height = 20; // 게임 영역의 높이
    public static int width = 10; // 게임 영역의 너비

    private static Transform[,] grid = new Transform[width, height]; // 게임 영역을 나타내는 2D 배열

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

    // 블록의 움직임이 유효한지 확인
    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            int roundX = Mathf.RoundToInt(children.transform.position.x);
            int roundY = Mathf.RoundToInt(children.transform.position.y);

            if (roundX < 0 || roundX >= width || roundY < 0 || roundY >= height)
            {
                return false; // 영역을 벗어나면 움직임이 유효하지 않음
            }

            if (grid[roundX, roundY] != null)
                return false; // 이미 블록이 있는 위치로의 움직임이 유효하지 않음
        }
        return true; // 모든 자식들의 위치에 대해 유효하면 true 반환
    }

    // 블록을 게임 영역에 추가
    void AddToGrid()
    {
        foreach (Transform childern in transform)
        {
            int roundX = Mathf.RoundToInt(childern.transform.position.x);
            int roundY = Mathf.RoundToInt(childern.transform.position.y);

            grid[roundX, roundY] = childern; // 해당 위치에 블록 추가
        }
    }

    // 가득 찬 줄이 있는지 확인하고 처리
    void CheckForLines()
    {
        for (int i = height - 1; i >= 0; i--) // 맨 아래에서부터 위로 검사
        {
            if (HasLine(i)) // 해당 줄이 가득 찼으면
            {
                DeleteLine(i); // 줄 삭제
                RowDown(i); // 위의 줄들을 한 칸씩 내림
            }
        }
    }

    // 특정 줄이 가득 찼는지 확인
    bool HasLine(int i)
    {
        for (int j = 0; j < width; j++) // 해당 줄을 검사
        {
            if (grid[j, i] == null)
                return false; // 하나라도 비어있으면 false 반환
        }
        return true; // 모든 칸이 차있으면 true 반환
    }

    // 특정 줄 삭제
    void DeleteLine(int i)
    {
        for (int j = 0; j < width; j++)
        {
            Destroy(grid[j, i].gameObject); // 해당 칸의 블록을 삭제
            grid[j, i] = null; // 해당 위치를 비움
        }
    }

    // 삭제된 줄 위의 줄들을 한 칸씩 내림
    void RowDown(int i)
    {
        for (int y = i; y < height; y++)
        {
            for (int j = 0; j < width; j++)
            {
                if (grid[j, y] != null)
                {
                    grid[j, y - 1] = grid[j, y]; // 현재 칸을 한 칸 내림
                    grid[j, y] = null; // 현재 위치를 비움
                    grid[j, y - 1].transform.position -= new Vector3(0, 1, 0); // 실제 위치도 조정
                }
            }
        }
    }
}
