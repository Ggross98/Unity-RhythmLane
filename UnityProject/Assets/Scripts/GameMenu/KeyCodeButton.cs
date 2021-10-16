using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Game.MainMenu{
public class KeyCodeButton : MonoBehaviour
{
    [SerializeField]private Button button;

    [SerializeField] private Text keyText;

    //public int value;

    public KeyCode keyCode;

    private bool inputing = false;

    void Awake()
    {
        //button = GetComponentInChildren<Button>();

        button.onClick.AddListener(
            delegate
            {
                inputing = true;
                keyText.text = "--";
            }
        );
    }

    public void SetKeyCode(int i)
    {
        SetKeyCode((KeyCode)i);
    }

    public void SetKeyCode(KeyCode kc)
    {
        keyCode = kc;
        keyText.text = keyCode.ToString();
    }

    void Update()
    {
        if(isActiveAndEnabled )
        {
            if (inputing)
            {
                if (Input.anyKeyDown)
                {
                    for (KeyCode kc = KeyCode.Space; kc < KeyCode.Insert; kc++)
                    {
                        if (Input.GetKeyDown(kc))
                        {
                            SetKeyCode(kc);
                            break;
                        }
                    }

                    inputing = false;
                }
            }

            //button.GetComponentInChildren<Text>().text = keyCode.ToString();
        }
    }
}

    
}
