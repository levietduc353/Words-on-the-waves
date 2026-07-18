using System;
using UnityEngine;
using UnityEngine.UI;
using WordsOnTheWaves.Events;
using WordsOnTheWaves.Core;

namespace WordsOnTheWaves.UI
{
    [Serializable]
    public struct MapLocationData
    {
        public Button locationButton;

#if UNITY_EDITOR
        [Tooltip("Kéo thả Scene bãi biển vào đây")]
        public UnityEditor.SceneAsset targetSceneAsset;
#endif

        [HideInInspector]
        public string targetSceneName;
    }

    public class MapCanvasController : MonoBehaviour
    {
        [Header("Map UI")]
        [SerializeField] private GameObject _mapPanel;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_locations == null) return;
            for (int i = 0; i < _locations.Length; i++)
            {
                if (_locations[i].targetSceneAsset != null)
                {
                    _locations[i].targetSceneName = _locations[i].targetSceneAsset.name;
                }
            }
        }
#endif
        
        [SerializeField] private MapLocationData[] _locations = new MapLocationData[4];
        [SerializeField] private Button _decorButton;

        [Header("Decor UI (Kéo thả thủ công)")]
        [SerializeField] private GameObject _decorPanel;
        [SerializeField] private Button _decorBackButton;

        private void Awake()
        {
            if (_mapPanel != null) _mapPanel.SetActive(false);
            if (_decorPanel != null) _decorPanel.SetActive(false);
        }

        private void OnEnable()
        {
            EventManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleGameStateChanged(GameManager.Instance.CurrentStateName);
            }

            // Gán sự kiện click cho các nút map
            foreach (var location in _locations)
            {
                if (location.locationButton != null && !string.IsNullOrEmpty(location.targetSceneName))
                {
                    string sceneName = location.targetSceneName; 
                    location.locationButton.onClick.AddListener(() => OnLocationButtonClicked(sceneName));
                }
            }

            // Gán sự kiện nút Decor
            if (_decorButton != null)
            {
                _decorButton.onClick.AddListener(() => EventManager.OnDecorButtonClicked?.Invoke());
            }

            // Gán sự kiện nút Back của Decor
            if (_decorBackButton != null)
            {
                Debug.Log("[MapCanvas] decorBackButton listener REGISTERED");
                _decorBackButton.onClick.AddListener(() =>
                {
                    Debug.Log("[MapCanvas] decorBackButton CLICKED → firing OnBackToMapClicked");
                    EventManager.OnBackToMapClicked?.Invoke();
                });
            }
            else
            {
                Debug.LogError("[MapCanvas] _decorBackButton chưa được gán trong Inspector!");
            }
        }

        private void OnLocationButtonClicked(string targetSceneName)
        {
            EventManager.OnMapSelected?.Invoke(targetSceneName);
        }

        private void HandleGameStateChanged(string stateName)
        {
            Debug.Log($"[MapCanvas] HandleGameStateChanged: {stateName}");
            if (_mapPanel != null)
            {
                _mapPanel.SetActive(stateName == "MapTravelState");
            }

            if (_decorPanel != null)
            {
                _decorPanel.SetActive(stateName == "DecorState");
            }
        }
    }
}
