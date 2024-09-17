using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewWordList", menuName = "Word List")]
public class WordList : ScriptableObject
{
   public List<string> words;
}
