using UnityEngine.UI;
using UnityEngine;
using UnityEditor.Animations;

public class RamFighter : MonoBehaviour
{
    [SerializeField] string ramName;
    [SerializeField] int maxHealth = 100;
    [SerializeField] int currentHealth;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float moveSpeed = 3f;
    private Animator animator;


    private Image healthBar;
    private RamFighter opponentFighter;
    private Vector3 startPosition;
    private Vector3 startRotation;
    private bool isMovingForward = false;
    private bool isMovingBackward = false;

    // Attack properties
    public int minDamage = 5;
    public int maxDamage = 15;
    public float criticalChance = 0.2f;
    public float criticalMultiplier = 1.5f;

    // Event to notify when attack sequence is complete
    public event System.Action OnAttackSequenceComplete;

    void Awake()
    {
        
        startPosition = transform.position;
        startRotation = transform.eulerAngles;

        


    }

    private void Start()
    {
        ResetHealth();
    }

    private void Update()
    {
        if (!IsAlive()) return;

        if (isMovingForward)
        {
            MoveTowardOpponent();
        }
        else if (isMovingBackward)
        {
            MoveBackToStart();
            CheckReturnComplete();
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.fillAmount = 1f;

        if (animator != null)
        {
            animator.SetBool("IsAlive", true);
            animator.Play("Idle");
        }

        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        isMovingForward = false;
        isMovingBackward = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;

        if (currentHealth > 0 && animator != null)
        {
            animator.SetTrigger("Hit");
        }
        else if (currentHealth <= 0 && animator != null)
        {
            animator.SetTrigger("Die");
            animator.SetBool("IsAlive", false);
            animator.Play("Death");
        }
    }

    public void InitiateAttackSequence()
    {
        if (!isMovingForward && !isMovingBackward && IsAlive() && opponentFighter.IsAlive())
        {
            isMovingForward = true;
            if (animator != null)
            {
                animator.SetFloat("Speed", 1f);
            }
        }
    }

    private void MoveTowardOpponent()
    {
        if (opponentFighter == null || !opponentFighter.IsAlive()) return;

        float distanceToOpponent = Vector3.Distance(transform.position, opponentFighter.transform.position);

        if (distanceToOpponent > attackRange)
        {
            Vector3 direction = (opponentFighter.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(opponentFighter.transform.position);
        }
        else
        {
            // Reached attack range, stop moving and attack
            isMovingForward = false;
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            Attack(opponentFighter);
        }
    }

    private void MoveBackToStart()
    {
        float distanceToStart = Vector3.Distance(transform.position, startPosition);

        if (distanceToStart > 0.1f)
        {
            Vector3 direction = (startPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void CheckReturnComplete()
    {
        if (Vector3.Distance(transform.position, startPosition) < 0.1f)
        {
            transform.position = startPosition;
            transform.eulerAngles = startRotation;
            isMovingBackward = false;

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }

            OnAttackSequenceComplete?.Invoke();
        }
    }

    public void Attack(RamFighter opponent)
    {
        if (animator != null && IsAlive())
        {
            int attackType = Random.Range(0, 3);
            switch (attackType)
            {
                case 0:
                    animator.SetTrigger("Attack");
                    break;
                case 1:
                    animator.SetTrigger("Attack01");
                    break;
                case 2:
                    animator.SetTrigger("Attack02");
                    break;
            }
        }
    }

    public void TakeOpponentDamage()
    {
        if (opponentFighter != null && opponentFighter.IsAlive())
        {
            int baseDamage = Random.Range(minDamage, maxDamage + 1);
            bool isCritical = Random.value < criticalChance;
            int finalDamage = isCritical ? Mathf.RoundToInt(baseDamage * criticalMultiplier) : baseDamage;

            opponentFighter.TakeDamage(finalDamage);

            if (isCritical)
            {
                Debug.Log(ramName + " scored a CRITICAL HIT!");
            }
        }
    }

    public void ReturnToPosition()
    {
        if (IsAlive())
        {
            isMovingBackward = true;
            if (animator != null)
            {
                animator.SetFloat("Speed", -1f);
            }
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void SetOpponent(RamFighter opponent)
    {
        opponentFighter = opponent;
    }

    public void InjectDependencies(Image healthBarImage, RamFighter opponent, RuntimeAnimatorController animatorController)
    {
        healthBar = healthBarImage;
        opponentFighter = opponent;

        if (animator == null)
            animator = GetComponent<Animator>();

        animator.runtimeAnimatorController = animatorController;

        InitializeAnimatorParameters();
    }


    private void InitializeAnimatorParameters()
    {
        if (animator != null)
        {
            animator.SetBool("IsAlive", true);
            animator.SetFloat("Speed", 0f);
        }
    }

    public string GetRamName()
    {
        return ramName;
    }
    public void SetRamName(string name)
    {
        ramName = name;
    }
}


