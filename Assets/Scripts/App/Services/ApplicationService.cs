using UnityEngine;

public interface IApplicationService
{
    void Quit();
}

public class ApplicationService : IApplicationService
{
    public void Quit()
    {
        Application.Quit();
        Debug.Log("The Application is quitting.");
    }
}
