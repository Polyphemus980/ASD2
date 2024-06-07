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

        public (int,List<int>) FindBestVertex(bool[] completed,int[] colors,Graph g,int maxColors,List<int> skippedVertexes,int[] uncNeighbors)
        {
            List<int> addedSkipped = new List<int>();
            int index = 0;
            int max=int.MaxValue;
            int maxUncoloredNeighbors=-1;
            for (int i = 0; i < colors.Length; i++)
            {
                if (completed[i])
                    continue;
                int notcoloredNeighbors = uncNeighbors[i];//uncoloredNeighbors(g, i, colors);
                int outNeighbors = g.Degree(i);
                int coloredNeighbors = outNeighbors - notcoloredNeighbors;
                if (maxColors - coloredNeighbors > notcoloredNeighbors && !completed[i])     
                {
                    skippedVertexes.Add(i);
                    addedSkipped.Add(i);
                    completed[i] = true;
                    continue;
                }
                if (maxColors - outNeighbors + notcoloredNeighbors < max)
                {
                    max = maxColors - outNeighbors + notcoloredNeighbors;
                    index = i;
                    maxUncoloredNeighbors = notcoloredNeighbors;
                }
                else if (maxColors - outNeighbors + notcoloredNeighbors == max)
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
            test++;
            int vertexCount = g.VertexCount;
            int[] bestColors=new int[g.VertexCount];
            int[] cols = new int[g.VertexCount];
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i] = -1;
            }
            int maxColorsYet=int.MaxValue;
            void FindColoring(Graph graph, int[] colors, int maxColors,List<int> skippedVertices,bool[] completed,int[] uncoloredNeighbors)
            {
                if (maxColorsYet <= maxColors)
                    return;
                bool all = AllCompleted(completed);
                if (all)
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
                
                int counter = 0;
                (int v,List<int> addedSkipped) = FindBestVertex(completed,colors,graph,maxColors,skippedVertices,uncoloredNeighbors);
                foreach (int neighbor in g.OutNeighbors(v))
                {
                    uncoloredNeighbors[neighbor]--;
                }
                for (int i = 0; i < maxColors; i++)
                {
                    if (CanColor(graph, v, i, colors))
                    {
                        completed[v] = true;
                        colors[v] = i;
                        FindColoring(graph,colors,maxColors,skippedVertices,completed,uncoloredNeighbors);
                        colors[v] = -1;
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
                        FindColoring(graph, colors, m + 1,skippedVertices,completed,uncoloredNeighbors);
                        colors[v] = -1;
                        completed[v] = false;
                        m++;
                    }
                }
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
            FindColoring(g, cols,1,new List<int>(),new bool[vertexCount],uncNeighbors);
            return (maxColorsYet, bestColors);
        }

    }
}

