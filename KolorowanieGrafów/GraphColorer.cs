using System;
using System.Collections.Generic;
using ASD.Graphs;

namespace ASD2
{
    public class GraphColorer : MarshalByRefObject
    {
        /// <summary>
        /// Metoda znajduje kolorowanie zadanego grafu g używające najmniejsze możliwej liczby kolorów.
        /// </summary>
        /// <param name="g">Graf (nieskierowany)</param>
        /// <returns>Liczba użytych kolorów i kolorowanie (coloring[i] to kolor wierzchołka i). Kolory mogą być dowolnymi liczbami całkowitymi.</returns>
        private void WriteTable<T>(T[] table)
        {
            Console.WriteLine();
            foreach (var item in table)
            {
                Console.Write(item + " ");
            }
            Console.WriteLine();
        }
        private bool CanColor(Graph g, int v,int color,int[] colors)
        {
            foreach (int neighbor in g.OutNeighbors(v))
            {
                if (colors[neighbor] == color)
                    return false;
            }
            return true;
        }

        private HashSet<int> AvailableColors(Graph g, int v, int[] colors,int maxColors)
        {
            HashSet<int> set = new HashSet<int>();
            for (int i = 0; i < maxColors; i++)
            {
                set.Add(i);
            }

            foreach (int neighbor in g.OutNeighbors(v))
            {
                if (colors[neighbor] != -1)
                    set.Remove(colors[neighbor]);
            }
            return set;
        }

        public bool AllCompleted(bool[] completed)
        {
            for (int i = 0; i < completed.Length; i++)
            {
                if (completed[i] == false)
                    return false;
            }
            return true;
        }

        public (int,HashSet<int>) FindBestVertex(bool[] completed,int[] colors,Graph g,int maxColors,HashSet<int> skippedVertexes,int[] uncolorNeighbors,int[] vertexDegrees,int[] avlColors)
        {
            HashSet<int> addedSkipped = new HashSet<int>();
            int index = 0;
            int max=int.MaxValue;
            int maxUncoloredNeighbors=-1;
            for (int i = 0; i < colors.Length; i++)
            {
                if (completed[i])
                    continue;
                int notcoloredNeighbors = uncolorNeighbors[i];
                int outNeighbors = vertexDegrees[i];
                if (avlColors[i] > notcoloredNeighbors && !completed[i])     
                {
                    skippedVertexes.Add(i);
                    addedSkipped.Add(i);
                    completed[i] = true;
                    continue;
                }
                if (avlColors[i] < max)
                {
                    max = maxColors - outNeighbors + notcoloredNeighbors;
                    index = i;
                    maxUncoloredNeighbors = notcoloredNeighbors;
                }
                else if (avlColors[i] == max)
                {
                    if (notcoloredNeighbors > maxUncoloredNeighbors)
                    {
                        maxUncoloredNeighbors = notcoloredNeighbors;
                        index = i;
                    }
                }
            }
            return (index,addedSkipped);
        }
        public (int, int[]) FindBestColoring(Graph g)
        {
            int vertexCount = g.VertexCount;
            int[] vertexDegrees = new int[vertexCount];
            for (int i = 0; i < vertexCount;i++)
            {
                vertexDegrees[i] = g.Degree(i);
            }
            int[] bestColors=new int[vertexCount];
            int[] cols = new int[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                cols[i] = -1;
            }
            int maxColorsYet=int.MaxValue;
            int[] availableColors = new int[vertexCount];
            int[,] howManyNeighborsUseGivenColor = new int[vertexCount, vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                availableColors[i] = 1;
            }
            void FindColoring(Graph graph, int[] colors, int maxColors,HashSet<int> skippedVertices,bool[] completed,int[] uncoloredNeighbors,int[] avlColors,int[,] neighborUsingColorsCount)
            {
                if (maxColorsYet <= maxColors)
                    return;
                if (AllCompleted(completed))
                {
                    if (skippedVertices.Count != 0)
                    {
                        foreach (int vertex in skippedVertices)
                        {
                            for (int color = 0; color < maxColors; color++)
                            {
                                if (CanColor(graph, vertex, color, colors))
                                {
                                    colors[vertex] = color;
                                    break;
                                }
                            }
                        }
                    }
                    Array.Copy(colors,bestColors,colors.Length);
                    maxColorsYet = maxColors;
                    return;
                }
                (int v,HashSet<int> addedSkipped) = FindBestVertex(completed,colors,graph,maxColors,skippedVertices,uncoloredNeighbors,vertexDegrees,avlColors);
                foreach (int neighbor in g.OutNeighbors(v))
                {
                    uncoloredNeighbors[neighbor]--;
                }
                HashSet<int> canUseColors = AvailableColors(g, v, colors, maxColors);
                completed[v] = true; 
                foreach (int color in canUseColors) 
                { 
                    colors[v] = color;
                    foreach (int neighbor in g.OutNeighbors(v))
                    {
                        neighborUsingColorsCount[neighbor, color]++;
                        if (neighborUsingColorsCount[neighbor, color] == 1)
                        {
                            avlColors[neighbor]--;
                        }
                    }
                    FindColoring(graph,colors,maxColors,skippedVertices,completed,uncoloredNeighbors,avlColors,neighborUsingColorsCount);
                    foreach (int neighbor in g.OutNeighbors(v))
                    {
                        neighborUsingColorsCount[neighbor, color]--;
                        if (neighborUsingColorsCount[neighbor, color] == 0)
                        {
                            avlColors[neighbor]++;
                        }
                    }
                    colors[v] = -1;
                }
                colors[v] = maxColors;
                for (int i = 0; i < vertexCount; i++)
                {
                    avlColors[i]++;
                }
                foreach (int neighbor in g.OutNeighbors(v))
                {
                    neighborUsingColorsCount[neighbor, maxColors]++;
                    if (neighborUsingColorsCount[neighbor, maxColors] == 1)
                    {
                        avlColors[neighbor]--;
                    }
                }
                FindColoring(graph,colors,maxColors+1,skippedVertices,completed,uncoloredNeighbors,avlColors,neighborUsingColorsCount);
                foreach (int neighbor in g.OutNeighbors(v))
                {
                    neighborUsingColorsCount[neighbor, maxColors]--;
                    if (neighborUsingColorsCount[neighbor, maxColors] == 0)
                    {
                        avlColors[neighbor]++;
                    }
                }
                for (int i = 0; i < vertexCount; i++)
                {
                    avlColors[i]--;
                }
                colors[v] = -1;
                completed[v] = false;
                foreach (int neighbor in g.OutNeighbors(v))
                {
                    uncoloredNeighbors[neighbor]++;
                }
                foreach (var s in addedSkipped)
                {
                    skippedVertices.Remove(s);
                    completed[s] = false;
                }
            }
            int[] uncNeighbors = new int[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                uncNeighbors[i] = g.Degree(i);
            }
            
            FindColoring(g, cols,1,new HashSet<int>(),new bool[vertexCount],uncNeighbors,availableColors,new int[vertexCount,vertexCount] );
            return (maxColorsYet, bestColors);
        }
    }

}

