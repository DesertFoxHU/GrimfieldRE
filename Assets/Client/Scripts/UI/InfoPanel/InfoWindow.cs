using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace InfoPanel
{

    /// <summary>
    /// InfoPanel is a more detailed panel for tile,building and entity information
    /// </summary>
    public class InfoWindow : MonoBehaviour
    {
        public enum ContentType
        {
            Tile,
            Building,
            Entity,
        }

        public ContentType? CurrentType { get; private set; } = null;
        [HideInInspector] public Tilemap map;
        public List<ContentPanel> contens;
        public GameObject visibilityPanel;

        public object LastObject { get; private set; }

        #region Resource
        public Image icon;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        #endregion

        private void Start()
        {
            map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
            SetActive(false);

            //Fake Building test
            /*AbstractBuilding test = new Village(new ServerSide.ServerPlayer(0, "Test"), new Vector3Int(0, 0, 0));
            Load(test);*/
        }

        public void SetActive(bool isActive)
        {
            visibilityPanel.SetActive(isActive);
            GetComponent<Image>().enabled = isActive;
        }

        public void HideAll()
        {
            contens.ForEach(x => x.gameObject.SetActive(false));
        }

        public void OpenRecruitPanel()
        {
            if(LastObject is AbstractBuilding building)
            {
                FindAnyObjectByType<RecruitPanel>().LoadPanel(building.Position, building.GetDefinition());
            }
        }

        public void Load(object obj)
        {
            if (obj == null) return;

            SetActive(true);
            LastObject = obj;
            if (obj.GetType() == typeof(Vector3Int))
            {
                CurrentType = ContentType.Tile;
                HideAll();

                Vector3Int pos = (Vector3Int)obj;
                if (!map.HasTile(pos)) return;

                GrimfieldTile tile = map.GetTile<GrimfieldTile>(pos);
                TileDefinition definition = tile.definition;

                icon.sprite = tile.sprite;
                title.text = definition.tileName;
                description.text = definition.description;
            }
            else if (obj is AbstractBuilding building)
            {
                CurrentType = ContentType.Building;

                icon.sprite = building.GetDefinition().GetSpriteByLevel(building.Level);
                title.text = building.GetDefinition().type.ToString();
                description.text = building.GetDefinition().description;
            }
            else if(obj is Entity entity)
            {
                CurrentType = ContentType.Entity;

                icon.sprite = entity.Definition.RecruitIcon;
                title.text = entity.Definition.Name;
                description.text = entity.Definition.Description;

            }

            HideAll();

            contens.Find(x => x.type == CurrentType).gameObject.SetActive(true);
            contens.Find(x => x.type == CurrentType).Load(obj);
        }

    }
}