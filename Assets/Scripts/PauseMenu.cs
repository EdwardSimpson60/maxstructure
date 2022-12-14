using System;
using StarterAssets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;
	private float m_TimeScaleRef = 1f;
    private float m_VolumeRef = 1f;
    private bool m_Paused;

    [SerializeField] GameObject pauseCanvas;

    private void Start()
    {
        instance = this;
    }

    private void MenuOn ()
    {
        GameController.instance.DisablePlayer();

        pauseCanvas.SetActive(true);

        //m_TimeScaleRef = Time.timeScale;
        //Time.timeScale = 0f;

        m_VolumeRef = AudioListener.volume;
        AudioListener.volume = 0f;

        m_Paused = true;
        
        ToolManager.instance.DisableActiveTool();
    }


    public void MenuOff ()
    {
        GameController.instance.EnablePlayer();

        pauseCanvas.SetActive(false);

        //Time.timeScale = m_TimeScaleRef;
        AudioListener.volume = m_VolumeRef;
        m_Paused = false;
    }


    public void OnMenuStatusChange ()
    {
        if (!m_Paused)
        {
            MenuOn();
        }
        else if (m_Paused)
        {
            MenuOff();
        }
    }


#if !MOBILE_INPUT
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) && !StereonetDashboard.instance.isActiveAndEnabled)
		{
            OnMenuStatusChange();
        }
    }
#endif

}
