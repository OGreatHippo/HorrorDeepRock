using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private AudioSource source;

    private GameObject menu;
    private GameObject options;

    private bool inOptions = true;
    public bool inMainMenu;

    private void Start()
    {
        source = gameObject.GetComponent<AudioSource>();

        if(inMainMenu)
        {
            Cursor.lockState = CursorLockMode.Confined;

            menu = gameObject.transform.Find("MenuPanel").gameObject;
        }

        options = gameObject.transform.Find("OptionsPanel").gameObject;
    }

    public void PlayButton()
    {
        source.Play();

        StartCoroutine(MenuDelay(true, false));
    }

    public void OptionsButton()
    {
        source.Play();

        menu.SetActive(!inOptions);
        options.SetActive(inOptions);
    }

    public void BackToMenuButton()
    {
        source.Play();

        StartCoroutine(MenuDelay(false, true));
    }

    public void BackButton()
    {
        source.Play();

        menu.SetActive(inOptions);
        options.SetActive(!inOptions);
    }

    public void QuitButton()
    {
        source.Play();

        StartCoroutine(MenuDelay(false, false));
    }

    private IEnumerator MenuDelay(bool play, bool menu)
    {
        yield return new WaitForSeconds(1f);

        if (play)
        {
            SceneManager.LoadScene(1);
        }

        else if (menu)
        {
            SceneManager.LoadScene(0);
        }

        else
        {
            Application.Quit();
        }
    }
}
