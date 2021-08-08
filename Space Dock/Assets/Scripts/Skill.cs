using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill {

    int level = 0; // the experience level of the skill
    long experience = 0; // the current experience of this skill that is used to calculate the total skill level

    public Skill(int level)
    {
        setLevel(level);
    }

    // exponentially mores experience is needed to go up in level
    public void increaseExperience(long value)
    {
        experience += value;
        level = (int)Mathf.Log10(experience);
    }

    public int getLevel()
    {
        return level;
    }

    public void setLevel(int newLevel)
    {
        level = newLevel;
        experience = (long)Mathf.Pow(10, newLevel);
    }
}
