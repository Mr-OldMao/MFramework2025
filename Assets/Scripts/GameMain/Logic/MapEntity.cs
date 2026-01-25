using MFramework.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace GameMain
{
    public partial class MapEntity : MonoBehaviour
    {
        public Vector2 gridPos;
        public EMapEntityType mapEntityType;

        public void SetMapEntityType(Vector2 gridPos, EMapEntityType eMapEntityType)
        {
            this.gridPos = gridPos;
            this.mapEntityType = eMapEntityType;
        }

        public void SetSprite(EMapEntityType eMapEntityType, SpriteAtlas itemAtlas)
        {
            SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            switch (eMapEntityType)
            {
                case EMapEntityType.Wall_LU:
                case EMapEntityType.Wall_LD:
                case EMapEntityType.Wall_RU:
                case EMapEntityType.Wall_RD:
                case EMapEntityType.Wall:
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        spriteRenderers[i].sprite = itemAtlas.GetSprite("wall32_0");
                    }
                    break;
                case EMapEntityType.Stone_LU:
                case EMapEntityType.Stone_LD:
                case EMapEntityType.Stone_RU:
                case EMapEntityType.Stone_RD:
                case EMapEntityType.Stone:
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        spriteRenderers[i].sprite = itemAtlas.GetSprite("stone_0");
                    }
                    break;
                default:
                    Debug.LogError($"SetSprite Error {eMapEntityType}");
                    break;
            }
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
                        DestroySubEntity(subEntity);
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
                            DestroySubEntity(subEntity);
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

        private void DestroySubEntity(GameObject subEntity)
        {
            if (subEntity != null)
            {
                subEntity.SetActive(false);
            }
        }

        private void SetBridDeadSprite()
        {
            mapEntityType = EMapEntityType.DeadBrid;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            GameEntry.Audio.PlaySound("explosion_bird.ogg");
        }



        public void FixedBridWall()
        {
            bool isBirdWall = GameEntry.UI.GetModel<UIModelMap>().IsBridWallGridPos(gridPos);
            bool isWall = mapEntityType == EMapEntityType.Wall
                || mapEntityType == EMapEntityType.Wall_RU
                || mapEntityType == EMapEntityType.Wall_RD
                || mapEntityType == EMapEntityType.Wall_LU
                || mapEntityType == EMapEntityType.Wall_LD;
            if (isBirdWall && isWall)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }

        public void ChangeBirdWallToStone()
        {
            mapEntityType = ChangeEntityTypeWallToStone(mapEntityType);
            if (mapEntityType ==  EMapEntityType.None)
            {
                return;
            }
            var wallGroup = transform.Find<Transform>("wallGroup");
            wallGroup.gameObject.SetActive(false);
            for (int i = 0;i < wallGroup.childCount; i++)
            {
                wallGroup.GetChild(i).gameObject.SetActive(false);
            }

            var stoneGroup = transform.Find<Transform>("stoneGroup");
            stoneGroup.gameObject.SetActive(true);
            //Í£Ö¹²¥·Å¶¯»­Animator¶¯»­

            stoneGroup.GetComponent<Animator>().Play("wallToStone",0,0f);
            for (int i = 0; i < stoneGroup.childCount; i++)
            {
                stoneGroup.GetChild(i).gameObject.SetActive(true);
            }
        }

        public void ChangeBirdStoneToWall()
        {
            mapEntityType = ChangeEntityTypeStoneToWall(mapEntityType);
            if (mapEntityType == EMapEntityType.None)
            {
                return;
            }
            var wallGroup = transform.Find<Transform>("wallGroup");
            wallGroup.gameObject.SetActive(true);
            for (int i = 0; i < wallGroup.childCount; i++)
            {
                wallGroup.GetChild(i).gameObject.SetActive(true);
            }

            var stoneGroup = transform.Find<Transform>("stoneGroup");
            stoneGroup.gameObject.SetActive(false);
            for (int i = 0; i < stoneGroup.childCount; i++)
            {
                stoneGroup.GetChild(i).gameObject.SetActive(false);
            }
        }


        private EMapEntityType ChangeEntityTypeWallToStone(EMapEntityType eMapEntityType)
        {
            EMapEntityType targetType = EMapEntityType.None;
            switch (eMapEntityType)
            {
                case EMapEntityType.Wall:
                    targetType = EMapEntityType.Stone;
                    break;
                case EMapEntityType.Wall_LU:
                    targetType = EMapEntityType.Stone_LU;
                    break;
                case EMapEntityType.Wall_LD:
                    targetType = EMapEntityType.Stone_LD;
                    break;
                case EMapEntityType.Wall_RU:
                    targetType = EMapEntityType.Stone_RU;
                    break;
                case EMapEntityType.Wall_RD:
                    targetType = EMapEntityType.Stone_RD;
                    break;
                case EMapEntityType.Stone:
                    targetType = EMapEntityType.Stone;
                    break;
                case EMapEntityType.Stone_LU:
                    targetType = EMapEntityType.Stone_LU;
                    break;
                case EMapEntityType.Stone_LD:
                    targetType = EMapEntityType.Stone_LD;
                    break;
                case EMapEntityType.Stone_RU:
                    targetType = EMapEntityType.Stone_RU;
                    break;
                case EMapEntityType.Stone_RD:
                    targetType = EMapEntityType.Stone_RD;
                    break;
                default:
                    Debugger.LogError($"ChangeEntityType fail, gridPos:{gridPos} ,eMapEntityType :{eMapEntityType}");
                    break;
            }
            return targetType;
        }

        private EMapEntityType ChangeEntityTypeStoneToWall(EMapEntityType eMapEntityType)
        {
            EMapEntityType targetType = EMapEntityType.None;
            switch (eMapEntityType)
            {
                case EMapEntityType.Wall:
                    targetType = EMapEntityType.Wall;
                    break;
                case EMapEntityType.Wall_LU:
                    targetType = EMapEntityType.Wall_LU;
                    break;
                case EMapEntityType.Wall_LD:
                    targetType = EMapEntityType.Wall_LD;
                    break;
                case EMapEntityType.Wall_RU:
                    targetType = EMapEntityType.Wall_RU;
                    break;
                case EMapEntityType.Wall_RD:
                    targetType = EMapEntityType.Wall_RD;
                    break;
                case EMapEntityType.Stone:
                    targetType = EMapEntityType.Wall;
                    break;
                case EMapEntityType.Stone_LU:
                    targetType = EMapEntityType.Wall_LU;
                    break;
                case EMapEntityType.Stone_LD:
                    targetType = EMapEntityType.Wall_LD;
                    break;
                case EMapEntityType.Stone_RU:
                    targetType = EMapEntityType.Wall_RU;
                    break;
                case EMapEntityType.Stone_RD:
                    targetType = EMapEntityType.Wall_RD;
                    break;
                default:
                    Debugger.LogError($"ChangeEntityType fail, gridPos:{gridPos} ,eMapEntityType :{eMapEntityType}");
                    break;
            }
            return targetType;
        }
    }
}
