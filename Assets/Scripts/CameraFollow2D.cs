using UnityEngine;

/// <summary>
/// Câmera simples que segue um alvo suavemente.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform alvo;
    [SerializeField] private float suavidade = 5f;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);

    private void LateUpdate()
    {
        if (alvo == null) return;

        Vector3 destino = new Vector3(alvo.position.x + offset.x, alvo.position.y + offset.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, destino, suavidade * Time.deltaTime);
    }

    public void DefinirAlvo(Transform novoAlvo) => alvo = novoAlvo;
}
