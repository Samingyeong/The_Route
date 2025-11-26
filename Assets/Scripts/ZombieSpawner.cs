using UnityEngine;
using UnityEngine.AI; // NavMesh 기능을 사용하기 위해 필요

public class ZombieSpawner : MonoBehaviour
{
    // 유니티 에디터에서 설정할 좀비 프리팹 (첨부 이미지의 'zombie_Random'에 해당)
    public GameObject zombiePrefab;

    // 생성할 총 좀비 개수
    public int totalZombiesToSpawn = 30;

    // 스폰 가능 영역을 나타내는 NavMeshSurface의 중심점과 반경
    // NavMeshSurface는 맵 전체에 걸쳐있다고 가정하고, 생성 영역을 제한하기 위해 사용
    public float spawnRadius = 20f;

    // 현재까지 생성된 좀비 개수
    private int spawnedCount = 0;

    void Start()
    {
        // 게임 시작 시 바로 생성 함수 호출
        SpawnZombies();
    }

    void SpawnZombies()
    {
        // 지정된 개수만큼 반복
        while (spawnedCount < totalZombiesToSpawn)
        {
            Vector3 randomPosition = GetRandomPointOnNavMesh();

            // 유효한 위치를 찾았을 경우
            if (randomPosition != Vector3.zero)
            {
                // 좀비 프리팹을 해당 위치에 생성 (인스턴스화)
                Instantiate(zombiePrefab, randomPosition, Quaternion.identity);
                spawnedCount++;
            }
        }
    }

    // NavMesh 위에서 무작위 위치를 찾는 핵심 함수
    Vector3 GetRandomPointOnNavMesh()
    {
        // 1. Spawner의 위치를 중심으로 spawnRadius 내에서 무작위 위치를 결정
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += transform.position;

        NavMeshHit hit;

        // 2. NavMesh.SamplePosition을 사용하여 이 무작위 위치가 NavMesh 상에 있는지 확인
        // 마지막 인자 (NavMesh.AllAreas)는 모든 NavMesh 영역을 검색
        if (NavMesh.SamplePosition(randomDirection, out hit, spawnRadius, NavMesh.AllAreas))
        {
            // NavMesh 상의 가장 가까운 유효한 위치를 반환
            return hit.position;
        }

        // 유효한 위치를 찾지 못했을 경우
        return Vector3.zero;
    }
}