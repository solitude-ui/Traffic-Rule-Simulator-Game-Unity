using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ArrowIndicator : MonoBehaviour
{
    [Header("Bob Settings")]
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobSpeed = 2f;

    private PathManager manager;
    private Vector3 baseLocalPosition;
    private float bobTimer;

    private void Awake()
    {
        baseLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        bobTimer = 0f;
        transform.localPosition = baseLocalPosition;
    }

    private void OnDisable()
    {
        transform.localPosition = baseLocalPosition;
    }

    private void Update()
    {
        bobTimer += Time.deltaTime;

        float offset = Mathf.Sin(bobTimer * bobSpeed) * bobHeight;
        transform.localPosition = baseLocalPosition + Vector3.up * offset;
    }

    public void Init(PathManager pathManager)
    {
        manager = pathManager;
    }

    private void Reset()
    {
        Collider arrowCollider = GetComponent<Collider>();
        if (arrowCollider != null)
            arrowCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (manager == null)
        {
            Debug.LogWarning("ArrowIndicator: PathManager reference is missing.", this);
            return;
        }

        if (!manager.IsPlayerCollider(other)) return;

        manager.OnArrowCollected();
        gameObject.SetActive(false);
    }
}
