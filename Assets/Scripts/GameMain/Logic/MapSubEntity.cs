using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public partial class MapSubEntity : MonoBehaviour
    {
        public EMapEntityType mapEntityType;

        public void SetMapEntityType(EMapEntityType eMapEntityType)
        {
            this.mapEntityType = eMapEntityType;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"{this.gameObject.name} OnCollisionEnter:{collision.gameObject.name}");
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"{this.gameObject.name} OnTriggerEnter:{other.gameObject.name}");

        }

        //public void BulletCollEvent(BulletEntity bulletEntity, GameObject subEntity)
        //{
        //    if (bulletEntity != null && subEntity != null)
        //    {
        //        switch (mapEntityType)
        //        {
        //            case EMapEntityType.None:
        //            case EMapEntityType.Grass:
        //            case EMapEntityType.Water:
        //            case EMapEntityType.Snow:
        //            case EMapEntityType.AirBorder:

        //                break;
        //            case EMapEntityType.Wall:
        //            case EMapEntityType.Wall_LU:
        //            case EMapEntityType.Wall_LD:
        //            case EMapEntityType.Wall_RU:
        //            case EMapEntityType.Wall_RD:
        //                Destroy(subEntity);
        //                break;
        //            case EMapEntityType.Stone:
        //            case EMapEntityType.Stone_LU:
        //            case EMapEntityType.Stone_LD:
        //            case EMapEntityType.Stone_RU:
        //            case EMapEntityType.Stone_RD:
        //                if (bulletEntity.bulletLevel == 4)
        //                {
        //                    Destroy(subEntity);
        //                }
        //                break;

        //            case EMapEntityType.Brid:
        //                Debug.Log("GameOver Brid");
        //                break;
        //            case EMapEntityType.DeadBrid:
        //                Debug.Log("GameOver DeadBrid");
        //                break;
        //        }

        //    }
        //}
    }
}
