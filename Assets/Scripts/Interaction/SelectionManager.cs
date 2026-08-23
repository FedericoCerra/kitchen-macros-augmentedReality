// Turns a tap into a selected product, by raycasting from the camera against the
// target colliders. On AppRoot. The only script that polls instead of listening.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KitchenMacros
{

    [DefaultExecutionOrder(-50)]
    public class SelectionManager : MonoBehaviour
    {
        [SerializeField] LayerMask selectableLayers = ~0;

        [Tooltip("Metres. A short ray avoids picking up anything far behind the products.")]
        [SerializeField] float maxRayDistance = 5f;

        [SerializeField] bool deselectOnEmptyTap = true;

        public static SelectionManager Instance { get; private set; }

        public event Action<TrackedProduct> OnSelectionChanged;

        public TrackedProduct Selected { get; private set; }

        Camera _camera;
        PointerEventData _pointerData;
        readonly List<RaycastResult> _uiResults = new List<RaycastResult>();

        void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            _camera = Camera.main;
            if (_camera == null) Debug.LogError("[Selection] No camera tagged MainCamera.");
        }

        void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;

            var screenPosition = pointer.position.ReadValue();
            if (IsOverBlockingUI(screenPosition)) return;

            HandleTap(screenPosition);
        }

        void HandleTap(Vector2 screenPosition)
        {
            if (_camera == null) return;

            var ray = _camera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out var hit, maxRayDistance, selectableLayers))
            {
                var product = hit.collider.GetComponentInParent<TrackedProduct>();

                if (product != null && product.IsTracked && product.Product != null)
                {
                    Select(product);
                    return;
                }
            }

            if (deselectOnEmptyTap) Select(null);
        }

        public void Select(TrackedProduct product)
        {
            if (Selected == product) return;

            if (Selected != null) Selected.SetSelected(false);

            Selected = product;

            if (Selected != null) Selected.SetSelected(true);

            OnSelectionChanged?.Invoke(Selected);
        }

        public void ClearSelection() => Select(null);


        bool IsOverBlockingUI(Vector2 screenPosition)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            _pointerData ??= new PointerEventData(eventSystem);
            _pointerData.Reset();
            _pointerData.position = screenPosition;

            _uiResults.Clear();
            eventSystem.RaycastAll(_pointerData, _uiResults);

            foreach (var result in _uiResults)
            {
                var canvas = result.gameObject != null
                    ? result.gameObject.GetComponentInParent<Canvas>()
                    : null;

                if (canvas != null && canvas.rootCanvas.renderMode != RenderMode.WorldSpace)
                    return true;
            }

            return false;
        }
    }
}
