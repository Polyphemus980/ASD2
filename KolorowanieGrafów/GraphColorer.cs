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
        public (int, int[]) FindBestColoring(Graph g)
        {
            test++;
            int vertexCount = g.VertexCount;
            int[] bestColors=new int[g.VertexCount];
            int[] cols = new int[g.VertexCount];
            Array.Fill(cols,-1);
            int maxColorsYet=int.MaxValue;
            void FindColoring(Graph graph, int[] colors, int maxColors,int v,List<int> skippedVertices,bool[] completed)
            {
                if (maxColorsYet <= maxColors)
                    return;
                if (v == vertexCount && skippedVertices.Count==0)
                {
                    if (maxColorsYet > maxColors)
                    { 
                        Array.Copy(colors,bestColors,colors.Length); 
                        maxColorsYet = maxColors;
                    }
                    return;
                }
                if (v == vertexCount)
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
                for (int i = 0; i < maxColors; i++)
                {
                    int uncoloredNeighbors = this.uncoloredNeighbors(graph, v, colors);
                    if (maxColors-graph.OutNeighbors(v).Count()+uncoloredNeighbors > uncoloredNeighbors)
                    {
                        skippedVertices.Add(v);
                        FindColoring(graph,colors,maxColors,v+1,skippedVertices,completed);
                        break;
                    } 
                    if (CanColor(graph, v, i, colors))
                    {
                        colors[v] = i;
                        FindColoring(graph,colors,maxColors,v+1,skippedVertices,completed);
                        colors[v] = -1;
                    }
                    else
                        counter++;
                }

                if (counter == maxColors)
                {
                    int m = maxColors;
                    while (m < maxColorsYet)
                    {
                        colors[v] = m;
                        FindColoring(graph, colors, m + 1, v + 1,skippedVertices,completed);
                        colors[v] = -1;
                        m++;
                    }
                }
            }
            FindColoring(g, cols,1,0,new List<int>(),new bool[vertexCount]);

            return (maxColorsYet, bestColors);
        }

    }
}

