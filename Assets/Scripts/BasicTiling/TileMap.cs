using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileMap : MonoBehaviour
{
    public GameObject[] listPrefabs;
    Tile[] tilelist;
    public int gridSize = 2;
    //Reglas de las tiles por ahora solo aparecen las que son adyacentes. No hay mas tipos reglas
    List<List<int>> rules = new List<List<int>>
    {
        new List<int> {0,1},
        new List<int> {0,1,2},
        new List<int> {1,2,3},
        new List<int> {2,3,4},
        new List<int> {3,4},
    };

    // Start is called before the first frame update
    void Start()
    {
        RunPerformanceTests();
    }

    public void RunPerformanceTests()
    {
        int[] gridSizes = { 5, 20, 50 };
        int iterations = 10;

        foreach (var size in gridSizes)
        {
            long totalTime = 0;
            Dictionary<int, int> tileCounts = new Dictionary<int, int>();

            for (int i = 0; i < iterations; i++)
            {
                var result = RunTileMapGeneration(size);
                totalTime += result.Item1;

                foreach (var tile in result.Item2)
                {
                    if (!tileCounts.ContainsKey(tile))
                    {
                        tileCounts[tile] = 0;
                    }
                    tileCounts[tile]++;
                }
            }

            UnityEngine.Debug.Log($"Grid Size: {size}x{size}");
            UnityEngine.Debug.Log($"Average Time: {totalTime / iterations} ms");
            foreach (var tileCount in tileCounts)
            {
                UnityEngine.Debug.Log($"Tile {tileCount.Key}: {tileCount.Value / iterations} average count");
            }
        }
    }

    private Tuple<long, List<int>> RunTileMapGeneration(int gridSize)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        //Inicialización del grid
        tilelist = new Tile[gridSize * gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                tilelist[gridSize * y + x] = new Tile();
                tilelist[gridSize * y + x].options = new int[listPrefabs.Length];
                for (int k = 0; k < listPrefabs.Length; k++)
                {
                    tilelist[gridSize * y + x].options[k] = k;
                }
                tilelist[gridSize * y + x].X = x;
                tilelist[gridSize * y + x].Y = y;
            }
        }
        int iterations = 0;
        bool finished = false;
        List<int> templist = new List<int>();
        while (!finished)
        {
            iterations++;
            //Creamos una copia del grid y al ordenamos para sacar los Tiles con menor entropia
            Tile[] tileCopy = new Tile[tilelist.Length];

            Array.Copy(tilelist, tileCopy, tilelist.Length);
            Array.Sort(tileCopy);
            int indexFinal = -1;
            bool ch = true;
            while (ch)
            {
                indexFinal++;
                if (!tilelist[indexFinal].collapse)
                {
                    ch = false;
                }
            }
            int newvalue = tilelist[indexFinal].options[Random.Range(0, tilelist[indexFinal].options.Length)];
            tilelist[indexFinal].options = new int[1];
            tilelist[indexFinal].options[0] = newvalue;
            tilelist[indexFinal].collapse = true;
            //Expandimos los cambios a los adyacentes del grid
            bool changed = true;

            while (changed)
            {
                changed = false;
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        if (tilelist[gridSize * y + x].collapse)
                        {

                            int tilenum = tilelist[gridSize * y + x].options[0];

                            Tile tile;
                            templist.Clear();
                            //miramos arriba
                            if (y > 0)
                            {
                                if (!tilelist[gridSize * (y - 1) + x].collapse)
                                {
                                    tile = tilelist[gridSize * (y - 1) + x];

                                    for (int k = 0; k < tile.options.Length; k++)
                                    {
                                        if (rules[tilenum].Contains(tile.options[k]))
                                        {
                                            templist.Add(tile.options[k]);
                                        }
                                    }
                                    if (templist.Count > 0)
                                    {
                                        if (templist.Count != tilelist[gridSize * (y - 1) + x].options.Length)
                                        {
                                            changed = true;
                                            tilelist[gridSize * (y - 1) + x].options = new int[templist.Count];
                                            for (int i = 0; i < templist.Count; i++)
                                            {
                                                tilelist[gridSize * (y - 1) + x].options[i] = templist[i];
                                            }
                                            if (tilelist[gridSize * (y - 1) + x].options.Length == 1)
                                            {
                                                tilelist[gridSize * (y - 1) + x].collapse = true;
                                            }
                                        }
                                    }
                                }
                            }
                            //miramos abajo

                            templist.Clear();
                            if (y < (gridSize - 1))
                            {

                                if (!tilelist[gridSize * (y + 1) + x].collapse)
                                {
                                    tile = tilelist[gridSize * (y + 1) + x];
                                    for (int k = 0; k < tile.options.Length; k++)
                                    {
                                        if (rules[tilenum].Contains(tile.options[k]))
                                        {
                                            templist.Add(tile.options[k]);
                                        }
                                    }
                                    if (templist.Count > 0)
                                    {
                                        if (templist.Count != tilelist[gridSize * (y + 1) + x].options.Length)
                                        {
                                            changed = true;
                                            tilelist[gridSize * (y + 1) + x].options = new int[templist.Count];
                                            for (int i = 0; i < templist.Count; i++)
                                            {
                                                tilelist[gridSize * (y + 1) + x].options[i] = templist[i];
                                            }
                                            if (tilelist[gridSize * (y + 1) + x].options.Length == 1)
                                            {
                                                tilelist[gridSize * (y + 1) + x].collapse = true;
                                            }
                                        }
                                    }
                                }
                            }
                            //miramos izquierda
                            templist.Clear();
                            if (x > 0)
                            {
                                if (!tilelist[gridSize * y + x - 1].collapse)
                                {
                                    tile = tilelist[gridSize * y + x - 1];
                                    for (int k = 0; k < tile.options.Length; k++)
                                    {
                                        if (rules[tilenum].Contains(tile.options[k]))
                                        {
                                            templist.Add(tile.options[k]);
                                        }
                                    }
                                    if (templist.Count > 0)
                                    {
                                        if (templist.Count != tilelist[gridSize * y + x - 1].options.Length)
                                        {
                                            changed = true;
                                            tilelist[gridSize * y + x - 1].options = new int[templist.Count];
                                            for (int i = 0; i < templist.Count; i++)
                                            {
                                                tilelist[gridSize * y + x - 1].options[i] = templist[i];
                                            }
                                            if (tilelist[gridSize * y + x - 1].options.Length == 1)
                                            {
                                                tilelist[gridSize * y + x - 1].collapse = true;
                                            }
                                        }
                                    }
                                }
                            }
                            //miramos derecha
                            templist.Clear();
                            if (x < (gridSize - 1))
                            {
                                if (!tilelist[gridSize * y + x + 1].collapse)
                                {
                                    tile = tilelist[gridSize * y + x + 1];
                                    for (int k = 0; k < tile.options.Length; k++)
                                    {
                                        if (rules[tilenum].Contains(tile.options[k]))
                                        {
                                            templist.Add(tile.options[k]);
                                        }
                                    }
                                    if (templist.Count > 0)
                                    {
                                        if (templist.Count != tilelist[gridSize * y + x + 1].options.Length)
                                        {
                                            changed = true;
                                            tilelist[gridSize * y + x + 1].options = new int[templist.Count];
                                            for (int i = 0; i < templist.Count; i++)
                                            {
                                                tilelist[gridSize * y + x + 1].options[i] = templist[i];
                                            }
                                            if (tilelist[gridSize * y + x + 1].options.Length == 1)
                                            {
                                                tilelist[gridSize * y + x + 1].collapse = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            bool cont = false;
            //Comprobamos si hemos acabado
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (!tilelist[gridSize * y + x].collapse)
                    {
                        cont = true;
                    }

                }
            }
            if (cont)
            {
                finished = false;
            }
            else
            {
                finished = true;
                //print("End");
            } 
        }

        stopwatch.Stop();

        List<int> finalTiles = new List<int>();
        foreach (Tile i in tilelist)
        {
            finalTiles.Add(i.options[0]);
        }

        return new Tuple<long, List<int>>(stopwatch.ElapsedMilliseconds, finalTiles);
    }
}
