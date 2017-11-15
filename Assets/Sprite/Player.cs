using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Networking;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Slider hpSlider;

    private Network mNetwork;
    public GameObject mEnemyCharacter;
    private Hashtable _htEnemies = new Hashtable();
    private GlobalSingleton globalsigton;
    public EnemyCharacter enemyPlayer;

    public int hp;
    public float the_Rate;
    private Rigidbody rigdby;
    private float H;
    private float V;

    public float max;
    public Vector3 maxVelocity;

    // Use this for initialization
    void Start()
    {

        hpSlider.maxValue = 100;
        hpSlider.minValue = 0;
        globalsigton = GlobalSingleton.GetInstance();


        if(globalsigton.mode == GlobalSingleton.Mode.Network)
        mNetwork = transform.GetComponent<Network>();

        rigdby = GetComponent<Rigidbody>();
        hp = 100;

        if(globalsigton.mode == GlobalSingleton.Mode.Alone)
        enemyPlayer =  AddEnemyCharacter(2333);



    }

    void Update()
    {


        hpSlider.value = hp;

        if (globalsigton.mode == GlobalSingleton.Mode.Network)
        {
            mNetwork.SendStatus(transform.position, transform.eulerAngles, rigdby.velocity,
             hp);
            ProcessPackage();
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //if (!isLocalPlayer)
        //    return;
        H = Input.GetAxis("Horizontal");
        V = Input.GetAxis("Vertical");
        rigdby.AddForce(new Vector3(H, 0, V) * the_Rate, ForceMode.Force);
        LimitVelocity(max);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "EnemyPlayer")
        {
            Rigidbody enemyRigbody = collision.gameObject.GetComponent<Rigidbody>();
            enemyRigbody.AddForce(rigdby.velocity - enemyRigbody.velocity , ForceMode.Impulse);
            rigdby.AddForce(enemyRigbody.velocity - rigdby.velocity , ForceMode.Impulse);

            if (enemyRigbody.velocity.magnitude >= rigdby.velocity.magnitude)
                this.hp -= ((int)enemyRigbody.velocity.magnitude - (int)rigdby.velocity.magnitude)*6;
            else
                enemyPlayer.SetHP(((int)rigdby.velocity.magnitude - (int)enemyRigbody.velocity.magnitude)*6);

            Debug.Log(this.hp);
            
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "EnemyPlayer")
        {
            Rigidbody enemyRigdoby = collision.gameObject.GetComponent<Rigidbody>();
            enemyRigdoby.AddForce((collision.gameObject.transform.position - transform.position).normalized*2, ForceMode.Impulse);
            rigdby.AddForce((transform.position - collision.gameObject.transform.position).normalized*2, ForceMode.Impulse);
        }
    }

    private void LimitVelocity(float max)
    {
        if (rigdby.velocity.magnitude >= max)
        {
            maxVelocity = rigdby.velocity.normalized * max;
        }
    }

    private void ProcessPackage()
    {
        Network.Package p;

        // 获取数据包直到完毕
        while (mNetwork.NextPackage(out p))
        {
            // 确定不是本机，避免重复
            if (mNetwork._name == p.name)
            {
                return;
            }

            // 获取该客户相对应的人物模组
            if (!_htEnemies.Contains(p.name))
            {
                AddEnemyCharacter(p.name);
            }

            // 更新客户的人物模型状态
            EnemyCharacter ec = (EnemyCharacter)_htEnemies[p.name];

            

            // 血量
            ec.SetHP(p.hp);

            // 移动动作
            ec.Move(p.pos.V3, p.rot.V3, p.velocity.V3);
 
        }
    }

    private EnemyCharacter AddEnemyCharacter(int name)
    {
        GameObject p = GameObject.Instantiate(mEnemyCharacter);
        EnemyCharacter ec = p.GetComponent<EnemyCharacter>();

        // 修改ID
        ec.SetName(name);

        // 加入到哈希表
        _htEnemies.Add(name, ec);

        return ec;
    }

    // 删除客户的人物模组
    private void RemoveEnemyCharacter(int id)
    {
        EnemyCharacter ec = (EnemyCharacter)_htEnemies[id];
        ec.Destroy();
        _htEnemies.Remove(id);
    }
     
    // 删除所有客户的人物模组
    public void RemoveAllEnemyCharacter()
    {
        foreach (int id in _htEnemies.Keys)
        {
            EnemyCharacter ec = (EnemyCharacter)_htEnemies[id];
            ec.Destroy();
        }
        _htEnemies.Clear();
    }


    
}
