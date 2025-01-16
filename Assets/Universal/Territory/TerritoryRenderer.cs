using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace Universal
{
    public class TerritoryRenderer : MonoBehaviour
    {
        [Flags]
        public enum Sides
        {
            None = 1,
            Left = 2,
            Right = 4,
            Top = 8,
            Bottom = 16,
            ALL = Left | Right | Top | Bottom,
        }

        public static TerritoryRenderer Instance;
        private Tilemap map;

        public GameObject spritePrefab;
        [Tooltip("0: Left line, 1: left-upper corner, 2: left-upper-bottom corner, 3: all corner")] public Sprite[] sprites;

        [HideInInspector] public List<Territory> territories = new List<Territory>();

        public void Start()
        {
            Instance = this;

            map = GameObject.FindGameObjectWithTag("GameMap").GetComponent<Tilemap>();
        }

        public void RenderAll()
        {
            foreach (Transform children in transform)
            {
                Destroy(children.gameObject);
            }

            foreach (Territory territory in territories)
            {
                Render(territory, territory.GetAll(), territory.GetColor());
            }
        }

        public void Render(Territory territory, List<Vector3Int> lands, Color color)
        {
            foreach (Vector3Int land in lands)
            {
                Vector3 pos = map.ToVector3(land);

                Sides drawSides = Sides.None;

                if (!territory.GetAll().Contains(new Vector3Int(land.x - 1, land.y))) //Empty left
                {
                    drawSides |= Sides.Left;
                }

                if (!territory.GetAll().Contains(new Vector3Int(land.x + 1, land.y))) //Empty right
                {
                    drawSides |= Sides.Right;
                }

                if (!territory.GetAll().Contains(new Vector3Int(land.x, land.y + 1))) //Empty upper
                {
                    drawSides |= Sides.Top;
                }

                if (!territory.GetAll().Contains(new Vector3Int(land.x, land.y - 1))) //Empty bottom
                {
                    drawSides |= Sides.Bottom;
                }

                if (drawSides != Sides.None)
                {
                    drawSides &= ~Sides.None; //Remove 'None' tag 
                }
                else continue; //Continue, because we dont need to draw anything

                GameObject go = Instantiate(spritePrefab, new Vector3(pos.x + .5f, pos.y + .5f, -1.1f), Quaternion.identity);
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                sr.color = color;

                if (drawSides == Sides.ALL)
                {
                    sr.sprite = sprites[3];
                }
                else if (drawSides == (Sides.Top | Sides.Left))
                {
                    sr.sprite = sprites[1];
                }
                else if (drawSides == (Sides.Top | Sides.Right))
                {
                    sr.sprite = sprites[1];
                    go.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 270f));
                }
                else if (drawSides == (Sides.Bottom | Sides.Left))
                {
                    sr.sprite = sprites[1];
                    go.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
                }
                else if (drawSides == (Sides.Bottom | Sides.Right))
                {
                    sr.sprite = sprites[1];
                    go.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                }
                else if (drawSides == Sides.Left)
                {
                    sr.sprite = sprites[0];
                }
                else if (drawSides == Sides.Bottom)
                {
                    sr.sprite = sprites[0];
                    go.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
                }
                else if (drawSides == Sides.Top)
                {
                    sr.sprite = sprites[0];
                    go.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 270f));
                }
                else if (drawSides == Sides.Right)
                {
                    sr.sprite = sprites[0];
                    go.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                }
            }
        }
    }
}