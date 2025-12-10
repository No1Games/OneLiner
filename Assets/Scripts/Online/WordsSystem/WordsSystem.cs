using UnityEngine;

public class WordsSystem : MonoBehaviour
{
    [SerializeField] private WordsUI _wordsUI;
    [SerializeField] private WordsNetworkStorage _networkHolder;
    [SerializeField] private WordsInitializer _wordsInitializer;

    private WordsLocalStorage _localHolder;

    private void Awake()
    {
        _localHolder = new WordsLocalStorage();
    }

    public void InitilizeSystem()
    {

    }
}
