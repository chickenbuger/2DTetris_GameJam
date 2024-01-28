using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Stage : MonoBehaviour
{
    [Header("Editor Objects")]
    public GameObject BacktilePrefab;
    public GameObject WaillTilePrefab;
    public GameObject NomailtilePrefab;

    public Transform backgroundNode;
    public Transform boardNode;
    public Transform tetrominoNode;
    public GameObject gameoverPanel;

    [Header("Game Settings")]
    [Range(4, 40)]
    public int boardWidth = 10;
    [Range(5, 20)]
    public int boardHeight = 20;
    public float fallCycle = 1.0f;

    [SerializeField] private Sprite[] ArraySprite;

    private float nextFallTime;

    private int curIndex = -1;
    private int halfWidth;
    private int halfHeight;

    private int Score = 0;
    private int Level = 0;

    private int nextMinoIndex = -1;

    private void Start()
    {
        gameoverPanel.SetActive(false);

        halfWidth = Mathf.RoundToInt(boardWidth * 0.5f);
        halfHeight = Mathf.RoundToInt(boardHeight * 0.5f);

        nextFallTime = Time.time + fallCycle;

        CreateBackground();

        for (int i = 0; i < boardHeight; ++i)
        {
            var col = new GameObject((boardHeight - i - 1).ToString());
            col.transform.position = new Vector3(0, halfHeight - i, 0);
            col.transform.parent = boardNode;
        }

        CreateTetromino();
    }

    void Update()
    {
        if (gameoverPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
        else
        {
            Vector3 moveDir = Vector3.zero;
            bool isRotate = false;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                moveDir.x = -1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                moveDir.x = 1f;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (curIndex != 3) isRotate = true;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                moveDir.y = -1;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                while (MoveTetromino(Vector3.down, false))
                {
                }
            }

            // 아래로 떨어지는 경우는 강제로 이동시킵니다.
            if (Time.time > nextFallTime)
            {
                nextFallTime = Time.time + fallCycle;
                moveDir = Vector3.down;
                isRotate = false;
            }
            if (moveDir != Vector3.zero || isRotate)
            {
                MoveTetromino(moveDir, isRotate);
            }
        }
    }

    void CheckBoardColumn()
    {
        bool isCleared = false;

        // 완성된 행 == 행의 자식 갯수가 가로 크기
        foreach (Transform column in boardNode)
        {
            if (column.childCount == boardWidth)
            {
                Score += 100;
                fallCycle = 1 - (Score / 1500) * 0.1f;
                Level = Score / 1500;
                foreach (Transform tile in column)
                {
                    Destroy(tile.gameObject);
                }

                column.DetachChildren();
                isCleared = true;
            }
        }
        if (isCleared)
        {
            for (int i = 1; i < boardNode.childCount; ++i)
            {
                var column = boardNode.Find(i.ToString());

                // 이미 비어 있는 행은 무시
                if (column.childCount == 0)
                    continue;

                int emptyCol = 0;
                int j = i - 1;
                while (j >= 0)
                {
                    if (boardNode.Find(j.ToString()).childCount == 0)
                    {
                        emptyCol++;
                    }
                    j--;
                }

                if (emptyCol > 0)
                {
                    var targetColumn = boardNode.Find((i - emptyCol).ToString());

                    while (column.childCount > 0)
                    {
                        Transform tile = column.GetChild(0);
                        tile.parent = targetColumn;
                        tile.transform.position += new Vector3(0, -emptyCol, 0);
                    }
                    column.DetachChildren();
                }
            }
        }
    }

    bool MoveTetromino(Vector3 moveDir, bool isRotate)
    {
        Vector3 oldPos = tetrominoNode.transform.position;
        Quaternion oldRot = tetrominoNode.transform.rotation;

        tetrominoNode.transform.position += moveDir;
        if (isRotate)
        {
            tetrominoNode.transform.rotation *= Quaternion.Euler(0, 0, 90);
        }

        if (!CanMoveTo(tetrominoNode))
        {
            tetrominoNode.transform.position = oldPos;
            tetrominoNode.transform.rotation = oldRot;

            if ((int)moveDir.y == -1 && (int)moveDir.x == 0 && isRotate == false)
            {
                AddToBoard(tetrominoNode);
                CheckBoardColumn();
                CreateTetromino();
                if (!CanMoveTo(tetrominoNode))
                {
                    gameoverPanel.SetActive(true);
                }
            }

            return false;
        }

        return true;
    }

    // 테트로미노를 보드에 추가
    void AddToBoard(Transform root)
    {
        while (root.childCount > 0)
        {
            var node = root.GetChild(0);

            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            node.parent = boardNode.Find(y.ToString());
            node.name = x.ToString();
        }
    }

    // 이동 가능한지 체크
    bool CanMoveTo(Transform root)
    {
        for (int i = 0; i < root.childCount; ++i)
        {
            var node = root.GetChild(i);
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            if (x < -0.0f || x > boardWidth - 2)
                return false;

            if (y < 0)
                return false;

            var column = boardNode.Find(y.ToString());

            if (column != null && column.Find(x.ToString()) != null)
                return false;
        }

        return true;

    }
    //백그라운드 생성 타일
    Tile BackCreateTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        var go = Instantiate(BacktilePrefab);
        go.transform.parent = parent;
        go.transform.localPosition = position;

        var tile = go.GetComponent<Tile>();
        tile.color = color;
        tile.sortingOrder = order;

        return tile;
    }
    // 밑에 생성 타일
    Tile WaillCreateTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        var go = Instantiate(WaillTilePrefab);
        go.transform.parent = parent;
        go.transform.localPosition = position;

        var tile = go.GetComponent<Tile>();
        tile.color = color;
        tile.sortingOrder = order;

        return tile;
    }    
    
    Tile NomailCreateTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        var go = Instantiate(NomailtilePrefab);
        go.transform.parent = parent;
        go.transform.localPosition = position;

        var tile = go.GetComponent<Tile>();
        tile.color = color;
        tile.sortingOrder = order;

        return tile;
    }



    void CreateBackground()
    {
        Color color = Color.gray;

        // 타일 보드
        color.a = 0.0f;
        for (int x = -halfWidth; x < halfWidth; ++x)
        {
            for (int y = halfHeight; y >= -halfHeight; --y)
            {
                BackCreateTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }

        // 좌우 테두리
        color.a = 0.0f;
        for (int y = halfHeight; y >= -halfHeight; --y)
        {
            WaillCreateTile(backgroundNode, new Vector2(-halfWidth - 1.0f, y), color, 0);
            WaillCreateTile(backgroundNode, new Vector2(halfWidth, y), color, 0);
        }

        // 아래 테두리
        for (int x = -halfWidth - 1 ; x <= halfWidth; ++x)
        {
            WaillCreateTile(backgroundNode, new Vector2(x, -halfHeight), color, 0);
        }
    }

    void CreateTetromino()
    {
        if (nextMinoIndex == -1)
        {
            int index = Random.Range(0, 7);
            nextMinoIndex= Random.Range(0, 7);
            if (index == nextMinoIndex) nextMinoIndex++;
            Color32 color = Color.white;

            curIndex = index;

            tetrominoNode.rotation = Quaternion.identity;
            tetrominoNode.position = new Vector2(0, halfHeight);

            switch (index)
            {
                // I : 하늘색
                case 0:
                    color = new Color32(115, 251, 253, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-2f, 0.0f), color).SpriteApply(ArraySprite[0]);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[1]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[2]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[3]);
                    break;

                // J : 파란색
                case 1:
                    color = new Color32(0, 33, 245, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[20]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[22]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[23]);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color).SpriteApply(ArraySprite[21]); ;
                    break;

                // L : 귤색
                case 2:
                    color = new Color32(243, 168, 59, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[24]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[25]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[26]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1.0f), color).SpriteApply(ArraySprite[27]);
                    break;

                // O : 노란색
                case 3:
                    color = new Color32(255, 253, 84, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[14]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[15]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[12]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1f), color).SpriteApply(ArraySprite[13]); ;
                    break;

                // S : 녹색
                case 4:
                    color = new Color32(117, 250, 76, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, -1f), color).SpriteApply(ArraySprite[10]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, -1f), color).SpriteApply(ArraySprite[11]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[8]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[9]); ;
                    break;

                // T : 자주색
                case 5:
                    color = new Color32(255, 253, 84, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[14]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[15]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[12]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1f), color).SpriteApply(ArraySprite[13]); ;
                    break;

                // Z : 빨간색
                case 6:
                    color = new Color32(235, 51, 35, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 1f), color).SpriteApply(ArraySprite[4]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[5]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[6]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[7]); ;
                    break;
            }

        }
        else
        {
            int index = nextMinoIndex;
            nextMinoIndex = Random.Range(0, 7);
            if (index == nextMinoIndex) nextMinoIndex++;
            Color32 color = Color.white;

            curIndex = index;

            tetrominoNode.rotation = Quaternion.identity;
            tetrominoNode.position = new Vector2(0, halfHeight);

            switch (index)
            {
                // I : 하늘색
                case 0:
                    color = new Color32(115, 251, 253, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-2f, 0.0f), color).SpriteApply(ArraySprite[0]);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[1]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[2]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[3]);
                    break;

                // J : 파란색
                case 1:
                    color = new Color32(0, 33, 245, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[20]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[22]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[23]);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color).SpriteApply(ArraySprite[21]); ;
                    break;

                // L : 귤색
                case 2:
                    color = new Color32(243, 168, 59, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[24]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[25]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[26]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1.0f), color).SpriteApply(ArraySprite[27]);
                    break;

                // O : 노란색
                case 3:
                    color = new Color32(255, 253, 84, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[14]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[15]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[12]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1f), color).SpriteApply(ArraySprite[13]); ;
                    break;

                // S : 녹색
                case 4:
                    color = new Color32(117, 250, 76, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, -1f), color).SpriteApply(ArraySprite[10]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, -1f), color).SpriteApply(ArraySprite[11]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[8]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[9]); ;
                    break;

                // T : 자주색
                case 5:
                    color = new Color32(155, 47, 246, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0f), color).SpriteApply(ArraySprite[17]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[18]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[19]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[16]); ;
                    break;

                // Z : 빨간색
                case 6:
                    color = new Color32(235, 51, 35, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 1f), color).SpriteApply(ArraySprite[4]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[5]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[6]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[7]); ;
                    break;
            }
        }
    }
}
