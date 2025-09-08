using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Hook this to the button’s OnClick
    public void GoToIntentScene()
    {
        SceneManager.LoadScene("Intent", LoadSceneMode.Single);
    }
}
