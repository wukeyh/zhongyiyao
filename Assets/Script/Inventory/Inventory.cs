using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/New Item")]
public class Inventory : ScriptableObject
{
    public List<Item> Bag = new List<Item>();
}
