using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Rotazione")]
    [Tooltip("Velocità di rotazione in gradi al secondo. Valori negativi invertono il senso.")]
    [SerializeField] private float rotationSpeed = 45f;

    [Tooltip("Se attivo, ruota nello spazio locale (consigliato per figli di altri oggetti).")]
    [SerializeField] private bool useLocalSpace = true;

    private void Update()
    {
        Space space = useLocalSpace ? Space.Self : Space.World;
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, space);
    }

    // Permette di cambiare la velocità da altri script (es. potenziamenti, danni al cristallo)
    public void SetSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }
}