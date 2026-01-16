using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }
    private Dictionary<Pole, HashSet<Pole>> graph = new Dictionary<Pole, HashSet<Pole>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void RegisterPole(Pole pole)
    {
        if (!graph.ContainsKey(pole))
        {
            graph.Add(pole, new HashSet<Pole>());
        }
    }
    //whenever player connects two pole it is called inside Touche tracker Script
    public void Connect(Pole a, Pole b)
    {
        if (a == null || b == null || a == b)
            return;
        RegisterPole(a);
        RegisterPole(b);

        graph[a].Add(b);
        graph[b].Add(a);

        // Debug.Log($"[Connect] Connected {a.name} <-> {b.name}");

        UpdateElectricityFullComponent(a);
        UpdateElectricityFullComponent(b);
    }

    // BFS Algo
    private void UpdateElectricityFullComponent(Pole start)
    {
        if (start == null || !graph.ContainsKey(start))
            return;

        HashSet<Pole> visited = new HashSet<Pole>();
        Queue<Pole> queue = new Queue<Pole>();

        queue.Enqueue(start);
        visited.Add(start);

        bool shouldBeConnected = false;

        while (queue.Count > 0)
        {
            Pole current = queue.Dequeue();

            if (current.isConnected || current.isConnected)
            {
                shouldBeConnected = true;
            }

            foreach (Pole neighbor in graph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        if (shouldBeConnected)
        {
            foreach (Pole pole in visited)
            {
                if (!pole.isConnected)
                {
                    pole.isConnected = true;
                    Debug.Log($"[Electricity] {pole.name} is now connected");
                }
            }
        }

    }
}
