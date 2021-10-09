using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

namespace Ggross.Utils {

    public class AudioClipGetter
    {
        
        public static AudioClip GetAudioClip(string path, AudioType type = AudioType.WAV)
        {
            var _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(path, type);
            
            if (_unityWebRequest.isHttpError || _unityWebRequest.isNetworkError)
            {
                Debug.Log(_unityWebRequest.error.ToString());
                return null;
            }
            else
            {
                return DownloadHandlerAudioClip.GetContent(_unityWebRequest);
            }

        }

    }


}

