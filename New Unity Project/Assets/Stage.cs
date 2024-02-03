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

    /** Board의 크기를 설정하는 위치 -> Range를 통한 범위 설정을함
    * 앞으로 여러 모양의 맵을 제작하는 데 있어서 가장 중요한 설정이 되는 부분
    * 특정 값을 기준으로 크기 조절이 가능한 함수 제작 필요
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
     * 기본 세팅
     * 보드 크기의 절반(너비, 높이)를 저장
     * 다음 블록의 떨어지는 시간 설정
     * 배경 생성
     * 보드 높이를 기반으로 판정을 해주는 boardNode와 연결
     * 테트리스 블록 생성
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
     * 키 바인딩
     * 회전 유무, 이동 방향을 저장하여 MoveTotermino 함수로 해당 값을 전달
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }
    }

    /**
     * 데이터가 한 줄이 찼는지 확인
     * BoardNode의 데이터(column)의 데이터 개수만큼 foreach
     * childcount == boardWidth -> 한 줄이 가득 참
     * 지운 뒤 위에 블록들 후속처리
     */
    void CheckBoardColumn()
    {
        bool isCleared = false;

        // 완성된 행 == 행의 자식 갯수가 가로 크기
        foreach (Transform column in boardNode)
        {
            if (column.childCount == boardWidth)
            {
                Score += 100;
                ScoreText.text = Score.ToString(); // 텍스트로 점수 표시
                fallCycle = 1 - (Score / 1500) * 0.1f;
                Level = Score / 1500 + 1;
                LevelText.text = "Lv."+ Level.ToString(); // 텍스트로 레벨 표시
                foreach (Transform tile in column)
                {
                    Destroy(tile.gameObject);
                    Delete.Play(); // 제거 사운드
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

    /**
     * 이동 가능 여부 체크
     * 기존 위치 저장 
     * 현재 값 이동 시켜보기, 회전 시키기(오일러를 이용하여 한 방향으로 돌림)
     * 이동이 가능한다면 true 불가능하다면 이전 위치로 이동 후 만약 바닥에 닿았다면 보드에 추가
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
                if (!CanMoveTo(tetrominoNode)) // 게임 오버 기능
                {
                    ScoreText.gameObject.SetActive(false);
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

    // 이동 가능한지 체크(범위에 들었는지 이동할 위치에 데이터가 있는지 체크)
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

    //테트리스 생성
    /**
     * 임시로 다음블록을 미리 알기위해서 nextMinoIndex사용(첫 함수 실행시에만 Random을 두 번 사용)
     * 이후는 next값을 받아서 사용하며 next값은 Random으로 다음 값을 받는다.
     * 나온 index값을 기준으로 Tile을 소환(뒤는 sprite를 적용하기위하여 사용)
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
