using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] treePrefabs; // Массив префабов деревьев.  Обязательно назначьте префабы в Inspector.

    [Header("Spawn Parameters")]
    public Vector3 spawnPosition; // Позиция спавна (относительно объекта, на котором висит скрипт). Используйте для смещения.
    public float spawnAreaWidth = 5f; // Ширина области спавна (по оси X). Для разнообразия.
    public float yOffset = 0f; // Смещение по высоте для позиции спавна.  Используйте, чтобы приподнять деревья над землей.

    [Header("Movement")]
    public Vector3 movementDirection = Vector3.back; // Направление движения деревьев.  Нормализовать!
    public float movementSpeed = 5f; // Скорость движения деревьев.

    [Header("Object Management")]
    public float spawnRate = 1f; // Интервал между спавнами (в секундах).
    public float destroyDistance = 20f; // Дистанция, после которой деревья удаляются (от игрока).
    public int maxTrees = 20; // Максимальное количество деревьев в сцене (для оптимизации).

    private Transform _playerTransform; // Ссылка на Transform игрока (или камеры).  Кэшируем для производительности.
    private List<GameObject> _trees = new List<GameObject>(); // Список всех деревьев. Управление памятью.

    void Start()
    {
        // Находим камеру по тегу "MainCamera".  Важно для работы скрипта.
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");
        if (player == null)
        {
            Debug.LogError("Не найдена камера с тегом 'MainCamera'! Убедитесь, что камера в сцене имеет этот тег.");
            enabled = false; // Отключаем скрипт, чтобы избежать ошибок.
            return;
        }
        _playerTransform = player.transform;

        // Нормализуем вектор направления, чтобы скорость была консистентной.
        movementDirection = movementDirection.normalized;

        // Запускаем корутину для спавна деревьев.
        StartCoroutine(SpawnAndMoveTrees());
    }

    IEnumerator SpawnAndMoveTrees()
    {
        while (true) // Бесконечный цикл для постоянного спавна.
        {
            // 1. Выбираем случайный префаб дерева.
            if (treePrefabs == null || treePrefabs.Length == 0)
            {
                Debug.LogError("Нет префабов деревьев!  Назначьте префабы в поле 'Tree Prefabs' в Inspector.");
                yield break; // Прерываем корутину, чтобы избежать ошибок.
            }
            GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

            // 2. Вычисляем случайную позицию спавна.
            float randomXOffset = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
            Vector3 actualSpawnPosition = transform.position + spawnPosition + new Vector3(randomXOffset, yOffset, 0f);

            // 3. Создаем дерево.
            GameObject newTree = Instantiate(treePrefab, actualSpawnPosition, Quaternion.identity);

            // 4. Настраиваем движение дерева (добавляем и настраиваем TreeMovement).
            TreeMovement treeMovement = newTree.AddComponent<TreeMovement>();
            treeMovement.direction = movementDirection;
            treeMovement.speed = movementSpeed;

            // 5. Добавляем дерево в список.
            _trees.Add(newTree);

            // 6. Ограничиваем количество деревьев в сцене.  Удаляем старые, если необходимо.
            while (_trees.Count > maxTrees)
            {
                DestroyOldestTree(); // Функция для удаления самого старого дерева.
            }

            // 7. Ждем заданное время перед следующим спавном.
            yield return new WaitForSeconds(spawnRate);
        }
    }


    // Вспомогательная функция для уничтожения самого старого дерева.
    void DestroyOldestTree()
    {
        if (_trees.Count == 0) return; // Защита от ошибок, если список пуст.

        GameObject oldestTree = _trees[0]; // Получаем самое старое дерево (первый элемент списка).
        _trees.RemoveAt(0); // Удаляем его из списка.

        if (oldestTree != null) // Проверяем, что дерево еще существует.
        {
            Destroy(oldestTree); // Уничтожаем дерево.
        }
    }


    // Update вызывается каждый кадр.  Используется для очистки списка и удаления деревьев, ушедших за destroyDistance.
    void Update()
    {
        // Проходим по списку с конца, чтобы безопасно удалять элементы.
        for (int i = _trees.Count - 1; i >= 0; i--)
        {
            GameObject tree = _trees[i];

            if (tree == null) // Проверяем, что объект еще существует.  Если нет - удаляем из списка.
            {
                _trees.RemoveAt(i);
                continue;
            }

            // Вычисляем расстояние до игрока.
            float distanceToPlayer = Vector3.Distance(_playerTransform.position, tree.transform.position);

            // Если дерево слишком далеко, уничтожаем его.
            if (distanceToPlayer > destroyDistance)
            {
                Destroy(tree); // Уничтожаем дерево.
                _trees.RemoveAt(i); // Удаляем его из списка.
            }
        }
    }
}