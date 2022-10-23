﻿namespace TowerDefense.Server.Models
{
    public class Unit : Observer.Observer
    {
        public double Speed { get; set; }
        public UnitStrategy UnitStrategy { get; set; }


        public Unit(string name)
        {
            this.name = name;
        }

        string name = "";

        public void SayHello()
        {
            Console.WriteLine();
        }

        public string GetName()
        {
            return name;
        }

        public void SetName(string name)
        {
            this.name = name;
        }


        public UnitStrategy GetUnitStrategy()
        {
            return UnitStrategy;
        }

        public void SetUnitStrategy(UnitStrategy UnitStrategy, Unit unit)
        {
            this.UnitStrategy = UnitStrategy;
            this.UnitStrategy.Act(unit);
        }

        public override void UpdateUnits()
        {
            throw new NotImplementedException();
        }
    }
}
