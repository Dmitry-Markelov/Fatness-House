using UnityEngine;

public class GymDumbbell : MonoBehaviour
{
    [Header("Настройки гантели")]
    public int currencyAmount = 1;
    public float rotationSpeed = 50f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    private Vector3 startPosition;
    private bool collected = false;

    [Header("Эффекты")]
    public ParticleSystem collectParticles;
    public AudioClip collectAudio;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!collected)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            CollectDumbbell();
        }
    }

    void CollectDumbbell()
    {
        collected = true;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(currencyAmount);
        }

        if (collectParticles != null)
        {
            Instantiate(collectParticles, transform.position, Quaternion.identity);
        }

        if (collectAudio != null)
        {
            AudioSource.PlayClipAtPoint(collectAudio, Camera.main.transform.position);
        }

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 1f);
    }

    public void ForceCollect()
    {
        if (!collected)
            CollectDumbbell();
    }
}