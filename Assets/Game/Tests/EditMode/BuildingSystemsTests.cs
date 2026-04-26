using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BuildingSystemsTests
{
    [Test]
    public void FoundationSnapUsesTwoMeterGrid()
    {
        Type buildingSystemType = FindType("BuildingSystem");
        MethodInfo snapMethod = buildingSystemType.GetMethod("SnapToFoundationGrid", BindingFlags.Static | BindingFlags.Public);

        Vector3 snapped = (Vector3)snapMethod.Invoke(null, new object[] { new Vector3(1.1f, 0f, 2.7f), 2f, 0.125f });

        Assert.That(snapped, Is.EqualTo(new Vector3(2f, 0.125f, 2f)));
    }

    [Test]
    public void FoundationRotationUsesOnlyYAxisAndSupportsFullCircle()
    {
        Type buildingSystemType = FindType("BuildingSystem");
        MethodInfo rotationMethod = buildingSystemType.GetMethod("CreateFoundationRotation", BindingFlags.Static | BindingFlags.Public);

        Quaternion rotation = (Quaternion)rotationMethod.Invoke(null, new object[] { 450f });
        Vector3 euler = rotation.eulerAngles;

        Assert.That(Mathf.DeltaAngle(euler.x, 0f), Is.EqualTo(0f).Within(0.01f));
        Assert.That(Mathf.DeltaAngle(euler.y, 90f), Is.EqualTo(0f).Within(0.01f));
        Assert.That(Mathf.DeltaAngle(euler.z, 0f), Is.EqualTo(0f).Within(0.01f));
    }

    [Test]
    public void InfiniteBuildToggleChangesModeState()
    {
        Type buildingSystemType = FindType("BuildingSystem");
        Component buildingSystem = new GameObject("BuildingSystem").AddComponent(buildingSystemType);
        PropertyInfo infiniteBuildEnabledProperty = buildingSystemType.GetProperty("InfiniteBuildEnabled", BindingFlags.Instance | BindingFlags.Public);

        InvokeInstanceMethod(buildingSystem, "ToggleInfiniteBuild");
        bool firstState = (bool)infiniteBuildEnabledProperty.GetValue(buildingSystem);

        InvokeInstanceMethod(buildingSystem, "ToggleInfiniteBuild");
        bool secondState = (bool)infiniteBuildEnabledProperty.GetValue(buildingSystem);

        Assert.That(firstState, Is.True);
        Assert.That(secondState, Is.False);

        UnityEngine.Object.DestroyImmediate(buildingSystem.gameObject);
    }

    [Test]
    public void WallSocketAcceptsWallAndDoorwayButRejectsFoundation()
    {
        GameObject pieceObject = new GameObject("Piece");
        Type pieceType = FindType("BuildPiece");
        Type socketType = FindType("BuildSocket");
        Type buildSocketType = FindType("BuildSocketType");
        Type buildPieceType = FindType("BuildPieceType");

        Component piece = pieceObject.AddComponent(pieceType);
        Component socket = pieceObject.AddComponent(socketType);

        InvokeInstanceMethod(socket, "Initialize", piece, Enum.Parse(buildSocketType, "Wall"));

        bool acceptsWall = (bool)InvokeInstanceMethod(socket, "CanAttach", Enum.Parse(buildPieceType, "Wall"));
        bool acceptsDoorway = (bool)InvokeInstanceMethod(socket, "CanAttach", Enum.Parse(buildPieceType, "Doorway"));
        bool acceptsFoundation = (bool)InvokeInstanceMethod(socket, "CanAttach", Enum.Parse(buildPieceType, "Foundation"));

        Assert.That(acceptsWall, Is.True);
        Assert.That(acceptsDoorway, Is.True);
        Assert.That(acceptsFoundation, Is.False);

        UnityEngine.Object.DestroyImmediate(pieceObject);
    }

    [Test]
    public void DoorwayKeepsSameOuterWidthAsWallAndProvidesWideOpening()
    {
        GameObject pieceObject = new GameObject("DoorwayPiece");
        Type pieceType = FindType("BuildPiece");
        Type buildPieceType = FindType("BuildPieceType");
        Component piece = pieceObject.AddComponent(pieceType);

        InvokeInstanceMethod(piece, "Initialize", Enum.Parse(buildPieceType, "Doorway"), false);

        Transform leftPost = pieceObject.transform.Find("DoorLeftPost");
        Transform rightPost = pieceObject.transform.Find("DoorRightPost");
        Transform topBeam = pieceObject.transform.Find("DoorTop");

        float outerLeft = leftPost.localPosition.x - (leftPost.localScale.x * 0.5f);
        float outerRight = rightPost.localPosition.x + (rightPost.localScale.x * 0.5f);
        float openingWidth = (rightPost.localPosition.x - (rightPost.localScale.x * 0.5f)) -
                             (leftPost.localPosition.x + (leftPost.localScale.x * 0.5f));
        float openingHeight = (topBeam.localPosition.y - (topBeam.localScale.y * 0.5f)) -
                              (-leftPost.localScale.y * 0.5f);

        Assert.That(outerLeft, Is.EqualTo(-1f).Within(0.001f));
        Assert.That(outerRight, Is.EqualTo(1f).Within(0.001f));
        Assert.That(openingWidth, Is.GreaterThanOrEqualTo(1.69f));
        Assert.That(openingHeight, Is.GreaterThanOrEqualTo(2.09f));

        UnityEngine.Object.DestroyImmediate(pieceObject);
    }

    [Test]
    public void PreviewFoundationIsMarkedAsPreview()
    {
        Type pieceType = FindType("BuildPiece");
        Type buildPieceType = FindType("BuildPieceType");
        GameObject previewFoundationObject = new GameObject("PreviewFoundation");
        Component previewFoundation = previewFoundationObject.AddComponent(pieceType);

        InvokeInstanceMethod(previewFoundation, "Initialize", Enum.Parse(buildPieceType, "Foundation"), true);

        PropertyInfo isPreviewProperty = pieceType.GetProperty("IsPreview", BindingFlags.Instance | BindingFlags.Public);
        bool isPreview = (bool)isPreviewProperty.GetValue(previewFoundation);

        Assert.That(isPreview, Is.True);

        UnityEngine.Object.DestroyImmediate(previewFoundationObject);
    }

    [Test]
    public void DoorwayBuildPieceCreatesThreeCollisionBoxes()
    {
        Type pieceType = FindType("BuildPiece");
        Type buildPieceType = FindType("BuildPieceType");
        Component doorwayPiece = new GameObject("DoorwayPiece").AddComponent(pieceType);
        PropertyInfo collisionBoxesProperty = pieceType.GetProperty("CollisionBoxes", BindingFlags.Instance | BindingFlags.Public);

        InvokeInstanceMethod(doorwayPiece, "Initialize", Enum.Parse(buildPieceType, "Doorway"), false);

        System.Collections.ICollection collisionBoxes = (System.Collections.ICollection)collisionBoxesProperty.GetValue(doorwayPiece);

        Assert.That(collisionBoxes.Count, Is.EqualTo(3));

        UnityEngine.Object.DestroyImmediate(doorwayPiece.gameObject);
    }

    [Test]
    public void PlacedFoundationIsNotMarkedAsPreview()
    {
        Type pieceType = FindType("BuildPiece");
        Type buildPieceType = FindType("BuildPieceType");
        Component placedFoundation = new GameObject("PlacedFoundation").AddComponent(pieceType);
        PropertyInfo isPreviewProperty = pieceType.GetProperty("IsPreview", BindingFlags.Instance | BindingFlags.Public);

        InvokeInstanceMethod(placedFoundation, "Initialize", Enum.Parse(buildPieceType, "Foundation"), false);

        bool isPreview = (bool)isPreviewProperty.GetValue(placedFoundation);

        Assert.That(isPreview, Is.False);

        UnityEngine.Object.DestroyImmediate(placedFoundation.gameObject);
    }

    [Test]
    public void FoundationSocketSnapOnlyTriggersWhenGroundPointIsNearSocket()
    {
        Type buildingSystemType = FindType("BuildingSystem");
        Type pieceType = FindType("BuildPiece");
        Type buildPieceType = FindType("BuildPieceType");

        Component buildingSystem = new GameObject("BuildingSystem").AddComponent(buildingSystemType);
        Component placedFoundation = new GameObject("PlacedFoundation").AddComponent(pieceType);
        InvokeInstanceMethod(placedFoundation, "Initialize", Enum.Parse(buildPieceType, "Foundation"), false);

        MethodInfo findSocketMethod = buildingSystemType.GetMethod("TryFindFoundationSocketNearPoint", BindingFlags.Instance | BindingFlags.NonPublic);
        Transform eastSocket = placedFoundation.transform.Find("FoundationEast");

        object[] nearArguments = { eastSocket.position + new Vector3(0.1f, 0f, 0.05f), null };
        bool foundNearSocket = (bool)findSocketMethod.Invoke(buildingSystem, nearArguments);

        object[] farArguments = { eastSocket.position + new Vector3(1.5f, 0f, 0f), null };
        bool foundFarSocket = (bool)findSocketMethod.Invoke(buildingSystem, farArguments);

        Assert.That(foundNearSocket, Is.True);
        Assert.That(nearArguments[1], Is.Not.Null);
        Assert.That(foundFarSocket, Is.False);
        Assert.That(farArguments[1], Is.Null);

        UnityEngine.Object.DestroyImmediate(buildingSystem.gameObject);
        UnityEngine.Object.DestroyImmediate(placedFoundation.gameObject);
    }

    [Test]
    public void StructureOverlapDetectsPiecesInSameSpace()
    {
        Type buildingSystemType = FindType("BuildingSystem");
        Type pieceType = FindType("BuildPiece");
        Type buildPieceType = FindType("BuildPieceType");
        Component buildingSystem = new GameObject("BuildingSystem").AddComponent(buildingSystemType);
        Component previewFoundation = new GameObject("PreviewFoundation").AddComponent(pieceType);
        Component placedFoundation = new GameObject("PlacedFoundation").AddComponent(pieceType);
        FieldInfo previewPieceField = buildingSystemType.GetField("previewPiece", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo hasStructureOverlapMethod = buildingSystemType.GetMethod("HasStructureOverlap", BindingFlags.Instance | BindingFlags.NonPublic);

        InvokeInstanceMethod(previewFoundation, "Initialize", Enum.Parse(buildPieceType, "Foundation"), true);
        InvokeInstanceMethod(placedFoundation, "Initialize", Enum.Parse(buildPieceType, "Foundation"), false);
        previewPieceField.SetValue(buildingSystem, previewFoundation);
        placedFoundation.transform.position = new Vector3(0f, 0.125f, 0f);

        bool overlaps = (bool)hasStructureOverlapMethod.Invoke(buildingSystem, new object[] { new Pose(new Vector3(0f, 0.125f, 0f), Quaternion.identity) });

        Assert.That(overlaps, Is.True);

        UnityEngine.Object.DestroyImmediate(buildingSystem.gameObject);
        UnityEngine.Object.DestroyImmediate(previewFoundation.gameObject);
        UnityEngine.Object.DestroyImmediate(placedFoundation.gameObject);
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
