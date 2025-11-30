// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;

// [CustomEditor(typeof(VendingMachine))]
// public class VendingMachineEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();
        
//         VendingMachine vm = (VendingMachine)target;
        
//         EditorGUILayout.Space();
//         EditorGUILayout.LabelField("ScriptableObject Management", EditorStyles.boldLabel);
        
//         if (GUILayout.Button("Загрузить товары из ScriptableObjects"))
//         {
//             vm.RefreshVendingMachine();
//             Debug.Log("Товары загружены из ScriptableObjects");
//         }
        
//         if (GUILayout.Button("Очистить все товары"))
//         {
//             ClearAllItems(vm);
//         }

//         EditorGUILayout.Space();
//         EditorGUILayout.LabelField("Быстрая настройка слотов", EditorStyles.boldLabel);
        
//         // Показываем доступные ScriptableObjects для быстрой настройки
//         InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("ScriptableObject/Data/Items");
        
//         foreach (InventoryItem item in allItems)
//         {
//             EditorGUILayout.BeginHorizontal();
//             EditorGUILayout.LabelField(item.itemName, GUILayout.Width(100));
            
//             for (int i = 0; i < 6; i++)
//             {
//                 if (GUILayout.Button($"Слот {i+1}", GUILayout.Width(60)))
//                 {
//                     vm.SetItemInSlot(i, item);
//                 }
//             }
//             EditorGUILayout.EndHorizontal();
//         }
//     }
    
//     private void ClearAllItems(VendingMachine vm)
//     {
//         for (int i = 0; i < vm.itemsForSale.Length; i++)
//         {
//             vm.itemsForSale[i] = null;
//         }
//         vm.RefreshVendingMachine();
//         Debug.Log("Все товары очищены");
//     }
// }
// #endif