﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessageTypes;
using Utilities;
using PlayerN;
using System.IO;

namespace DungeonNamespace
{
    public class Room
    {
        public int RoomIndex { set; get; }
        public String name = "";
        public String desc = " A description";


        public List<String> graffitiList = new List<String>();

        public int north = -1;
        public int east = -1;
        public int south = -1;
        public int west = -1;
        private Random r = new Random();

        public List<Vector2D> availableDirections = new List<Vector2D>();

        public Vector2D Position { set; get; }

        public Vector2D GetAvailableDirection()
        {
            if (availableDirections.Count < 1)
            {
                return new Vector2D(0, 0);
            }
            else
            {
                Vector2D v = availableDirections[(r.Next(availableDirections.Count))];
                availableDirections.Remove(v);
                return v;
            }

        }

        public Room()
        {
            graffitiList = new List<string>();
        }

        public Room(String Name, int index)
        {
            name = Name;
            RoomIndex = index;
            Init();
        }

        public Room(String name, String desc)
        {
            this.desc = desc;
            this.name = name;
            Init();
        }

        public void Init()
        {
            Position = new Vector2D();
            availableDirections.Add(Dungeon.NORTH);
            availableDirections.Add(Dungeon.EAST);
            availableDirections.Add(Dungeon.SOUTH);
            availableDirections.Add(Dungeon.WEST);
        }

        public int[] GetExitIndexs()
        {
            int[] rInt = { north, east, south, west };
            return rInt;
        }

        public bool AddConection(Vector2D Direction, int indexOfRoom)
        {
            if (indexOfRoom >= 0)
            {
                if (Direction.Equals(Dungeon.NORTH) && north == -1) { north = indexOfRoom; return true; }
                else if (Direction.Equals(Dungeon.EAST) && east == -1) { east = indexOfRoom; return true; }
                else if (Direction.Equals(Dungeon.SOUTH) && south == -1) { south = indexOfRoom; return true; }
                else if (Direction.Equals(Dungeon.WEST) && west == -1) { west = indexOfRoom; return true; }
            }
            return false;
        }

    }
}
