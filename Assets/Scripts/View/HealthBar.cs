using UnityEngine;
namespace View
{
    public class HealthBar : MonoBehaviour
    {
		/// <summary>
		/// The object whose X-scale we change to decrease the health bar. Should have a default uniform scale
		/// </summary>
		[SerializeField] private Transform healthBar;
		
		/// <summary>
		/// The object whose X-scale we change to increase the health bar background. Should have a default uniform scale
		/// </summary>
		[SerializeField] private Transform backgroundBar;

		/// <summary>
		/// Whether to show this health bar even when it is full
		/// </summary>
		[SerializeField] private bool showWhenFull;

		/// <summary>
		/// Camera to face the visualization at
		/// </summary>
		private Transform m_CameraToFace;

		/// <summary>
		/// Updates the visualization of the health
		/// </summary>
		/// <param name="normalizedHealth">Normalized health value</param>
		public void UpdateHealth(float normalizedHealth)
		{
			Vector3 scale = Vector3.one;

			if (healthBar != null)
			{
				scale.x = normalizedHealth;
				healthBar.transform.localScale = scale;
			}

			if (backgroundBar != null)
			{
				scale.x = 1 - normalizedHealth;
				backgroundBar.transform.localScale = scale;
			}

			SetVisible(showWhenFull || normalizedHealth < 1.0f);
		}

		private void SetVisible(bool visible)
		{
			gameObject.SetActive(visible);
		}

		/// <summary>
		/// Caches the main camera
		/// </summary>
		private void Start()
		{
			m_CameraToFace = Camera.main.transform;
		}

		/// <summary>
		/// Turns us to face the camera
		/// </summary>
		private void Update()
		{
			Vector3 direction = m_CameraToFace.transform.forward;
			transform.forward = -direction;
		}
    }
}
