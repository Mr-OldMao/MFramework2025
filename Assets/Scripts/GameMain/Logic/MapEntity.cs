using MFramework.Runtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameMain
{
    public partial class MapEntity : MonoBehaviour
    {
        public EMapEntityType mapEntityType;

        public Dictionary<Vector2 , GameObject> dicSubEntity =  new Dictionary<Vector2, GameObject> ();

        private void Awake()
        {
            
        }

        public void SetMapEntityType(EMapEntityType eMapEntityType)
        {
            this.mapEntityType = eMapEntityType;

            //for (int i = 0; i < transform.childCount; i++)
            //{
            //    transform.GetChild(i).AddComponent<MapSubEntity>().mapEntityType = eMapEntityType;
            //}
        }



        public void BulletCollEvent(BulletEntity bulletEntity, GameObject subEntity)
        {
            if (bulletEntity != null && subEntity != null)
            {
                switch (mapEntityType)
                {
                    case EMapEntityType.None:
                    case EMapEntityType.Grass:
                    case EMapEntityType.Water:
                    case EMapEntityType.Snow:
                        break;
                    case EMapEntityType.AirBorder:
                        if (bulletEntity.tankOwnerType == TankOwnerType.Player1 || bulletEntity.tankOwnerType == TankOwnerType.Player2)
                        {
                            GameEntry.Audio.PlaySound("bullet_hit_border.mp3");
                        }
                        break;
                    case EMapEntityType.Wall:
                    case EMapEntityType.Wall_LU:
                    case EMapEntityType.Wall_LD:
                    case EMapEntityType.Wall_RU:
                    case EMapEntityType.Wall_RD:
                        if (bulletEntity.tankOwnerType == TankOwnerType.Player1 || bulletEntity.tankOwnerType == TankOwnerType.Player2)
                        {
                            GameEntry.Audio.PlaySound("bullet_hit_wall.mp3");
                        }
                        Destroy(subEntity);
                        break;
                    case EMapEntityType.Stone:
                    case EMapEntityType.Stone_LU:
                    case EMapEntityType.Stone_LD:
                    case EMapEntityType.Stone_RU:
                    case EMapEntityType.Stone_RD:
                        if (bulletEntity.tankOwnerType == TankOwnerType.Player1 || bulletEntity.tankOwnerType == TankOwnerType.Player2)
                        {
                            GameEntry.Audio.PlaySound("bullet_hit_border.mp3");
                        }
                        if (bulletEntity.BulletData.IsCanStone)
                        {
                            Destroy(subEntity);
                        }
                        break;
                    case EMapEntityType.Brid:
                        Debug.Log("GameOver Brid");
                        SetBridDeadSprite();
                        if (GameMainLogic.Instance.GameStateType != GameStateType.GameFail)
                        {
                            GameMainLogic.Instance.GameStateType = GameStateType.GameFail;
                        }
                        break;
                    case EMapEntityType.DeadBrid:
                        Debug.Log("GameOver DeadBrid");
                        break;
                }
                
            }
        }

        private void SetBridDeadSprite()
        {
            mapEntityType = EMapEntityType.DeadBrid;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            GameEntry.Audio.PlaySound("explosion_bird.ogg");
        }
    }
}
