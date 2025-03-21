using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] treePrefabs; // ������ �������� ��������.  ����������� ��������� ������� � Inspector.

    [Header("Spawn Parameters")]
    public Vector3 spawnPosition; // ������� ������ (������������ �������, �� ������� ����� ������). ����������� ��� ��������.
    public float spawnAreaWidth = 5f; // ������ ������� ������ (�� ��� X). ��� ������������.
    public float yOffset = 0f; // �������� �� ������ ��� ������� ������.  �����������, ����� ���������� ������� ��� ������.

    [Header("Movement")]
    public Vector3 movementDirection = Vector3.back; // ����������� �������� ��������.  �������������!
    public float movementSpeed = 5f; // �������� �������� ��������.

    [Header("Object Management")]
    public float spawnRate = 1f; // �������� ����� �������� (� ��������).
    public float destroyDistance = 20f; // ���������, ����� ������� ������� ��������� (�� ������).
    public int maxTrees = 20; // ������������ ���������� �������� � ����� (��� �����������).

    private Transform _playerTransform; // ������ �� Transform ������ (��� ������).  �������� ��� ������������������.
    private List<GameObject> _trees = new List<GameObject>(); // ������ ���� ��������. ���������� �������.

    void Start()
    {
        // ������� ������ �� ���� "MainCamera".  ����� ��� ������ �������.
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");
        if (player == null)
        {
            Debug.LogError("�� ������� ������ � ����� 'MainCamera'! ���������, ��� ������ � ����� ����� ���� ���.");
            enabled = false; // ��������� ������, ����� �������� ������.
            return;
        }
        _playerTransform = player.transform;

        // ����������� ������ �����������, ����� �������� ���� �������������.
        movementDirection = movementDirection.normalized;

        // ��������� �������� ��� ������ ��������.
        StartCoroutine(SpawnAndMoveTrees());
    }

    IEnumerator SpawnAndMoveTrees()
    {
        while (true) // ����������� ���� ��� ����������� ������.
        {
            // 1. �������� ��������� ������ ������.
            if (treePrefabs == null || treePrefabs.Length == 0)
            {
                Debug.LogError("��� �������� ��������!  ��������� ������� � ���� 'Tree Prefabs' � Inspector.");
                yield break; // ��������� ��������, ����� �������� ������.
            }
            GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

            // 2. ��������� ��������� ������� ������.
            float randomXOffset = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
            Vector3 actualSpawnPosition = transform.position + spawnPosition + new Vector3(randomXOffset, yOffset, 0f);

            // 3. ������� ������.
            GameObject newTree = Instantiate(treePrefab, actualSpawnPosition, Quaternion.identity);

            // 4. ����������� �������� ������ (��������� � ����������� TreeMovement).
            TreeMovement treeMovement = newTree.AddComponent<TreeMovement>();
            treeMovement.direction = movementDirection;
            treeMovement.speed = movementSpeed;

            // 5. ��������� ������ � ������.
            _trees.Add(newTree);

            // 6. ������������ ���������� �������� � �����.  ������� ������, ���� ����������.
            while (_trees.Count > maxTrees)
            {
                DestroyOldestTree(); // ������� ��� �������� ������ ������� ������.
            }

            // 7. ���� �������� ����� ����� ��������� �������.
            yield return new WaitForSeconds(spawnRate);
        }
    }


    // ��������������� ������� ��� ����������� ������ ������� ������.
    void DestroyOldestTree()
    {
        if (_trees.Count == 0) return; // ������ �� ������, ���� ������ ����.

        GameObject oldestTree = _trees[0]; // �������� ����� ������ ������ (������ ������� ������).
        _trees.RemoveAt(0); // ������� ��� �� ������.

        if (oldestTree != null) // ���������, ��� ������ ��� ����������.
        {
            Destroy(oldestTree); // ���������� ������.
        }
    }


    // Update ���������� ������ ����.  ������������ ��� ������� ������ � �������� ��������, ������� �� destroyDistance.
    void Update()
    {
        // �������� �� ������ � �����, ����� ��������� ������� ��������.
        for (int i = _trees.Count - 1; i >= 0; i--)
        {
            GameObject tree = _trees[i];

            if (tree == null) // ���������, ��� ������ ��� ����������.  ���� ��� - ������� �� ������.
            {
                _trees.RemoveAt(i);
                continue;
            }

            // ��������� ���������� �� ������.
            float distanceToPlayer = Vector3.Distance(_playerTransform.position, tree.transform.position);

            // ���� ������ ������� ������, ���������� ���.
            if (distanceToPlayer > destroyDistance)
            {
                Destroy(tree); // ���������� ������.
                _trees.RemoveAt(i); // ������� ��� �� ������.
            }
        }
    }
}