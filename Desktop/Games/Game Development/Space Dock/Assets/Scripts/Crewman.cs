using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crewman {

    public enum Professions {Any, Pilot, Marine, Engineer};

    Sprite portrait;
    string name;
    Professions profession;
    Skill intellect;
    Skill coordination;
    Skill productivity;
    Skill brawn;
    Skill temperment;

    public Crewman(Sprite portrait, string name, Professions profession, int intellect, int coordination, int productivity, int brawn, int temperment)
    {
        this.portrait = portrait;
        this.name = name;
        this.profession = profession;
        this.intellect = new Skill(intellect);
        this.coordination = new Skill(coordination);
        this.productivity = new Skill(productivity);
        this.brawn = new Skill(brawn);
        this.temperment = new Skill(temperment);
    }

    public Skill getIntellect()
    {
        return intellect;
    }

    public Skill getCoordination()
    {
        return coordination;
    }

    public Skill getProductivity()
    {
        return productivity;
    }

    public Skill getBrawn()
    {
        return brawn;
    }

    public Skill getTemperment()
    {
        return temperment;
    }

    public Sprite getPortrait()
    {
        return portrait;
    }

    public string getName()
    {
        return name;
    }

    public Professions getProfession()
    {
        return profession;
    }
}
