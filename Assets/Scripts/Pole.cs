using UnityEngine;

public class Pole : MonoBehaviour
{
    [SerializeField] bool connected;
    [SerializeField] GameObject Light;
    enum PoleType{
        mainPole,
        connectorPole,
        homePole
    }
    [SerializeField] PoleType poleType;
    // Start is called before the first frame update
    void Start()
    {
        if(poleType == PoleType.mainPole)
            connected = true;
        else
            connected = false;
    }
    public bool isConnected
    {
        get { return connected; }
        set { 
            connected = value;
            if (connected && poleType == PoleType.homePole)
            {
                Light.SetActive(true);
            }
        }
    }
}
