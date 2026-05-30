using UnityEngine;

/// <summary>
/// シーン遷移後のスポーン位置を示すマーカー。
/// このコンポーネント自体は何もしない。
/// MapTransition.CheckSpawnPoint() が GameObject.Find() でこのオブジェクトを名前で検索し、
/// transform.position をプレイヤーの到着位置として使う。
/// </summary>
[DisallowMultipleComponent]
public class SpawnPoint : MonoBehaviour { }
