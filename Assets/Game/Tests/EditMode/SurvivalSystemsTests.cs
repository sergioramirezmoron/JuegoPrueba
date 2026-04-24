using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class SurvivalSystemsTests
{
    [Test]
    public void InventoryRespectsSlotCapacityAcrossDifferentResources()
    {
        GameObject player = new GameObject("InventoryPlayer");
        Type inventoryType = FindType("PlayerInventory");
        Type resourceType = FindType("ResourceType");

        Component inventory = player.AddComponent(inventoryType);
        SetFieldOrProperty(inventory, "slotCapacity", 1);
        SetFieldOrProperty(inventory, "maxStackSize", 2);

        object wood = Enum.Parse(resourceType, "Wood");
        object scrap = Enum.Parse(resourceType, "Scrap");

        bool addedWood = (bool)InvokeInstanceMethod(inventory, "TryAddResource", wood, 2);
        bool addedScrap = (bool)InvokeInstanceMethod(inventory, "TryAddResource", scrap, 1);

        Assert.That(addedWood, Is.True);
        Assert.That(addedScrap, Is.False);
        Assert.That((int)InvokeInstanceMethod(inventory, "GetOccupiedSlotCount"), Is.EqualTo(1));

        UnityEngine.Object.DestroyImmediate(player);
    }

    [Test]
    public void EatingFoodConsumesInventoryAndRestoresHunger()
    {
        GameObject player = new GameObject("VitalsPlayer");
        Type inventoryType = FindType("PlayerInventory");
        Type vitalsType = FindType("PlayerVitals");
        Type resourceType = FindType("ResourceType");

        Component inventory = player.AddComponent(inventoryType);
        Component vitals = player.AddComponent(vitalsType);

        object food = Enum.Parse(resourceType, "Food");
        InvokeInstanceMethod(inventory, "TryAddResource", food, 2);
        InvokeInstanceMethod(vitals, "BindInventory", inventory);
        InvokeInstanceMethod(vitals, "DrainHunger", 40f);

        bool ateFood = (bool)InvokeInstanceMethod(vitals, "TryEatFood");
        float currentHunger = (float)GetFieldOrProperty(vitals, "CurrentHunger");
        int foodCount = (int)InvokeInstanceMethod(inventory, "GetResourceCount", food);

        Assert.That(ateFood, Is.True);
        Assert.That(currentHunger, Is.EqualTo(85f).Within(0.001f));
        Assert.That(foodCount, Is.EqualTo(1));

        UnityEngine.Object.DestroyImmediate(player);
    }

    [Test]
    public void InteractableScoringPrefersCenteredCandidates()
    {
        Type interactorType = FindType("PlayerInteractor");
        MethodInfo scoringMethod = interactorType.GetMethod("TryCalculateCandidateScore", BindingFlags.Static | BindingFlags.Public);

        object[] centeredArguments = { Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 1f), 2.5f, 0.35f, 0f };
        object[] angledArguments = { Vector3.zero, Vector3.forward, new Vector3(1f, 0f, 1f), 2.5f, 0.35f, 0f };

        bool centeredValid = (bool)scoringMethod.Invoke(null, centeredArguments);
        bool angledValid = (bool)scoringMethod.Invoke(null, angledArguments);

        float centeredScore = (float)centeredArguments[5];
        float angledScore = (float)angledArguments[5];

        Assert.That(centeredValid, Is.True);
        Assert.That(angledValid, Is.True);
        Assert.That(centeredScore, Is.GreaterThan(angledScore));
    }

    private static Type FindType(string typeName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        Assert.Fail($"No se encontró el tipo '{typeName}' en los ensamblados cargados.");
        return null;
    }

    private static object GetFieldOrProperty(Component component, string memberName)
    {
        Type componentType = component.GetType();
        FieldInfo field = componentType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            return field.GetValue(component);
        }

        PropertyInfo property = componentType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null)
        {
            return property.GetValue(component);
        }

        Assert.Fail($"No se encontró el miembro '{memberName}' en '{componentType.Name}'.");
        return null;
    }

    private static void SetFieldOrProperty(Component component, string memberName, object value)
    {
        Type componentType = component.GetType();
        FieldInfo field = componentType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(component, value);
            return;
        }

        PropertyInfo property = componentType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null)
        {
            property.SetValue(component, value);
            return;
        }

        Assert.Fail($"No se encontró el miembro '{memberName}' en '{componentType.Name}'.");
    }

    private static object InvokeInstanceMethod(Component component, string methodName, params object[] arguments)
    {
        MethodInfo method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            Assert.Fail($"No se encontró el método '{methodName}' en '{component.GetType().Name}'.");
            return null;
        }

        return method.Invoke(component, arguments);
    }
}
