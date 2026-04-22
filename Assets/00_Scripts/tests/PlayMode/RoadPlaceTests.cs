//using System.Collections;
//using NUnit.Framework;
//using UnityEngine;
//using UnityEngine.TestTools;

//public class RoadPlaceTests
//{
//    private Game game;
//    private Road road;
//    private BuildingSystem bs;

//    [SetUp]
//    public void Init()
//    {
//        // 1. ALAP OBJEKTUMOK LÉTREHOZÁSA
//        GameObject gameObj = new GameObject("TestGame");
//        game = gameObj.AddComponent<Game>();
//        bs = gameObj.AddComponent<BuildingSystem>();

//        // 2. GRID (RÁCS) INICIALIZÁLÁSA
//        GameObject gridObj = new GameObject("Grid");
//        // 1. LÉPÉS: Kikapcsoljuk a GameObjectet, mielõtt hozzáadjuk a szkriptet
//        gridObj.SetActive(false);

//        BuildingGrid bg = gridObj.AddComponent<BuildingGrid>();
//        bg.Width = 10;
//        bg.Height = 10;
//        bs.Grid = bg;

//        // 2. LÉPÉS: Reflexióval beállítjuk a rácsot (ahogy eddig)
//        var gridField = typeof(BuildingGrid).GetField("grid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        gridField?.SetValue(bg, new BuildingGridCell[10, 10]);

//        for (int x = 0; x < 10; x++)
//        {
//            for (int y = 0; y < 10; y++)
//            {
//                bg.Grid[x, y] = new BuildingGridCell();
//            }
//        }

//        // 3. LÉPÉS: A treeVisuals tömböt is inicializáljuk üresre, 
//        // így ha a kód mégis hívná a Refresh-t, nem lesz NullRef a tömb elérésekor
//        typeof(BuildingGrid).GetField("treeVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            ?.SetValue(bg, new TreeVisual[10, 10]);

//        // 3. ÚT PREFAB KONFIGURÁLÁSA (Ebbõl példányosít a kódod a PlaceRoad-ban)
//        GameObject roadPrefabObj = new GameObject("RoadPrefab");
//        bs.RoadPrefab = roadPrefabObj.AddComponent<Road>();

//        // 4. PREVIEW PREFAB KONFIGURÁLÁSA
//        GameObject previewPrefabObj = new GameObject("RoadPreviewPrefab");
//        var rp = previewPrefabObj.AddComponent<RoadPreview>();

//        // ScriptableObject adatok létrehozása
//        rp.Data = ScriptableObject.CreateInstance<RoadData>();

//        // Konkrét modell (RoadStraightModel) hozzáadása az absztrakt RoadModel helyett
//        RoadStraightModel straightModel = previewPrefabObj.AddComponent<RoadStraightModel>();

//        // --- WRAPPER BEÁLLÍTÁSA REFLEXIÓVAL ---
//        // Ez azért kell, mert a RoadModel.Rotation a 'wrapper' változót használja!
//        GameObject wrapperObj = new GameObject("Wrapper");
//        wrapperObj.transform.SetParent(previewPrefabObj.transform);

//        var wrapperField = typeof(RoadModel).GetField("wrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        if (wrapperField != null)
//        {
//            wrapperField.SetValue(straightModel, wrapperObj.transform);
//        }
//        // --------------------------------------

//        // Modell és Prefab véglegesítése
//        rp.RoadModel = straightModel;
//        bs.RoadPreviewPrefab = rp;

//        GameObject dummyContainer = new GameObject("TreeContainer");
//        bg.GetType().GetField("treeContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            ?.SetValue(bg, dummyContainer.transform);

//        GameObject dummyTreePrefab = new GameObject("DummyTree");
//        dummyTreePrefab.AddComponent<TreeVisual>(); // Kell rá a script, hogy ne legyen cast hiba
//        bg.GetType().GetField("treeVisualPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            ?.SetValue(bg, dummyTreePrefab.GetComponent<TreeVisual>());

//        // Fontos: a treeVisuals tömböt is inicializálni kell reflexióval!
//        bg.GetType().GetField("treeVisuals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//            ?.SetValue(bg, new TreeVisual[10, 10]);
//    }

//    [TearDown]
//    public void CleanUp()
//    {
//        Object.DestroyImmediate(game);
//    }

//    [Test]
//    public void RoadPlacedInGrid()
//    {
//        // A prefab-ból csinálunk egy élõ példányt a jelenetben
//        bs.Preview = Object.Instantiate(bs.RoadPreviewPrefab);

//        // Most futtatjuk a lehelyezést a (0,0,0) pozícióra
//        bs.PlaceRoad(new Vector3(0, 0, 0));

//        // Ellenõrizzük, hogy a rács 0,0 cellájába bekerült-e az út
//        Assert.IsNotNull(bs.Grid.Grid[0, 0].Road, "Az út nem került be a rácsba!");
//    }
//}
