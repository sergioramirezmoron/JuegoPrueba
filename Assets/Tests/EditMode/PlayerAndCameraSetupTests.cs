using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PlayerAndCameraSetupTests
{
    [Test]
    public void ApplyCharacterSetupAlignsControllerAndCameraPivot()
    {
        GameObject player = new GameObject("Player");
        CharacterController controller = player.AddComponent<CharacterController>();
        Type playerMovementType = FindType("PlayerMovement");
        Component movement = player.AddComponent(playerMovementType);

        GameObject pivotObject = new GameObject("CameraPivot");
        pivotObject.transform.SetParent(player.transform, false);

        SetFieldOrProperty(movement, "controllerHeight", 1.84f);
        SetFieldOrProperty(movement, "controllerRadius", 0.32f);
        SetFieldOrProperty(movement, "cameraHeight", 1.68f);

        InvokeInstanceMethod(movement, "ApplyCharacterSetup");

        Assert.That(controller.height, Is.EqualTo(1.84f).Within(0.001f));
        Assert.That(controller.radius, Is.EqualTo(0.32f).Within(0.001f));
        Assert.That(controller.center.y, Is.EqualTo(0.92f).Within(0.001f));
        Assert.That(pivotObject.transform.localPosition.y, Is.EqualTo(1.68f).Within(0.001f));

        UnityEngine.Object.DestroyImmediate(player);
    }

    [Test]
    public void ApplyLookInputClampsVerticalRotation()
    {
        GameObject player = new GameObject("PlayerRoot");
        GameObject pivotObject = new GameObject("CameraPivot");
        pivotObject.transform.SetParent(player.transform, false);

        Type cameraControllerType = FindType("CameraController");
        Component cameraController = pivotObject.AddComponent(cameraControllerType);
        SetFieldOrProperty(cameraController, "minPitch", -85f);
        SetFieldOrProperty(cameraController, "maxPitch", 85f);

        InvokeInstanceMethod(cameraController, "ApplyLookInput", 0f, 500f);

        float clampedPitch = pivotObject.transform.localEulerAngles.x;
        if (clampedPitch > 180f)
        {
            clampedPitch -= 360f;
        }

        Assert.That(clampedPitch, Is.EqualTo(-85f).Within(0.001f));

        UnityEngine.Object.DestroyImmediate(player);
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

    private static void InvokeInstanceMethod(Component component, string methodName, params object[] arguments)
    {
        MethodInfo method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            Assert.Fail($"No se encontró el método '{methodName}' en '{component.GetType().Name}'.");
            return;
        }

        method.Invoke(component, arguments);
    }
}
