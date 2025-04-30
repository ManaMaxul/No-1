using UnityEngine;
using System.Collections.Generic;

public class FoodManager : MonoBehaviour
{
    [SerializeField] GameObject foodPrefab;
    [SerializeField] int maxFoodCount = 5;
    [SerializeField] Vector2 spawnArea = new Vector2(20f, 20f);

    void Start()
    {
        SpawnInitialFood();
    }

    void Update()
    {
        GameManager.Instance.foods.RemoveAll(food => food == null);
        if (GameManager.Instance.foods.Count < maxFoodCount)
        {
            SpawnFood();
        }
    }

    void SpawnInitialFood()
    {
        for (int i = 0; i < maxFoodCount; i++)
        {
            SpawnFood();
        }
    }

    void SpawnFood()
    {
        Vector3 position = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            0,
            Random.Range(-spawnArea.y, spawnArea.y)
        );
        GameObject food = Instantiate(foodPrefab, position, Quaternion.identity);
        food.tag = "Food";
        GameManager.Instance.foods.Add(food.transform);
    }
}