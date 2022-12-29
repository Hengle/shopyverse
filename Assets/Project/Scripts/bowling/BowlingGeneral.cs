using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class BowlingGeneral : MonoBehaviourPunCallbacks
{
    public GameObject BowlingText;

    public GameObject getBallText;

    public GameObject Player_text1;

    public GameObject Player_text2;

    public GameObject Playerscore_text1;

    public GameObject Playerscore_text2;

    public TMP_Text Player1_text;

    public TMP_Text Player2_text;

    public TMP_Text P1_score_text;

    public TMP_Text P2_score_text;

    public GameObject canvas;

    public GameObject board_hud;

    public PhotonView Pv;

    public NetworkPlayerSync nps;

    bool isLoggined = false;

    public GameObject myB_Ball;

    public bool ball_Actived;

    public GameObject GameProps;

    public float speed = 1000;

    public Rigidbody rb;

    float force = 70;

    public bool shooting;

    public int score = 0;

    public Transform ThrowingPoint;

    public GameObject BorderObj;

    public BowlingGame border;

    public bool roundPlayed;

    bool startround = false;

    public TMP_Text roundtxt;

    public GameObject roundobj;

    public Animator _animator;

    public bool played = false;

    public PlayfabCoinController PFCC;

    public GameObject[] pins;

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "border")
        {
            if (Pv.IsMine)
            {
                if (BorderObj == null)
                {
                    BorderObj = other.gameObject;
                }
                if (border == null)
                {
                    border = BorderObj.GetComponent<BowlingGame>();
                }
                nps = gameObject.GetComponent<NetworkPlayerSync>();

                board_hud = GameObject.FindWithTag("board_hud");
                canvas = GameObject.FindWithTag("canvas");
                BowlingText = canvas.transform.GetChild(2).gameObject;
                getBallText = canvas.transform.GetChild(3).gameObject;
                Player1_text = border.p1_text;
                Player2_text = border.p2_text;
                P1_score_text = border.p1_scoretext;
                P2_score_text = border.p2_scoretext;
                if (!isLoggined)
                {
                    BowlingText.SetActive(true);
                }
                if (isLoggined)
                {
                    if (border.isFinished)
                    {
                        if (border.WinnerName == Pv.Owner.NickName)
                        {
                            if (!played)
                            {
                                _animator.SetTrigger("Victory");
                                PFCC.GrantVirtualCurrency();
                                played = true;
                                StartCoroutine(RoundFinish());
                            }
                        }
                        else
                        {
                            if (!played)
                            {
                                _animator.SetTrigger("Defeat");
                                played = true;
                                StartCoroutine(RoundFinish());
                            }
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.F))
                {
                    isLoggined = true;

                    Pv.RPC("SetName", RpcTarget.All, null);
                    if (isLoggined)
                    {
                        BowlingText.SetActive(false);
                    }
                }
            }
        }
        if (other.gameObject.tag == "GetBall")
        {
            if (Pv.IsMine)
            {
                if (isLoggined)
                {
                    if (border.RoundName == Pv.Owner.NickName)
                    {
                        if (ball_Actived == false)
                        {
                            getBallText.SetActive(true);
                            if (Input.GetKeyDown(KeyCode.G))
                            {
                                Pv.RPC("BallActive", RpcTarget.All, null);
                                ball_Actived = true;
                                if (ball_Actived)
                                {
                                    getBallText.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Playland")
        {
            if (Pv.IsMine)
            {
                BowlingText.SetActive(false);
                isLoggined = false;
                Pv.RPC("ClearText", RpcTarget.All, null);
                score = 0;
                Pv.RPC("canNull", RpcTarget.All, null);
            }
        }
        if (other.gameObject.tag == "border")
        {
            if (Pv.IsMine)
            {
                BowlingText.SetActive(false);
            }
        }
        if (other.gameObject.tag == "GetBall")
        {
            if (Pv.IsMine)
            {
                getBallText.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Pv.IsMine)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (ball_Actived)
                {
                    _animator.SetTrigger("Throwing");
                    StartCoroutine(ballCanGo());
                }
            }
            if (ball_Actived == false)
            {
                pins = null;
            }
            if (shooting)
            {
                StartCoroutine(BallReset());
            }
            if (isLoggined)
            {
                Pv.RPC("CountPinsDown", RpcTarget.All, null);
                Pv.RPC("ScoreUpdater", RpcTarget.All, score);
            }
        }
    }

    void Start()
    {
        rb = myB_Ball.GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        rb.maxAngularVelocity = 50;
    }

    IEnumerator BallReset()
    {
        yield return new WaitForSeconds(10);
        if (Pv.IsMine)
        {
            Pv.RPC("GetParent", RpcTarget.All, null);
            shooting = false;
            rb.isKinematic = true;
            myB_Ball.transform.position = GameProps.transform.position;
            Pv.RPC("BallDeactive", RpcTarget.All, null);
            ball_Actived = false;
            roundPlayed = false;
        }
    }

    IEnumerator ballCanGo()
    {
        yield return new WaitForSeconds(2f);
        pins = border.pins;
        Pv.RPC("NullParent", RpcTarget.All, null);
        rb.isKinematic = false;
        rb.AddForce(ThrowingPoint.transform.forward * force);
        shooting = true;
        roundPlayed = true;
    }

    [PunRPC]
    void SetName()
    {
        if (Player1_text.text == "" || Player1_text.text == null)
        {
            Player1_text.text = nps.username.text;
        }
        else if (Player2_text.text == "" || Player2_text.text == null)
        {
            if (Player1_text.text != nps.username.text)
            {
                Player2_text.text = nps.username.text;
            }
        }
    }

    [PunRPC]
    void ScoreUpdater(int score)
    {
        if (Player1_text.text == nps.username.text)
        {
            P1_score_text.text = score.ToString();
        }
        else if (Player2_text.text == nps.username.text)
        {
            P2_score_text.text = score.ToString();
        }
    }

    [PunRPC]
    void ClearText()
    {
        if (Player1_text.text == nps.username.text)
        {
            Player1_text.text = "";
            P1_score_text.text = "";
        }
        if (Player2_text.text == nps.username.text)
        {
            Player2_text.text = "";
            P2_score_text.text = "";
        }
    }

    [PunRPC]
    void canNull()
    {
        BorderObj = null;
        border = null;
    }

    [PunRPC]
    void BallActive()
    {
        myB_Ball.SetActive(true);
    }

    [PunRPC]
    void BallDeactive()
    {
        myB_Ball.SetActive(false);
    }

    [PunRPC]
    void GetParent()
    {
        myB_Ball.transform.parent = GameProps.transform;
    }

    [PunRPC]
    void NullParent()
    {
        myB_Ball.transform.SetParent(null);
    }

    [PunRPC]
    void CountPinsDown()
    {
        for (int i = 0; i < pins.Length; i++)
        {
            if (
                pins[i].transform.eulerAngles.z > 5 &&
                pins[i].transform.eulerAngles.z < 355 &&
                pins[i].activeSelf
            )
            {
                if (Pv.IsMine)
                {
                    StartCoroutine(ScoreCounter());
                }

                if (canFalse)
                {
                    if (Pv.IsMine)
                    {
                        score++;
                        canFalse = false;
                    }
                    pins[i].SetActive(false);
                }
            }
        }
    }

    public bool canFalse = false;

    IEnumerator ScoreCounter()
    {
        yield return new WaitForSeconds(2);

        canFalse = true;
    }

    IEnumerator RoundFinish()
    {
        yield return new WaitForSeconds(6);

        isLoggined = false;
        score = 0;
        played = false;
    }
}