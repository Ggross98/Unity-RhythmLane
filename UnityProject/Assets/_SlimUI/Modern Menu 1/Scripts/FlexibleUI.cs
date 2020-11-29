using UnityEngine;

namespace SlimUI.ModernMenu{
	[ExecuteInEditMode()]
	[System.Serializable]
	public class FlexibleUI : MonoBehaviour {

		public FlexibleUIData themeController;

		protected virtual void OnSkinUI(){

		}

		public virtual void Awake(){
			OnSkinUI();
		}

		public virtual void Update(){
			//if(Application.isEditor){
				OnSkinUI();
			//}
		}
	}
}
