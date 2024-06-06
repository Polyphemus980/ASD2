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

        private int uncoloredNeighbors(Graph g, int vertex,int[] colors)
        {
            int counter = 0;
            foreach (int neighbor in g.OutNeighbors(vertex))
            {
                if (colors[neighbor] == -1)
                    counter++;
            }

            return counter;
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
        static int test=0;

        public bool AllCompleted(bool[] completed)
        {
            for (int i = 0; i < completed.Length; i++)
            {
                if (completed[i] == false)
                    return false;
            }

            return true;
        }

        public (int,List<int>) FindBestVertex(bool[] completed,int[] colors,Graph g,int maxColors,List<int> skippedVertexes)
        {
            List<int> addedSkipped = new List<int>();
            int index = 0;
            int max=int.MaxValue;
            int maxUncoloredNeighbors=-1;
            for (int i = 0; i < colors.Length; i++)
            {
                if (completed[i])
                    continue;
                int notcoloredNeighbors = uncoloredNeighbors(g, i, colors);
                int outNeighbors = g.OutNeighbors(i).Count();
                if (maxColors - outNeighbors + notcoloredNeighbors > notcoloredNeighbors)
                {
                    skippedVertexes.Add(i);
                    addedSkipped.Add(i);
                    completed[i] = true;
                    continue;
                }
                if (maxColors - outNeighbors + notcoloredNeighbors < max)
                {
                    index = i;
                }
                else if (maxColors - outNeighbors + notcoloredNeighbors == max)
                {
                    if (notcoloredNeighbors > maxUncoloredNeighbors)
                    {
                        index = i;
                    }
                }
            }
            return (index,addedSkipped);
        }
        public (int, int[]) FindBestColoring(Graph g)
        {
            test++;
            int vertexCount = g.VertexCount;
            int[] bestColors=new int[g.VertexCount];
            int[] cols = new int[g.VertexCount];
            Array.Fill(cols,-1);
            int maxColorsYet=int.MaxValue;
            void FindColoring(Graph graph, int[] colors, int maxColors,List<int> skippedVertices,bool[] completed)
            {
                if (maxColorsYet <= maxColors)
                    return;
                bool all = AllCompleted(completed);
                if (all && skippedVertices.Count==0)
                {
                    if (maxColorsYet > maxColors)
                    { 
                        Array.Copy(colors,bestColors,colors.Length); 
                        maxColorsYet = maxColors;
                    }
                    return;
                }
                if (all)
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
                    if (maxColorsYet > maxColors)
                    {
                        Array.Copy(colors,bestColors,colors.Length);
                        maxColorsYet = maxColors;
                        return;
                    }
                }
                
                int counter = 0;
                (int v,List<int> addedSkipped) = FindBestVertex(completed,colors,graph,maxColors,skippedVertices);
                for (int i = 0; i < maxColors; i++)
                {
                    if (CanColor(graph, v, i, colors))
                    {
                        completed[v] = true;
                        colors[v] = i;
                        FindColoring(graph,colors,maxColors,skippedVertices,completed);
                        colors[v] = -1;
                        foreach (var s in addedSkipped)
                        {
                            skippedVertices.Remove(s);
                            completed[s] = false;
                        }
                        completed[v] = false;
                    }
                    else
                        counter++;
                }

                if (counter == maxColors)
                {
                    int m = maxColors;
                    while (m < maxColorsYet)
                    {
                        completed[v] = true;
                        colors[v] = m;
                        FindColoring(graph, colors, m + 1,skippedVertices,completed);
                        colors[v] = -1;
                        foreach (var s in addedSkipped)
                        {
                            skippedVertices.Remove(s);
                            completed[s] = false;
                        }
                        completed[v] = false;
                        m++;
                    }
                }
            }
            FindColoring(g, cols,1,new List<int>(),new bool[vertexCount]);

            return (maxColorsYet, bestColors);
        }

    }
}

