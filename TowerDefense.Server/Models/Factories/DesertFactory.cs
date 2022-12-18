﻿using TowerDefense.Server.Models.Bridge;
using TowerDefense.Server.Models.Enemies;
using TowerDefense.Server.Models.Maps;
using TowerDefense.Server.Models.Strategies;
using TowerDefense.Server.Models.Towers;

namespace TowerDefense.Server.Models
{
    public class DesertFactory : AbstractFactory
    {
        public override Unit CreateEnemy(string type)
        {
            if (type == "S")
            {
                return new DesertShootingEnemy();
            }
            else if (type == "H")
            {
                return new DesertHealingEnemy();
            }
            else
            {
                return null;
            }
        }

        public override Tower CreateTower(string type)
        {
            if (type == "S")
            {
                return new DesertSingleShotTower(new FirstEnteredRangeShootingStyle());
            }
            else if (type == "A")
            {
                return new DesertAoeTower(new FirstEnteredRangeShootingStyle());
            }
            else
            {
                return null;
            }
        }
    }
}
