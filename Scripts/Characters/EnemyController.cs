using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD }
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    private Vector3 guardPos;
    private CharacterStats characterStats;

    [Header("Basic Settings")]

    public float sightRadius;
    public bool isGuard;
    private float speed;
    private GameObject attackTarget;
    public float lookAtTime;
    private float remainLookAtTime;
    private float lastAttackTime;

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;

    // bool配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    
    void Awake(){
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        speed = agent.speed;
        guardPos = agent.transform.position;
        remainLookAtTime = lookAtTime;
    }

    void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
    }

    void Update(){ // 此处因为把Update拼错找bug找了两个小时！！！！！
        SwitchStates();
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }

    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
    }

    void SwitchStates(){
        // 如果发现player则自动追击
        if (FoundPlayer())
            {
                enemyStates = EnemyStates.CHASE;
            }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                break;
            case EnemyStates.PATROL:

                isChase = false;
                agent.speed = speed * 0.5f;

                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                        remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
            case EnemyStates.CHASE:

                isWalk = false;
                isChase = true;

                agent.speed = speed;
                if (!FoundPlayer())
                {
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }

                    else if (isGuard)
                        enemyStates = EnemyStates.GUARD;
                    else
                        enemyStates = EnemyStates.PATROL;

                }else{
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                // 在可攻击范围之内
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;

                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.collDown;

                        //暴击判断
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }
                }

                break;
            case EnemyStates.DEAD:
                break;
        }
    }

    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //近身攻击
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //远程攻击
            anim.SetTrigger("Skill");
        }
    }

    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    //近战攻击
    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else
            return false;
    }

    //远程攻击
    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }

    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);

        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    //Animation Event
    void Hit()
    {
        if (attackTarget != null)
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
}
