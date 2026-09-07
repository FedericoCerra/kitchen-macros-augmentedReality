// Links a Vuforia target to its product, and owns the panel above it.

using UnityEngine;
using Vuforia;

namespace KitchenMacros
{

    [RequireComponent(typeof(ObserverBehaviour))]
    public class TrackedProduct : MonoBehaviour
    {
        [Header("Info panel")]
        [SerializeField] ProductInfoPanel infoPanelPrefab;

        [Tooltip("Multiplies the scale set on the prefab; does not replace it.")]
        [SerializeField] float panelScale = 1f;

        [Tooltip("Gap in metres between the top of the product and the panel.")]
        [SerializeField] float panelClearance = 0.045f;

        [Header("Highlight")]
        [SerializeField] GameObject highlightVisual;
        [SerializeField] Renderer[] highlightRenderers;
        // Idle pulses between these two; selected holds the warm colour steady.
        [SerializeField] Color highlightColorA = new Color(0.48f, 0.68f, 1f, 0.30f);
        [SerializeField] Color highlightColorB = new Color(0.48f, 0.68f, 1f, 0.95f);
        [SerializeField] Color highlightSelectedColor = new Color(1f, 0.60f, 0.47f, 1f);
        [SerializeField] float highlightPulseHz = 1.2f;

        public ProductData Product { get; private set; }
        public bool IsTracked { get; private set; }

        bool _isSelected;
        ProductInfoPanel _panel;
        ObserverBehaviour _observer;
        BoxCollider _bounds;
        MaterialPropertyBlock _propertyBlock;
        float _panelHalfHeight;

        void Awake()
        {
            _observer = GetComponent<ObserverBehaviour>();
            _bounds = GetComponent<BoxCollider>();
            SetHighlight(false);

            _observer.OnTargetStatusChanged += HandleStatusChanged;
        }

        void OnDestroy() => _observer.OnTargetStatusChanged -= HandleStatusChanged;

        void Start()
        {
            if (ProductCatalog.Instance == null)
            {
                Debug.LogError($"[TrackedProduct] No ProductCatalog in the scene ('{name}').");
                return;
            }

            BindProduct();
        }

        void BindProduct()
        {
            var key = _observer.TargetName;

            if (!ProductCatalog.Instance.TryGet(key, out var product))
            {
                Debug.LogError($"[TrackedProduct] No products.json entry named '{key}'.");
                return;
            }

            Product = product;
            CreatePanel();
            ApplyVisibility();
        }

        void CreatePanel()
        {
            if (_panel != null || infoPanelPrefab == null) return;

            _panel = Instantiate(infoPanelPrefab, transform);

            _panel.transform.localScale = infoPanelPrefab.transform.localScale * panelScale;

            var rect = _panel.transform as RectTransform;
            if (rect != null)
                _panelHalfHeight = rect.rect.height * _panel.transform.lossyScale.y * 0.5f;

            _panel.Bind(Product);
            _panel.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (_panel == null || !_panel.gameObject.activeSelf) return;

            var center = _bounds != null ? transform.TransformPoint(_bounds.center) : transform.position;

            _panel.transform.position =
                center + Vector3.up * (ExtentAlongWorldUp() + _panelHalfHeight + panelClearance);
        }

        float ExtentAlongWorldUp()
        {
            if (_bounds == null) return 0f;

            var e = _bounds.size * 0.5f;

            return Mathf.Abs(Vector3.Dot(transform.right * e.x, Vector3.up)) +
                   Mathf.Abs(Vector3.Dot(transform.up * e.y, Vector3.up)) +
                   Mathf.Abs(Vector3.Dot(transform.forward * e.z, Vector3.up));
        }

        void HandleStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
        {
            var tracked = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;
            if (tracked == IsTracked) return;

            IsTracked = tracked;
            ApplyVisibility();
        }

        public void SetSelected(bool selected)
        {
            if (_isSelected == selected) return;

            _isSelected = selected;
            ApplyVisibility();
        }

        void ApplyVisibility()
        {
            var show = IsTracked && Product != null && _panel != null;

            if (_panel != null && _panel.gameObject.activeSelf != show)
                _panel.gameObject.SetActive(show);

            if (show)
            {

                var canvas = _panel.GetComponentInChildren<Canvas>(true);
                if (canvas != null) canvas.enabled = true;
            }

            SetHighlight(IsTracked && Product != null);
        }

        void SetHighlight(bool on)
        {
            if (highlightVisual != null && highlightVisual.activeSelf != on)
                highlightVisual.SetActive(on);

            ApplyHighlightColor(on && _isSelected ? highlightSelectedColor : highlightColorA);
        }

        void Update()
        {
            if (highlightRenderers == null || highlightRenderers.Length == 0) return;
            if (!IsTracked || Product == null) return;

            if (_isSelected)
            {
                ApplyHighlightColor(highlightSelectedColor);
                return;
            }

            var t = Mathf.PingPong(Time.time * highlightPulseHz, 1f);
            ApplyHighlightColor(Color.Lerp(highlightColorA, highlightColorB, t));
        }

        void ApplyHighlightColor(Color color)
        {
            if (highlightRenderers == null) return;

            _propertyBlock ??= new MaterialPropertyBlock();

            foreach (var renderer in highlightRenderers)
            {
                if (renderer == null) continue;

                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_Color", color);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
