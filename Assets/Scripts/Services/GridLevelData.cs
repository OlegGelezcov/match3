namespace Ozh.Services {
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GridLevelData  {
        public int level;
        public float cellWidth;
        public float cellHeight;
        public float cellPadding;
        public int numRows;
        public int numColumns;
    }

    public interface IGridLevelRepository : IRepository {
        GridLevelData GetGridLevelData(int level);
    }

    public class GridLevelRepository : IGridLevelRepository {

        public bool IsLoaded { get; private set; } = false;

        private Dictionary<int, GridLevelData> GridLevels { get; } = new Dictionary<int, GridLevelData>();


        public GridLevelData GetGridLevelData(int level) {
            return GridLevels.ContainsKey(level) ? GridLevels[level] : null;
        }

        public void Load(object obj = null) {
            if(!IsLoaded) {
                string file = obj as string;
                var listItems = JsonConvert.DeserializeObject<List<GridLevelData>>(Resources.Load<TextAsset>(file).text);
                GridLevels.Clear();
                listItems.ForEach(li => GridLevels.Add(li.level, li));
                IsLoaded = true;
            }
        }
    }
}