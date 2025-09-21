using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class QuickUtils
{
    public static T Choice<T>(IList<T> list) {
        if (list == null || list.Count == 0) {
            throw new ArgumentException("List is null or empty!");
        }
        int randomIndex = UnityEngine.Random.Range(0, list.Count);
        return list[randomIndex];
    }
    
    public static string GetVFXNameFromEnum(SpecialEffect effect) {
        if (effect == SpecialEffect.NONE) throw new InvalidEnumArgumentException("SpecialEffect cannot be NONE!");
        switch (effect) {
            case SpecialEffect.COLLISION:
                return "CollisionEffect";
            case SpecialEffect.SMALL_EXPLOSION:
                return "SmallExplosionEffect";
            case SpecialEffect.LARGE_EXPLOSION:
                return "ExplosionEffect";
        }
        throw new InvalidEnumArgumentException("Unknown special effect: " + effect);
    }
    
    public static bool GetIsExplosiveFromSpecialEffectEnum(SpecialEffect effect) {
        if (effect == SpecialEffect.LARGE_EXPLOSION || effect == SpecialEffect.SMALL_EXPLOSION) {
            return true;
        }
        return false;
    }

    public static string GetRandomName() {
        return Choice(TankNameBank.Adjectives) + "\n" + Choice(TankNameBank.Nouns);
    }
}
