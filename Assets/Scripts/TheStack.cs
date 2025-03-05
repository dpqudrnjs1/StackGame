using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour
{
    // Const Value
    private const float BoundSize = 3.5f;          //블록 사이즈
    private const float MovingBoundsSize = 3f;     //블록이 이동하는 양
    private const float StackMovingSpeed = 5.0f;   //이동하는 스피드
    private const float BlockMovingSpeed = 3.5f;    //블록이 이동하는 스피드
    private const float ErrorMargin = 0.1f;         //성공판정

    public GameObject originBlock = null;         //Prehab으로 한 블록

    private Vector3 prevBlockPosition;          //이전의 블록위치
    private Vector3 desiredPosition;            //이동해야 되는 포지션
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);        //다음 생성할 블록 생성

    Transform lastBlock = null;     //새 블록을 생성
    float blockTransition = 0f;
    float secondaryPosition = 0f;

    int stackCount = -1;        //시작할때 +1사용하기 때문에 -1
    public int Score { get { return stackCount; } } 
    
    int comboCount = 0;
    public int Combo { get { return comboCount; } }
    private int maxCombo = 0;
    public int MaxCombo { get => maxCombo; }

    public Color prevColor;     //이전의 색
    public Color nextColor;     //다음의 색

    bool isMovingX = true; // X축 이동

    int bestScore = 0;
    public int BestScore { get => bestScore; }  //최고점수

    int bestCombo = 0;
    public int BestCombo { get => bestCombo; }  //최고콤보

    private const string BestScoreKey = "BestScore";
    private const string BestComboKey = "BestCombo";
   
    private bool isGameOver = true;    //게임오버
    void Start()
    {
        if (originBlock == null)            //블록이 없다면 돌아간다.
        {
            Debug.Log("OriginBlock is NULL");
            return;
        }

        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        bestCombo = PlayerPrefs.GetInt(BestComboKey, 0);

        prevBlockPosition = Vector3.down;          //-1지점에 블록 쌓기
        Spawn_Block();
        Spawn_Block();
    }

    void Update()
    {
        if (isGameOver)
            return;

        if (Input.GetMouseButtonDown(0))        //마우스 왼쪽 클릭하면   
        {
            if(PlaceBlock())                    //잘 맞춰지면
            {
                Spawn_Block();                  //블록을 스폰한다.
            }
            else
            {
                Debug.Log("Game Over");         //아니면 게임오버
                UpdateScore();
                isGameOver = true;
                GameOverEffect();
                UIManager.Instance.SetScoreUI();
            }
        }

        MoveBlock();
        transform.position = Vector3.Lerp(transform.position, desiredPosition, StackMovingSpeed * Time.deltaTime);
    } //매 프레임마다 지속적으로 desiredPosition에 이동하게 만드는 코드

    bool Spawn_Block()
    {
        // 이전블럭 저장
        if (lastBlock != null)              //스폰 블록이 null 아니면
            prevBlockPosition = lastBlock.localPosition;

        GameObject newBlock = null;
        Transform newTrans = null;

        newBlock = Instantiate(originBlock);

        if (newBlock == null)
        {
            Debug.Log("NewBlock Instantiate Failed!");
            return false;
        }

        ColorChange(newBlock);

        newTrans = newBlock.transform;
        newTrans.parent = this.transform;   //오브젝트의 부모를 transform에 가진다
        newTrans.localPosition = prevBlockPosition + Vector3.up;    //Y값이 1이라 한 칸 올리면 바로 위로 올리기
        newTrans.localRotation = Quaternion.identity; //Quaternion의 초기값, 회전이 없는 상태
        newTrans.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        stackCount++;

        desiredPosition = Vector3.down * stackCount;
        blockTransition = 0f;       //이동에 대한 처리하기 위한 기준값

        lastBlock = newTrans;

        isMovingX = isMovingX;      //true → false, false → true

        UIManager.Instance.UpdateScore();
        return true;
    }

    Color GetRandomColor() //랜덤 컬러 생성
    {
        float r = Random.Range(100f, 250f) / 255f;
        float g = Random.Range(100f, 250f) / 255f;
        float b = Random.Range(100f, 250f) / 255f;

        return new Color(r, g, b);
    }
    void ColorChange(GameObject go)
    {
        Color applyColor = Color.Lerp(prevColor, nextColor, (stackCount % 11) / 10f); // 0~10 값 순환

        Renderer rn = go.GetComponent<Renderer>();

        if (rn == null)
        {
            Debug.Log("Renderer is NULL!");
            return;
        }

        rn.material.color = applyColor;
        Camera.main.backgroundColor = applyColor - new Color(0.1f, 0.1f, 0.1f);

        if (applyColor.Equals(nextColor) == true)
        {
            prevColor = nextColor;
            nextColor = GetRandomColor();
        }
    }
    void MoveBlock()
    {
        blockTransition += Time.deltaTime * BlockMovingSpeed;

        float movePosition = Mathf.PingPong(blockTransition, BoundSize) - BoundSize / 2;   //BoundSize 기준으로 전체 BoundSize 만큼 왔다갔다 가는 크기 지정

        if (isMovingX)
        {
            lastBlock.localPosition = new Vector3(movePosition * MovingBoundsSize, stackCount, secondaryPosition);
        }
        else
        {
            lastBlock.localPosition = new Vector3(secondaryPosition, stackCount, -movePosition * MovingBoundsSize);
        }
    }
    bool PlaceBlock()
    {
        Vector3 lastPosition = lastBlock.localPosition;

        if (isMovingX)  //X축 이동
        {
            float deltaX = prevBlockPosition.x - lastPosition.x;    //deltaX가 잘려 나가야 되는 크기
            bool isNegativeNum = (deltaX < 0) ? true : false;       //파편이 떨어지는 방향을 지정

            deltaX = Mathf.Abs(deltaX);         //절대값 구하기
            if (deltaX > ErrorMargin)           //deltaX가 ErrorMArgin보다 크면 잘라내기
            {
                stackBounds.x -= deltaX;       //다음 블록을 생성할 크기
                if (stackBounds.x <= 0)
                {
                    return false;              
                }

                float middle = (prevBlockPosition.x + lastPosition.x) / 2;      //두 블록 중심값 구하기
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                Vector3 tempPosition = lastBlock.localPosition;
                tempPosition.x = middle;
                lastBlock.localPosition = lastPosition = tempPosition;

                float rubbleHalfScale = deltaX / 2f;
                CreateRubble(
                    new Vector3(isNegativeNum
                        ? lastPosition.x + stackBounds.x / 2 + rubbleHalfScale
                        : lastPosition.x - stackBounds.x / 2 - rubbleHalfScale
                        , lastPosition.y
                        , lastPosition.z),
                    new Vector3(deltaX, 1, stackBounds.y)
                    );

                comboCount = 0;
            }
            else                                //작을 시 위치보정
            {
                ComboCheck();
                lastBlock.localPosition = prevBlockPosition + Vector3.up;       //이전 블록보다 위
            }
        }
        else
        {
            float deltaZ = prevBlockPosition.z - lastPosition.z;
            bool isNegativeNum = (deltaZ < 0) ? true : false;

            deltaZ = Mathf.Abs(deltaZ);
            if (deltaZ > ErrorMargin)
            {
                stackBounds.y -= deltaZ;
                if (stackBounds.y <= 0)
                {
                    return false;
                }

                float middle = (prevBlockPosition.z + lastPosition.z) / 2;
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                Vector3 tempPosition = lastBlock.localPosition;
                tempPosition.z = middle;
                lastBlock.localPosition = lastPosition = tempPosition;

                float rubbleHalfScale = deltaZ / 2f;
                CreateRubble(
                    new Vector3(
                        lastPosition.x,
                        lastPosition.y,
                        isNegativeNum
                        ? lastPosition.z + stackBounds.y / 2 + rubbleHalfScale
                        : lastPosition.z - stackBounds.y / 2 - rubbleHalfScale),
                    new Vector3(stackBounds.x, 1, deltaZ)
                );

                comboCount = 0;
            }
            else
            {
                ComboCheck();
                lastBlock.localPosition = prevBlockPosition + Vector3.up;
            }
        }

        secondaryPosition = (isMovingX) ? lastBlock.localPosition.x : lastBlock.localPosition.z;

        return true;
    }
    void CreateRubble(Vector3 pos, Vector3 scale)       //파편생성
    {
        GameObject go = Instantiate(lastBlock.gameObject);  //지금 이동하는 블록을 gameObject에 복제
        go.transform.parent = this.transform;               //The Stack 안으로

        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.identity;

        go.AddComponent<Rigidbody>();
        go.name = "Rubble";
    }
    void ComboCheck()   //콤보 만들기
    {
        comboCount++;   //증가시키기

        if (comboCount > maxCombo)  //콤보가 맥스콤보보다 높으면
            maxCombo = comboCount;  //맥스콤보 최신화하기

        if ((comboCount % 5) == 0)  //5의 배수마다 크기 키우기
        {
            Debug.Log("5Combo Success!");
            stackBounds += new Vector3(0.5f, 0.5f);
            stackBounds.x =
                (stackBounds.x > BoundSize) ? BoundSize : stackBounds.x;
            stackBounds.y =
                (stackBounds.y > BoundSize) ? BoundSize : stackBounds.y;
        }
    }
    void UpdateScore()  //점수 업데이트
    {
        if (bestScore < stackCount) //최고점수가 stackCount보다 작다면
        {
            Debug.Log("최고 점수 갱신");
            bestScore = stackCount;
            bestCombo = maxCombo;

            PlayerPrefs.SetInt(BestScoreKey, bestScore); //최고점수 저장
            PlayerPrefs.SetInt(BestComboKey, bestCombo); //최고콤보 저장
        }
    }
    void GameOverEffect()   //게임오버 효과
    {
        int childCount = this.transform.childCount; //하위에 있는 오브젝트 갯수

        for (int i = 1; i < 20; i++)
        {
            if (childCount < i)
                break;

            GameObject go =
                this.transform.GetChild(childCount - i).gameObject; //끝에 있는 Object 찾기. GetChild : 하위 object를 index로 찾아오는 기능

            if (go.name.Equals("Rubble"))
                continue;

            Rigidbody rigid = go.AddComponent<Rigidbody>();

            rigid.AddForce(
                (Vector3.up * Random.Range(0, 10f)
                 + Vector3.right * (Random.Range(0, 10f) - 5f))
                * 100f
            );
        }
    }
    public void Restart()
    {
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        isGameOver = false;

        lastBlock = null;
        desiredPosition = Vector3.zero;
        stackBounds = new Vector3(BoundSize, BoundSize);

        stackCount = -1;
        isMovingX = true;
        blockTransition = 0f;
        secondaryPosition = 0f;

        comboCount = 0;
        maxCombo = 0;

        prevBlockPosition = Vector3.down;

        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        Spawn_Block();
        Spawn_Block();
    }
}
