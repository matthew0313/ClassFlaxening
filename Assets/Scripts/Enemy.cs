using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
public class Enemy : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] string m_enemyName;
    [SerializeField] float m_maxHp;
    [SerializeField] Transform m_effectAnchor;
    public string enemyName => m_enemyName;
    public float maxHp => m_maxHp;
    public Transform effectAnchor => m_effectAnchor;
    public void Damage() => anim.SetTrigger("Damage");
    public void Attack() => anim.SetTrigger("Attack");
    public void Die() => anim.SetTrigger("Die");
}