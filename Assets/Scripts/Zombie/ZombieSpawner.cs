using UnityEngine;
using UnityEngine.AI;
using System.Collections; // 코루틴(IEnumerator)을 사용하기 위해 필요

public class ZombieSpawner : MonoBehaviour
{
    // 유니티 에디터에서 설정할 좀비 프리팹
    public GameObject zombiePrefab;

    // **[수정]** 씬에 유지할 **최대 좀비 개수** (이 개수보다 적을 때 리스폰)
    public int maxActiveZombies = 30;

    // **[추가]** 좀비를 리스폰할 **주기** (초 단위)
    public float respawnInterval = 10f;

    // 스폰 가능 영역을 나타내는 NavMeshSurface의 반경
    public float spawnRadius = 20f;

    // 플레이어의 위치를 가져오기 위한 Transform
    private Transform playerTransform;

    // *기존의 spawnedCount는 주기적인 리스폰 로직에서는 더 이상 필요하지 않아 제거했습니다.

    void Start()
    {
        // 씬에서 "Player" 태그를 가진 오브젝트를 찾아서 Transform 할당
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 씬에서 찾을 수 없습니다. 'Player' 태그를 확인하세요. 리스폰 기능을 시작할 수 없습니다.");
            return;
        }

        // 게임 시작 시 주기적인 리스폰을 담당할 코루틴을 호출
        StartCoroutine(SpawnZombiesRoutine());
    }

    // 좀비를 주기적으로 리스폰시키는 코루틴
    IEnumerator SpawnZombiesRoutine()
    {
        // 코루틴 시작 시 1초 정도의 딜레이 후 리스폰을 시작합니다.
        yield return new WaitForSeconds(1f);

        while (true) // 무한 루프를 돌며 주기적으로 좀비 개수 확인 및 리스폰
        {
            // 현재 씬에 활성화된 "Zombie" 태그를 가진 오브젝트의 개수를 확인
            int currentZombieCount = GameObject.FindGameObjectsWithTag("Zombie").Length;

            // 최대 개수와 현재 개수를 비교하여 생성해야 할 좀비 개수를 계산
            int zombiesToSpawn = maxActiveZombies - currentZombieCount;

            if (zombiesToSpawn > 0)
            {
                Debug.Log($"현재 활성 좀비: {currentZombieCount}. {zombiesToSpawn}마리를 리스폰합니다.");

                // 필요한 개수만큼 좀비를 생성
                for (int i = 0; i < zombiesToSpawn; i++)
                {
                    Vector3 randomPosition = GetRandomPointOnNavMesh();

                    // NavMesh 위에서 유효한 위치를 찾았을 경우에만 생성
                    if (randomPosition != Vector3.zero)
                    {
                        Instantiate(zombiePrefab, randomPosition, Quaternion.identity);
                    }
                }
            }

            // 지정된 리스폰 주기만큼 대기한 후 루프를 다시 실행
            yield return new WaitForSeconds(respawnInterval);
        }
    }

    // NavMesh 위에서 **플레이어 주변**의 무작위 유효 위치를 찾는 핵심 함수
    Vector3 GetRandomPointOnNavMesh()
    {
        if (playerTransform == null)
        {
            return Vector3.zero;
        }

        // 1. 플레이어의 위치를 중심으로 spawnRadius 내에서 무작위 위치를 결정
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += playerTransform.position;

        NavMeshHit hit;

        // 2. NavMesh.SamplePosition을 사용하여 이 무작위 위치가 NavMesh 상에 있는지 확인
        if (NavMesh.SamplePosition(randomDirection, out hit, spawnRadius, NavMesh.AllAreas))
        {
            // NavMesh 상의 가장 가까운 유효한 위치를 반환
            return hit.position;
        }

        // 유효한 위치를 찾지 못했을 경우
        return Vector3.zero;
    }
}