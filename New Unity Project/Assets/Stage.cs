using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

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

    public AudioSource Delete; 

    /** Board�� ũ�⸦ �����ϴ� ��ġ -> Range�� ���� ���� ��������
    * ������ ���� ����� ���� �����ϴ� �� �־ ���� �߿��� ������ �Ǵ� �κ�
    * Ư�� ���� �������� ũ�� ������ ������ �Լ� ���� �ʿ�
    */
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

    public int Score = 0;
    public int Level = 1;

    public Text ScoreText;
    public Text LevelText;
    public Text GameOverScoreText;

    private int nextMinoIndex = -1;

    /**
     * �⺻ ����
     * ���� ũ���� ����(�ʺ�, ����)�� ����
     * ���� ����� �������� �ð� ����
     * ��� ����
     * ���� ���̸� ������� ������ ���ִ� boardNode�� ����
     * ��Ʈ���� ��� ����
     */
    private void Start()
    {
        ScoreText.text = "0";
        GameOverScoreText.text = "0";
        LevelText.text = "Lv.1";

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

    /**
     * Ű ���ε�
     * ȸ�� ����, �̵� ������ �����Ͽ� MoveTotermino �Լ��� �ش� ���� ����
     */
    void Update()
    {
        if (gameoverPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
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

            // �Ʒ��� �������� ���� ������ �̵���ŵ�ϴ�.
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }
    }

    /**
     * �����Ͱ� �� ���� á���� Ȯ��
     * BoardNode�� ������(column)�� ������ ������ŭ foreach
     * childcount == boardWidth -> �� ���� ���� ��
     * ���� �� ���� ��ϵ� �ļ�ó��
     */
    void CheckBoardColumn()
    {
        bool isCleared = false;

        // �ϼ��� �� == ���� �ڽ� ������ ���� ũ��
        foreach (Transform column in boardNode)
        {
            if (column.childCount == boardWidth)
            {
                Score += 100;
                ScoreText.text = Score.ToString(); // �ؽ�Ʈ�� ���� ǥ��
                fallCycle = 1 - (Score / 1500) * 0.1f;
                Level = Score / 1500 + 1;
                LevelText.text = "Lv."+ Level.ToString(); // �ؽ�Ʈ�� ���� ǥ��
                foreach (Transform tile in column)
                {
                    Destroy(tile.gameObject);
                    Delete.Play(); // ���� ����
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

                // �̹� ��� �ִ� ���� ����
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

    /**
     * �̵� ���� ���� üũ
     * ���� ��ġ ���� 
     * ���� �� �̵� ���Ѻ���, ȸ�� ��Ű��(���Ϸ��� �̿��Ͽ� �� �������� ����)
     * �̵��� �����Ѵٸ� true �Ұ����ϴٸ� ���� ��ġ�� �̵� �� ���� �ٴڿ� ��Ҵٸ� ���忡 �߰�
     */
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
                if (!CanMoveTo(tetrominoNode)) // ���� ���� ���
                {
                    ScoreText.gameObject.SetActive(false);
                    gameoverPanel.SetActive(true);
                }
            }

            return false;
        }

        return true;
    }

    // ��Ʈ�ι̳븦 ���忡 �߰�
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

    // �̵� �������� üũ(������ ������� �̵��� ��ġ�� �����Ͱ� �ִ��� üũ)
    bool CanMoveTo(Transform root)
    {
        for (int i = 0; i < root.childCount; ++i)
        {
            var node = root.GetChild(i);
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            if (x < -0.0f || x > boardWidth - 3)
                return false;

            if (y < 0)
                return false;

            var column = boardNode.Find(y.ToString());

            if (column != null && column.Find(x.ToString()) != null)
                return false;
        }

        return true;

    }
    //��׶��� ���� Ÿ��
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
    // �ؿ� ���� Ÿ��
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

        // Ÿ�� ����
        color.a = 0.0f;
        for (int x = -halfWidth; x < halfWidth; ++x)
        {
            for (int y = halfHeight; y >= -halfHeight; --y)
            {
                BackCreateTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }

        // �¿� �׵θ�
        color.a = 0.0f;
        for (int y = halfHeight; y >= -halfHeight; --y)
        {
            WaillCreateTile(backgroundNode, new Vector2(-halfWidth - 1.0f, y), color, 0);
            WaillCreateTile(backgroundNode, new Vector2(halfWidth, y), color, 0);
        }

        // �Ʒ� �׵θ�
        for (int x = -halfWidth - 1 ; x <= halfWidth; ++x)
        {
            WaillCreateTile(backgroundNode, new Vector2(x, -halfHeight), color, 0);
        }
    }

    //��Ʈ���� ����
    /**
     * �ӽ÷� ��������� �̸� �˱����ؼ� nextMinoIndex���(ù �Լ� ����ÿ��� Random�� �� �� ���)
     * ���Ĵ� next���� �޾Ƽ� ����ϸ� next���� Random���� ���� ���� �޴´�.
     * ���� index���� �������� Tile�� ��ȯ(�ڴ� sprite�� �����ϱ����Ͽ� ���)
     */
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
                // I : �ϴû�
                case 0:
                    color = new Color32(115, 251, 253, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-2f, 0.0f), color).SpriteApply(ArraySprite[0]);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[1]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[2]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[3]);
                    break;

                // J : �Ķ���
                case 1:
                    color = new Color32(0, 33, 245, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[20]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[22]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[23]);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color).SpriteApply(ArraySprite[21]); ;
                    break;

                // L : �ֻ�
                case 2:
                    color = new Color32(243, 168, 59, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[24]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[25]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[26]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1.0f), color).SpriteApply(ArraySprite[27]);
                    break;

                // O : �����
                case 3:
                    color = new Color32(255, 253, 84, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[14]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[15]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[12]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1f), color).SpriteApply(ArraySprite[13]); ;
                    break;

                // S : ���
                case 4:
                    color = new Color32(117, 250, 76, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, -1f), color).SpriteApply(ArraySprite[10]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, -1f), color).SpriteApply(ArraySprite[11]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[8]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[9]); ;
                    break;

                // T : ���ֻ�
                case 5:
                    color = new Color32(255, 253, 84, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[14]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[15]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[12]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1f), color).SpriteApply(ArraySprite[13]); ;
                    break;

                // Z : ������
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
                // I : �ϴû�
                case 0:
                    color = new Color32(115, 251, 253, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-2f, 0.0f), color).SpriteApply(ArraySprite[0]);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[1]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[2]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[3]);
                    break;

                // J : �Ķ���
                case 1:
                    color = new Color32(0, 33, 245, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[20]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[22]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[23]);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color).SpriteApply(ArraySprite[21]); ;
                    break;

                // L : �ֻ�
                case 2:
                    color = new Color32(243, 168, 59, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color).SpriteApply(ArraySprite[24]);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0.0f), color).SpriteApply(ArraySprite[25]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0.0f), color).SpriteApply(ArraySprite[26]);
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1.0f), color).SpriteApply(ArraySprite[27]);
                    break;

                // O : �����
                case 3:
                    color = new Color32(255, 253, 84, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[14]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[15]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[12]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 1f), color).SpriteApply(ArraySprite[13]); ;
                    break;

                // S : ���
                case 4:
                    color = new Color32(117, 250, 76, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, -1f), color).SpriteApply(ArraySprite[10]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, -1f), color).SpriteApply(ArraySprite[11]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[8]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[9]); ;
                    break;

                // T : ���ֻ�
                case 5:
                    color = new Color32(155, 47, 246, 255);
                    NomailCreateTile(tetrominoNode, new Vector2(-1f, 0f), color).SpriteApply(ArraySprite[17]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 0f), color).SpriteApply(ArraySprite[18]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(1f, 0f), color).SpriteApply(ArraySprite[19]); ;
                    NomailCreateTile(tetrominoNode, new Vector2(0f, 1f), color).SpriteApply(ArraySprite[16]); ;
                    break;

                // Z : ������
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
