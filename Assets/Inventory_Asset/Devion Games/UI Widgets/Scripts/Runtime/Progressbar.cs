using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace DevionGames.UIWidgets
{
	public class Progressbar : UIWidget
	{
		[Header ("Reference")]
		[SerializeField]
		protected Image progressbar;
		[SerializeField]
		protected Text m_ProgressbarTitle;
		[SerializeField]
		protected Text progressLabel;
		[SerializeField]
		protected string format = "F0";

		protected override void OnStart ()
		{
			// progressbar Image가 설정되지 않았으면 자동으로 찾기
			if (progressbar == null)
			{
				// 모든 자식에서 Image 찾기 (Background 제외)
				Image[] images = GetComponentsInChildren<Image>(true);
				foreach (Image img in images)
				{
					if (img != null && img.transform != transform && img.gameObject.name != "Background")
					{
						progressbar = img;
						Debug.Log("[Progressbar] ✓ Progressbar Image를 자동으로 찾았습니다: " + img.gameObject.name, this);
						break;
					}
				}
			}
			
			if (progressbar != null) {
				progressbar.type = Image.Type.Filled;
			} else {
				Debug.LogError("[Progressbar] ✗ Progressbar Image를 찾을 수 없습니다! " +
					"Unity Inspector에서 HealthBar 선택 → HealthBar (Progressbar) 컴포넌트 → Progressbar 필드에 " +
					"Hierarchy의 HealthBar → Progressbar 오브젝트를 드래그하세요!", this);
			}
		}

		public virtual void SetProgress (float progress)
		{
			if (progressbar == null) {
				Debug.LogError("[Progressbar] Progressbar Image가 설정되지 않았습니다!", this);
				return;
			}
			progressbar.fillAmount = progress;
			if (progressLabel != null) {
				progressLabel.text = (progress * 100f).ToString (format) + "%";
			}
		}

		public override void Show()
		{
			this.Show("");
		}

		public virtual void Show(string title)
		{
			if (this.m_ProgressbarTitle != null) {
				this.m_ProgressbarTitle.text = title;
			}
			if (progressbar != null) {
				progressbar.fillAmount = 0f;
			}
			if (progressLabel != null)
			{
				progressLabel.text = "0%";
			}
			base.Show();
		}

		
	}
}