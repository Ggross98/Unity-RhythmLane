using UnityEngine;

namespace SlimUI.ModernMenu{
	[CreateAssetMenu(menuName = "ThemeSettings")]
	[System.Serializable]
	public class FlexibleUIData : ScriptableObject {
		[System.Serializable]
		public class Custom1{
			[Header("Text")]	
			public Color graphic1;
			public Color32 text1;
		}

		[System.Serializable]
		public class Custom2{
			[Header("Text")]	
			public Color graphic2;
			public Color32 text2;
		}

		[System.Serializable]
		public class Custom3{
			[Header("Text")]	
			public Color graphic3;
			public Color32 text3;
		}

		[Header("CUSTOM SETTINGS")]
		public Custom1 custom1;
		public Custom2 custom2;
		public Custom3 custom3;

		[HideInInspector]
		public Color currentColor;
		[HideInInspector]
		public Color32 textColor;
	}
}