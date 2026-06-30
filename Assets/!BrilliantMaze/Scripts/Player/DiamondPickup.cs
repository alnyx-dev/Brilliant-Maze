using UnityEngine;

public class DiamondPickup : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private DiamondCounter _diamondCounter;
    [SerializeField] private AudioClip _pickupClip;
    [SerializeField] [Range(0f, 1f)] private float _pickupVolume = 0.7f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Diamond"))
        {
            other.gameObject.SetActive(false);
            _diamondCounter.Increment();

            if (_pickupClip != null)
                _audioSource.PlayOneShot(_pickupClip, _pickupVolume);

            _gameManager.ReportDiamondCollected();
        }
    }
}
