using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Transform hunter;
    [SerializeField] float weightSeparation = 0.3f;
    [SerializeField] float weightAlignment = 0.4f;
    [SerializeField] float weightCohesion = 0.3f;

    public List<Character> boids = new List<Character>();
    public List<Transform> foods = new List<Transform>();

    public Transform Hunter => hunter;
    public float WeightSeparation => weightSeparation;
    public float WeightAlignment => weightAlignment;
    public float WeightCohesion => weightCohesion;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}