using TMPro;
using UnityEngine;

namespace Study3D.MiniGolf
{
    [RequireComponent(typeof(LineRenderer))]
    public class GolfAimIndicatorV2 : MonoBehaviour
    {
        [Header("Settings")]
        public float minLength;
        public float maxLength;

        [Header("세기 UI - Screen Space Overlay Canvas 사용")]
        [SerializeField] private TMP_Text powerText;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform ballTarget;
        [SerializeField] private Vector2 screenOffset = new Vector2(70f, 0f);

        private LineRenderer line;
        private Vector3 aimOrigin;
        private bool isShowing;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;

            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            Hide();
        }

        public void Show(Vector3 origin, Vector3 direction, float power01)
        {
            float clampedPower = Mathf.Clamp01(power01);
            float length = Mathf.Lerp(minLength, maxLength, clampedPower);

            line.SetPosition(0, origin);
            line.SetPosition(1, origin + direction * length);
            line.enabled = true;

            aimOrigin = origin;
            isShowing = true;

            if (powerText != null)
            {
                int percent = Mathf.RoundToInt(clampedPower * 100f);
                powerText.SetText("{0}%", percent);
                UpdatePowerTextPosition();
            }
        }

        private void LateUpdate()
        {
            if (isShowing)
            {
                UpdatePowerTextPosition();
            }
        }

        private void UpdatePowerTextPosition()
        {
            if (powerText == null)
            {
                return;
            }

            if (aimCamera == null)
            {
                powerText.gameObject.SetActive(false);
                return;
            }

            Vector3 worldPosition = ballTarget != null ? ballTarget.position : aimOrigin;
            Vector3 screenPosition = aimCamera.WorldToScreenPoint(worldPosition);

            // 공이 화면 밖이나 카메라 뒤에 있으면 숫자를 숨깁니다.
            bool isOnScreen = screenPosition.z > 0f
                && screenPosition.x >= 0f && screenPosition.x <= Screen.width
                && screenPosition.y >= 0f && screenPosition.y <= Screen.height;

            powerText.gameObject.SetActive(isOnScreen);

            if (isOnScreen == false)
            {
                return;
            }

            RectTransform parentRect = powerText.rectTransform.parent as RectTransform;

            if (parentRect == null)
            {
                return;
            }

            Vector2 uiScreenPosition = new Vector2(screenPosition.x, screenPosition.y) + screenOffset;

            // Overlay Canvas이므로 UI 카메라 인수에는 null을 전달합니다.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, uiScreenPosition, null, out Vector2 localPoint))
            {
                Vector3 localPosition = powerText.rectTransform.localPosition;
                localPosition.x = localPoint.x;
                localPosition.y = localPoint.y;
                powerText.rectTransform.localPosition = localPosition;
            }
        }

        public void Hide()
        {
            isShowing = false;

            if (line != null)
            {
                line.enabled = false;
            }

            if (powerText != null)
            {
                powerText.gameObject.SetActive(false);
            }
        }
    }
}
