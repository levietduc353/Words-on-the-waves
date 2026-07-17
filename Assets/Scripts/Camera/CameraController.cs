using UnityEngine;
using WordsOnTheWaves.Events;

namespace WordsOnTheWaves.CameraHandling
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target Views")]
        [SerializeField] private Transform _mapViewTransform;
        [SerializeField] private Transform _preparationViewTransform;
        [SerializeField] private Transform _decorViewTransform;

        private void OnEnable()
        {
            EventManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(string stateName)
        {
            if (stateName == "MapTravelState" && _mapViewTransform != null)
            {
                SetTarget(_mapViewTransform);
            }
            else if (stateName == "PreparationState" && _preparationViewTransform != null)
            {
                SetTarget(_preparationViewTransform);
            }
            else if (stateName == "DecorState" && _decorViewTransform != null)
            {
                SetTarget(_decorViewTransform);
            }
        }

        private void SetTarget(Transform target)
        {
            // Teleport thẳng đến vị trí đích thay vì nội suy Lerp
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }
}
