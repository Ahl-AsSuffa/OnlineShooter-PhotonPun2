using UnityEngine;

public class SortingSlots : MonoBehaviour
{
    public string SortName;
    public bool IsOwnObj = false;
    public Transform SortingSlotsContent;
    public SortingSlots[] AllSlots;
    public void Start()
    {
        if(IsOwnObj)
            AllSlots = SortingSlotsContent.GetComponentsInChildren<SortingSlots>();
    }
    public void SortItems(string sortName)
    {
        for (int i = 0; i < AllSlots.Length; i++)
        {
            if(AllSlots[i].SortName != sortName)
            {
                AllSlots[i].gameObject.SetActive(false);
            }
            else
            {
                AllSlots[i].gameObject.SetActive(true);
            }
        }
    }
}
