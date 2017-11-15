using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : MonoBehaviour
{
    private Transform mCharacter;
    //private Player mCharacterComponent;
    private Transform mCamera;
    private Transform mRightHand;
    private AudioSource mGunAudio;
    private AudioSource mAudio;
    private ParticleSystem mFireEffect;     // 开枪后的火花
    private bool isDestroy = false;

    private Rigidbody eRigbody;
    private Vector3 toMCharacter;
    public float max = 20;
    private Vector3 maxVelocity;


    // Use this for initialization
    void Start()
    {
        // 获取本机玩家的对象
        mCharacter = GameObject.Find("Player").transform;
        //mCharacterComponent = mCharacter.GetComponent<Player>();
        // 显示血量和ID的组件
        txID = transform.Find("Name");
        txIDText = transform.Find("Name").GetComponent<TextMesh>();
        txHP = transform.Find("HP");
        txHPText = transform.Find("HP").GetComponent<TextMesh>();

        eRigbody = this.GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        // 摧毁对象
        if (isDestroy)
        {
            Destroy(gameObject);
        }

        // 更新对象属性
        UpdataProperties();
    }

    private void FixedUpdate()
    {
        Ai();
        LimitVelocity(max);

    }



    // 销毁角色
    public void Destroy()
    {
        isDestroy = true;
    }

    // 角色移动动作
    public void Move(Vector3 pos, Vector3 rot, Vector3 velocity)
    {
        if (pos != transform.position)
        {
            transform.position = pos;
        }

        transform.eulerAngles = rot;


    }





    // 人物变量
    private int _name = 1;
    public int _hp = 100;

    private Transform txID;
    private TextMesh txIDText;
    private Transform txHP;
    private TextMesh txHPText;
    
    public void SetName(int name)
    {
        _name = name;
    }

    // 角色血量
    public void SetHP(int hpChanged)
    {
        _hp -= hpChanged;
    }


    // 更新角色变量/属性
    private void UpdataProperties()
    {

        // 显示血量和ID
        txIDText.text = "Name:" + _name.ToString();
        txHPText.text = "HP:" + _hp.ToString();

        // 血量和ID的方向，面向着本机玩家
        txID.rotation = mCharacter.rotation;
        txHP.rotation = mCharacter.rotation;
    }

    private void Ai()
    {
        toMCharacter = mCharacter.transform.position - this.transform.position;
        eRigbody.AddForce(toMCharacter.normalized*20, ForceMode.Force);
    }

    private void LimitVelocity(float max)
    {
        if (eRigbody.velocity.magnitude >= max)
        {
            maxVelocity = eRigbody.velocity.normalized * max;
        }
    }
}
