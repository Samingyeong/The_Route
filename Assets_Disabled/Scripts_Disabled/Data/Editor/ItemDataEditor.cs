using UnityEditor;
using DevionGames.InventorySystem;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StoreGame.Data.Editor
{
    [CustomEditor(typeof(ItemData))]
    [CanEditMultipleObjects]
    public class ItemDataEditor : UnityEditor.Editor
    {
        private string _manualSearchName = string.Empty;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ItemData itemData = (ItemData)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Devion Item 자동 연결", EditorStyles.boldLabel);

            _manualSearchName = EditorGUILayout.TextField("검색할 이름 (선택)", _manualSearchName);

            if (GUILayout.Button("이름으로 Devion Item 찾아서 연결"))
            {
                FindAndAssignDevionItem(itemData);
            }

            if (itemData.DevionItemTemplate == null && !string.IsNullOrEmpty(itemData.DisplayName))
            {
                EditorGUILayout.HelpBox($"Devion Item Template이 비어 있습니다. 위 버튼을 눌러 '{itemData.DisplayName}' 이름으로 자동 검색하거나, 수동으로 연결하세요.", MessageType.Warning);
            }
        }

        private void FindAndAssignDevionItem(ItemData itemData)
        {
            string searchName = string.IsNullOrWhiteSpace(_manualSearchName)
                ? itemData.DisplayName
                : _manualSearchName.Trim();
            if (string.IsNullOrEmpty(searchName))
            {
                EditorUtility.DisplayDialog("오류", "ItemData의 Display Name이 비어 있습니다. 먼저 Display Name을 설정해주세요.", "확인");
                return;
            }

            Item foundItem = null;
            if (!TryFindItemInSceneInventoryManager(searchName, out foundItem))
            {
                TryFindItemInProject(searchName, out foundItem);
            }

            if (foundItem != null)
            {
                // Reflection을 사용하여 private 필드에 접근
                var field = typeof(ItemData).GetField("devionItemTemplate", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(itemData, foundItem);
                    EditorUtility.SetDirty(itemData);
                    UnityEngine.Debug.Log($"✅ '{searchName}'에 해당하는 Devion Item '{foundItem.Name}'을 연결했습니다!");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("찾을 수 없음", 
                    $"'{searchName}' 이름의 Devion Item을 찾을 수 없습니다.\n\n" +
                    $"사용 가능한 아이템:\n" +
                    $"{string.Join(", ", System.Linq.Enumerable.Take(InventoryManager.Database.allItems.Where(x => x != null).Select(x => x.Name), 10))}...", 
                    "확인");
            }
        }

        private bool TryFindItemInSceneInventoryManager(string searchName, out Item foundItem)
        {
            foundItem = null;

            var manager = Resources.FindObjectsOfTypeAll<InventoryManager>().FirstOrDefault();
            if (manager == null)
            {
                return false;
            }

            var databaseField = typeof(InventoryManager).GetField("m_Database", BindingFlags.NonPublic | BindingFlags.Instance);
            var database = databaseField?.GetValue(manager) as ItemDatabase;
            if (database == null || database.allItems == null)
            {
                return false;
            }

            foreach (var item in database.allItems)
            {
                if (item != null && item.Name.Equals(searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    foundItem = item;
                    return true;
                }
            }

            return false;
        }

        private bool TryFindItemInProject(string searchName, out Item foundItem)
        {
            foundItem = null;

            var guids = AssetDatabase.FindAssets("t:DevionGames.InventorySystem.Item");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<Item>(path);

                if (item != null && item.Name.Equals(searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    foundItem = item;
                    return true;
                }
            }

            return false;
        }
    }
}


