using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    public void EnsurePrototypeResources()
    {
        if (FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).Length > 0)
        {
            return;
        }

        SpawnNode(ResourceType.Wood, PrimitiveType.Cylinder, new Vector3(-3f, 0.6f, -3f), new Vector3(0.7f, 0.9f, 0.7f), 3);
        SpawnNode(ResourceType.Wood, PrimitiveType.Cylinder, new Vector3(3f, 0.6f, -3f), new Vector3(0.7f, 0.9f, 0.7f), 3);
        SpawnNode(ResourceType.Wood, PrimitiveType.Cylinder, new Vector3(-3f, 0.6f, 3f), new Vector3(0.7f, 0.9f, 0.7f), 3);
        SpawnNode(ResourceType.Wood, PrimitiveType.Cylinder, new Vector3(3f, 0.6f, 3f), new Vector3(0.7f, 0.9f, 0.7f), 3);

        SpawnNode(ResourceType.Scrap, PrimitiveType.Cube, new Vector3(-2.2f, 0.5f, 0f), new Vector3(0.75f, 0.75f, 0.75f), 2);
        SpawnNode(ResourceType.Scrap, PrimitiveType.Cube, new Vector3(2.2f, 0.5f, 0f), new Vector3(0.75f, 0.75f, 0.75f), 2);
        SpawnNode(ResourceType.Scrap, PrimitiveType.Cube, new Vector3(0f, 0.5f, 2.2f), new Vector3(0.75f, 0.75f, 0.75f), 2);

        SpawnNode(ResourceType.Food, PrimitiveType.Sphere, new Vector3(0f, 0.45f, -2.2f), new Vector3(0.6f, 0.6f, 0.6f), 1);
        SpawnNode(ResourceType.Food, PrimitiveType.Sphere, new Vector3(-1.3f, 0.45f, 1.4f), new Vector3(0.6f, 0.6f, 0.6f), 1);
        SpawnNode(ResourceType.Food, PrimitiveType.Sphere, new Vector3(1.3f, 0.45f, 1.4f), new Vector3(0.6f, 0.6f, 0.6f), 1);
    }

    private void SpawnNode(ResourceType resourceType, PrimitiveType primitiveType, Vector3 position, Vector3 scale, int amount)
    {
        GameObject node = GameObject.CreatePrimitive(primitiveType);
        node.name = $"{resourceType}Node";
        node.transform.SetParent(transform, false);
        node.transform.position = position;
        node.transform.localScale = scale;

        ResourceNode resourceNode = node.AddComponent<ResourceNode>();
        resourceNode.Configure(resourceType, amount);
    }
}
